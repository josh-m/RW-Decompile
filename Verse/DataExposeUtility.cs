using System;
using System.Text;

namespace Verse
{
	public static class DataExposeUtility
	{
		private const int NewlineInterval = 100;

		public static void ByteArray(ref byte[] arr, string label)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (arr != null)
				{
					byte[] array = CompressUtility.Compress(arr);
					if (array.Length < arr.Length)
					{
						string text = DataExposeUtility.AddLineBreaksToLongString(Convert.ToBase64String(array));
						Scribe_Values.Look<string>(ref text, label + "Deflate", null, false);
					}
					else
					{
						string text2 = DataExposeUtility.AddLineBreaksToLongString(Convert.ToBase64String(arr));
						Scribe_Values.Look<string>(ref text2, label, null, false);
					}
				}
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				string text3 = null;
				Scribe_Values.Look<string>(ref text3, label + "Deflate", null, false);
				if (text3 != null)
				{
					arr = CompressUtility.Decompress(Convert.FromBase64String(DataExposeUtility.RemoveLineBreaks(text3)));
				}
				else
				{
					Scribe_Values.Look<string>(ref text3, label, null, false);
					if (text3 != null)
					{
						arr = Convert.FromBase64String(DataExposeUtility.RemoveLineBreaks(text3));
					}
					else
					{
						arr = null;
					}
				}
			}
		}

		public static void BoolArray(ref bool[] arr, int elements, string label)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (arr.Length != elements)
				{
					Log.ErrorOnce(string.Format("Bool array length mismatch for {0}", label), 74135877, false);
				}
				elements = arr.Length;
			}
			int num = (elements + 7) / 8;
			byte[] array = null;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				array = new byte[num];
				int num2 = 0;
				byte b = 1;
				for (int i = 0; i < elements; i++)
				{
					if (arr[i])
					{
						byte[] expr_68_cp_0 = array;
						int expr_68_cp_1 = num2;
						expr_68_cp_0[expr_68_cp_1] |= b;
					}
					b *= 2;
					if (b == 0)
					{
						b = 1;
						num2++;
					}
				}
			}
			DataExposeUtility.ByteArray(ref array, label);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (arr == null)
				{
					arr = new bool[elements];
				}
				if (array != null && array.Length != 0)
				{
					if (array.Length != num)
					{
						int num3 = 0;
						byte b2 = 1;
						for (int j = 0; j < elements; j++)
						{
							arr[j] = ((array[num3] & b2) != 0);
							b2 *= 2;
							if (b2 > 32)
							{
								b2 = 1;
								num3++;
							}
						}
					}
					else
					{
						int num4 = 0;
						byte b3 = 1;
						for (int k = 0; k < elements; k++)
						{
							arr[k] = ((array[num4] & b3) != 0);
							b3 *= 2;
							if (b3 == 0)
							{
								b3 = 1;
								num4++;
							}
						}
					}
				}
			}
		}

		public static string AddLineBreaksToLongString(string str)
		{
			StringBuilder stringBuilder = new StringBuilder(str.Length + (str.Length / 100 + 3) * 2 + 1);
			stringBuilder.AppendLine();
			for (int i = 0; i < str.Length; i++)
			{
				if (i % 100 == 0 && i != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(str[i]);
			}
			stringBuilder.AppendLine();
			return stringBuilder.ToString();
		}

		public static string RemoveLineBreaks(string str)
		{
			return str.Replace("\n", string.Empty).Replace("\r", string.Empty);
		}
	}
}
