using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Verse
{
	public static class TranslationHandleUtility
	{
		public const char HandleIndexCharacter = '-';

		private static Regex StringFormatSymbolsRegex = new Regex("{.*?}");

		private static StringBuilder tmpStringBuilder = new StringBuilder();

		public static int GetElementIndexByHandle(object list, string handle, int handleIndex)
		{
			if (list == null)
			{
				throw new InvalidOperationException("Tried to get element by handle on null object.");
			}
			if (handleIndex < 0)
			{
				handleIndex = 0;
			}
			PropertyInfo property = list.GetType().GetProperty("Count");
			if (property == null)
			{
				throw new InvalidOperationException("Tried to get element by handle on non-list (missing 'Count' property).");
			}
			PropertyInfo property2 = list.GetType().GetProperty("Item");
			if (property2 == null)
			{
				throw new InvalidOperationException("Tried to get element by handle on non-list (missing 'Item' property).");
			}
			int num = (int)property.GetValue(list, null);
			FieldInfo fieldInfo = null;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				object value = property2.GetValue(list, new object[]
				{
					i
				});
				if (value != null)
				{
					FieldInfo[] fields = value.GetType().GetFields(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					FieldInfo[] array = fields;
					for (int j = 0; j < array.Length; j++)
					{
						FieldInfo fieldInfo2 = array[j];
						TranslationHandleAttribute translationHandleAttribute = fieldInfo2.TryGetAttribute<TranslationHandleAttribute>();
						if (translationHandleAttribute != null)
						{
							object value2 = fieldInfo2.GetValue(value);
							if (value2 != null)
							{
								if (TranslationHandleUtility.HandlesMatch(value2, handle))
								{
									int priority = translationHandleAttribute.Priority;
									if (fieldInfo == null || priority > num2)
									{
										fieldInfo = fieldInfo2;
										num2 = priority;
									}
								}
							}
						}
					}
				}
			}
			if (fieldInfo == null)
			{
				throw new InvalidOperationException("None of the list elements have a handle named " + handle + ".");
			}
			int num3 = 0;
			for (int k = 0; k < num; k++)
			{
				object value3 = property2.GetValue(list, new object[]
				{
					k
				});
				if (value3 != null)
				{
					FieldInfo[] fields2 = value3.GetType().GetFields(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					FieldInfo[] array2 = fields2;
					for (int l = 0; l < array2.Length; l++)
					{
						FieldInfo fieldInfo3 = array2[l];
						if (TranslationHandleUtility.FieldInfosEqual(fieldInfo3, fieldInfo))
						{
							object value4 = fieldInfo3.GetValue(value3);
							if (value4 != null)
							{
								if (TranslationHandleUtility.HandlesMatch(value4, handle))
								{
									if (num3 == handleIndex)
									{
										return k;
									}
									num3++;
								}
							}
						}
					}
				}
			}
			throw new InvalidOperationException(string.Concat(new object[]
			{
				"Tried to access handle ",
				handle,
				"[",
				handleIndex,
				"], but there are only ",
				num3,
				" handles matching this name."
			}));
		}

		public static string GetBestHandleWithIndexForListElement(object list, object element)
		{
			if (list == null || element == null)
			{
				return null;
			}
			PropertyInfo property = list.GetType().GetProperty("Count");
			if (property == null)
			{
				return null;
			}
			PropertyInfo property2 = list.GetType().GetProperty("Item");
			if (property2 == null)
			{
				return null;
			}
			FieldInfo fieldInfo = null;
			string handle = null;
			int num = 0;
			FieldInfo[] fields = element.GetType().GetFields(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo[] array = fields;
			for (int i = 0; i < array.Length; i++)
			{
				FieldInfo fieldInfo2 = array[i];
				TranslationHandleAttribute translationHandleAttribute = fieldInfo2.TryGetAttribute<TranslationHandleAttribute>();
				if (translationHandleAttribute != null)
				{
					object value = fieldInfo2.GetValue(element);
					if (value != null)
					{
						Type type = value as Type;
						string text;
						if (type != null)
						{
							text = type.Name;
						}
						else
						{
							try
							{
								text = value.ToString();
							}
							catch
							{
								return null;
							}
						}
						if (!text.NullOrEmpty())
						{
							int priority = translationHandleAttribute.Priority;
							if (fieldInfo == null || priority > num)
							{
								fieldInfo = fieldInfo2;
								handle = text;
								num = priority;
							}
						}
					}
				}
			}
			if (fieldInfo == null)
			{
				return null;
			}
			int num2 = 0;
			int num3 = -1;
			int num4 = (int)property.GetValue(list, null);
			for (int j = 0; j < num4; j++)
			{
				object value2 = property2.GetValue(list, new object[]
				{
					j
				});
				if (value2 != null)
				{
					if (value2 == element)
					{
						num3 = num2;
						num2++;
					}
					else
					{
						FieldInfo[] fields2 = value2.GetType().GetFields(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						FieldInfo[] array2 = fields2;
						for (int k = 0; k < array2.Length; k++)
						{
							FieldInfo fieldInfo3 = array2[k];
							if (TranslationHandleUtility.FieldInfosEqual(fieldInfo3, fieldInfo))
							{
								object value3 = fieldInfo3.GetValue(value2);
								if (value3 != null)
								{
									if (TranslationHandleUtility.HandlesMatch(value3, handle))
									{
										num2++;
										break;
									}
								}
							}
						}
					}
				}
			}
			if (num3 < 0)
			{
				return null;
			}
			string text2 = TranslationHandleUtility.NormalizedHandle(handle);
			if (num2 <= 1)
			{
				return text2;
			}
			return text2 + '-' + num3;
		}

		public static bool HandlesMatch(object item, string handle)
		{
			if (item == null)
			{
				return false;
			}
			if (handle.NullOrEmpty())
			{
				return false;
			}
			handle = TranslationHandleUtility.NormalizedHandle(handle);
			if (handle.NullOrEmpty())
			{
				return false;
			}
			Type type = item as Type;
			if (type != null)
			{
				return TranslationHandleUtility.NormalizedHandle(type.Name) == handle || TranslationHandleUtility.NormalizedHandle(type.FullName) == handle || TranslationHandleUtility.NormalizedHandle(type.ToString()) == handle;
			}
			string text;
			try
			{
				text = item.ToString();
			}
			catch (Exception arg)
			{
				throw new InvalidOperationException("Could not get element by handle because one of the elements threw an exception in its ToString(): " + arg);
			}
			return !text.NullOrEmpty() && TranslationHandleUtility.NormalizedHandle(text) == handle;
		}

		private static string NormalizedHandle(string handle)
		{
			if (handle.NullOrEmpty())
			{
				return handle;
			}
			handle = handle.Trim();
			handle = handle.Replace(' ', '_');
			handle = handle.Replace('\n', '_');
			handle = handle.Replace("\r", string.Empty);
			handle = handle.Replace('\t', '_');
			handle = handle.Replace(".", string.Empty);
			if (handle.IndexOf('-') >= 0)
			{
				handle = handle.Replace('-'.ToString(), string.Empty);
			}
			if (handle.IndexOf("{") >= 0)
			{
				handle = TranslationHandleUtility.StringFormatSymbolsRegex.Replace(handle, string.Empty);
			}
			TranslationHandleUtility.tmpStringBuilder.Length = 0;
			for (int i = 0; i < handle.Length; i++)
			{
				if ("qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890-_".IndexOf(handle[i]) >= 0)
				{
					TranslationHandleUtility.tmpStringBuilder.Append(handle[i]);
				}
			}
			handle = TranslationHandleUtility.tmpStringBuilder.ToString();
			TranslationHandleUtility.tmpStringBuilder.Length = 0;
			for (int j = 0; j < handle.Length; j++)
			{
				if (j == 0 || handle[j] != '_' || handle[j - 1] != '_')
				{
					TranslationHandleUtility.tmpStringBuilder.Append(handle[j]);
				}
			}
			handle = TranslationHandleUtility.tmpStringBuilder.ToString();
			handle = handle.Trim(new char[]
			{
				'_'
			});
			if (!handle.NullOrEmpty())
			{
				if (handle.All(new Func<char, bool>(char.IsDigit)))
				{
					handle = "_" + handle;
				}
			}
			return handle;
		}

		private static bool FieldInfosEqual(FieldInfo lhs, FieldInfo rhs)
		{
			return lhs.DeclaringType == rhs.DeclaringType && lhs.Name == rhs.Name;
		}
	}
}
