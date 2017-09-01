using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_HostilityResponse : PawnColumnWorker
	{
		private const int TopPadding = 3;

		private const int Width = 24;

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (!pawn.RaceProps.Humanlike)
			{
				return;
			}
			Vector2 pos = new Vector2(rect.x, rect.y + 3f);
			HostilityResponseModeUtility.DrawResponseButton(pos, pawn);
		}

		public override int GetMinCellHeight(Pawn pawn)
		{
			return Mathf.Max(base.GetMinCellHeight(pawn), Mathf.CeilToInt(24f) + 3);
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 24);
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return Mathf.Min(base.GetMaxWidth(table), this.GetMinWidth(table));
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return this.GetValueToCompare(a).CompareTo(this.GetValueToCompare(b));
		}

		private int GetValueToCompare(Pawn pawn)
		{
			if (pawn.playerSettings == null)
			{
				return -2147483648;
			}
			return (int)pawn.playerSettings.hostilityResponse;
		}
	}
}
