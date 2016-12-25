using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	public static class SettleInExistingMapUtility
	{
		private static List<Pawn> tmpPlayerPawns = new List<Pawn>();

		public static Command SettleCommand(Map map)
		{
			Command_Settle command_Settle = new Command_Settle();
			command_Settle.defaultLabel = "CommandSettle".Translate();
			command_Settle.defaultDesc = "CommandSettleDesc".Translate();
			command_Settle.icon = SettleUtility.SettleCommandTex;
			command_Settle.action = delegate
			{
				SoundDefOf.TickHigh.PlayOneShotOnCamera();
				SettleInExistingMapUtility.Settle(map);
			};
			if (SettleUtility.PlayerHomesCountLimitReached)
			{
				if (Prefs.MaxNumberOfPlayerHomes > 1)
				{
					command_Settle.Disable("CommandSettleFailReachedMaximumNumberOfBases".Translate());
				}
				else
				{
					command_Settle.Disable("CommandSettleFailAlreadyHaveBase".Translate());
				}
			}
			return command_Settle;
		}

		public static void Settle(Map map)
		{
			MapParent parent = map.info.parent;
			FactionBase factionBase = SettleUtility.AddNewHome(map.info.tile, Faction.OfPlayer);
			map.info.parent = factionBase;
			if (parent != null)
			{
				Find.WorldObjects.Remove(parent);
			}
			Messages.Message("MessageSettledInExistingMap".Translate(), factionBase, MessageSound.Benefit);
			SettleInExistingMapUtility.tmpPlayerPawns.Clear();
			SettleInExistingMapUtility.tmpPlayerPawns.AddRange(from x in map.mapPawns.AllPawnsSpawned
			where x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer
			select x);
			CaravanEnterMapUtility.DropAllInventory(SettleInExistingMapUtility.tmpPlayerPawns);
			SettleInExistingMapUtility.tmpPlayerPawns.Clear();
			List<Pawn> prisonersOfColonySpawned = map.mapPawns.PrisonersOfColonySpawned;
			for (int i = 0; i < prisonersOfColonySpawned.Count; i++)
			{
				prisonersOfColonySpawned[i].guest.WaitInsteadOfEscapingForDefaultTicks();
			}
		}
	}
}
