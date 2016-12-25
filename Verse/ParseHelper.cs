using RimWorld;
using Steamworks;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Verse
{
	public static class ParseHelper
	{
		public static object FromString(string str, Type itemType)
		{
			object result;
			try
			{
				if (itemType == typeof(string))
				{
					str = str.Replace("\\n", "\n");
					result = str;
				}
				else if (itemType == typeof(int))
				{
					result = int.Parse(str, CultureInfo.InvariantCulture);
				}
				else if (itemType == typeof(float))
				{
					result = float.Parse(str, CultureInfo.InvariantCulture);
				}
				else if (itemType == typeof(bool))
				{
					result = bool.Parse(str);
				}
				else if (itemType == typeof(long))
				{
					result = long.Parse(str, CultureInfo.InvariantCulture);
				}
				else if (itemType == typeof(double))
				{
					result = double.Parse(str, CultureInfo.InvariantCulture);
				}
				else if (itemType == typeof(sbyte))
				{
					result = sbyte.Parse(str, CultureInfo.InvariantCulture);
				}
				else
				{
					if (itemType.IsEnum)
					{
						try
						{
							result = Enum.Parse(itemType, str);
							return result;
						}
						catch (ArgumentException innerException)
						{
							string text = string.Concat(new object[]
							{
								"'",
								str,
								"' is not a valid value for ",
								itemType,
								". Valid values are: \n"
							});
							text += GenText.StringFromEnumerable(Enum.GetValues(itemType));
							ArgumentException ex = new ArgumentException(text, innerException);
							throw ex;
						}
					}
					if (itemType == typeof(Type))
					{
						if (str == "null" || str == "Null")
						{
							result = null;
						}
						else
						{
							Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(str);
							if (typeInAnyAssembly == null)
							{
								Log.Error("Could not find a type named " + str);
							}
							result = typeInAnyAssembly;
						}
					}
					else if (itemType == typeof(Action))
					{
						string[] array = str.Split(new char[]
						{
							'.'
						});
						string methodName = array[array.Length - 1];
						string typeName = string.Empty;
						if (array.Length == 3)
						{
							typeName = array[0] + "." + array[1];
						}
						else
						{
							typeName = array[0];
						}
						Type typeInAnyAssembly2 = GenTypes.GetTypeInAnyAssembly(typeName);
						MethodInfo method = typeInAnyAssembly2.GetMethods().First((MethodInfo m) => m.Name == methodName);
						result = (Action)Delegate.CreateDelegate(typeof(Action), method);
					}
					else if (itemType == typeof(Vector3))
					{
						result = ParseHelper.FromStringVector3(str);
					}
					else if (itemType == typeof(Vector2))
					{
						result = ParseHelper.FromStringVector2(str);
					}
					else if (itemType == typeof(Rect))
					{
						result = ParseHelper.FromStringRect(str);
					}
					else if (itemType == typeof(Color))
					{
						str = str.TrimStart(new char[]
						{
							'(',
							'R',
							'G',
							'B',
							'A'
						});
						str = str.TrimEnd(new char[]
						{
							')'
						});
						string[] array2 = str.Split(new char[]
						{
							','
						});
						float num = (float)ParseHelper.FromString(array2[0], typeof(float));
						float num2 = (float)ParseHelper.FromString(array2[1], typeof(float));
						float num3 = (float)ParseHelper.FromString(array2[2], typeof(float));
						bool flag = num > 1f || num3 > 1f || num2 > 1f;
						float num4 = (float)((!flag) ? 1 : 255);
						if (array2.Length == 4)
						{
							num4 = (float)ParseHelper.FromString(array2[3], typeof(float));
						}
						Color color;
						if (!flag)
						{
							color.r = num;
							color.g = num2;
							color.b = num3;
							color.a = num4;
						}
						else
						{
							color = GenColor.FromBytes(Mathf.RoundToInt(num), Mathf.RoundToInt(num2), Mathf.RoundToInt(num3), Mathf.RoundToInt(num4));
						}
						result = color;
					}
					else if (itemType == typeof(PublishedFileId_t))
					{
						result = new PublishedFileId_t(ulong.Parse(str));
					}
					else if (itemType == typeof(IntVec2))
					{
						result = IntVec2.FromString(str);
					}
					else if (itemType == typeof(IntVec3))
					{
						result = IntVec3.FromString(str);
					}
					else if (itemType == typeof(Rot4))
					{
						result = Rot4.FromString(str);
					}
					else
					{
						if (itemType != typeof(CellRect))
						{
							if (itemType == typeof(NameTriple))
							{
								NameTriple nameTriple = NameTriple.FromString(str);
								nameTriple.ResolveMissingPieces(null);
							}
							else
							{
								if (itemType == typeof(FloatRange))
								{
									result = FloatRange.FromString(str);
									return result;
								}
								if (itemType == typeof(IntRange))
								{
									result = IntRange.FromString(str);
									return result;
								}
								if (itemType == typeof(QualityRange))
								{
									result = QualityRange.FromString(str);
									return result;
								}
								if (itemType == typeof(ColorInt))
								{
									str = str.TrimStart(new char[]
									{
										'(',
										'R',
										'G',
										'B',
										'A'
									});
									str = str.TrimEnd(new char[]
									{
										')'
									});
									string[] array3 = str.Split(new char[]
									{
										','
									});
									ColorInt colorInt = new ColorInt(255, 255, 255, 255);
									colorInt.r = (int)ParseHelper.FromString(array3[0], typeof(int));
									colorInt.g = (int)ParseHelper.FromString(array3[1], typeof(int));
									colorInt.b = (int)ParseHelper.FromString(array3[2], typeof(int));
									if (array3.Length == 4)
									{
										colorInt.a = (int)ParseHelper.FromString(array3[3], typeof(int));
									}
									else
									{
										colorInt.a = 255;
									}
									result = colorInt;
									return result;
								}
							}
							throw new ArgumentException(string.Concat(new string[]
							{
								"Trying to parse to unknown data type ",
								itemType.Name,
								". Content is '",
								str,
								"'."
							}));
						}
						result = CellRect.FromString(str);
					}
				}
			}
			catch (Exception innerException2)
			{
				ArgumentException ex2 = new ArgumentException(string.Concat(new object[]
				{
					"Exception parsing ",
					itemType,
					" from \"",
					str,
					"\""
				}), innerException2);
				throw ex2;
			}
			return result;
		}

		public static bool HandlesType(Type type)
		{
			return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(IntVec3) || type == typeof(IntVec2) || type == typeof(Type) || type == typeof(Action) || type == typeof(Vector3) || type == typeof(Vector2) || type == typeof(Rect) || type == typeof(Color) || type == typeof(PublishedFileId_t) || type == typeof(Rot4) || type == typeof(CellRect) || type == typeof(NameTriple) || type == typeof(FloatRange) || type == typeof(IntRange) || type == typeof(QualityRange) || type == typeof(ColorInt);
		}

		private static Vector3 FromStringVector3(string Str)
		{
			Str = Str.TrimStart(new char[]
			{
				'('
			});
			Str = Str.TrimEnd(new char[]
			{
				')'
			});
			string[] array = Str.Split(new char[]
			{
				','
			});
			float x = Convert.ToSingle(array[0]);
			float y = Convert.ToSingle(array[1]);
			float z = Convert.ToSingle(array[2]);
			return new Vector3(x, y, z);
		}

		private static Vector2 FromStringVector2(string Str)
		{
			Str = Str.TrimStart(new char[]
			{
				'('
			});
			Str = Str.TrimEnd(new char[]
			{
				')'
			});
			string[] array = Str.Split(new char[]
			{
				','
			});
			float x;
			float y;
			if (array.Length == 1)
			{
				y = (x = Convert.ToSingle(array[0]));
			}
			else
			{
				if (array.Length != 2)
				{
					throw new InvalidOperationException();
				}
				x = Convert.ToSingle(array[0]);
				y = Convert.ToSingle(array[1]);
			}
			return new Vector2(x, y);
		}

		public static Rect FromStringRect(string str)
		{
			str = str.TrimStart(new char[]
			{
				'('
			});
			str = str.TrimEnd(new char[]
			{
				')'
			});
			string[] array = str.Split(new char[]
			{
				','
			});
			float x = Convert.ToSingle(array[0]);
			float y = Convert.ToSingle(array[1]);
			float width = Convert.ToSingle(array[2]);
			float height = Convert.ToSingle(array[3]);
			return new Rect(x, y, width, height);
		}
	}
}
