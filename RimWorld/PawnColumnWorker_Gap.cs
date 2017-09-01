using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Gap : PawnColumnWorker
	{
		protected virtual int Width
		{
			get
			{
				return this.def.gap;
			}
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), this.Width);
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return Mathf.Min(base.GetMaxWidth(table), this.Width);
		}

		public override int GetMinCellHeight(Pawn pawn)
		{
			return 0;
		}
	}
}
