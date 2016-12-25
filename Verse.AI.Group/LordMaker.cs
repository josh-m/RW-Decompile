using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse.AI.Group
{
	public static class LordMaker
	{
		public static Lord MakeNewLord(Faction faction, LordJob lordJob, Map map, IEnumerable<Pawn> startingPawns = null)
		{
			if (faction == null)
			{
				Log.Warning("Tried to create a lord with null faction.");
				return null;
			}
			if (map == null)
			{
				Log.Warning("Tried to create a lord with null map.");
				return null;
			}
			Lord lord = new Lord();
			lord.loadID = Find.World.uniqueIDsManager.GetNextLordID();
			lord.faction = faction;
			map.lordManager.AddLord(lord);
			lord.SetJob(lordJob);
			lord.GotoToil(lord.Graph.StartingToil);
			if (startingPawns != null)
			{
				foreach (Pawn current in startingPawns)
				{
					lord.AddPawn(current);
				}
			}
			return lord;
		}
	}
}
