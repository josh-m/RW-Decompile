using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Verse
{
	public class DefInjectionPackage
	{
		public class DefInjection
		{
			public string path;

			public string normalizedPath;

			public string nonBackCompatiblePath;

			public string suggestedPath;

			public string injection;

			public List<string> fullListInjection;

			public List<Pair<int, string>> fullListInjectionComments;

			public string fileSource;

			public bool injected;

			public string replacedString;

			public IEnumerable<string> replacedList;

			public bool isPlaceholder;

			public bool IsFullListInjection
			{
				get
				{
					return this.fullListInjection != null;
				}
			}

			public string DefName
			{
				get
				{
					return (!this.path.NullOrEmpty()) ? this.path.Split(new char[]
					{
						'.'
					})[0] : string.Empty;
				}
			}
		}

		public Type defType;

		public Dictionary<string, DefInjectionPackage.DefInjection> injections = new Dictionary<string, DefInjectionPackage.DefInjection>();

		public List<string> loadErrors = new List<string>();

		public List<string> loadSyntaxSuggestions = new List<string>();

		public bool usedOldRepSyntax;

		public const BindingFlags FieldBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public const string RepNodeName = "rep";

		public DefInjectionPackage(Type defType)
		{
			this.defType = defType;
		}

		private string ProcessedPath(string path)
		{
			if (path == null)
			{
				path = string.Empty;
			}
			if (!path.Contains('[') && !path.Contains(']'))
			{
				return path;
			}
			return path.Replace("]", string.Empty).Replace('[', '.');
		}

		private string ProcessedTranslation(string rawTranslation)
		{
			return rawTranslation.Replace("\\n", "\n");
		}

		public void AddDataFromFile(FileInfo file, out bool xmlParseError)
		{
			xmlParseError = false;
			try
			{
				XDocument xDocument = XDocument.Load(file.FullName);
				foreach (XElement current in xDocument.Root.Elements())
				{
					if (current.Name == "rep")
					{
						string key = this.ProcessedPath(current.Elements("path").First<XElement>().Value);
						string translation = this.ProcessedTranslation(current.Elements("trans").First<XElement>().Value);
						this.TryAddInjection(file, key, translation);
						this.usedOldRepSyntax = true;
					}
					else
					{
						string text = this.ProcessedPath(current.Name.ToString());
						if (current.HasElements)
						{
							List<string> list = new List<string>();
							List<Pair<int, string>> list2 = null;
							bool flag = false;
							foreach (XNode current2 in current.DescendantNodes())
							{
								XElement xElement = current2 as XElement;
								if (xElement != null)
								{
									if (xElement.Name == "li")
									{
										list.Add(this.ProcessedTranslation(xElement.Value));
									}
									else if (!flag)
									{
										this.loadErrors.Add(text + " has elements which are not 'li' (" + file.Name + ")");
										flag = true;
									}
								}
								XComment xComment = current2 as XComment;
								if (xComment != null)
								{
									if (list2 == null)
									{
										list2 = new List<Pair<int, string>>();
									}
									list2.Add(new Pair<int, string>(list.Count, xComment.Value));
								}
							}
							this.TryAddFullListInjection(file, text, list, list2);
						}
						else
						{
							string translation2 = this.ProcessedTranslation(current.Value);
							this.TryAddInjection(file, text, translation2);
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.loadErrors.Add(string.Concat(new object[]
				{
					"Exception loading translation data from file ",
					file.Name,
					": ",
					ex
				}));
				xmlParseError = true;
			}
		}

		private void TryAddInjection(FileInfo file, string key, string translation)
		{
			string text = key;
			key = this.BackCompatibleKey(key);
			if (this.CheckErrors(file, key, text, false))
			{
				return;
			}
			DefInjectionPackage.DefInjection defInjection = new DefInjectionPackage.DefInjection();
			if (translation == "TODO")
			{
				defInjection.isPlaceholder = true;
				translation = string.Empty;
			}
			defInjection.path = key;
			defInjection.injection = translation;
			defInjection.fileSource = file.Name;
			defInjection.nonBackCompatiblePath = text;
			this.injections.Add(key, defInjection);
		}

		private void TryAddFullListInjection(FileInfo file, string key, List<string> translation, List<Pair<int, string>> comments)
		{
			string text = key;
			key = this.BackCompatibleKey(key);
			if (this.CheckErrors(file, key, text, true))
			{
				return;
			}
			if (translation == null)
			{
				translation = new List<string>();
			}
			DefInjectionPackage.DefInjection defInjection = new DefInjectionPackage.DefInjection();
			defInjection.path = key;
			defInjection.fullListInjection = translation;
			defInjection.fullListInjectionComments = comments;
			defInjection.fileSource = file.Name;
			defInjection.nonBackCompatiblePath = text;
			this.injections.Add(key, defInjection);
		}

		private string BackCompatibleKey(string key)
		{
			string[] array = key.Split(new char[]
			{
				'.'
			});
			if (array.Any<string>())
			{
				array[0] = BackCompatibility.BackCompatibleDefName(this.defType, array[0], true);
			}
			key = string.Join(".", array);
			if (this.defType == typeof(ConceptDef) && key.Contains(".helpTexts.0"))
			{
				key = key.Replace(".helpTexts.0", ".helpText");
			}
			return key;
		}

		private bool CheckErrors(FileInfo file, string key, string nonBackCompatibleKey, bool replacingFullList)
		{
			if (!key.Contains('.'))
			{
				this.loadErrors.Add(string.Concat(new string[]
				{
					"Error loading DefInjection from file ",
					file.Name,
					": Key lacks a dot: ",
					key,
					(!(key == nonBackCompatibleKey)) ? (" (auto-renamed from " + nonBackCompatibleKey + ")") : string.Empty,
					" (",
					file.Name,
					")"
				}));
				return true;
			}
			DefInjectionPackage.DefInjection defInjection;
			if (this.injections.TryGetValue(key, out defInjection))
			{
				string text;
				if (key != nonBackCompatibleKey)
				{
					text = " (auto-renamed from " + nonBackCompatibleKey + ")";
				}
				else if (defInjection.path != defInjection.nonBackCompatiblePath)
				{
					text = string.Concat(new string[]
					{
						" (",
						defInjection.nonBackCompatiblePath,
						" was auto-renamed to ",
						defInjection.path,
						")"
					});
				}
				else
				{
					text = string.Empty;
				}
				this.loadErrors.Add(string.Concat(new string[]
				{
					"Duplicate def-injected translation key: ",
					key,
					text,
					" (",
					file.Name,
					")"
				}));
				return true;
			}
			bool flag = false;
			if (replacingFullList)
			{
				if (this.injections.Any((KeyValuePair<string, DefInjectionPackage.DefInjection> x) => !x.Value.IsFullListInjection && x.Key.StartsWith(key + ".")))
				{
					flag = true;
				}
			}
			else if (key.Contains('.') && char.IsNumber(key[key.Length - 1]))
			{
				string key2 = key.Substring(0, key.LastIndexOf('.'));
				if (this.injections.ContainsKey(key2) && this.injections[key2].IsFullListInjection)
				{
					flag = true;
				}
			}
			if (flag)
			{
				this.loadErrors.Add(string.Concat(new string[]
				{
					"Replacing the whole list and individual elements at the same time doesn't make sense. Either replace the whole list or translate individual elements by using their indexes. key=",
					key,
					(!(key == nonBackCompatibleKey)) ? (" (auto-renamed from " + nonBackCompatibleKey + ")") : string.Empty,
					" (",
					file.Name,
					")"
				}));
				return true;
			}
			return false;
		}

		public void InjectIntoDefs(bool errorOnDefNotFound)
		{
			foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> current in this.injections)
			{
				if (!current.Value.injected)
				{
					string normalizedPath;
					string suggestedPath;
					if (current.Value.IsFullListInjection)
					{
						current.Value.injected = this.SetDefFieldAtPath(this.defType, current.Key, current.Value.fullListInjection, typeof(List<string>), errorOnDefNotFound, current.Value.fileSource, current.Value.isPlaceholder, out normalizedPath, out suggestedPath, out current.Value.replacedString, out current.Value.replacedList);
					}
					else
					{
						current.Value.injected = this.SetDefFieldAtPath(this.defType, current.Key, current.Value.injection, typeof(string), errorOnDefNotFound, current.Value.fileSource, current.Value.isPlaceholder, out normalizedPath, out suggestedPath, out current.Value.replacedString, out current.Value.replacedList);
					}
					current.Value.normalizedPath = normalizedPath;
					current.Value.suggestedPath = suggestedPath;
				}
			}
			GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), this.defType, "ClearCachedData");
		}

		private bool SetDefFieldAtPath(Type defType, string path, object value, Type ensureFieldType, bool errorOnDefNotFound, string fileSource, bool isPlaceholder, out string normalizedPath, out string suggestedPath, out string replacedString, out IEnumerable<string> replacedStringsList)
		{
			replacedString = null;
			replacedStringsList = null;
			normalizedPath = path;
			suggestedPath = path;
			string text = path.Split(new char[]
			{
				'.'
			})[0];
			text = BackCompatibility.BackCompatibleDefName(defType, text, true);
			if (GenDefDatabase.GetDefSilentFail(defType, text, false) == null)
			{
				if (errorOnDefNotFound)
				{
					this.loadErrors.Add(string.Concat(new object[]
					{
						"Found no ",
						defType,
						" named ",
						text,
						" to match ",
						path,
						" (",
						fileSource,
						")"
					}));
				}
				return false;
			}
			bool flag = false;
			int num = 0;
			bool result;
			try
			{
				List<string> list = path.Split(new char[]
				{
					'.'
				}).ToList<string>();
				object obj = GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), defType, "GetNamedSilentFail", new object[]
				{
					list[0]
				});
				if (obj == null)
				{
					throw new InvalidOperationException("Def named " + list[0] + " not found.");
				}
				list.RemoveAt(0);
				num++;
				string text2;
				int num2;
				DefInjectionPathPartKind defInjectionPathPartKind;
				FieldInfo field;
				int num3;
				int num4;
				while (true)
				{
					text2 = list[0];
					num2 = -1;
					if (int.TryParse(text2, out num2))
					{
						defInjectionPathPartKind = DefInjectionPathPartKind.ListIndex;
					}
					else if (this.GetFieldNamed(obj.GetType(), text2) != null)
					{
						defInjectionPathPartKind = DefInjectionPathPartKind.Field;
					}
					else if (obj.GetType().GetProperty("Count") != null)
					{
						if (text2.Contains('-'))
						{
							defInjectionPathPartKind = DefInjectionPathPartKind.ListHandleWithIndex;
							string[] array = text2.Split(new char[]
							{
								'-'
							});
							text2 = array[0];
							num2 = (int)ParseHelper.FromString(array[1], typeof(int));
						}
						else
						{
							defInjectionPathPartKind = DefInjectionPathPartKind.ListHandle;
						}
					}
					else
					{
						defInjectionPathPartKind = DefInjectionPathPartKind.Field;
					}
					if (list.Count == 1)
					{
						break;
					}
					if (defInjectionPathPartKind == DefInjectionPathPartKind.Field)
					{
						field = obj.GetType().GetField(text2, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						if (field == null)
						{
							goto Block_41;
						}
						if (field.HasAttribute<NoTranslateAttribute>())
						{
							goto Block_42;
						}
						if (field.HasAttribute<UnsavedAttribute>())
						{
							goto Block_43;
						}
						if (field.HasAttribute<TranslationCanChangeCountAttribute>())
						{
							flag = true;
						}
						if (defInjectionPathPartKind == DefInjectionPathPartKind.Field)
						{
							obj = field.GetValue(obj);
						}
						else
						{
							object value2 = field.GetValue(obj);
							PropertyInfo property = value2.GetType().GetProperty("Item");
							if (property == null)
							{
								goto Block_46;
							}
							num3 = (int)value2.GetType().GetProperty("Count").GetValue(value2, null);
							if (num2 < 0 || num2 >= num3)
							{
								goto IL_ADC;
							}
							obj = property.GetValue(value2, new object[]
							{
								num2
							});
						}
					}
					else if (defInjectionPathPartKind == DefInjectionPathPartKind.ListIndex || defInjectionPathPartKind == DefInjectionPathPartKind.ListHandle || defInjectionPathPartKind == DefInjectionPathPartKind.ListHandleWithIndex)
					{
						object obj2 = obj;
						PropertyInfo property2 = obj2.GetType().GetProperty("Item");
						if (property2 == null)
						{
							goto Block_50;
						}
						bool flag2;
						if (defInjectionPathPartKind == DefInjectionPathPartKind.ListHandle || defInjectionPathPartKind == DefInjectionPathPartKind.ListHandleWithIndex)
						{
							num2 = TranslationHandleUtility.GetElementIndexByHandle(obj2, text2, num2);
							defInjectionPathPartKind = DefInjectionPathPartKind.ListIndex;
							flag2 = true;
						}
						else
						{
							flag2 = false;
						}
						num4 = (int)obj2.GetType().GetProperty("Count").GetValue(obj2, null);
						if (num2 < 0 || num2 >= num4)
						{
							goto IL_BB7;
						}
						obj = property2.GetValue(obj2, new object[]
						{
							num2
						});
						if (flag2)
						{
							string[] array2 = normalizedPath.Split(new char[]
							{
								'.'
							});
							array2[num] = num2.ToString();
							normalizedPath = string.Join(".", array2);
						}
						else
						{
							string bestHandleWithIndexForListElement = TranslationHandleUtility.GetBestHandleWithIndexForListElement(obj2, obj);
							if (!bestHandleWithIndexForListElement.NullOrEmpty())
							{
								string[] array3 = suggestedPath.Split(new char[]
								{
									'.'
								});
								array3[num] = bestHandleWithIndexForListElement;
								suggestedPath = string.Join(".", array3);
							}
						}
					}
					else
					{
						this.loadErrors.Add(string.Concat(new object[]
						{
							"Can't enter node ",
							text2,
							" at path ",
							path,
							", element kind is ",
							defInjectionPathPartKind,
							". (",
							fileSource,
							")"
						}));
					}
					list.RemoveAt(0);
					num++;
				}
				bool flag3 = false;
				foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> current in this.injections)
				{
					if (!(current.Key == path))
					{
						if (current.Value.injected && current.Value.normalizedPath == normalizedPath)
						{
							string text3 = string.Concat(new string[]
							{
								"Duplicate def-injected translation key. Both ",
								current.Value.path,
								" and ",
								path,
								" refer to the same field (",
								suggestedPath,
								")"
							});
							if (current.Value.path != current.Value.nonBackCompatiblePath)
							{
								string text4 = text3;
								text3 = string.Concat(new string[]
								{
									text4,
									" (",
									current.Value.nonBackCompatiblePath,
									" was auto-renamed to ",
									current.Value.path,
									")"
								});
							}
							text3 = text3 + " (" + current.Value.fileSource + ")";
							this.loadErrors.Add(text3);
							flag3 = true;
							break;
						}
					}
				}
				bool flag4 = false;
				if (!flag3)
				{
					if (defInjectionPathPartKind == DefInjectionPathPartKind.Field)
					{
						FieldInfo fieldNamed = this.GetFieldNamed(obj.GetType(), text2);
						if (fieldNamed == null)
						{
							throw new InvalidOperationException(string.Concat(new object[]
							{
								"Field ",
								text2,
								" does not exist in type ",
								obj.GetType(),
								"."
							}));
						}
						if (fieldNamed.HasAttribute<NoTranslateAttribute>())
						{
							this.loadErrors.Add(string.Concat(new object[]
							{
								"Translated untranslatable field ",
								fieldNamed.Name,
								" of type ",
								fieldNamed.FieldType,
								" at path ",
								path,
								". Translating this field will break the game. (",
								fileSource,
								")"
							}));
						}
						else if (fieldNamed.HasAttribute<UnsavedAttribute>())
						{
							this.loadErrors.Add(string.Concat(new object[]
							{
								"Translated untranslatable field (UnsavedAttribute) ",
								fieldNamed.Name,
								" of type ",
								fieldNamed.FieldType,
								" at path ",
								path,
								". Translating this field will break the game. (",
								fileSource,
								")"
							}));
						}
						else if (!isPlaceholder && fieldNamed.FieldType != ensureFieldType)
						{
							this.loadErrors.Add(string.Concat(new object[]
							{
								"Translated non-",
								ensureFieldType,
								" field ",
								fieldNamed.Name,
								" of type ",
								fieldNamed.FieldType,
								" at path ",
								path,
								". Expected ",
								ensureFieldType,
								". (",
								fileSource,
								")"
							}));
						}
						else if (!isPlaceholder && ensureFieldType != typeof(string) && !fieldNamed.HasAttribute<TranslationCanChangeCountAttribute>())
						{
							this.loadErrors.Add(string.Concat(new object[]
							{
								"Tried to translate field ",
								fieldNamed.Name,
								" of type ",
								fieldNamed.FieldType,
								" at path ",
								path,
								", but this field doesn't have [TranslationCanChangeCount] attribute so it doesn't allow this type of translation. (",
								fileSource,
								")"
							}));
						}
						else if (!isPlaceholder)
						{
							if (ensureFieldType == typeof(string))
							{
								replacedString = (string)fieldNamed.GetValue(obj);
							}
							else
							{
								replacedStringsList = (fieldNamed.GetValue(obj) as IEnumerable<string>);
							}
							fieldNamed.SetValue(obj, value);
							flag4 = true;
						}
					}
					else if (defInjectionPathPartKind == DefInjectionPathPartKind.ListIndex || defInjectionPathPartKind == DefInjectionPathPartKind.ListHandle || defInjectionPathPartKind == DefInjectionPathPartKind.ListHandleWithIndex)
					{
						object obj3 = obj;
						if (obj3 == null)
						{
							throw new InvalidOperationException("Tried to use index on null list at " + path);
						}
						Type type = obj3.GetType();
						PropertyInfo property3 = type.GetProperty("Count");
						if (property3 == null)
						{
							throw new InvalidOperationException("Tried to use index on non-list (missing 'Count' property).");
						}
						if (defInjectionPathPartKind == DefInjectionPathPartKind.ListHandle || defInjectionPathPartKind == DefInjectionPathPartKind.ListHandleWithIndex)
						{
							num2 = TranslationHandleUtility.GetElementIndexByHandle(obj3, text2, num2);
							defInjectionPathPartKind = DefInjectionPathPartKind.ListIndex;
						}
						int num5 = (int)property3.GetValue(obj3, null);
						if (num2 >= num5)
						{
							throw new InvalidOperationException(string.Concat(new object[]
							{
								"Trying to translate ",
								defType,
								".",
								path,
								" at index ",
								num2,
								" but the list only has ",
								num5,
								" entries (so max index is ",
								(num5 - 1).ToString(),
								")."
							}));
						}
						PropertyInfo property4 = type.GetProperty("Item");
						if (property4 == null)
						{
							throw new InvalidOperationException("Tried to use index on non-list (missing 'Item' property).");
						}
						Type propertyType = property4.PropertyType;
						if (!isPlaceholder && propertyType != ensureFieldType)
						{
							this.loadErrors.Add(string.Concat(new object[]
							{
								"Translated non-",
								ensureFieldType,
								" list item of type ",
								propertyType,
								" at path ",
								path,
								". Expected ",
								ensureFieldType,
								". (",
								fileSource,
								")"
							}));
						}
						else if (!isPlaceholder && ensureFieldType != typeof(string) && !flag)
						{
							this.loadErrors.Add(string.Concat(new object[]
							{
								"Tried to translate field of type ",
								propertyType,
								" at path ",
								path,
								", but this field doesn't have [TranslationCanChangeCount] attribute so it doesn't allow this type of translation. (",
								fileSource,
								")"
							}));
						}
						else if (num2 < 0 || num2 >= (int)type.GetProperty("Count").GetValue(obj3, null))
						{
							this.loadErrors.Add("Index out of bounds (max index is " + ((int)type.GetProperty("Count").GetValue(obj3, null) - 1) + ")");
						}
						else if (!isPlaceholder)
						{
							replacedString = (string)property4.GetValue(obj3, new object[]
							{
								num2
							});
							property4.SetValue(obj3, value, new object[]
							{
								num2
							});
							flag4 = true;
						}
					}
					else
					{
						this.loadErrors.Add(string.Concat(new object[]
						{
							"Translated ",
							text2,
							" at path ",
							path,
							" but it's not a field, it's ",
							defInjectionPathPartKind,
							". (",
							fileSource,
							")"
						}));
					}
				}
				if (path != suggestedPath)
				{
					IList<string> list2 = value as IList<string>;
					string text5;
					if (list2 != null)
					{
						text5 = list2.ToStringSafeEnumerable();
					}
					else
					{
						text5 = value.ToString();
					}
					this.loadSyntaxSuggestions.Add(string.Concat(new string[]
					{
						"Consider using ",
						suggestedPath,
						" instead of ",
						path,
						" for translation '",
						text5,
						"' (",
						fileSource,
						")"
					}));
				}
				result = flag4;
				return result;
				Block_41:
				throw new InvalidOperationException("Field " + text2 + " does not exist.");
				Block_42:
				throw new InvalidOperationException(string.Concat(new object[]
				{
					"Translated untranslatable field ",
					field.Name,
					" of type ",
					field.FieldType,
					" at path ",
					path,
					". Translating this field will break the game."
				}));
				Block_43:
				throw new InvalidOperationException(string.Concat(new object[]
				{
					"Translated untranslatable field ([Unsaved] attribute) ",
					field.Name,
					" of type ",
					field.FieldType,
					" at path ",
					path,
					". Translating this field will break the game."
				}));
				Block_46:
				throw new InvalidOperationException("Tried to use index on non-list (missing 'Item' property).");
				IL_ADC:
				throw new InvalidOperationException("Index out of bounds (max index is " + (num3 - 1) + ")");
				Block_50:
				throw new InvalidOperationException("Tried to use index on non-list (missing 'Item' property).");
				IL_BB7:
				throw new InvalidOperationException("Index out of bounds (max index is " + (num4 - 1) + ")");
			}
			catch (Exception ex)
			{
				string text6 = string.Concat(new object[]
				{
					"Couldn't inject ",
					path,
					" into ",
					defType,
					" (",
					fileSource,
					"): ",
					ex.Message
				});
				if (ex.InnerException != null)
				{
					text6 = text6 + " -> " + ex.InnerException.Message;
				}
				this.loadErrors.Add(text6);
				result = false;
			}
			return result;
		}

		private FieldInfo GetFieldNamed(Type type, string name)
		{
			FieldInfo field = type.GetField(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				FieldInfo[] fields = type.GetFields(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				for (int i = 0; i < fields.Length; i++)
				{
					object[] customAttributes = fields[i].GetCustomAttributes(typeof(LoadAliasAttribute), false);
					if (customAttributes != null && customAttributes.Length > 0)
					{
						for (int j = 0; j < customAttributes.Length; j++)
						{
							LoadAliasAttribute loadAliasAttribute = (LoadAliasAttribute)customAttributes[j];
							if (loadAliasAttribute.alias == name)
							{
								return fields[i];
							}
						}
					}
				}
			}
			return field;
		}

		public List<string> MissingInjections(List<string> outUnnecessaryDefInjections)
		{
			List<string> missingInjections = new List<string>();
			Dictionary<string, DefInjectionPackage.DefInjection> injectionsByNormalizedPath = new Dictionary<string, DefInjectionPackage.DefInjection>();
			foreach (KeyValuePair<string, DefInjectionPackage.DefInjection> current in this.injections)
			{
				if (!injectionsByNormalizedPath.ContainsKey(current.Value.normalizedPath))
				{
					injectionsByNormalizedPath.Add(current.Value.normalizedPath, current.Value);
				}
			}
			DefInjectionUtility.ForEachPossibleDefInjection(this.defType, delegate(string suggestedPath, string normalizedPath, bool isCollection, string str, IEnumerable<string> collection, bool translationAllowed, bool fullListTranslationAllowed, FieldInfo fieldInfo, Def def)
			{
				DefInjectionPackage.DefInjection defInjection2;
				if (!isCollection)
				{
					bool flag = false;
					string text = null;
					DefInjectionPackage.DefInjection defInjection;
					if (injectionsByNormalizedPath.TryGetValue(normalizedPath, out defInjection) && !defInjection.IsFullListInjection)
					{
						if (!translationAllowed)
						{
							outUnnecessaryDefInjections.Add(defInjection.path + " '" + defInjection.injection.Replace("\n", "\\n") + "'");
						}
						else if (defInjection.isPlaceholder)
						{
							flag = true;
							text = defInjection.fileSource;
						}
					}
					else
					{
						flag = true;
					}
					if (flag && translationAllowed && DefInjectionUtility.ShouldCheckMissingInjection(str, fieldInfo, def))
					{
						missingInjections.Add(string.Concat(new string[]
						{
							suggestedPath,
							" '",
							str.Replace("\n", "\\n"),
							"'",
							(!text.NullOrEmpty()) ? (" (placeholder exists in " + text + ")") : string.Empty
						}));
					}
				}
				else if (injectionsByNormalizedPath.TryGetValue(normalizedPath, out defInjection2) && defInjection2.IsFullListInjection)
				{
					if (!translationAllowed || !fullListTranslationAllowed)
					{
						outUnnecessaryDefInjections.Add(defInjection2.path + " '" + defInjection2.fullListInjection.ToStringSafeEnumerable().Replace("\n", "\\n") + "'");
					}
					else if (defInjection2.isPlaceholder && translationAllowed && !def.generated)
					{
						missingInjections.Add(suggestedPath + ((!defInjection2.fileSource.NullOrEmpty()) ? (" (placeholder exists in " + defInjection2.fileSource + ")") : string.Empty));
					}
				}
				else
				{
					int num = 0;
					foreach (string current2 in collection)
					{
						string key = normalizedPath + "." + num;
						string text2 = suggestedPath + "." + num;
						bool flag2 = false;
						string text3 = null;
						DefInjectionPackage.DefInjection defInjection3;
						if (injectionsByNormalizedPath.TryGetValue(key, out defInjection3) && !defInjection3.IsFullListInjection)
						{
							if (!translationAllowed)
							{
								outUnnecessaryDefInjections.Add(defInjection3.path + " '" + defInjection3.injection.Replace("\n", "\\n") + "'");
							}
							else if (defInjection3.isPlaceholder)
							{
								flag2 = true;
								text3 = defInjection3.fileSource;
							}
						}
						else
						{
							flag2 = true;
						}
						if (flag2 && translationAllowed && DefInjectionUtility.ShouldCheckMissingInjection(current2, fieldInfo, def))
						{
							DefInjectionPackage.DefInjection defInjection4;
							if (text3.NullOrEmpty() && injectionsByNormalizedPath.TryGetValue(normalizedPath, out defInjection4) && defInjection4.isPlaceholder)
							{
								text3 = defInjection4.fileSource;
							}
							missingInjections.Add(string.Concat(new string[]
							{
								text2,
								" '",
								current2.Replace("\n", "\\n"),
								"'",
								(!fullListTranslationAllowed) ? string.Empty : " (hint: this list allows full-list translation by using <li> nodes)",
								(!text3.NullOrEmpty()) ? (" (placeholder exists in " + text3 + ")") : string.Empty
							}));
						}
						num++;
					}
				}
			});
			return missingInjections;
		}
	}
}
