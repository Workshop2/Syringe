﻿namespace Syringe.Core.Configuration
{
	public interface IConfiguration
	{
		string ServiceUrl { get; }
		string WebsiteUrl { get; }
		string TestFilesBaseDirectory { get; }
	    TestFileFormat TestFileFormat { get; }
		OAuthConfiguration OAuthConfiguration { get; }
        OctopusConfiguration OctopusConfiguration { get; }
		bool ReadonlyMode { get; }
	    DataStoreType DataStore { get; set; }
        string ScriptSnippetDirectory { get; set; }
		string EncryptionKey { get; set; }
	}
}