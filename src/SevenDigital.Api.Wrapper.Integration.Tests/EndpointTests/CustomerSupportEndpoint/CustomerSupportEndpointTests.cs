﻿using NUnit.Framework;
using SevenDigital.Api.Schema.CustomerSupport;

namespace SevenDigital.Api.Wrapper.Integration.Tests.EndpointTests.CustomerSupportEndpoint
{
	[TestFixture]
	public class CustomerSupportEndpointTests
	{
		[Test]
		public async void Should_get_an_empty_response()
		{
			var reponse = await Api<CustomerSupport>
				.Create
				.WithParameter("Email", "web-devteam-testing@7digital.com")
				.WithParameter("Message", "Integration test")
				.WithParameter("ClientName", "Web Team")
				.WithParameter("AdditionalClientInfo", "")
				.WithParameter("PartnerId", "0")
				.WithParameter("ShopId", "34")
				.PleaseAsync();

			Assert.That(reponse, Is.Not.Null);
		}
	}
}
