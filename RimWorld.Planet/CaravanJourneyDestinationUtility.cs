using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class CaravanJourneyDestinationUtility
	{
		private static readonly Texture2D TakeOffCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true);

		public static bool AnyJurneyDestinationAt(int tile)
		{
			return CaravanJourneyDestinationUtility.JurneyDestinationAt(tile) != null;
		}

		public static Command TakeOffCommand(int tile)
		{
			return new Command_Action
			{
				defaultLabel = "CommandTakeOff".Translate(),
				defaultDesc = "CommandTakeOffDesc".Translate(),
				icon = CaravanJourneyDestinationUtility.TakeOffCommandTex,
				action = delegate
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CommandTakeOffConfirmation".Translate(), delegate
					{
						CaravanJourneyDestinationUtility.TakeOff(tile);
					}, false, null));
				}
			};
		}

		public static WorldObject JurneyDestinationAt(int tile)
		{
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				WorldObject worldObject = allWorldObjects[i];
				if (worldObject.Tile == tile && worldObject.def == WorldObjectDefOf.JourneyDestination)
				{
					return worldObject;
				}
			}
			return null;
		}

		public static void PlayerCaravansAt(int tile, List<Caravan> outCaravans)
		{
			outCaravans.Clear();
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan = caravans[i];
				if (caravan.Tile == tile && caravan.Faction == Faction.OfPlayer)
				{
					outCaravans.Add(caravan);
				}
			}
		}

		private static void TakeOff(int tile)
		{
			ShipCountdown.InitiateCountdown(null, tile);
		}
	}
}
