using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Verse
{
	public static class BackstoryTranslationUtility
	{
		[DebuggerHidden]
		private static IEnumerable<XElement> BackstoryTranslationElements(LoadedLanguage lang)
		{
			foreach (string folderPath in lang.FolderPaths)
			{
				string localFolderPath = folderPath;
				FileInfo fi = new FileInfo(Path.Combine(localFolderPath.ToString(), "Backstories/Backstories.xml"));
				if (!fi.Exists)
				{
					break;
				}
				XDocument doc;
				try
				{
					doc = XDocument.Load(fi.FullName);
				}
				catch (Exception ex)
				{
					Exception e = ex;
					Log.Warning(string.Concat(new object[]
					{
						"Exception loading backstory translation data from file ",
						fi,
						": ",
						e
					}));
					break;
				}
				foreach (XElement element in doc.Root.Elements())
				{
					yield return element;
				}
			}
		}

		public static void LoadAndInjectBackstoryData(LoadedLanguage lang)
		{
			foreach (XElement current in BackstoryTranslationUtility.BackstoryTranslationElements(lang))
			{
				string text = "[unknown]";
				try
				{
					text = current.Name.ToString();
					string value = current.Element("title").Value;
					string value2 = current.Element("titleShort").Value;
					string value3 = current.Element("desc").Value;
					Backstory backstory;
					if (!BackstoryDatabase.TryGetWithIdentifier(text, out backstory))
					{
						throw new Exception("Backstory not found matching identifier " + text);
					}
					if (value == backstory.Title && value2 == backstory.TitleShort && value3 == backstory.baseDesc)
					{
						Log.Error("Backstory translation exactly matches default data: " + text);
					}
					else
					{
						if (value != null)
						{
							backstory.SetTitle(value);
						}
						if (value2 != null)
						{
							backstory.SetTitleShort(value2);
						}
						if (value3 != null)
						{
							backstory.baseDesc = value3;
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warning(string.Concat(new object[]
					{
						"Couldn't load backstory ",
						text,
						": ",
						ex,
						"\nFull XML text:\n\n",
						current.ToString()
					}));
				}
			}
		}

		[DebuggerHidden]
		public static IEnumerable<string> MissingBackstoryTranslations(LoadedLanguage lang)
		{
			List<string> neededTranslations = (from kvp in BackstoryDatabase.allBackstories
			select kvp.Key).ToList<string>();
			foreach (XElement element in BackstoryTranslationUtility.BackstoryTranslationElements(lang))
			{
				string identifier = element.Name.ToString();
				if (neededTranslations.Contains(identifier))
				{
					neededTranslations.Remove(identifier);
				}
				else
				{
					yield return "Translation doesn't correspond to any backstory: " + identifier;
				}
				string title = element.Element("title").Value;
				string titleShort = element.Element("titleShort").Value;
				string desc = element.Element("desc").Value;
				if (title.NullOrEmpty())
				{
					yield return identifier + ".title missing";
				}
				if (titleShort.NullOrEmpty())
				{
					yield return identifier + ".titleShort missing";
				}
				if (desc.NullOrEmpty())
				{
					yield return identifier + ".desc missing";
				}
			}
			foreach (string tra in neededTranslations)
			{
				yield return "Missing backstory: " + tra;
			}
		}
	}
}
