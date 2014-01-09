using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SevenDigital.Api.Wrapper.Http;
using OAuth;

namespace SevenDigital.Api.Wrapper.EndpointResolution.RequestHandlers
{
	public class PostRequestHandler : RequestHandler
	{
		private readonly IOAuthCredentials _oAuthCredentials;

		public PostRequestHandler(IApiUri apiUri, IOAuthCredentials oAuthCredentials)
			: base(apiUri)
		{
			_oAuthCredentials = oAuthCredentials;
		}

		public override bool HandlesMethod(HttpMethod method)
		{
			return method == HttpMethod.Post;
		}

		public override Task<Response> HitEndpoint(RequestData requestData)
		{
			var postRequest = BuildPostRequest(requestData);
			return HttpClient.PostAsync(postRequest);
		}

		private PostRequest BuildPostRequest(RequestData requestData)
		{
			var apiRequest = MakeApiRequest(requestData);
			var requestBody = SignHttpPostParams(apiRequest, requestData);
			var postRequest = new PostRequest(apiRequest.AbsoluteUrl, requestData.Headers, requestBody);
			return postRequest;
		}

		private string SignHttpPostParams(ApiRequest apiRequest, RequestData requestData)
		{
			if (!requestData.RequiresSignature)
			{
				var @params = new Dictionary<string, string>(apiRequest.Parameters)
					{
						{"oauth_consumer_key", _oAuthCredentials.ConsumerKey}
					};

				return @params.ToQueryString();
			}

			var oauthRequest = new OAuthRequest
				{
					Type = OAuthRequestType.ProtectedResource,
					RequestUrl = apiRequest.AbsoluteUrl,
					Method = "POST",
					ConsumerKey = _oAuthCredentials.ConsumerKey,
					ConsumerSecret = _oAuthCredentials.ConsumerSecret
				};

			AddTokenIfRequired(oauthRequest, requestData);

			return oauthRequest.GetAuthorizationQuery(apiRequest.Parameters) + apiRequest.Parameters.ToQueryString();
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
			return MakeApiRequest(requestData).FullUri;
		}
	}
}