using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Verse
{
	public static class XmlLoaderSimple
	{
		[DebuggerHidden]
		public static IEnumerable<KeyValuePair<string, string>> ValuesFromXmlFile(FileInfo file)
		{
			XDocument doc = XDocument.Load(file.FullName);
			foreach (XElement element in doc.Root.Elements())
			{
				string key = element.Name.ToString();
				string value = element.Value;
				value = value.Replace("\\n", "\n");
				yield return new KeyValuePair<string, string>(key, value);
			}
		}
	}
}
