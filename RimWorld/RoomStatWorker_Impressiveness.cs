using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_Impressiveness : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			float factor = this.GetFactor(room.GetStat(RoomStatDefOf.Wealth) / 3500f);
			float factor2 = this.GetFactor(room.GetStat(RoomStatDefOf.Beauty) / 6f);
			float factor3 = this.GetFactor(room.GetStat(RoomStatDefOf.Space) / 125f);
			float factor4 = this.GetFactor(1f + room.GetStat(RoomStatDefOf.Cleanliness) / 2.5f);
			float num = Mathf.Min(factor, Mathf.Min(factor2, Mathf.Min(factor3, factor4)));
			float num2 = (factor + factor2 + factor3 + factor4) / 4f;
			num2 = (num2 + num) / 2f;
			return num2 * 100f;
		}

		private float GetFactor(float baseFactor)
		{
			if (Mathf.Abs(baseFactor) < 1f)
			{
				return baseFactor;
			}
			if (baseFactor > 0f)
			{
				return 1f + Mathf.Log(baseFactor);
			}
			return -1f - Mathf.Log(-baseFactor);
		}
	}
}
