using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	public static class CaravanSettleUtility
	{
		private const int MinStartingLocCellsCount = 600;

		public static void Settle(Caravan caravan)
		{
			Faction faction = caravan.Faction;
			if (faction != Faction.OfPlayer)
			{
				Log.Error("Cannot settle with non-player faction.");
				return;
			}
			FactionBase newHome = SettleUtility.AddNewHome(caravan.Tile, faction);
			LongEventHandler.QueueLongEvent(delegate
			{
				Map visibleMap = MapGenerator.GenerateMap(Find.World.info.initialMapSize, caravan.Tile, newHome, null, null);
				Current.Game.VisibleMap = visibleMap;
			}, "GeneratingMap", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap));
			LongEventHandler.QueueLongEvent(delegate
			{
				Map map = newHome.Map;
				Pawn pawn = caravan.PawnsListForReading[0];
				Predicate<IntVec3> extraCellValidator = (IntVec3 x) => x.GetRegion(map).CellCount >= 600;
				CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Center, CaravanDropInventoryMode.DropInstantly, false, extraCellValidator);
				Find.CameraDriver.JumpTo(pawn.Position);
				Find.MainTabsRoot.EscapeCurrentTab(false);
			}, "SpawningColonists", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap));
		}

		public static Command SettleCommand(Caravan caravan)
		{
			Command_Settle command_Settle = new Command_Settle();
			command_Settle.defaultLabel = "CommandSettle".Translate();
			command_Settle.defaultDesc = "CommandSettleDesc".Translate();
			command_Settle.icon = SettleUtility.SettleCommandTex;
			command_Settle.action = delegate
			{
				SoundDefOf.TickHigh.PlayOneShotOnCamera();
				CaravanSettleUtility.Settle(caravan);
			};
			bool flag = false;
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				WorldObject worldObject = allWorldObjects[i];
				if (worldObject.Tile == caravan.Tile && worldObject != caravan)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				command_Settle.Disable("CommandSettleFailOtherWorldObjectsHere".Translate());
			}
			else if (SettleUtility.PlayerHomesCountLimitReached)
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
	}
}
