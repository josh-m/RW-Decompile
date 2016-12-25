using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_SurgerySuccessChanceFactor : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			float stat = room.GetStat(RoomStatDefOf.Cleanliness);
			float value = GenMath.UnboundedValueToFactor(stat * 0.05f);
			return Mathf.Clamp(value, 0.6f, 1.5f);
		}
	}
}
