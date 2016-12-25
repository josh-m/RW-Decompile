using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Verse
{
	public static class XmlToObject
	{
		public const string ListItemName = "li";

		public const string DictionaryKeyName = "key";

		public const string DictionaryValueName = "value";

		public const string LoadDataFromXmlCustomMethodName = "LoadDataFromXmlCustom";

		public const string PostLoadMethodName = "PostLoad";

		public const string ObjectFromXmlMethodName = "ObjectFromXml";

		public const string ListFromXmlMethodName = "ListFromXml";

		public const string DictionaryFromXmlMethodName = "DictionaryFromXml";

		public const string ClassAttributeName = "Class";

		public const string IsNullAttributeName = "IsNull";

		public static T ObjectFromXml<T>(XmlNode xmlRoot, bool doPostLoad) where T : new()
		{
			MethodInfo methodInfo = XmlToObject.CustomDataLoadMethodOf(typeof(T));
			if (methodInfo != null)
			{
				xmlRoot = XmlInheritance.GetResolvedNodeFor(xmlRoot);
				Type type = XmlToObject.ClassTypeOf<T>(xmlRoot);
				T t = (T)((object)Activator.CreateInstance(type));
				try
				{
					methodInfo.Invoke(t, new object[]
					{
						xmlRoot
					});
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Exception in custom XMl loader for ",
						typeof(T),
						". Node is:\n ",
						xmlRoot.OuterXml,
						"\n\nException is:\n ",
						ex.ToString()
					}));
					t = default(T);
				}
				if (doPostLoad)
				{
					XmlToObject.TryDoPostLoad(t);
				}
				return t;
			}
			if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild.NodeType == XmlNodeType.CDATA)
			{
				if (typeof(T) != typeof(string))
				{
					Log.Error("CDATA can only be used for strings. Bad xml: " + xmlRoot.OuterXml);
					return default(T);
				}
				return (T)((object)xmlRoot.FirstChild.Value);
			}
			else
			{
				if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild.NodeType == XmlNodeType.Text)
				{
					try
					{
						return (T)((object)ParseHelper.FromString(xmlRoot.InnerText, typeof(T)));
					}
					catch (Exception ex2)
					{
						Log.Error(string.Concat(new object[]
						{
							"Exception parsing ",
							xmlRoot.OuterXml,
							" to type ",
							typeof(T),
							": ",
							ex2
						}));
					}
					return default(T);
				}
				if (Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
				{
					List<T> list = XmlToObject.ListFromXml<T>(xmlRoot);
					int num = 0;
					foreach (T current in list)
					{
						int num2 = (int)((object)current);
						num |= num2;
					}
					return (T)((object)num);
				}
				if (typeof(T).HasGenericDefinition(typeof(List<>)))
				{
					MethodInfo method = typeof(XmlToObject).GetMethod("ListFromXml", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					Type[] genericArguments = typeof(T).GetGenericArguments();
					MethodInfo methodInfo2 = method.MakeGenericMethod(genericArguments);
					object[] parameters = new object[]
					{
						xmlRoot
					};
					object obj = methodInfo2.Invoke(null, parameters);
					return (T)((object)obj);
				}
				if (typeof(T).HasGenericDefinition(typeof(Dictionary<, >)))
				{
					MethodInfo method2 = typeof(XmlToObject).GetMethod("DictionaryFromXml", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					Type[] genericArguments2 = typeof(T).GetGenericArguments();
					MethodInfo methodInfo3 = method2.MakeGenericMethod(genericArguments2);
					object[] parameters2 = new object[]
					{
						xmlRoot
					};
					object obj2 = methodInfo3.Invoke(null, parameters2);
					return (T)((object)obj2);
				}
				if (!xmlRoot.HasChildNodes)
				{
					if (typeof(T) == typeof(string))
					{
						return (T)((object)string.Empty);
					}
					XmlAttribute xmlAttribute = xmlRoot.Attributes["IsNull"];
					if (xmlAttribute != null && xmlAttribute.Value.ToUpperInvariant() == "TRUE")
					{
						return default(T);
					}
					if (typeof(T).IsGenericType)
					{
						Type genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
						if (genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(HashSet<>) || genericTypeDefinition == typeof(Dictionary<, >))
						{
							return (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
						}
					}
				}
				xmlRoot = XmlInheritance.GetResolvedNodeFor(xmlRoot);
				Type type2 = XmlToObject.ClassTypeOf<T>(xmlRoot);
				T t2 = (T)((object)Activator.CreateInstance(type2));
				List<string> list2 = null;
				if (xmlRoot.ChildNodes.Count > 1)
				{
					list2 = new List<string>();
				}
				for (int i = 0; i < xmlRoot.ChildNodes.Count; i++)
				{
					XmlNode xmlNode = xmlRoot.ChildNodes[i];
					if (!(xmlNode is XmlComment))
					{
						if (xmlRoot.ChildNodes.Count > 1)
						{
							if (list2.Contains(xmlNode.Name))
							{
								Log.Error(string.Concat(new object[]
								{
									"XML ",
									typeof(T),
									" defines the same field twice: ",
									xmlNode.Name,
									".\n\nField contents: ",
									xmlNode.InnerText,
									".\n\nWhole XML:\n\n",
									xmlRoot.OuterXml
								}));
							}
							else
							{
								list2.Add(xmlNode.Name);
							}
						}
						FieldInfo fieldInfo = null;
						Type type3 = t2.GetType();
						while (true)
						{
							fieldInfo = type3.GetField(xmlNode.Name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							if (fieldInfo != null || type3.BaseType == typeof(object))
							{
								break;
							}
							type3 = type3.BaseType;
						}
						if (fieldInfo == null)
						{
							FieldInfo[] fields = t2.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							for (int j = 0; j < fields.Length; j++)
							{
								FieldInfo fieldInfo2 = fields[j];
								object[] customAttributes = fieldInfo2.GetCustomAttributes(typeof(LoadAliasAttribute), true);
								for (int k = 0; k < customAttributes.Length; k++)
								{
									object obj3 = customAttributes[k];
									string alias = ((LoadAliasAttribute)obj3).alias;
									if (alias.EqualsIgnoreCase(xmlNode.Name))
									{
										fieldInfo = fieldInfo2;
										break;
									}
								}
								if (fieldInfo != null)
								{
									break;
								}
							}
						}
						if (fieldInfo == null)
						{
							bool flag = false;
							object[] customAttributes2 = t2.GetType().GetCustomAttributes(typeof(IgnoreSavedElementAttribute), true);
							for (int l = 0; l < customAttributes2.Length; l++)
							{
								object obj4 = customAttributes2[l];
								string elementToIgnore = ((IgnoreSavedElementAttribute)obj4).elementToIgnore;
								if (string.Equals(elementToIgnore, xmlNode.Name, StringComparison.OrdinalIgnoreCase))
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								Log.Error(string.Concat(new string[]
								{
									"XML error: ",
									xmlNode.OuterXml,
									" doesn't correspond to any field in type ",
									t2.GetType().Name,
									"."
								}));
							}
						}
						else if (typeof(Def).IsAssignableFrom(fieldInfo.FieldType))
						{
							if (xmlNode.InnerText.NullOrEmpty())
							{
								fieldInfo.SetValue(t2, null);
							}
							else
							{
								CrossRefLoader.RegisterObjectWantsCrossRef(t2, fieldInfo, xmlNode.InnerText);
							}
						}
						else
						{
							object value = null;
							try
							{
								MethodInfo method3 = typeof(XmlToObject).GetMethod("ObjectFromXml", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
								MethodInfo methodInfo4 = method3.MakeGenericMethod(new Type[]
								{
									fieldInfo.FieldType
								});
								value = methodInfo4.Invoke(null, new object[]
								{
									xmlNode,
									doPostLoad
								});
							}
							catch (Exception ex3)
							{
								Log.Error("Exception loading from " + xmlNode.ToString() + ": " + ex3.ToString());
								goto IL_7F3;
							}
							if (!typeof(T).IsValueType)
							{
								fieldInfo.SetValue(t2, value);
							}
							else
							{
								object obj5 = t2;
								fieldInfo.SetValue(obj5, value);
								t2 = (T)((object)obj5);
							}
						}
					}
					IL_7F3:;
				}
				if (doPostLoad)
				{
					XmlToObject.TryDoPostLoad(t2);
				}
				return t2;
			}
		}

		private static Type ClassTypeOf<T>(XmlNode xmlRoot)
		{
			XmlAttribute xmlAttribute = xmlRoot.Attributes["Class"];
			if (xmlAttribute == null)
			{
				return typeof(T);
			}
			Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(xmlAttribute.Value);
			if (typeInAnyAssembly == null)
			{
				Log.Error("Could not find type named " + xmlAttribute.Value + " from node " + xmlRoot.OuterXml);
				return typeof(T);
			}
			return typeInAnyAssembly;
		}

		private static void TryDoPostLoad(object obj)
		{
			MethodInfo method = obj.GetType().GetMethod("PostLoad");
			if (method != null)
			{
				method.Invoke(obj, null);
			}
		}

		private static List<T> ListFromXml<T>(XmlNode listRootNode) where T : new()
		{
			List<T> list = new List<T>();
			try
			{
				bool flag = typeof(Def).IsAssignableFrom(typeof(T));
				foreach (XmlNode xmlNode in listRootNode.ChildNodes)
				{
					if (XmlToObject.ValidateListNode(xmlNode, listRootNode, typeof(T)))
					{
						if (flag)
						{
							CrossRefLoader.RegisterListWantsCrossRef<T>(list, xmlNode.InnerText);
						}
						else
						{
							list.Add(XmlToObject.ObjectFromXml<T>(xmlNode, true));
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new object[]
				{
					"Exception loading list from XML: ",
					ex,
					"\nXML:\n",
					listRootNode.OuterXml
				}));
			}
			return list;
		}

		private static Dictionary<K, V> DictionaryFromXml<K, V>(XmlNode dictRootNode) where K : new() where V : new()
		{
			Dictionary<K, V> dictionary = new Dictionary<K, V>();
			try
			{
				bool flag = typeof(Def).IsAssignableFrom(typeof(K));
				bool flag2 = typeof(Def).IsAssignableFrom(typeof(V));
				if (!flag && !flag2)
				{
					foreach (XmlNode xmlNode in dictRootNode.ChildNodes)
					{
						if (XmlToObject.ValidateListNode(xmlNode, dictRootNode, typeof(KeyValuePair<K, V>)))
						{
							K key = XmlToObject.ObjectFromXml<K>(xmlNode["key"], true);
							V value = XmlToObject.ObjectFromXml<V>(xmlNode["value"], true);
							dictionary.Add(key, value);
						}
					}
				}
				else
				{
					foreach (XmlNode xmlNode2 in dictRootNode.ChildNodes)
					{
						if (XmlToObject.ValidateListNode(xmlNode2, dictRootNode, typeof(KeyValuePair<K, V>)))
						{
							CrossRefLoader.RegisterDictionaryWantsCrossRef<K, V>(dictionary, xmlNode2);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new object[]
				{
					"Malformed dictionary XML. Node: ",
					dictRootNode.OuterXml,
					".\n\nException: ",
					ex
				}));
			}
			return dictionary;
		}

		private static MethodInfo CustomDataLoadMethodOf(Type type)
		{
			return type.GetMethod("LoadDataFromXmlCustom", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		private static bool ValidateListNode(XmlNode listEntryNode, XmlNode listRootNode, Type listItemType)
		{
			if (listEntryNode is XmlComment)
			{
				return false;
			}
			if (listEntryNode is XmlText)
			{
				Log.Error("XML format error: Raw text found inside a list element. Did you mean to surround it with list item <li> tags? " + listRootNode.OuterXml);
				return false;
			}
			if (listEntryNode.Name != "li" && XmlToObject.CustomDataLoadMethodOf(listItemType) == null)
			{
				Log.Error("XML format error: List item found with name that is not <li>, and which does not have a custom XML loader method, in " + listRootNode.OuterXml);
				return false;
			}
			return true;
		}
	}
}
