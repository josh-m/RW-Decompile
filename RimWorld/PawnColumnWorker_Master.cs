using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Master : PawnColumnWorker
	{
		protected override GameFont DefaultHeaderFont
		{
			get
			{
				return GameFont.Tiny;
			}
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 100);
		}

		public override int GetOptimalWidth(PawnTable table)
		{
			return Mathf.Clamp(170, this.GetMinWidth(table), this.GetMaxWidth(table));
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (!this.CanAssignMaster(pawn))
			{
				return;
			}
			Rect rect2 = rect.ContractedBy(2f);
			string label = TrainableUtility.MasterString(pawn).Truncate(rect2.width, null);
			if (Widgets.ButtonText(rect2, label, true, false, true))
			{
				TrainableUtility.OpenMasterSelectMenu(pawn);
			}
		}

		public override int Compare(Pawn a, Pawn b)
		{
			int valueToCompare = this.GetValueToCompare1(a);
			int valueToCompare2 = this.GetValueToCompare1(b);
			if (valueToCompare != valueToCompare2)
			{
				return valueToCompare.CompareTo(valueToCompare2);
			}
			return this.GetValueToCompare2(a).CompareTo(this.GetValueToCompare2(b));
		}

		private bool CanAssignMaster(Pawn pawn)
		{
			return pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && pawn.training.IsCompleted(TrainableDefOf.Obedience);
		}

		private int GetValueToCompare1(Pawn pawn)
		{
			if (!this.CanAssignMaster(pawn))
			{
				return 0;
			}
			if (pawn.playerSettings.master == null)
			{
				return 1;
			}
			return 2;
		}

		private string GetValueToCompare2(Pawn pawn)
		{
			if (pawn.playerSettings != null && pawn.playerSettings.master != null)
			{
				return pawn.playerSettings.master.Label;
			}
			return string.Empty;
		}
	}
}
