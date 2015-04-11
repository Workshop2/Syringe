﻿using System.IO;
using System.Linq;
using System.Xml.Linq;
using ConfigurationException = Syringe.Core.Exceptions.ConfigurationException;

namespace Syringe.Core.Xml
{
	internal class ConfigReader
	{
		public Config Read(TextReader textReader)
		{
			var config = new Config();
			XDocument doc = XDocument.Load(textReader);

			// Check for <root>
			XElement rootElement = doc.Elements().FirstOrDefault(i => i.Name.LocalName == "root");
			if (rootElement == null)
				throw new ConfigurationException("<root> node is missing from the config file.");

			// Fill the known properties
			config.BaseUrl = XmlHelper.GetOptionalElementValue(rootElement, "baseurl");
			config.Proxy = XmlHelper.GetOptionalElementValue(rootElement, "proxy");
			config.Useragent = XmlHelper.GetOptionalElementValue(rootElement, "useragent");
			config.Httpauth = XmlHelper.GetOptionalElementValue(rootElement, "httpauth");
			config.GlobalHttpLog = XmlHelper.GetOptionalElementValue(rootElement, "globalhttplog");
			config.Comment = XmlHelper.GetOptionalElementValue(rootElement, "comment");
			config.Timeout = XmlHelper.GetOptionalElementValue(rootElement, "timeout");
			config.GlobalTimeout = XmlHelper.GetOptionalElementValue(rootElement, "globaltimeout");

			// All elements get stored in the variables, for custom variables.
			foreach (XElement element in rootElement.Elements())
			{
				if (!config.Variables.ContainsKey(element.Name.LocalName))
				{
					config.Variables.Add(element.Name.LocalName, element.Value);
				}
			}

			return config;
		}
	}
}