using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class Dialog_DebugTables : Window
	{
		private const float RowHeight = 23f;

		private const float ColExtraWidth = 8f;

		private string[,] table;

		private Vector2 scrollPosition = Vector2.zero;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2((float)UI.screenWidth, (float)UI.screenHeight);
			}
		}

		public Dialog_DebugTables(string[,] tables)
		{
			this.table = tables;
			this.doCloseButton = true;
			this.doCloseX = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Tiny;
			inRect.yMax -= 40f;
			Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, (float)this.table.GetLength(1) * 23f);
			Widgets.BeginScrollView(inRect, ref this.scrollPosition, viewRect);
			List<float> list = new List<float>();
			for (int i = 0; i < this.table.GetLength(0); i++)
			{
				float num = 0f;
				for (int j = 0; j < this.table.GetLength(1); j++)
				{
					string text = this.table[i, j];
					float x = Text.CalcSize(text).x;
					if (x > num)
					{
						num = x;
					}
				}
				list.Add(num + 8f);
			}
			float num2 = 0f;
			for (int k = 0; k < this.table.GetLength(0); k++)
			{
				for (int l = 0; l < this.table.GetLength(1); l++)
				{
					Rect rect = new Rect(num2, (float)l * 23f, list[k], 23f);
					Rect rect2 = rect;
					rect2.xMin -= 999f;
					rect2.xMax += 999f;
					if (Mouse.IsOver(rect2) || k % 2 == 0)
					{
						Widgets.DrawHighlight(rect);
					}
					Widgets.Label(rect, this.table[k, l]);
				}
				num2 += list[k];
			}
			Widgets.EndScrollView();
		}
	}
}
