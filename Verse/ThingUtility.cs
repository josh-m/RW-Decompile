using RimWorld.Planet;
using System;
using UnityEngine;

namespace Verse
{
	public static class ThingUtility
	{
		public static bool DestroyedOrNull(this Thing t)
		{
			return t == null || t.Destroyed;
		}

		public static void DestroyOrPassToWorld(this Thing t, DestroyMode mode = DestroyMode.Vanish)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				if (!Find.WorldPawns.Contains(pawn))
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
				}
			}
			else
			{
				t.Destroy(mode);
			}
		}

		public static int TryAbsorbStackNumToTake(Thing thing, Thing other, bool respectStackLimit)
		{
			int result;
			if (respectStackLimit)
			{
				result = Mathf.Min(other.stackCount, thing.def.stackLimit - thing.stackCount);
			}
			else
			{
				result = other.stackCount;
			}
			return result;
		}
	}
}
