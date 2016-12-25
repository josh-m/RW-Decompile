using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ColonistBarDrawLocsFinder
	{
		private const float MarginTop = 21f;

		private List<int> entriesInGroup = new List<int>();

		private List<int> horizontalSlotsPerGroup = new List<int>();

		private ColonistBar ColonistBar
		{
			get
			{
				return Find.ColonistBar;
			}
		}

		private static float MaxColonistBarWidth
		{
			get
			{
				return (float)UI.screenWidth - 520f;
			}
		}

		public void CalculateDrawLocs(List<Vector2> outDrawLocs, out float scale)
		{
			if (this.ColonistBar.Entries.Count == 0)
			{
				outDrawLocs.Clear();
				scale = 1f;
				return;
			}
			this.CalculateColonistsInGroup();
			bool onlyOneRow;
			int maxPerGlobalRow;
			scale = this.FindBestScale(out onlyOneRow, out maxPerGlobalRow);
			this.CalculateDrawLocs(outDrawLocs, scale, onlyOneRow, maxPerGlobalRow);
		}

		private void CalculateColonistsInGroup()
		{
			this.entriesInGroup.Clear();
			List<ColonistBar.Entry> entries = this.ColonistBar.Entries;
			int num = this.CalculateGroupsCount();
			for (int i = 0; i < num; i++)
			{
				this.entriesInGroup.Add(0);
			}
			for (int j = 0; j < entries.Count; j++)
			{
				List<int> list;
				List<int> expr_49 = list = this.entriesInGroup;
				int num2;
				int expr_5C = num2 = entries[j].group;
				num2 = list[num2];
				expr_49[expr_5C] = num2 + 1;
			}
		}

		private int CalculateGroupsCount()
		{
			List<ColonistBar.Entry> entries = this.ColonistBar.Entries;
			int num = -1;
			int num2 = 0;
			for (int i = 0; i < entries.Count; i++)
			{
				if (num != entries[i].group)
				{
					num2++;
					num = entries[i].group;
				}
			}
			return num2;
		}

		private float FindBestScale(out bool onlyOneRow, out int maxPerGlobalRow)
		{
			float num = 1f;
			List<ColonistBar.Entry> entries = this.ColonistBar.Entries;
			int num2 = this.CalculateGroupsCount();
			while (true)
			{
				float num3 = (ColonistBar.BaseSize.x + 24f) * num;
				float num4 = ColonistBarDrawLocsFinder.MaxColonistBarWidth - (float)(num2 - 1) * 25f * num;
				maxPerGlobalRow = Mathf.FloorToInt(num4 / num3);
				onlyOneRow = true;
				if (this.TryDistributeHorizontalSlotsBetweenGroups(maxPerGlobalRow))
				{
					int allowedRowsCountForScale = ColonistBarDrawLocsFinder.GetAllowedRowsCountForScale(num);
					bool flag = true;
					int num5 = -1;
					for (int i = 0; i < entries.Count; i++)
					{
						if (num5 != entries[i].group)
						{
							num5 = entries[i].group;
							int num6 = Mathf.CeilToInt((float)this.entriesInGroup[entries[i].group] / (float)this.horizontalSlotsPerGroup[entries[i].group]);
							if (num6 > 1)
							{
								onlyOneRow = false;
							}
							if (num6 > allowedRowsCountForScale)
							{
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
				num *= 0.95f;
			}
			return num;
		}

		private bool TryDistributeHorizontalSlotsBetweenGroups(int maxPerGlobalRow)
		{
			int num = this.CalculateGroupsCount();
			this.horizontalSlotsPerGroup.Clear();
			for (int k = 0; k < num; k++)
			{
				this.horizontalSlotsPerGroup.Add(0);
			}
			GenMath.DHondtDistribution(this.horizontalSlotsPerGroup, (int i) => (float)this.entriesInGroup[i], maxPerGlobalRow);
			for (int j = 0; j < this.horizontalSlotsPerGroup.Count; j++)
			{
				if (this.horizontalSlotsPerGroup[j] == 0)
				{
					int num2 = this.horizontalSlotsPerGroup.Max();
					if (num2 <= 1)
					{
						return false;
					}
					int num3 = this.horizontalSlotsPerGroup.IndexOf(num2);
					List<int> list;
					List<int> expr_89 = list = this.horizontalSlotsPerGroup;
					int num4;
					int expr_8E = num4 = num3;
					num4 = list[num4];
					expr_89[expr_8E] = num4 - 1;
					List<int> list2;
					List<int> expr_AB = list2 = this.horizontalSlotsPerGroup;
					int expr_AF = num4 = j;
					num4 = list2[num4];
					expr_AB[expr_AF] = num4 + 1;
				}
			}
			return true;
		}

		private static int GetAllowedRowsCountForScale(float scale)
		{
			if (scale > 0.58f)
			{
				return 1;
			}
			if (scale > 0.42f)
			{
				return 2;
			}
			return 3;
		}

		private void CalculateDrawLocs(List<Vector2> outDrawLocs, float scale, bool onlyOneRow, int maxPerGlobalRow)
		{
			outDrawLocs.Clear();
			int num = maxPerGlobalRow;
			if (onlyOneRow)
			{
				for (int i = 0; i < this.horizontalSlotsPerGroup.Count; i++)
				{
					this.horizontalSlotsPerGroup[i] = Mathf.Min(this.horizontalSlotsPerGroup[i], this.entriesInGroup[i]);
				}
				num = this.ColonistBar.Entries.Count;
			}
			int num2 = this.CalculateGroupsCount();
			float num3 = (ColonistBar.BaseSize.x + 24f) * scale;
			float num4 = (float)num * num3 + (float)(num2 - 1) * 25f * scale;
			List<ColonistBar.Entry> entries = this.ColonistBar.Entries;
			int num5 = -1;
			int num6 = -1;
			float num7 = ((float)UI.screenWidth - num4) / 2f;
			for (int j = 0; j < entries.Count; j++)
			{
				if (num5 != entries[j].group)
				{
					if (num5 >= 0)
					{
						num7 += 25f * scale;
						num7 += (float)this.horizontalSlotsPerGroup[num5] * scale * (ColonistBar.BaseSize.x + 24f);
					}
					num6 = 0;
					num5 = entries[j].group;
				}
				else
				{
					num6++;
				}
				Vector2 drawLoc = this.GetDrawLoc(num7, 21f, entries[j].group, num6, scale);
				outDrawLocs.Add(drawLoc);
			}
		}

		private Vector2 GetDrawLoc(float groupStartX, float groupStartY, int group, int numInGroup, float scale)
		{
			float num = groupStartX + (float)(numInGroup % this.horizontalSlotsPerGroup[group]) * scale * (ColonistBar.BaseSize.x + 24f);
			float y = groupStartY + (float)(numInGroup / this.horizontalSlotsPerGroup[group]) * scale * (ColonistBar.BaseSize.y + 32f);
			bool flag = numInGroup >= this.entriesInGroup[group] - this.entriesInGroup[group] % this.horizontalSlotsPerGroup[group];
			if (flag)
			{
				int num2 = this.horizontalSlotsPerGroup[group] - this.entriesInGroup[group] % this.horizontalSlotsPerGroup[group];
				num += (float)num2 * scale * (ColonistBar.BaseSize.x + 24f) * 0.5f;
			}
			return new Vector2(num, y);
		}
	}
}
