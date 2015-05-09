using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Syringe.Core.Configuration;
using Syringe.Core.Logging;

namespace Syringe.Core.Runner
{
	internal class SessionVariables
	{
		private readonly Dictionary<string, string> _currentVariables;

		public SessionVariables()
		{
			_currentVariables = new Dictionary<string, string>();
		}

		public void AddGlobalVariables(Config config)
		{
			if (!string.IsNullOrEmpty(config.BaseUrl))
				_currentVariables.Add("baseurl", config.BaseUrl);

			foreach (Variable variable in config.Variables)
			{
				_currentVariables.Add(variable.Name, variable.Value);
			}
		}

		public void AddOrUpdateVariables(Dictionary<string, string> variables)
		{
			foreach (KeyValuePair<string, string> keyValuePair in variables)
			{
				AddOrUpdateVariable(keyValuePair.Key, keyValuePair.Value);
			}
		}

		public void AddOrUpdateVariable(string name, string value)
		{
			if (_currentVariables.ContainsKey(name))
			{
				_currentVariables[name] = value;
			}
			else
			{
				_currentVariables.Add(name, value);
			}
		}

		public string GetVariableValue(string key)
		{
			if (_currentVariables.ContainsKey(key))
				return _currentVariables[key];

			return "";
		}

		public string ReplacePlainTextVariablesIn(string text)
		{
			foreach (KeyValuePair<string, string> keyValuePair in _currentVariables)
			{
				text = text.Replace("{" + keyValuePair.Key + "}", keyValuePair.Value);
			}

			return text;
		}

		public string ReplaceVariablesIn(string text)
		{
			foreach (KeyValuePair<string, string> keyValuePair in _currentVariables)
			{
				text = text.Replace("{" + keyValuePair.Key + "}", Regex.Escape(keyValuePair.Value));
			}

			return text;
		}

		public void Dump()
		{
			Log.Information("In my bag of magic variables I have:");

			foreach (KeyValuePair<string, string> keyValuePair in _currentVariables)
			{
				Log.Information(" - {{{0}}} : {1}", keyValuePair.Key, keyValuePair.Value);
			}
		}
	}
}