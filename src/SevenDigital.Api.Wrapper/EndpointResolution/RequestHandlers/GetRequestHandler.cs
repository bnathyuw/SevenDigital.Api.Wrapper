using System.Net.Http;
using System.Threading.Tasks;
using SevenDigital.Api.Wrapper.Http;
using OAuth;

namespace SevenDigital.Api.Wrapper.EndpointResolution.RequestHandlers
{
	public class GetRequestHandler : RequestHandler
	{
		private readonly IOAuthCredentials _oAuthCredentials;

		public GetRequestHandler(IApiUri apiUri, IOAuthCredentials oAuthCredentials) : base(apiUri)
		{
			_oAuthCredentials = oAuthCredentials;
		}

		public override bool HandlesMethod(HttpMethod method)
		{
			return method == HttpMethod.Get;
		}

		public override Task<Response> HitEndpoint(RequestData requestData)
		{
			var getRequest = BuildGetRequest(requestData);
			return HttpClient.GetAsync(getRequest);
		}

		private GetRequest BuildGetRequest(RequestData requestData)
		{
			var apiRequest = MakeApiRequest(requestData);
			var signedUrl = SignHttpGetUrl(apiRequest, requestData);
			var getRequest = new GetRequest(signedUrl, requestData.Headers);
			return getRequest;
		}

		private string SignHttpGetUrl(ApiRequest apiRequest, RequestData requestData)
		{
			if (!requestData.RequiresSignature)
			{
				apiRequest.Parameters.Add("oauth_consumer_key", _oAuthCredentials.ConsumerKey);
				return apiRequest.FullUri;
			}

			var oauthRequest = new OAuthRequest
				{
					Type = OAuthRequestType.ProtectedResource,
					RequestUrl = apiRequest.AbsoluteUrl,
					Method = "GET",
					ConsumerKey = _oAuthCredentials.ConsumerKey,
					ConsumerSecret = _oAuthCredentials.ConsumerSecret
				};

			AddTokenIfRequired(oauthRequest, requestData);

			return apiRequest.AbsoluteUrl 
				+ "?" 
				+ oauthRequest.GetAuthorizationQuery(apiRequest.Parameters) 
				+ apiRequest.Parameters.ToQueryString();
		}

		private void AddTokenIfRequired(OAuthRequest oauthRequest, RequestData requestData)
		{
			if (requestData.HasToken)
			{
				oauthRequest.Token = requestData.UserToken;
				oauthRequest.TokenSecret = requestData.TokenSecret;
			}
		}

		public override string GetDebugUri(RequestData requestData)
		{
			var apiRequest = MakeApiRequest(requestData);
			apiRequest.Parameters.Add("oauth_consumer_key", _oAuthCredentials.ConsumerKey);
			return apiRequest.FullUri;
		}
	}
}