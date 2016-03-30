﻿using System.Collections.Generic;
using NUnit.Framework;
using Syringe.Core.Extensions;
using Syringe.Core.Logging;
using Syringe.Core.Runner;
using Syringe.Core.Tests;

namespace Syringe.Tests.Unit.Runner
{
	public class ParseResponseMatcherTests
	{
		[SetUp]
		public void Setup()
		{
			TestHelpers.EnableLogging();
        }

		[Test]
		public void should_match_regex_groups_and_set_variable_names_and_values_to_matched_items()
		{
			// Arrange
			var parseResponses = new List<CapturedVariable>()
			{
				new CapturedVariable("1", @"(\d+)"),
				new CapturedVariable("foo", "(<html.+?>)")
			};
			string content = "<html class='bootstrap'><p>Tap tap tap 123</p></html>";

			// Act
			List<Variable> variables = CapturedVariableProvider.MatchVariables(parseResponses, content, new SimpleLogger());

			// Assert
			Assert.That(variables.Count, Is.EqualTo(2));
			Assert.That(variables.ValueByName("capturedvariable1"), Is.EqualTo("123"));
			Assert.That(variables.ValueByName("capturedvariablefoo"), Is.EqualTo("<html class='bootstrap'>"));
		}

		[Test]
		public void should_set_value_to_empty_string_when_regex_is_not_matched()
		{
			// Arrange
			var parseResponses = new List<CapturedVariable>()
			{
				new CapturedVariable("1", @"foo"),
				new CapturedVariable("2", @"bar"),

			};
			string content = "<html>123 abc</html>";

			// Act
			List<Variable> variables = CapturedVariableProvider.MatchVariables(parseResponses, content, new SimpleLogger());

			// Assert
			Assert.That(variables.Count, Is.EqualTo(2));
			Assert.That(variables.ValueByName("capturedvariable1"), Is.EqualTo(""));
			Assert.That(variables.ValueByName("capturedvariable2"), Is.EqualTo(""));
		}

		[Test]
		public void should_set_value_to_empty_string_when_regex_is_invalid()
		{
			// Arrange
			var parseResponses = new List<CapturedVariable>()
			{
				new CapturedVariable("1", @"(\d+)"),
				new CapturedVariable("2", @"(() this is a bad regex?("),

			};
			string content = "<html>123 abc</html>";

			// Act
			List<Variable> variables = CapturedVariableProvider.MatchVariables(parseResponses, content, new SimpleLogger());

			// Assert
			Assert.That(variables.Count, Is.EqualTo(2));
			Assert.That(variables.ValueByName("capturedvariable1"), Is.EqualTo("123"));
			Assert.That(variables.ValueByName("capturedvariable2"), Is.EqualTo(""));
		}

		[Test]
		public void should_concatenate_multiple_matches_into_variable_value()
		{
			// Arrange
			var parseResponses = new List<CapturedVariable>()
			{
				new CapturedVariable("1", @"(\d+)"),
			};
			string content = "<html>The number 3 and the number 4 combined make 7</html>";

			// Act
			List<Variable> variables = CapturedVariableProvider.MatchVariables(parseResponses, content, new SimpleLogger());

			// Assert
			Assert.That(variables.ValueByName("capturedvariable1"), Is.EqualTo("347"));
		}
	}
}