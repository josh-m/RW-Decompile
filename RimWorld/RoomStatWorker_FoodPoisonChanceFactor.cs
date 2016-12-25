using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_FoodPoisonChanceFactor : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			float stat = room.GetStat(RoomStatDefOf.Cleanliness);
			float value = 1f / GenMath.UnboundedValueToFactor(stat * 0.21f);
			return Mathf.Clamp(value, 0.7f, 1.6f);
		}
	}
}
