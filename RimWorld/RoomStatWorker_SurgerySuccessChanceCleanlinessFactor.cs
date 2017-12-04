using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_SurgerySuccessChanceCleanlinessFactor : RoomStatWorker
	{
		private const float MinFactor = 0.6f;

		private const float MaxFactor = 1.5f;

		private const float CleanlinessInfluence = 0.05f;

		public override float GetScore(Room room)
		{
			float stat = room.GetStat(RoomStatDefOf.Cleanliness);
			float value = GenMath.LerpDouble(-5f, 5f, 0.6f, 1.5f, stat);
			return Mathf.Clamp(value, 0.6f, 1.5f);
		}
	}
}
