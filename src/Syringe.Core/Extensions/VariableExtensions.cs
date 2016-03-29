﻿using System;
using System.Collections.Generic;
using System.Linq;
using Syringe.Core.TestCases;

namespace Syringe.Core.Extensions
{
	public static class VariableExtensions
	{
		public static string ValueByName(this List<Variable> list, string name)
		{
			Variable item = list.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
			if (item != null)
			{
				return item.Value;
			}
			else
			{
				return "";
			}
		}
	}
}
