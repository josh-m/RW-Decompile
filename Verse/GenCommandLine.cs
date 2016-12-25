using System;
using System.Linq;

namespace Verse
{
	public static class GenCommandLine
	{
		public static bool CommandLineArgPassed(string key)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (string.Compare(commandLineArgs[i], key, true) == 0 || string.Compare(commandLineArgs[i], "-" + key, true) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryGetCommandLineArg(string key, out string value)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (commandLineArgs[i].Contains('='))
				{
					string[] array = commandLineArgs[i].Split(new char[]
					{
						'='
					});
					if (array.Length == 2)
					{
						if (string.Compare(array[0], key, true) == 0 || string.Compare(array[0], "-" + key, true) == 0)
						{
							value = array[1];
							return true;
						}
					}
				}
			}
			value = null;
			return false;
		}
	}
}
