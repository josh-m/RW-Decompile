using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Verse
{
	public static class DirectXmlToObject
	{
		public static Stack<Type> currentlyInstantiatingObjectOfType = new Stack<Type>();

		public const string DictionaryKeyName = "key";

		public const string DictionaryValueName = "value";

		public const string LoadDataFromXmlCustomMethodName = "LoadDataFromXmlCustom";

		public const string PostLoadMethodName = "PostLoad";

		public const string ObjectFromXmlMethodName = "ObjectFromXml";

		public const string ListFromXmlMethodName = "ListFromXml";

		public const string DictionaryFromXmlMethodName = "DictionaryFromXml";

		private static Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoLookup = new Dictionary<Type, Dictionary<string, FieldInfo>>();

		public static T ObjectFromXml<T>(XmlNode xmlRoot, bool doPostLoad) where T : new()
		{
			MethodInfo methodInfo = DirectXmlToObject.CustomDataLoadMethodOf(typeof(T));
			if (methodInfo != null)
			{
				xmlRoot = XmlInheritance.GetResolvedNodeFor(xmlRoot);
				Type type = DirectXmlToObject.ClassTypeOf<T>(xmlRoot);
				DirectXmlToObject.currentlyInstantiatingObjectOfType.Push(type);
				T t;
				try
				{
					t = (T)((object)Activator.CreateInstance(type));
				}
				finally
				{
					DirectXmlToObject.currentlyInstantiatingObjectOfType.Pop();
				}
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
						"Exception in custom XML loader for ",
						typeof(T),
						". Node is:\n ",
						xmlRoot.OuterXml,
						"\n\nException is:\n ",
						ex.ToString()
					}), false);
					t = default(T);
				}
				if (doPostLoad)
				{
					DirectXmlToObject.TryDoPostLoad(t);
				}
				return t;
			}
			if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild.NodeType == XmlNodeType.CDATA)
			{
				if (typeof(T) != typeof(string))
				{
					Log.Error("CDATA can only be used for strings. Bad xml: " + xmlRoot.OuterXml, false);
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
						}), false);
					}
					return default(T);
				}
				if (Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
				{
					List<T> list = DirectXmlToObject.ListFromXml<T>(xmlRoot);
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
					MethodInfo method = typeof(DirectXmlToObject).GetMethod("ListFromXml", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
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
					MethodInfo method2 = typeof(DirectXmlToObject).GetMethod("DictionaryFromXml", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
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
							return Activator.CreateInstance<T>();
						}
					}
				}
				xmlRoot = XmlInheritance.GetResolvedNodeFor(xmlRoot);
				Type type2 = DirectXmlToObject.ClassTypeOf<T>(xmlRoot);
				Type type3 = Nullable.GetUnderlyingType(type2) ?? type2;
				DirectXmlToObject.currentlyInstantiatingObjectOfType.Push(type3);
				T t2;
				try
				{
					t2 = (T)((object)Activator.CreateInstance(type3));
				}
				finally
				{
					DirectXmlToObject.currentlyInstantiatingObjectOfType.Pop();
				}
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
								}), false);
							}
							else
							{
								list2.Add(xmlNode.Name);
							}
						}
						FieldInfo fieldInfo = DirectXmlToObject.GetFieldInfoForType(t2.GetType(), xmlNode.Name, xmlRoot);
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
						if (fieldInfo != null && fieldInfo.TryGetAttribute<UnsavedAttribute>() != null)
						{
							Log.Error(string.Concat(new string[]
							{
								"XML error: ",
								xmlNode.OuterXml,
								" corresponds to a field in type ",
								t2.GetType().Name,
								" which has an Unsaved attribute. Context: ",
								xmlRoot.OuterXml
							}), false);
						}
						else if (fieldInfo == null)
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
									". Context: ",
									xmlRoot.OuterXml
								}), false);
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
								DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(t2, fieldInfo, xmlNode.InnerText);
							}
						}
						else
						{
							object value = null;
							try
							{
								MethodInfo method3 = typeof(DirectXmlToObject).GetMethod("ObjectFromXml", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
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
								Log.Error("Exception loading from " + xmlNode.ToString() + ": " + ex3.ToString(), false);
								goto IL_850;
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
					IL_850:;
				}
				if (doPostLoad)
				{
					DirectXmlToObject.TryDoPostLoad(t2);
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
				Log.Error("Could not find type named " + xmlAttribute.Value + " from node " + xmlRoot.OuterXml, false);
				return typeof(T);
			}
			return typeInAnyAssembly;
		}

		private static void TryDoPostLoad(object obj)
		{
			try
			{
				MethodInfo method = obj.GetType().GetMethod("PostLoad");
				if (method != null)
				{
					method.Invoke(obj, null);
				}
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new object[]
				{
					"Exception while executing PostLoad on ",
					obj.ToStringSafe<object>(),
					": ",
					ex
				}), false);
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
					if (DirectXmlToObject.ValidateListNode(xmlNode, listRootNode, typeof(T)))
					{
						if (flag)
						{
							DirectXmlCrossRefLoader.RegisterListWantsCrossRef<T>(list, xmlNode.InnerText, listRootNode.Name);
						}
						else
						{
							list.Add(DirectXmlToObject.ObjectFromXml<T>(xmlNode, true));
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
				}), false);
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
						if (DirectXmlToObject.ValidateListNode(xmlNode, dictRootNode, typeof(KeyValuePair<K, V>)))
						{
							K key = DirectXmlToObject.ObjectFromXml<K>(xmlNode["key"], true);
							V value = DirectXmlToObject.ObjectFromXml<V>(xmlNode["value"], true);
							dictionary.Add(key, value);
						}
					}
				}
				else
				{
					foreach (XmlNode xmlNode2 in dictRootNode.ChildNodes)
					{
						if (DirectXmlToObject.ValidateListNode(xmlNode2, dictRootNode, typeof(KeyValuePair<K, V>)))
						{
							DirectXmlCrossRefLoader.RegisterDictionaryWantsCrossRef<K, V>(dictionary, xmlNode2, dictRootNode.Name);
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
				}), false);
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
				Log.Error("XML format error: Raw text found inside a list element. Did you mean to surround it with list item <li> tags? " + listRootNode.OuterXml, false);
				return false;
			}
			if (listEntryNode.Name != "li" && DirectXmlToObject.CustomDataLoadMethodOf(listItemType) == null)
			{
				Log.Error("XML format error: List item found with name that is not <li>, and which does not have a custom XML loader method, in " + listRootNode.OuterXml, false);
				return false;
			}
			return true;
		}

		private static FieldInfo GetFieldInfoForType(Type type, string token, XmlNode debugXmlNode)
		{
			Dictionary<string, FieldInfo> dictionary = DirectXmlToObject.fieldInfoLookup.TryGetValue(type, null);
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, FieldInfo>();
				DirectXmlToObject.fieldInfoLookup[type] = dictionary;
			}
			FieldInfo fieldInfo = dictionary.TryGetValue(token, null);
			if (fieldInfo == null && !dictionary.ContainsKey(token))
			{
				fieldInfo = DirectXmlToObject.SearchTypeHierarchy(type, token, BindingFlags.Default);
				if (fieldInfo == null)
				{
					fieldInfo = DirectXmlToObject.SearchTypeHierarchy(type, token, BindingFlags.IgnoreCase);
					if (fieldInfo != null && !type.HasAttribute<CaseInsensitiveXMLParsing>())
					{
						string text = string.Format("Attempt to use string {0} to refer to field {1} in type {2}; xml tags are now case-sensitive", token, fieldInfo.Name, type);
						if (debugXmlNode != null)
						{
							text = text + ". XML: " + debugXmlNode.OuterXml;
						}
						Log.Error(text, false);
					}
				}
				dictionary[token] = fieldInfo;
			}
			return fieldInfo;
		}

		private static FieldInfo SearchTypeHierarchy(Type type, string token, BindingFlags extraFlags)
		{
			FieldInfo field;
			while (true)
			{
				field = type.GetField(token, extraFlags | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (field != null || type.BaseType == typeof(object))
				{
					break;
				}
				type = type.BaseType;
			}
			return field;
		}
	}
}
