using System;
using System.Collections.Generic;

namespace Verse
{
	public static class GrammarResolverSimpleStringExtensions
	{
		private static List<string> argsLabels = new List<string>();

		private static List<object> argsObjects = new List<object>();

		public static string Formatted(this string str, NamedArgument arg1)
		{
			GrammarResolverSimpleStringExtensions.argsLabels.Clear();
			GrammarResolverSimpleStringExtensions.argsObjects.Clear();
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg1.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg1.arg);
			return GrammarResolverSimple.Formatted(str, GrammarResolverSimpleStringExtensions.argsLabels, GrammarResolverSimpleStringExtensions.argsObjects);
		}

		public static string Formatted(this string str, NamedArgument arg1, NamedArgument arg2)
		{
			GrammarResolverSimpleStringExtensions.argsLabels.Clear();
			GrammarResolverSimpleStringExtensions.argsObjects.Clear();
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg1.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg1.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg2.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg2.arg);
			return GrammarResolverSimple.Formatted(str, GrammarResolverSimpleStringExtensions.argsLabels, GrammarResolverSimpleStringExtensions.argsObjects);
		}

		public static string Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
		{
			GrammarResolverSimpleStringExtensions.argsLabels.Clear();
			GrammarResolverSimpleStringExtensions.argsObjects.Clear();
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg1.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg1.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg2.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg2.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg3.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg3.arg);
			return GrammarResolverSimple.Formatted(str, GrammarResolverSimpleStringExtensions.argsLabels, GrammarResolverSimpleStringExtensions.argsObjects);
		}

		public static string Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
		{
			GrammarResolverSimpleStringExtensions.argsLabels.Clear();
			GrammarResolverSimpleStringExtensions.argsObjects.Clear();
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg1.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg1.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg2.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg2.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg3.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg3.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg4.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg4.arg);
			return GrammarResolverSimple.Formatted(str, GrammarResolverSimpleStringExtensions.argsLabels, GrammarResolverSimpleStringExtensions.argsObjects);
		}

		public static string Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5)
		{
			GrammarResolverSimpleStringExtensions.argsLabels.Clear();
			GrammarResolverSimpleStringExtensions.argsObjects.Clear();
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg1.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg1.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg2.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg2.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg3.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg3.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg4.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg4.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg5.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg5.arg);
			return GrammarResolverSimple.Formatted(str, GrammarResolverSimpleStringExtensions.argsLabels, GrammarResolverSimpleStringExtensions.argsObjects);
		}

		public static string Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6)
		{
			GrammarResolverSimpleStringExtensions.argsLabels.Clear();
			GrammarResolverSimpleStringExtensions.argsObjects.Clear();
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg1.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg1.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg2.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg2.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg3.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg3.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg4.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg4.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg5.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg5.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg6.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg6.arg);
			return GrammarResolverSimple.Formatted(str, GrammarResolverSimpleStringExtensions.argsLabels, GrammarResolverSimpleStringExtensions.argsObjects);
		}

		public static string Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6, NamedArgument arg7)
		{
			GrammarResolverSimpleStringExtensions.argsLabels.Clear();
			GrammarResolverSimpleStringExtensions.argsObjects.Clear();
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg1.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg1.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg2.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg2.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg3.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg3.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg4.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg4.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg5.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg5.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg6.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg6.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg7.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg7.arg);
			return GrammarResolverSimple.Formatted(str, GrammarResolverSimpleStringExtensions.argsLabels, GrammarResolverSimpleStringExtensions.argsObjects);
		}

		public static string Formatted(this string str, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6, NamedArgument arg7, NamedArgument arg8)
		{
			GrammarResolverSimpleStringExtensions.argsLabels.Clear();
			GrammarResolverSimpleStringExtensions.argsObjects.Clear();
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg1.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg1.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg2.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg2.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg3.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg3.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg4.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg4.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg5.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg5.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg6.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg6.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg7.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg7.arg);
			GrammarResolverSimpleStringExtensions.argsLabels.Add(arg8.label);
			GrammarResolverSimpleStringExtensions.argsObjects.Add(arg8.arg);
			return GrammarResolverSimple.Formatted(str, GrammarResolverSimpleStringExtensions.argsLabels, GrammarResolverSimpleStringExtensions.argsObjects);
		}

		public static string Formatted(this string str, params NamedArgument[] args)
		{
			GrammarResolverSimpleStringExtensions.argsLabels.Clear();
			GrammarResolverSimpleStringExtensions.argsObjects.Clear();
			for (int i = 0; i < args.Length; i++)
			{
				GrammarResolverSimpleStringExtensions.argsLabels.Add(args[i].label);
				GrammarResolverSimpleStringExtensions.argsObjects.Add(args[i].arg);
			}
			return GrammarResolverSimple.Formatted(str, GrammarResolverSimpleStringExtensions.argsLabels, GrammarResolverSimpleStringExtensions.argsObjects);
		}
	}
}
