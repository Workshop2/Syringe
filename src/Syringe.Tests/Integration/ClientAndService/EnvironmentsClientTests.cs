﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Syringe.Client;
using Syringe.Core.Environment;

namespace Syringe.Tests.Integration.ClientAndService
{
	[TestFixture]
	public class EnvironmentsClientTests
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			ServiceConfig.StartSelfHostedOwin();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			ServiceConfig.OwinServer.Dispose();
		}

		[Test]
		public void List_should_return_list_of_environments()
		{
			// This test relies on the environments.json in the service always having some environments. 

			// given
			var client = new EnvironmentsClient(ServiceConfig.BaseUrl);

			// when
			IEnumerable<Environment> environments = client.List();

			// then
			Assert.That(environments, Is.Not.Null);
			Assert.That(environments.Count(), Is.GreaterThan(0));
		}
	}
}
