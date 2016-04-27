﻿using Newtonsoft.Json;

namespace Syringe.Core.Configuration
{
	public class JsonConfiguration : IConfiguration
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string ServiceUrl { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string WebsiteUrl { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string TestFilesBaseDirectory { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TestFileFormat TestFileFormat { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string MongoDbDatabaseName { get; set; }

		public OAuthConfiguration OAuthConfiguration { get; set; }
		public GitConfiguration GitConfiguration { get; set; }

        public OctopusConfiguration OctopusConfiguration { get; set; }

        public JsonConfiguration()
		{
			// Defaults
			WebsiteUrl = "http://localhost:1980";
			ServiceUrl = "http://*:1981";
			TestFilesBaseDirectory = @"D:\Syringe\";
            TestFileFormat = TestFileFormat.Xml;
			MongoDbDatabaseName = "Syringe";

			OAuthConfiguration = new OAuthConfiguration();
			GitConfiguration = new GitConfiguration();
		}
	}
}
