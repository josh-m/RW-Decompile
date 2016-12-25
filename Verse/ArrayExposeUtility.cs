using System;
using System.Text;

namespace Verse
{
	public static class ArrayExposeUtility
	{
		private const int NewlineInterval = 100;

		public static void ExposeByteArray(ref byte[] arr, string label)
		{
			string text = null;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				text = Convert.ToBase64String(arr);
				text = ArrayExposeUtility.AddLineBreaksToLongString(text);
			}
			Scribe_Values.LookValue<string>(ref text, label, null, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				text = ArrayExposeUtility.RemoveLineBreaks(text);
				arr = Convert.FromBase64String(text);
			}
		}

		public static void ExposeBoolArray(ref bool[] arr, int mapSizeX, int mapSizeZ, string label)
		{
			int num = mapSizeX * mapSizeZ;
			int num2 = (int)Math.Ceiling((double)((float)num / 6f));
			byte[] array = new byte[num2];
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				int num3 = 0;
				byte b = 1;
				for (int i = 0; i < num; i++)
				{
					if (arr[i])
					{
						byte[] expr_44_cp_0 = array;
						int expr_44_cp_1 = num3;
						expr_44_cp_0[expr_44_cp_1] |= b;
					}
					b *= 2;
					if (b > 32)
					{
						b = 1;
						num3++;
					}
				}
			}
			ArrayExposeUtility.ExposeByteArray(ref array, label);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				int num4 = 0;
				byte b2 = 1;
				for (int j = 0; j < num; j++)
				{
					if (arr == null)
					{
						arr = new bool[num];
					}
					arr[j] = ((array[num4] & b2) != 0);
					b2 *= 2;
					if (b2 > 32)
					{
						b2 = 1;
						num4++;
					}
				}
			}
		}

		public static string AddLineBreaksToLongString(string str)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine();
			for (int i = 0; i < str.Length; i++)
			{
				stringBuilder.Append(str[i]);
				if (i % 100 == 0)
				{
					stringBuilder.AppendLine();
				}
			}
			stringBuilder.AppendLine();
			return stringBuilder.ToString();
		}

		public static string RemoveLineBreaks(string str)
		{
			return str.Replace("\n", string.Empty);
		}
	}
}
