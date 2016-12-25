using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_GraveVisitingJoyGainFactor : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			float stat = room.GetStat(RoomStatDefOf.Impressiveness);
			return Mathf.Lerp(0.8f, 1.2f, Mathf.InverseLerp(-150f, 150f, stat));
		}
	}
}
