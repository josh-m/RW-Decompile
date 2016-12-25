using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Verse
{
	public static class XmlSaver
	{
		public static bool IsSimpleTextType(Type type)
		{
			return type == typeof(float) || type == typeof(int) || type == typeof(bool) || type == typeof(string) || type.IsEnum;
		}

		public static void SaveDataObject(object obj, string filePath)
		{
			try
			{
				XDocument xDocument = new XDocument();
				XElement content = XmlSaver.XElementFromObject(obj, obj.GetType());
				xDocument.Add(content);
				xDocument.Save(filePath);
			}
			catch (Exception ex)
			{
				GenUI.ErrorDialog("ProblemSavingFile".Translate(new object[]
				{
					filePath,
					ex.ToString()
				}));
				Log.Error(string.Concat(new object[]
				{
					"Exception saving data object ",
					obj,
					": ",
					ex
				}));
			}
		}

		public static XElement XElementFromObject(object obj, Type expectedClass)
		{
			return XmlSaver.XElementFromObject(obj, expectedClass, expectedClass.Name, null, false);
		}

		public static XElement XElementFromObject(object obj, Type expectedType, string nodeName, FieldInfo owningField = null, bool saveDefsAsRefs = false)
		{
			DefaultValueAttribute defaultValueAttribute;
			if (owningField != null && owningField.TryGetAttribute(out defaultValueAttribute) && defaultValueAttribute.ObjIsDefault(obj))
			{
				return null;
			}
			if (obj == null)
			{
				XElement xElement = new XElement(nodeName);
				xElement.SetAttributeValue("IsNull", "True");
				return xElement;
			}
			Type type = obj.GetType();
			XElement xElement2 = new XElement(nodeName);
			if (XmlSaver.IsSimpleTextType(type))
			{
				xElement2.Add(new XText(obj.ToString()));
			}
			else if (saveDefsAsRefs && typeof(Def).IsAssignableFrom(type))
			{
				string defName = ((Def)obj).defName;
				xElement2.Add(new XText(defName));
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				Type expectedType2 = type.GetGenericArguments()[0];
				int num = (int)type.GetProperty("Count").GetValue(obj, null);
				for (int i = 0; i < num; i++)
				{
					object[] index = new object[]
					{
						i
					};
					object value = type.GetProperty("Item").GetValue(obj, index);
					XNode content = XmlSaver.XElementFromObject(value, expectedType2, "li", null, true);
					xElement2.Add(content);
				}
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
			{
				Type expectedType3 = type.GetGenericArguments()[0];
				Type expectedType4 = type.GetGenericArguments()[1];
				foreach (object current in (obj as IEnumerable))
				{
					object value2 = current.GetType().GetProperty("Key").GetValue(current, null);
					object value3 = current.GetType().GetProperty("Value").GetValue(current, null);
					XElement xElement3 = new XElement("li");
					xElement3.Add(XmlSaver.XElementFromObject(value2, expectedType3, "key", null, true));
					xElement3.Add(XmlSaver.XElementFromObject(value3, expectedType4, "value", null, true));
					xElement2.Add(xElement3);
				}
			}
			else
			{
				if (type != expectedType)
				{
					XAttribute content2 = new XAttribute("Class", GenTypes.GetTypeNameWithoutIgnoredNamespaces(obj.GetType()));
					xElement2.Add(content2);
				}
				foreach (FieldInfo current2 in from f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				orderby f.MetadataToken
				select f)
				{
					try
					{
						XElement xElement4 = XmlSaver.XElementFromField(current2, obj);
						if (xElement4 != null)
						{
							xElement2.Add(xElement4);
						}
					}
					catch
					{
						throw;
					}
				}
			}
			return xElement2;
		}

		private static XElement XElementFromField(FieldInfo fi, object owningObj)
		{
			if (Attribute.IsDefined(fi, typeof(UnsavedAttribute)))
			{
				return null;
			}
			object value = fi.GetValue(owningObj);
			return XmlSaver.XElementFromObject(value, fi.FieldType, fi.Name, fi, false);
		}
	}
}
