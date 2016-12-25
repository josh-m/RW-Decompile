using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class DebugTables
	{
		public static void MakeTablesDialog<T>(IEnumerable<T> dataSources, params TableDataGetter<T>[] getters)
		{
			List<TableDataGetter<T>> list = getters.ToList<TableDataGetter<T>>();
			int num = dataSources.Count<T>() + 1;
			int count = list.Count;
			string[,] array = new string[count, num];
			int num2 = 0;
			for (int i = 0; i < getters.Length; i++)
			{
				TableDataGetter<T> tableDataGetter = getters[i];
				array[num2, 0] = tableDataGetter.label;
				num2++;
			}
			int num3 = 1;
			foreach (T current in dataSources)
			{
				for (int j = 0; j < count; j++)
				{
					array[j, num3] = list[j].getter(current);
				}
				num3++;
			}
			Find.WindowStack.Add(new Dialog_DebugTables(array));
		}

		public static void MakeTablesDialog<TColumn, TRow>(IEnumerable<TColumn> colValues, Func<TColumn, string> colLabelFormatter, IEnumerable<TRow> rowValues, Func<TRow, string> rowLabelFormatter, Func<TColumn, TRow, string> func, string tlLabel = "")
		{
			int num = colValues.Count<TColumn>() + 1;
			int num2 = rowValues.Count<TRow>() + 1;
			string[,] array = new string[num, num2];
			array[0, 0] = tlLabel;
			int num3 = 1;
			foreach (TColumn current in colValues)
			{
				array[num3, 0] = colLabelFormatter(current);
				num3++;
			}
			int num4 = 1;
			foreach (TRow current2 in rowValues)
			{
				array[0, num4] = rowLabelFormatter(current2);
				num4++;
			}
			int num5 = 1;
			foreach (TRow current3 in rowValues)
			{
				int num6 = 1;
				foreach (TColumn current4 in colValues)
				{
					array[num6, num5] = func(current4, current3);
					num6++;
				}
				num5++;
			}
			Find.WindowStack.Add(new Dialog_DebugTables(array));
		}
	}
}
