﻿using System;
using Newtonsoft.Json;

namespace Syringe.Core.Configuration
{
	public class JsonConfiguration : IConfiguration
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ServiceUrl { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string WebsiteUrl { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string TestCasesBaseDirectory { get; set; }

		public OAuthConfiguration OAuthConfiguration { get; set; }
		public GitConfiguration GitConfiguration { get; set; }

		public JsonConfiguration()
		{
			WebsiteUrl = "http://localhost:1980";
			ServiceUrl = "http://*:1981";
			TestCasesBaseDirectory = @"D:\Syringe\";
			OAuthConfiguration = new OAuthConfiguration();
			GitConfiguration = new GitConfiguration();
		}
	}
}
