using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class LogSimple
	{
		private static List<string> messages = new List<string>();

		private static int tabDepth = 0;

		public static void Message(string text)
		{
			for (int i = 0; i < LogSimple.tabDepth; i++)
			{
				text = "  " + text;
			}
			LogSimple.messages.Add(text);
		}

		public static void BeginTabMessage(string text)
		{
			LogSimple.Message(text);
			LogSimple.tabDepth++;
		}

		public static void EndTab()
		{
			LogSimple.tabDepth--;
		}

		public static void FlushToFileAndOpen()
		{
			if (LogSimple.messages.Count == 0)
			{
				return;
			}
			string value = LogSimple.CompiledLog();
			string path = GenFilePaths.SaveDataFolderPath + Path.DirectorySeparatorChar + "LogSimple.txt";
			using (StreamWriter streamWriter = new StreamWriter(path, false))
			{
				streamWriter.Write(value);
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				Application.OpenURL(path);
			});
			LogSimple.messages.Clear();
		}

		public static void FlushToStandardLog()
		{
			if (LogSimple.messages.Count == 0)
			{
				return;
			}
			string text = LogSimple.CompiledLog();
			Log.Message(text);
			LogSimple.messages.Clear();
		}

		private static string CompiledLog()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string current in LogSimple.messages)
			{
				stringBuilder.AppendLine(current);
			}
			return stringBuilder.ToString().TrimEnd(new char[0]);
		}
	}
}
