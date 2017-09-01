using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class Dialog_PawnTableTest : Window
	{
		private const int TableTitleHeight = 30;

		private PawnColumnDef singleColumn;

		private PawnTable pawnTableMin;

		private PawnTable pawnTableOptimal;

		private PawnTable pawnTableMax;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2((float)UI.screenWidth, (float)UI.screenHeight);
			}
		}

		private List<Pawn> Pawns
		{
			get
			{
				return Find.VisibleMap.mapPawns.PawnsInFaction(Faction.OfPlayer).ToList<Pawn>();
			}
		}

		public Dialog_PawnTableTest(PawnColumnDef singleColumn)
		{
			this.singleColumn = singleColumn;
		}

		public override void DoWindowContents(Rect inRect)
		{
			int num = ((int)inRect.height - 90) / 3;
			if (this.pawnTableMin == null)
			{
				this.pawnTableMin = new PawnTable(Gen.YieldSingle<PawnColumnDef>(this.singleColumn), () => this.Pawns, 0, 0, 0, 0);
				this.pawnTableMin.SetMinMaxSize(Mathf.Min(this.singleColumn.Worker.GetMinWidth(this.pawnTableMin) + 16, (int)inRect.width), Mathf.Min(this.singleColumn.Worker.GetMinWidth(this.pawnTableMin) + 16, (int)inRect.width), 0, num);
			}
			if (this.pawnTableOptimal == null)
			{
				this.pawnTableOptimal = new PawnTable(Gen.YieldSingle<PawnColumnDef>(this.singleColumn), () => this.Pawns, 0, 0, 0, 0);
				this.pawnTableOptimal.SetMinMaxSize(Mathf.Min(this.singleColumn.Worker.GetOptimalWidth(this.pawnTableOptimal) + 16, (int)inRect.width), Mathf.Min(this.singleColumn.Worker.GetOptimalWidth(this.pawnTableOptimal) + 16, (int)inRect.width), 0, num);
			}
			if (this.pawnTableMax == null)
			{
				this.pawnTableMax = new PawnTable(Gen.YieldSingle<PawnColumnDef>(this.singleColumn), () => this.Pawns, 0, 0, 0, 0);
				this.pawnTableMax.SetMinMaxSize(Mathf.Min(this.singleColumn.Worker.GetMaxWidth(this.pawnTableMax) + 16, (int)inRect.width), Mathf.Min(this.singleColumn.Worker.GetMaxWidth(this.pawnTableMax) + 16, (int)inRect.width), 0, num);
			}
			int num2 = 0;
			Text.Font = GameFont.Small;
			GUI.color = Color.gray;
			Widgets.Label(new Rect(0f, (float)num2, inRect.width, 30f), "Min size");
			GUI.color = Color.white;
			num2 += 30;
			this.pawnTableMin.PawnTableOnGUI(new Vector2(0f, (float)num2));
			num2 += num;
			GUI.color = Color.gray;
			Widgets.Label(new Rect(0f, (float)num2, inRect.width, 30f), "Optimal size");
			GUI.color = Color.white;
			num2 += 30;
			this.pawnTableOptimal.PawnTableOnGUI(new Vector2(0f, (float)num2));
			num2 += num;
			GUI.color = Color.gray;
			Widgets.Label(new Rect(0f, (float)num2, inRect.width, 30f), "Max size");
			GUI.color = Color.white;
			num2 += 30;
			this.pawnTableMax.PawnTableOnGUI(new Vector2(0f, (float)num2));
			num2 += num;
		}
	}
}
