﻿using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Syringe.Core.Xml;
using XmlException = Syringe.Core.Exceptions.XmlException;

namespace Syringe.Tests.Unit.Xml
{
    public class XmlHelperTests
    {
		[Test]
		public void GetOptionalElementValue_should_return_element_value()
		{
			// Arrange
			string xml = GetBasicXml();
			var element = XElement.Parse(xml);

			// Act
			string actualValue = XmlHelper.GetOptionalElementValue(element, "baseurl");

			// Assert
			Assert.That(actualValue, Is.EqualTo("a value"));
		}

		[Test]
		public void GetOptionalElementValue_should_return_empty_string_when_element_does_not_exist()
		{
			// Arrange
			string xml = GetBasicXml();
			var element = XElement.Parse(xml);

			// Act
			string actualValue = XmlHelper.GetOptionalElementValue(element, "foobar");

			// Assert
			Assert.That(actualValue, Is.EqualTo(""));
		}

		[Test]
		public void GetRequiredElementValue_should_return_element_value()
		{
			// Arrange
			string xml = GetBasicXml();
			var element = XElement.Parse(xml);

			// Act
			string actualValue = XmlHelper.GetRequiredElementValue(element, "baseurl");

			// Assert
			Assert.That(actualValue, Is.EqualTo("a value"));
		}

		[Test]
	    public void GetRequiredElementValue_should_throw_exception_for_missing_element()
	    {
		    // Arrange
			string xml = GetBasicXml();
			var element = XElement.Parse(xml);

			// Act + Assert
			Assert.Throws<XmlException>(() => XmlHelper.GetRequiredElementValue(element, "foobar"));
	    }

		[Test]
		public void ReEncodeAttributeValues_should_change_invalid_xml_into_valid_xml()
		{
			// Arrange
			string invalidXml = GetInvalidXml();
			string nbsp = XmlConvert.EncodeName("&nbsp;");

			// Act
			string validXml = XmlHelper.ReEncodeAttributeValues(invalidXml);

			// Assert
			XDocument document = XDocument.Parse(validXml);
			XAttribute verifyPositive = document.Root.Elements().First(x => x.Name.LocalName == "case").Attribute("verifypositive");
			Assert.That(verifyPositive.Value, Is.EqualTo("<SELECT>somequerystring=a&anotherquerystring=b" +nbsp));
		}

		[Test]
		public void ReEncodeAttributeValues_should_ignore_empty_attribute_value()
		{
			// Arrange
			string xml = GetInvalidXml();
			xml = xml.Replace(@"verifypositive=""\<SELECT\>somequerystring=a&anotherquerystring=b&nbsp;""", "verifypositive=\"\"");

			// Act
			string validXml = XmlHelper.ReEncodeAttributeValues(xml);

			// Assert
			XDocument document = XDocument.Parse(validXml);
			XAttribute verifyPositive = document.Root.Elements().First(x => x.Name.LocalName == "case").Attribute("verifypositive");
			Assert.That(verifyPositive.Value, Is.EqualTo(""));
		}

		[Test]
		[TestCase("verifypositive")]
		[TestCase("verifypositive99")]
		[TestCase("verifynegative")]
		[TestCase("verifypositive88")]
		[TestCase("verifynextpositive")]
		[TestCase("verifynextnegative")]
		[TestCase("url")]
		[TestCase("addheader")]
		[TestCase("postbody")]
		public void ReEncodeAttributeValues_should_transform_all_known_webinject_attributes(string attributeName)
		{
			// Arrange
			string invalidXml = GetInvalidXml(attributeName);
			string nbsp = XmlConvert.EncodeName("&nbsp;");

			// Act
			string validXml = XmlHelper.ReEncodeAttributeValues(invalidXml);

			// Assert
			XDocument document = XDocument.Parse(validXml);
			XAttribute verifyPositive = document.Root.Elements().First(x => x.Name.LocalName == "case").Attribute(attributeName);
			Assert.That(verifyPositive.Value, Is.EqualTo("<SELECT>somequerystring=a&anotherquerystring=b" + nbsp));
		}

		[Test]
		public void GetRequiredAttribute_should_return_attribute_value()
		{
			// Arrange
			string xml = GetXmlWithAttributes();
			var element = XElement.Parse(xml);

			// Act
			string actualValue = XmlHelper.GetRequiredAttribute(element, "repeat");

			// Assert
			Assert.That(actualValue, Is.EqualTo("1333"));
		}

		[Test]
		public void GetRequiredAttribute_should_throw_exception_when_attribute_does_not_exist()
		{
			// Arrange
			string xml = GetXmlWithAttributes();
			var element = XElement.Parse(xml);

			// Act + Assert
			Assert.Throws<XmlException>(() => XmlHelper.GetRequiredAttribute(element, "blah"));
		}

		[Test]
		public void GetOptionalAttribute_should_return_attribute_value()
		{
			// Arrange
			string xml = GetXmlWithAttributes();
			var element = XElement.Parse(xml);

			// Act
			string actualValue = XmlHelper.GetOptionalAttribute(element, "repeat");

			// Assert
			Assert.That(actualValue, Is.EqualTo("1333"));
		}

		[Test]
		public void GetOptionalAttribute_should_return_empty_string_when_attribute_does_not_exist()
		{
			// Arrange
			string xml = GetXmlWithAttributes();
			var element = XElement.Parse(xml);

			// Act
			string actualValue = XmlHelper.GetOptionalAttribute(element, "blah");

			// Assert
			Assert.That(actualValue, Is.EqualTo(""));
		}

		[Test]
		public void GetOptionalAttribute_should_return_defaultvalue_when_attribute_does_not_exist()
		{
			// Arrange
			string xml = GetXmlWithAttributes();
			var element = XElement.Parse(xml);

			// Act
			string actualValue = XmlHelper.GetOptionalAttribute(element, "blah", "default value");

			// Assert
			Assert.That(actualValue, Is.EqualTo("default value"));
		}

	    private string GetBasicXml()
	    {
		    return @"<?xml version=""1.0"" encoding=""utf-8""?>
		            <root>
						<baseurl>a value</baseurl>
					</root>";
	    }

		private string GetXmlWithAttributes()
		{
			return @"<?xml version=""1.0"" encoding=""utf-8""?>
		            <testcases repeat=""1333"">
							<case
								id=""2""
								verifypositive=""verify this string exists""
								verifynextpositive=""{TIMESTAMP}""
							/>
					</testcases>";
		}

		private string GetInvalidXml(string attributeName = "verifypositive")
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
		            <testcases>
						<case
							id=""1""
							description1=""short description""
							{attributeName}=""\<SELECT\>somequerystring=a&anotherquerystring=b&nbsp;""
						/>
					</testcases>";
			return  xml.Replace("{attributeName}", attributeName);
		}
    }
}
