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
		public const string BackstoriesFolder = "Backstories";

		public const string BackstoriesFileName = "Backstories.xml";

		[DebuggerHidden]
		private static IEnumerable<XElement> BackstoryTranslationElements(IEnumerable<string> folderPaths, List<string> loadErrors)
		{
			foreach (string folderPath in folderPaths)
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
					if (loadErrors != null)
					{
						loadErrors.Add(string.Concat(new object[]
						{
							"Exception loading backstory translation data from file ",
							fi,
							": ",
							ex
						}));
					}
					break;
				}
				foreach (XElement element in doc.Root.Elements())
				{
					yield return element;
				}
			}
		}

		public static void LoadAndInjectBackstoryData(IEnumerable<string> folderPaths, List<string> loadErrors)
		{
			foreach (XElement current in BackstoryTranslationUtility.BackstoryTranslationElements(folderPaths, loadErrors))
			{
				string text = "[unknown]";
				try
				{
					text = current.Name.ToString();
					string text2 = BackstoryTranslationUtility.GetText(current, "title");
					string text3 = BackstoryTranslationUtility.GetText(current, "titleFemale");
					string text4 = BackstoryTranslationUtility.GetText(current, "titleShort");
					string text5 = BackstoryTranslationUtility.GetText(current, "titleShortFemale");
					string text6 = BackstoryTranslationUtility.GetText(current, "desc");
					Backstory backstory;
					if (!BackstoryDatabase.TryGetWithIdentifier(text, out backstory, false))
					{
						throw new Exception("Backstory not found matching identifier " + text);
					}
					if (text2 == backstory.title && text3 == backstory.titleFemale && text4 == backstory.titleShort && text5 == backstory.titleShortFemale && text6 == backstory.baseDesc)
					{
						throw new Exception("Backstory translation exactly matches default data: " + text);
					}
					if (text2 != null)
					{
						backstory.SetTitle(text2, backstory.titleFemale);
						backstory.titleTranslated = true;
					}
					if (text3 != null)
					{
						backstory.SetTitle(backstory.title, text3);
						backstory.titleFemaleTranslated = true;
					}
					if (text4 != null)
					{
						backstory.SetTitleShort(text4, backstory.titleShortFemale);
						backstory.titleShortTranslated = true;
					}
					if (text5 != null)
					{
						backstory.SetTitleShort(backstory.titleShort, text5);
						backstory.titleShortFemaleTranslated = true;
					}
					if (text6 != null)
					{
						backstory.baseDesc = text6;
						backstory.descTranslated = true;
					}
				}
				catch (Exception ex)
				{
					loadErrors.Add(string.Concat(new string[]
					{
						"Couldn't load backstory ",
						text,
						": ",
						ex.Message,
						"\nFull XML text:\n\n",
						current.ToString()
					}));
				}
			}
		}

		public static List<string> MissingBackstoryTranslations(LoadedLanguage lang)
		{
			List<KeyValuePair<string, Backstory>> list = BackstoryDatabase.allBackstories.ToList<KeyValuePair<string, Backstory>>();
			List<string> list2 = new List<string>();
			foreach (XElement current in BackstoryTranslationUtility.BackstoryTranslationElements(lang.FolderPaths, null))
			{
				try
				{
					string text = current.Name.ToString();
					string modifiedIdentifier = BackstoryDatabase.GetIdentifierClosestMatch(text, false);
					bool flag = list.Any((KeyValuePair<string, Backstory> x) => x.Key == modifiedIdentifier);
					KeyValuePair<string, Backstory> backstory = list.Find((KeyValuePair<string, Backstory> x) => x.Key == modifiedIdentifier);
					if (flag)
					{
						list.RemoveAt(list.FindIndex((KeyValuePair<string, Backstory> x) => x.Key == backstory.Key));
						string text2 = BackstoryTranslationUtility.GetText(current, "title");
						string text3 = BackstoryTranslationUtility.GetText(current, "titleFemale");
						string text4 = BackstoryTranslationUtility.GetText(current, "titleShort");
						string text5 = BackstoryTranslationUtility.GetText(current, "titleShortFemale");
						string text6 = BackstoryTranslationUtility.GetText(current, "desc");
						if (text2.NullOrEmpty())
						{
							list2.Add(text + ".title missing");
						}
						if (flag && !backstory.Value.titleFemale.NullOrEmpty() && text3.NullOrEmpty())
						{
							list2.Add(text + ".titleFemale missing");
						}
						if (text4.NullOrEmpty())
						{
							list2.Add(text + ".titleShort missing");
						}
						if (flag && !backstory.Value.titleShortFemale.NullOrEmpty() && text5.NullOrEmpty())
						{
							list2.Add(text + ".titleShortFemale missing");
						}
						if (text6.NullOrEmpty())
						{
							list2.Add(text + ".desc missing");
						}
					}
					else
					{
						list2.Add("Translation doesn't correspond to any backstory: " + text);
					}
				}
				catch (Exception ex)
				{
					list2.Add(string.Concat(new object[]
					{
						"Exception reading ",
						current.Name,
						": ",
						ex.Message
					}));
				}
			}
			foreach (KeyValuePair<string, Backstory> current2 in list)
			{
				list2.Add("Missing backstory: " + current2.Key);
			}
			return list2;
		}

		public static List<string> BackstoryTranslationsMatchingEnglish(LoadedLanguage lang)
		{
			List<string> list = new List<string>();
			foreach (XElement current in BackstoryTranslationUtility.BackstoryTranslationElements(lang.FolderPaths, null))
			{
				try
				{
					string text = current.Name.ToString();
					Backstory backstory;
					if (BackstoryDatabase.allBackstories.TryGetValue(BackstoryDatabase.GetIdentifierClosestMatch(text, true), out backstory))
					{
						string text2 = BackstoryTranslationUtility.GetText(current, "title");
						string text3 = BackstoryTranslationUtility.GetText(current, "titleFemale");
						string text4 = BackstoryTranslationUtility.GetText(current, "titleShort");
						string text5 = BackstoryTranslationUtility.GetText(current, "titleShortFemale");
						string text6 = BackstoryTranslationUtility.GetText(current, "desc");
						if (!text2.NullOrEmpty() && text2 == backstory.untranslatedTitle)
						{
							list.Add(text + ".title '" + text2.Replace("\n", "\\n") + "'");
						}
						if (!text3.NullOrEmpty() && text3 == backstory.untranslatedTitleFemale)
						{
							list.Add(text + ".titleFemale '" + text3.Replace("\n", "\\n") + "'");
						}
						if (!text4.NullOrEmpty() && text4 == backstory.untranslatedTitleShort)
						{
							list.Add(text + ".titleShort '" + text4.Replace("\n", "\\n") + "'");
						}
						if (!text5.NullOrEmpty() && text5 == backstory.untranslatedTitleShortFemale)
						{
							list.Add(text + ".titleShortFemale '" + text5.Replace("\n", "\\n") + "'");
						}
						if (!text6.NullOrEmpty() && text6 == backstory.untranslatedDesc)
						{
							list.Add(text + ".desc '" + text6.Replace("\n", "\\n") + "'");
						}
					}
				}
				catch (Exception ex)
				{
					list.Add(string.Concat(new object[]
					{
						"Exception reading ",
						current.Name,
						": ",
						ex.Message
					}));
				}
			}
			return list;
		}

		private static string GetText(XElement backstory, string fieldName)
		{
			XElement xElement = backstory.Element(fieldName);
			if (xElement == null || xElement.Value == "TODO")
			{
				return null;
			}
			return xElement.Value.Replace("\\n", "\n");
		}
	}
}
