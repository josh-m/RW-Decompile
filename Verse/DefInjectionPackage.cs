using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Verse
{
	public class DefInjectionPackage
	{
		private const BindingFlags FieldBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public Type defType;

		private Dictionary<string, string> injections = new Dictionary<string, string>();

		public DefInjectionPackage(Type defType)
		{
			this.defType = defType;
		}

		private string ProcessedPath(string path)
		{
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

		public void AddDataFromFile(FileInfo file)
		{
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
					}
					else
					{
						string key2 = this.ProcessedPath(current.Name.ToString());
						string translation2 = this.ProcessedTranslation(current.Value);
						this.TryAddInjection(file, key2, translation2);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Exception loading translation data from file ",
					file,
					": ",
					ex
				}));
			}
		}

		private void TryAddInjection(FileInfo file, string key, string translation)
		{
			if (this.HasError(file, key))
			{
				return;
			}
			this.injections.Add(key, translation);
		}

		private bool HasError(FileInfo file, string key)
		{
			if (!key.Contains('.'))
			{
				Log.Warning(string.Concat(new object[]
				{
					"Error loading DefInjection from file ",
					file,
					": Key lacks a dot: ",
					key
				}));
				return true;
			}
			if (this.injections.ContainsKey(key))
			{
				Log.Warning("Duplicate def-linked translation key: " + key);
				return true;
			}
			return false;
		}

		public void InjectIntoDefs()
		{
			foreach (KeyValuePair<string, string> current in this.injections)
			{
				string[] array = current.Key.Split(new char[]
				{
					'.'
				});
				string text = array[0];
				text = BackCompatibility.BackCompatibleDefName(this.defType, text);
				if (GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), this.defType, "GetNamedSilentFail", new object[]
				{
					text
				}) == null)
				{
					Log.Warning(string.Concat(new object[]
					{
						"Def-linked translation error: Found no ",
						this.defType,
						" named ",
						text,
						" to match ",
						current.Key
					}));
				}
				else
				{
					this.SetDefFieldAtPath(this.defType, current.Key, current.Value);
				}
			}
			GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), this.defType, "ClearCachedData");
		}

		private void SetDefFieldAtPath(Type defType, string path, string value)
		{
			path = BackCompatibility.BackCompatibleModifiedTranslationPath(defType, path);
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
				DefInjectionPathPartKind defInjectionPathPartKind;
				string text;
				int num;
				while (true)
				{
					defInjectionPathPartKind = DefInjectionPathPartKind.Field;
					text = list[0];
					num = -1;
					if (text.Contains('['))
					{
						defInjectionPathPartKind = DefInjectionPathPartKind.FieldWithListIndex;
						string[] array = text.Split(new char[]
						{
							'['
						});
						string text2 = array[1];
						text2 = text2.Substring(0, text2.Length - 1);
						num = (int)ParseHelper.FromString(text2, typeof(int));
						text = array[0];
					}
					else if (int.TryParse(text, out num))
					{
						defInjectionPathPartKind = DefInjectionPathPartKind.ListIndex;
					}
					if (list.Count == 1)
					{
						break;
					}
					if (defInjectionPathPartKind == DefInjectionPathPartKind.ListIndex)
					{
						PropertyInfo property = obj.GetType().GetProperty("Item");
						if (property == null)
						{
							goto Block_17;
						}
						obj = property.GetValue(obj, new object[]
						{
							num
						});
					}
					else
					{
						FieldInfo field = obj.GetType().GetField(text, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						if (field == null)
						{
							goto Block_18;
						}
						if (defInjectionPathPartKind == DefInjectionPathPartKind.Field)
						{
							obj = field.GetValue(obj);
						}
						else
						{
							object value2 = field.GetValue(obj);
							PropertyInfo property2 = value2.GetType().GetProperty("Item");
							if (property2 == null)
							{
								goto Block_20;
							}
							obj = property2.GetValue(value2, new object[]
							{
								num
							});
						}
					}
					list.RemoveAt(0);
				}
				if (defInjectionPathPartKind == DefInjectionPathPartKind.Field)
				{
					FieldInfo fieldNamed = this.GetFieldNamed(obj.GetType(), text);
					if (fieldNamed == null)
					{
						throw new InvalidOperationException("Field " + text + " does not exist.");
					}
					if (fieldNamed.HasAttribute<NoTranslateAttribute>())
					{
						Log.Error(string.Concat(new object[]
						{
							"Translated untranslateable field ",
							fieldNamed.Name,
							" of type ",
							fieldNamed.FieldType,
							" at path ",
							path,
							". Translating this field will break the game."
						}));
					}
					else if (fieldNamed.FieldType != typeof(string))
					{
						Log.Error(string.Concat(new object[]
						{
							"Translated non-string field ",
							fieldNamed.Name,
							" of type ",
							fieldNamed.FieldType,
							" at path ",
							path,
							". Only string fields should be translated."
						}));
					}
					else
					{
						fieldNamed.SetValue(obj, value);
					}
				}
				else
				{
					object obj2;
					if (defInjectionPathPartKind == DefInjectionPathPartKind.FieldWithListIndex)
					{
						FieldInfo field2 = obj.GetType().GetField(text, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						if (field2 == null)
						{
							throw new InvalidOperationException("Field " + text + " does not exist.");
						}
						obj2 = field2.GetValue(obj);
					}
					else
					{
						obj2 = obj;
					}
					Type type = obj2.GetType();
					PropertyInfo property3 = type.GetProperty("Count");
					if (property3 == null)
					{
						throw new InvalidOperationException("Tried to use index on non-list (missing 'Count' property).");
					}
					int num2 = (int)property3.GetValue(obj2, null);
					if (num >= num2)
					{
						throw new InvalidOperationException(string.Concat(new object[]
						{
							"Trying to translate ",
							defType,
							".",
							path,
							" at index ",
							num,
							" but the original list only has ",
							num2,
							" entries (so the max index is ",
							(num2 - 1).ToString(),
							")."
						}));
					}
					PropertyInfo property4 = type.GetProperty("Item");
					if (property4 == null)
					{
						throw new InvalidOperationException("Tried to use index on non-list (missing 'Item' property).");
					}
					property4.SetValue(obj2, value, new object[]
					{
						num
					});
				}
				return;
				Block_17:
				throw new InvalidOperationException("Tried to use index on non-list (missing 'Item' property).");
				Block_18:
				throw new InvalidOperationException("Field " + text + " does not exist.");
				Block_20:
				throw new InvalidOperationException("Tried to use index on non-list (missing 'Item' property).");
			}
			catch (Exception ex)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Def-linked translation error: Exception getting field at path ",
					path,
					" in ",
					defType,
					": ",
					ex.ToString()
				}));
			}
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

		[DebuggerHidden]
		public IEnumerable<string> MissingInjections()
		{
			Type databaseType = typeof(DefDatabase<>).MakeGenericType(new Type[]
			{
				this.defType
			});
			PropertyInfo allDefsProperty = databaseType.GetProperty("AllDefs");
			MethodInfo allDefsMethod = allDefsProperty.GetGetMethod();
			IEnumerable allDefsEnum = (IEnumerable)allDefsMethod.Invoke(null, null);
			foreach (Def def in allDefsEnum)
			{
				foreach (string mi in this.MissingInjectionsFromDef(def))
				{
					yield return mi;
				}
			}
		}

		[DebuggerHidden]
		private IEnumerable<string> MissingInjectionsFromDef(Def def)
		{
			if (!def.label.NullOrEmpty())
			{
				string path = def.defName + ".label";
				if (!this.injections.ContainsKey(path))
				{
					yield return path + " '" + def.label + "'";
				}
			}
			if (!def.description.NullOrEmpty())
			{
				string path2 = def.defName + ".description";
				if (!this.injections.ContainsKey(path2))
				{
					yield return path2 + " '" + def.description + "'";
				}
			}
			FieldInfo[] fields = def.GetType().GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo fi = fields[i];
				if (fi.FieldType == typeof(string))
				{
					string val = (string)fi.GetValue(def);
					if (!val.NullOrEmpty() && !(fi.Name == "defName") && !(fi.Name == "label") && !(fi.Name == "description") && !fi.HasAttribute<NoTranslateAttribute>() && !fi.HasAttribute<UnsavedAttribute>())
					{
						if (fi.HasAttribute<MustTranslateAttribute>() || (val != null && val.Contains(' ')))
						{
							string path3 = def.defName + "." + fi.Name;
							if (!this.injections.ContainsKey(path3))
							{
								yield return path3 + " '" + val + "'";
							}
						}
					}
				}
			}
		}
	}
}
