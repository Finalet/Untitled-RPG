﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace AwesomeTechnologies.Extensions
{
	public static class ReflectionExtentions
	{
		public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException(nameof(assembly));
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(t => t != null);
			}
		}
	}
}
