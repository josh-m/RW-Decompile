using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace Verse
{
	public static class TranslationFilesCleaner
	{
		private class PossibleDefInjection
		{
			public string suggestedPath;

			public string normalizedPath;

			public bool isCollection;

			public bool fullListTranslationAllowed;

			public string curValue;

			public IEnumerable<string> curValueCollection;

			public FieldInfo fieldInfo;

			public Def def;
		}

		private const string NewlineTag = "NEWLINE";

		private const string NewlineTagFull = "<!--NEWLINE-->";

		public static void CleanupTranslationFiles()
		{
			LoadedLanguage curLang = LanguageDatabase.activeLanguage;
			LoadedLanguage english = LanguageDatabase.defaultLanguage;
			if (curLang == english)
			{
				return;
			}
			IEnumerable<ModMetaData> activeModsInLoadOrder = ModsConfig.ActiveModsInLoadOrder;
			if (activeModsInLoadOrder.Count<ModMetaData>() != 1 || !activeModsInLoadOrder.First<ModMetaData>().IsCoreMod)
			{
				Messages.Message("MessageDisableModsBeforeCleaningTranslationFiles".Translate(), MessageTypeDefOf.RejectInput, false);
				return;
			}
			LongEventHandler.QueueLongEvent(delegate
			{
				if (curLang.anyKeyedReplacementsXmlParseError || curLang.anyDefInjectionsXmlParseError)
				{
					string text = curLang.lastKeyedReplacementsXmlParseErrorInFile ?? curLang.lastDefInjectionsXmlParseErrorInFile;
					Messages.Message("MessageCantCleanupTranslationFilesBeucaseOfXmlError".Translate(new object[]
					{
						text
					}), MessageTypeDefOf.RejectInput, false);
					return;
				}
				english.LoadData();
				curLang.LoadData();
				Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation("ConfirmCleanupTranslationFiles".Translate(new object[]
				{
					curLang.FriendlyNameNative
				}), delegate
				{
					LongEventHandler.QueueLongEvent(new Action(TranslationFilesCleaner.DoCleanupTranslationFiles), "CleaningTranslationFiles".Translate(), true, null);
				}, true, null);
				dialog_MessageBox.buttonAText = "ConfirmCleanupTranslationFiles_Confirm".Translate();
				Find.WindowStack.Add(dialog_MessageBox);
			}, null, false, null);
		}

		private static void DoCleanupTranslationFiles()
		{
			if (LanguageDatabase.activeLanguage == LanguageDatabase.defaultLanguage)
			{
				return;
			}
			try
			{
				try
				{
					TranslationFilesCleaner.CleanupKeyedTranslations();
				}
				catch (Exception arg)
				{
					Log.Error("Could not cleanup keyed translations: " + arg, false);
				}
				try
				{
					TranslationFilesCleaner.CleanupDefInjections();
				}
				catch (Exception arg2)
				{
					Log.Error("Could not cleanup def-injections: " + arg2, false);
				}
				try
				{
					TranslationFilesCleaner.CleanupBackstories();
				}
				catch (Exception arg3)
				{
					Log.Error("Could not cleanup backstories: " + arg3, false);
				}
				Messages.Message("MessageTranslationFilesCleanupDone".Translate(new object[]
				{
					TranslationFilesCleaner.GetActiveLanguageCoreModFolderPath()
				}), MessageTypeDefOf.TaskCompletion, false);
			}
			catch (Exception arg4)
			{
				Log.Error("Could not cleanup translation files: " + arg4, false);
			}
		}

		private static void CleanupKeyedTranslations()
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			LoadedLanguage english = LanguageDatabase.defaultLanguage;
			string activeLanguageCoreModFolderPath = TranslationFilesCleaner.GetActiveLanguageCoreModFolderPath();
			string text = Path.Combine(activeLanguageCoreModFolderPath, "CodeLinked");
			string text2 = Path.Combine(activeLanguageCoreModFolderPath, "Keyed");
			DirectoryInfo directoryInfo = new DirectoryInfo(text);
			if (directoryInfo.Exists)
			{
				if (!Directory.Exists(text2))
				{
					Directory.Move(text, text2);
					Thread.Sleep(1000);
					directoryInfo = new DirectoryInfo(text2);
				}
			}
			else
			{
				directoryInfo = new DirectoryInfo(text2);
			}
			if (!directoryInfo.Exists)
			{
				Log.Error("Could not find keyed translations folder for the active language.", false);
				return;
			}
			DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(TranslationFilesCleaner.GetEnglishLanguageCoreModFolderPath(), "Keyed"));
			if (!directoryInfo2.Exists)
			{
				Log.Error("English keyed translations folder doesn't exist.", false);
				return;
			}
			FileInfo[] files = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = files[i];
				try
				{
					fileInfo.Delete();
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not delete ",
						fileInfo.Name,
						": ",
						ex
					}), false);
				}
			}
			FileInfo[] files2 = directoryInfo2.GetFiles("*.xml", SearchOption.AllDirectories);
			for (int j = 0; j < files2.Length; j++)
			{
				FileInfo fileInfo2 = files2[j];
				try
				{
					string path = new Uri(directoryInfo2.FullName + Path.DirectorySeparatorChar).MakeRelativeUri(new Uri(fileInfo2.FullName)).ToString();
					string text3 = Path.Combine(directoryInfo.FullName, path);
					Directory.CreateDirectory(Path.GetDirectoryName(text3));
					fileInfo2.CopyTo(text3);
				}
				catch (Exception ex2)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not copy ",
						fileInfo2.Name,
						": ",
						ex2
					}), false);
				}
			}
			List<LoadedLanguage.KeyedReplacement> list = (from x in activeLanguage.keyedReplacements
			where !x.Value.isPlaceholder && !english.HaveTextForKey(x.Key, false)
			select x.Value).ToList<LoadedLanguage.KeyedReplacement>();
			HashSet<LoadedLanguage.KeyedReplacement> writtenUnusedKeyedTranslations = new HashSet<LoadedLanguage.KeyedReplacement>();
			FileInfo[] files3 = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
			for (int k = 0; k < files3.Length; k++)
			{
				FileInfo fileInfo3 = files3[k];
				try
				{
					XDocument xDocument = XDocument.Load(fileInfo3.FullName, LoadOptions.PreserveWhitespace);
					XElement xElement = xDocument.DescendantNodes().OfType<XElement>().FirstOrDefault<XElement>();
					if (xElement != null)
					{
						try
						{
							foreach (XNode current in xElement.DescendantNodes())
							{
								XElement xElement2 = current as XElement;
								if (xElement2 != null)
								{
									foreach (XNode current2 in xElement2.DescendantNodes())
									{
										try
										{
											XText xText = current2 as XText;
											if (xText != null && !xText.Value.NullOrEmpty())
											{
												string value = " EN: " + xText.Value + " ";
												current.AddBeforeSelf(new XComment(value));
												current.AddBeforeSelf(Environment.NewLine);
												current.AddBeforeSelf("  ");
											}
										}
										catch (Exception ex3)
										{
											Log.Error(string.Concat(new object[]
											{
												"Could not add comment node in ",
												fileInfo3.Name,
												": ",
												ex3
											}), false);
										}
										current2.Remove();
									}
									try
									{
										string text4;
										if (activeLanguage.TryGetTextFromKey(xElement2.Name.ToString(), out text4))
										{
											if (!text4.NullOrEmpty())
											{
												xElement2.Add(new XText(text4.Replace("\n", "\\n")));
											}
										}
										else
										{
											xElement2.Add(new XText("TODO"));
										}
									}
									catch (Exception ex4)
									{
										Log.Error(string.Concat(new object[]
										{
											"Could not add existing translation or placeholder in ",
											fileInfo3.Name,
											": ",
											ex4
										}), false);
									}
								}
							}
							bool flag = false;
							foreach (LoadedLanguage.KeyedReplacement current3 in list)
							{
								if (new Uri(fileInfo3.FullName).Equals(new Uri(current3.fileSourceFullPath)))
								{
									if (!flag)
									{
										xElement.Add("  ");
										xElement.Add(new XComment(" UNUSED "));
										xElement.Add(Environment.NewLine);
										flag = true;
									}
									XElement xElement3 = new XElement(current3.key);
									if (current3.isPlaceholder)
									{
										xElement3.Add(new XText("TODO"));
									}
									else if (!current3.value.NullOrEmpty())
									{
										xElement3.Add(new XText(current3.value.Replace("\n", "\\n")));
									}
									xElement.Add("  ");
									xElement.Add(xElement3);
									xElement.Add(Environment.NewLine);
									writtenUnusedKeyedTranslations.Add(current3);
								}
							}
							if (flag)
							{
								xElement.Add(Environment.NewLine);
							}
						}
						finally
						{
							TranslationFilesCleaner.SaveXMLDocumentWithProcessedNewlineTags(xDocument.Root, fileInfo3.FullName);
						}
					}
				}
				catch (Exception ex5)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not process ",
						fileInfo3.Name,
						": ",
						ex5
					}), false);
				}
			}
			foreach (IGrouping<string, LoadedLanguage.KeyedReplacement> current4 in from x in list
			where !writtenUnusedKeyedTranslations.Contains(x)
			group x by x.fileSourceFullPath)
			{
				try
				{
					if (File.Exists(current4.Key))
					{
						Log.Error("Could not save unused keyed translations to " + current4.Key + " because this file already exists.", false);
					}
					else
					{
						object[] expr_676 = new object[1];
						int arg_6DB_1 = 0;
						XName arg_6D6_0 = "LanguageData";
						object[] expr_688 = new object[4];
						expr_688[0] = new XComment("NEWLINE");
						expr_688[1] = new XComment(" UNUSED ");
						expr_688[2] = current4.Select(delegate(LoadedLanguage.KeyedReplacement x)
						{
							string text5 = (!x.isPlaceholder) ? x.value : "TODO";
							return new XElement(x.key, new XText((!text5.NullOrEmpty()) ? text5.Replace("\n", "\\n") : string.Empty));
						});
						expr_688[3] = new XComment("NEWLINE");
						expr_676[arg_6DB_1] = new XElement(arg_6D6_0, expr_688);
						XDocument doc = new XDocument(expr_676);
						TranslationFilesCleaner.SaveXMLDocumentWithProcessedNewlineTags(doc, current4.Key);
					}
				}
				catch (Exception ex6)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not save unused keyed translations to ",
						current4.Key,
						": ",
						ex6
					}), false);
				}
			}
		}

		private static void CleanupDefInjections()
		{
			string activeLanguageCoreModFolderPath = TranslationFilesCleaner.GetActiveLanguageCoreModFolderPath();
			string text = Path.Combine(activeLanguageCoreModFolderPath, "DefLinked");
			string text2 = Path.Combine(activeLanguageCoreModFolderPath, "DefInjected");
			DirectoryInfo directoryInfo = new DirectoryInfo(text);
			if (directoryInfo.Exists)
			{
				if (!Directory.Exists(text2))
				{
					Directory.Move(text, text2);
					Thread.Sleep(1000);
					directoryInfo = new DirectoryInfo(text2);
				}
			}
			else
			{
				directoryInfo = new DirectoryInfo(text2);
			}
			if (!directoryInfo.Exists)
			{
				Log.Error("Could not find def-injections folder for the active language.", false);
				return;
			}
			FileInfo[] files = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = files[i];
				try
				{
					fileInfo.Delete();
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not delete ",
						fileInfo.Name,
						": ",
						ex
					}), false);
				}
			}
			foreach (Type current in GenDefDatabase.AllDefTypesWithDatabases())
			{
				try
				{
					TranslationFilesCleaner.CleanupDefInjectionsForDefType(current, directoryInfo.FullName);
				}
				catch (Exception ex2)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not process def-injections for type ",
						current.Name,
						": ",
						ex2
					}), false);
				}
			}
		}

		private static void CleanupDefInjectionsForDefType(Type defType, string defInjectionsFolderPath)
		{
			LoadedLanguage activeLanguage = LanguageDatabase.activeLanguage;
			List<KeyValuePair<string, DefInjectionPackage.DefInjection>> list = (from x in (from x in activeLanguage.defInjections
			where x.defType == defType
			select x).SelectMany((DefInjectionPackage x) => x.injections)
			where !x.Value.isPlaceholder
			select x).ToList<KeyValuePair<string, DefInjectionPackage.DefInjection>>();
			Dictionary<string, DefInjectionPackage.DefInjection> dictionary = new Dictionary<string, DefInjectionPackage.DefInjection>();
			foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> current in list)
			{
				if (!dictionary.ContainsKey(current.Value.normalizedPath))
				{
					dictionary.Add(current.Value.normalizedPath, current.Value);
				}
			}
			List<TranslationFilesCleaner.PossibleDefInjection> possibleDefInjections = new List<TranslationFilesCleaner.PossibleDefInjection>();
			DefInjectionUtility.ForEachPossibleDefInjection(defType, delegate(string suggestedPath, string normalizedPath, bool isCollection, string str, IEnumerable<string> collection, bool translationAllowed, bool fullListTranslationAllowed, FieldInfo fieldInfo, Def def)
			{
				if (translationAllowed)
				{
					TranslationFilesCleaner.PossibleDefInjection possibleDefInjection = new TranslationFilesCleaner.PossibleDefInjection();
					possibleDefInjection.suggestedPath = suggestedPath;
					possibleDefInjection.normalizedPath = normalizedPath;
					possibleDefInjection.isCollection = isCollection;
					possibleDefInjection.fullListTranslationAllowed = fullListTranslationAllowed;
					possibleDefInjection.curValue = str;
					possibleDefInjection.curValueCollection = collection;
					possibleDefInjection.fieldInfo = fieldInfo;
					possibleDefInjection.def = def;
					possibleDefInjections.Add(possibleDefInjection);
				}
			});
			if (!possibleDefInjections.Any<TranslationFilesCleaner.PossibleDefInjection>() && !list.Any<KeyValuePair<string, DefInjectionPackage.DefInjection>>())
			{
				return;
			}
			List<KeyValuePair<string, DefInjectionPackage.DefInjection>> source = (from x in list
			where !x.Value.injected
			select x).ToList<KeyValuePair<string, DefInjectionPackage.DefInjection>>();
			foreach (string fileName in (from x in possibleDefInjections
			select TranslationFilesCleaner.GetSourceFile(x.def)).Concat(from x in source
			select x.Value.fileSource).Distinct<string>())
			{
				try
				{
					XDocument xDocument = new XDocument();
					bool flag = false;
					try
					{
						XElement xElement = new XElement("LanguageData");
						xDocument.Add(xElement);
						xElement.Add(new XComment("NEWLINE"));
						List<TranslationFilesCleaner.PossibleDefInjection> source2 = (from x in possibleDefInjections
						where TranslationFilesCleaner.GetSourceFile(x.def) == fileName
						select x).ToList<TranslationFilesCleaner.PossibleDefInjection>();
						List<KeyValuePair<string, DefInjectionPackage.DefInjection>> source3 = (from x in source
						where x.Value.fileSource == fileName
						select x).ToList<KeyValuePair<string, DefInjectionPackage.DefInjection>>();
						foreach (string defName in from x in (from x in source2
						select x.def.defName).Concat(from x in source3
						select x.Value.DefName).Distinct<string>()
						orderby x
						select x)
						{
							try
							{
								IEnumerable<TranslationFilesCleaner.PossibleDefInjection> enumerable = source2.Where((TranslationFilesCleaner.PossibleDefInjection x) => x.def.defName == defName);
								IEnumerable<KeyValuePair<string, DefInjectionPackage.DefInjection>> enumerable2 = source3.Where((KeyValuePair<string, DefInjectionPackage.DefInjection> x) => x.Value.DefName == defName);
								if (enumerable.Any<TranslationFilesCleaner.PossibleDefInjection>())
								{
									bool flag2 = false;
									foreach (TranslationFilesCleaner.PossibleDefInjection current2 in enumerable)
									{
										if (current2.isCollection)
										{
											IEnumerable<string> englishList = TranslationFilesCleaner.GetEnglishList(current2.normalizedPath, current2.curValueCollection, dictionary);
											bool flag3 = false;
											if (englishList != null)
											{
												int num = 0;
												foreach (string current3 in englishList)
												{
													if (dictionary.ContainsKey(current2.normalizedPath + "." + num))
													{
														flag3 = true;
														break;
													}
													num++;
												}
											}
											if (flag3 || !current2.fullListTranslationAllowed)
											{
												if (englishList != null)
												{
													int num2 = -1;
													foreach (string current4 in englishList)
													{
														num2++;
														string key = current2.normalizedPath + "." + num2;
														string suggestedPath2 = current2.suggestedPath + "." + num2;
														DefInjectionPackage.DefInjection defInjection;
														if (!dictionary.TryGetValue(key, out defInjection))
														{
															defInjection = null;
														}
														if (defInjection != null || DefInjectionUtility.ShouldCheckMissingInjection(current4, current2.fieldInfo, current2.def))
														{
															flag2 = true;
															flag = true;
															try
															{
																if (!current4.NullOrEmpty())
																{
																	xElement.Add(new XComment(" EN: " + current4.Replace("\n", "\\n") + " "));
																}
															}
															catch (Exception ex)
															{
																Log.Error(string.Concat(new object[]
																{
																	"Could not add comment node in ",
																	fileName,
																	": ",
																	ex
																}), false);
															}
															xElement.Add(TranslationFilesCleaner.GetDefInjectableFieldNode(suggestedPath2, defInjection));
														}
													}
												}
											}
											else
											{
												bool flag4 = false;
												if (englishList != null)
												{
													foreach (string current5 in englishList)
													{
														if (DefInjectionUtility.ShouldCheckMissingInjection(current5, current2.fieldInfo, current2.def))
														{
															flag4 = true;
															break;
														}
													}
												}
												DefInjectionPackage.DefInjection defInjection2;
												if (!dictionary.TryGetValue(current2.normalizedPath, out defInjection2))
												{
													defInjection2 = null;
												}
												if (defInjection2 != null || flag4)
												{
													flag2 = true;
													flag = true;
													try
													{
														string text = TranslationFilesCleaner.ListToLiNodesString(englishList);
														if (!text.NullOrEmpty())
														{
															xElement.Add(new XComment(" EN:\n" + text.Indented("    ") + "\n  "));
														}
													}
													catch (Exception ex2)
													{
														Log.Error(string.Concat(new object[]
														{
															"Could not add comment node in ",
															fileName,
															": ",
															ex2
														}), false);
													}
													xElement.Add(TranslationFilesCleaner.GetDefInjectableFieldNode(current2.suggestedPath, defInjection2));
												}
											}
										}
										else
										{
											DefInjectionPackage.DefInjection defInjection3;
											if (!dictionary.TryGetValue(current2.normalizedPath, out defInjection3))
											{
												defInjection3 = null;
											}
											string text2 = (defInjection3 == null || !defInjection3.injected) ? current2.curValue : defInjection3.replacedString;
											if (defInjection3 != null || DefInjectionUtility.ShouldCheckMissingInjection(text2, current2.fieldInfo, current2.def))
											{
												flag2 = true;
												flag = true;
												try
												{
													if (!text2.NullOrEmpty())
													{
														xElement.Add(new XComment(" EN: " + text2.Replace("\n", "\\n") + " "));
													}
												}
												catch (Exception ex3)
												{
													Log.Error(string.Concat(new object[]
													{
														"Could not add comment node in ",
														fileName,
														": ",
														ex3
													}), false);
												}
												xElement.Add(TranslationFilesCleaner.GetDefInjectableFieldNode(current2.suggestedPath, defInjection3));
											}
										}
									}
									if (flag2)
									{
										xElement.Add(new XComment("NEWLINE"));
									}
								}
								if (enumerable2.Any<KeyValuePair<string, DefInjectionPackage.DefInjection>>())
								{
									flag = true;
									xElement.Add(new XComment(" UNUSED "));
									foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> current6 in enumerable2)
									{
										xElement.Add(TranslationFilesCleaner.GetDefInjectableFieldNode(current6.Value.path, current6.Value));
									}
									xElement.Add(new XComment("NEWLINE"));
								}
							}
							catch (Exception ex4)
							{
								Log.Error(string.Concat(new object[]
								{
									"Could not process def-injections for def ",
									defName,
									": ",
									ex4
								}), false);
							}
						}
					}
					finally
					{
						if (flag)
						{
							string text3 = Path.Combine(defInjectionsFolderPath, defType.Name);
							Directory.CreateDirectory(text3);
							TranslationFilesCleaner.SaveXMLDocumentWithProcessedNewlineTags(xDocument, Path.Combine(text3, fileName));
						}
					}
				}
				catch (Exception ex5)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not process def-injections for file ",
						fileName,
						": ",
						ex5
					}), false);
				}
			}
		}

		private static void CleanupBackstories()
		{
			string activeLanguageCoreModFolderPath = TranslationFilesCleaner.GetActiveLanguageCoreModFolderPath();
			string text = Path.Combine(activeLanguageCoreModFolderPath, "Backstories");
			Directory.CreateDirectory(text);
			string path = Path.Combine(text, "Backstories.xml");
			File.Delete(path);
			XDocument xDocument = new XDocument();
			try
			{
				XElement xElement = new XElement("BackstoryTranslations");
				xDocument.Add(xElement);
				xElement.Add(new XComment("NEWLINE"));
				foreach (KeyValuePair<string, Backstory> current in from x in BackstoryDatabase.allBackstories
				orderby x.Key
				select x)
				{
					try
					{
						XElement xElement2 = new XElement(current.Key);
						TranslationFilesCleaner.AddBackstoryFieldElement(xElement2, "title", current.Value.title, current.Value.untranslatedTitle, current.Value.titleTranslated);
						TranslationFilesCleaner.AddBackstoryFieldElement(xElement2, "titleFemale", current.Value.titleFemale, current.Value.untranslatedTitleFemale, current.Value.titleFemaleTranslated);
						TranslationFilesCleaner.AddBackstoryFieldElement(xElement2, "titleShort", current.Value.titleShort, current.Value.untranslatedTitleShort, current.Value.titleShortTranslated);
						TranslationFilesCleaner.AddBackstoryFieldElement(xElement2, "titleShortFemale", current.Value.titleShortFemale, current.Value.untranslatedTitleShortFemale, current.Value.titleShortFemaleTranslated);
						TranslationFilesCleaner.AddBackstoryFieldElement(xElement2, "desc", current.Value.baseDesc, current.Value.untranslatedDesc, current.Value.descTranslated);
						xElement.Add(xElement2);
						xElement.Add(new XComment("NEWLINE"));
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat(new object[]
						{
							"Could not process backstory ",
							current.Key,
							": ",
							ex
						}), false);
					}
				}
			}
			finally
			{
				TranslationFilesCleaner.SaveXMLDocumentWithProcessedNewlineTags(xDocument, path);
			}
		}

		private static void AddBackstoryFieldElement(XElement addTo, string fieldName, string currentValue, string untranslatedValue, bool wasTranslated)
		{
			if (wasTranslated || !untranslatedValue.NullOrEmpty())
			{
				if (!untranslatedValue.NullOrEmpty())
				{
					addTo.Add(new XComment(" EN: " + untranslatedValue.Replace("\n", "\\n") + " "));
				}
				string text = (!wasTranslated) ? "TODO" : currentValue;
				addTo.Add(new XElement(fieldName, (!text.NullOrEmpty()) ? text.Replace("\n", "\\n") : string.Empty));
			}
		}

		private static string GetActiveLanguageCoreModFolderPath()
		{
			return TranslationFilesCleaner.GetLanguageCoreModFolderPath(LanguageDatabase.activeLanguage);
		}

		private static string GetEnglishLanguageCoreModFolderPath()
		{
			return TranslationFilesCleaner.GetLanguageCoreModFolderPath(LanguageDatabase.defaultLanguage);
		}

		private static string GetLanguageCoreModFolderPath(LoadedLanguage language)
		{
			ModContentPack modContentPack = LoadedModManager.RunningMods.FirstOrDefault((ModContentPack x) => x.IsCoreMod);
			string path = Path.Combine(modContentPack.RootDir, "Languages");
			return Path.Combine(path, language.folderName);
		}

		private static void SaveXMLDocumentWithProcessedNewlineTags(XNode doc, string path)
		{
			File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + doc.ToString().Replace("<!--NEWLINE-->", string.Empty).Replace("&gt;", ">"), Encoding.UTF8);
		}

		private static string ListToLiNodesString(IEnumerable<string> list)
		{
			if (list == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string current in list)
			{
				stringBuilder.Append("<li>");
				if (!current.NullOrEmpty())
				{
					stringBuilder.Append(current.Replace("\n", "\\n"));
				}
				stringBuilder.Append("</li>");
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		private static XElement ListToXElement(IEnumerable<string> list, string name, List<Pair<int, string>> comments)
		{
			XElement xElement = new XElement(name);
			if (list != null)
			{
				int num = 0;
				foreach (string current in list)
				{
					if (comments != null)
					{
						for (int i = 0; i < comments.Count; i++)
						{
							if (comments[i].First == num)
							{
								xElement.Add(new XComment(comments[i].Second));
							}
						}
					}
					XElement xElement2 = new XElement("li");
					if (!current.NullOrEmpty())
					{
						xElement2.Add(new XText(current.Replace("\n", "\\n")));
					}
					xElement.Add(xElement2);
					num++;
				}
				if (comments != null)
				{
					for (int j = 0; j < comments.Count; j++)
					{
						if (comments[j].First == num)
						{
							xElement.Add(new XComment(comments[j].Second));
						}
					}
				}
			}
			return xElement;
		}

		private static string AppendXmlExtensionIfNotAlready(string fileName)
		{
			if (!fileName.ToLower().EndsWith(".xml"))
			{
				return fileName + ".xml";
			}
			return fileName;
		}

		private static string GetSourceFile(Def def)
		{
			if (def.defPackage != null)
			{
				return TranslationFilesCleaner.AppendXmlExtensionIfNotAlready(def.defPackage.fileName);
			}
			return "Unknown.xml";
		}

		private static string TryRemoveLastIndexSymbol(string str)
		{
			int num = str.LastIndexOf('.');
			if (num >= 0)
			{
				if (str.Substring(num + 1).All(new Func<char, bool>(char.IsNumber)))
				{
					return str.Substring(0, num);
				}
			}
			return str;
		}

		private static IEnumerable<string> GetEnglishList(string normalizedPath, IEnumerable<string> curValue, Dictionary<string, DefInjectionPackage.DefInjection> injectionsByNormalizedPath)
		{
			DefInjectionPackage.DefInjection defInjection;
			if (injectionsByNormalizedPath.TryGetValue(normalizedPath, out defInjection) && defInjection.injected)
			{
				return defInjection.replacedList;
			}
			if (curValue == null)
			{
				return null;
			}
			List<string> list = curValue.ToList<string>();
			for (int i = 0; i < list.Count; i++)
			{
				string key = normalizedPath + "." + i;
				DefInjectionPackage.DefInjection defInjection2;
				if (injectionsByNormalizedPath.TryGetValue(key, out defInjection2) && defInjection2.injected)
				{
					list[i] = defInjection2.replacedString;
				}
			}
			return list;
		}

		private static XElement GetDefInjectableFieldNode(string suggestedPath, DefInjectionPackage.DefInjection existingInjection)
		{
			if (existingInjection == null || existingInjection.isPlaceholder)
			{
				return new XElement(suggestedPath, new XText("TODO"));
			}
			if (existingInjection.IsFullListInjection)
			{
				return TranslationFilesCleaner.ListToXElement(existingInjection.fullListInjection, suggestedPath, existingInjection.fullListInjectionComments);
			}
			XElement xElement = new XElement(suggestedPath);
			if (!existingInjection.injection.NullOrEmpty())
			{
				xElement.Add(new XText(existingInjection.injection.Replace("\n", "\\n")));
			}
			return xElement;
		}
	}
}
