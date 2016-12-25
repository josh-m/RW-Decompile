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

		public static void ExposeBoolArray(ref bool[] arr, string label)
		{
			int num = (int)Math.Ceiling((double)((float)(Find.Map.Size.x * Find.Map.Size.z) / 6f));
			byte[] array = new byte[num];
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				int num2 = 0;
				byte b = 1;
				for (int i = 0; i < CellIndices.NumGridCells; i++)
				{
					if (arr[i])
					{
						byte[] expr_65_cp_0 = array;
						int expr_65_cp_1 = num2;
						expr_65_cp_0[expr_65_cp_1] |= b;
					}
					b *= 2;
					if (b > 32)
					{
						b = 1;
						num2++;
					}
				}
			}
			ArrayExposeUtility.ExposeByteArray(ref array, label);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				int num3 = 0;
				byte b2 = 1;
				for (int j = 0; j < CellIndices.NumGridCells; j++)
				{
					if (arr == null)
					{
						arr = new bool[CellIndices.NumGridCells];
					}
					arr[j] = ((array[num3] & b2) != 0);
					b2 *= 2;
					if (b2 > 32)
					{
						b2 = 1;
						num3++;
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
