using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_InfectionChanceFactor : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			float stat = room.GetStat(RoomStatDefOf.Cleanliness);
			float value = 1f / GenMath.UnboundedValueToFactor(stat * 0.12f);
			return Mathf.Clamp(value, 0.6f, 1.5f);
		}
	}
}
