using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class CaravanMergeUtility
	{
		private static readonly Texture2D MergeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/MergeCaravans", true);

		private static List<Caravan> tmpSelectedPlayerCaravans = new List<Caravan>();

		private static List<Caravan> tmpCaravansOnSameTile = new List<Caravan>();

		public static bool CanMergeAnySelectedCaravans
		{
			get
			{
				List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
				for (int i = 0; i < selectedObjects.Count; i++)
				{
					Caravan caravan = selectedObjects[i] as Caravan;
					if (caravan != null && caravan.IsPlayerControlled)
					{
						for (int j = i + 1; j < selectedObjects.Count; j++)
						{
							Caravan caravan2 = selectedObjects[j] as Caravan;
							if (caravan2 != null && caravan2.IsPlayerControlled && CaravanMergeUtility.CloseToEachOther(caravan, caravan2))
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		public static Command MergeCommand(Caravan caravan)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandMergeCaravans".Translate();
			command_Action.defaultDesc = "CommandMergeCaravansDesc".Translate();
			command_Action.icon = CaravanMergeUtility.MergeCommandTex;
			command_Action.action = delegate
			{
				CaravanMergeUtility.TryMergeSelectedCaravans();
				SoundDefOf.TickHigh.PlayOneShotOnCamera();
			};
			return command_Action;
		}

		public static void TryMergeSelectedCaravans()
		{
			CaravanMergeUtility.tmpSelectedPlayerCaravans.Clear();
			List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
			for (int i = 0; i < selectedObjects.Count; i++)
			{
				Caravan caravan = selectedObjects[i] as Caravan;
				if (caravan != null && caravan.IsPlayerControlled)
				{
					CaravanMergeUtility.tmpSelectedPlayerCaravans.Add(caravan);
				}
			}
			while (CaravanMergeUtility.tmpSelectedPlayerCaravans.Any<Caravan>())
			{
				Caravan caravan2 = CaravanMergeUtility.tmpSelectedPlayerCaravans[0];
				CaravanMergeUtility.tmpSelectedPlayerCaravans.RemoveAt(0);
				CaravanMergeUtility.tmpCaravansOnSameTile.Clear();
				CaravanMergeUtility.tmpCaravansOnSameTile.Add(caravan2);
				for (int j = CaravanMergeUtility.tmpSelectedPlayerCaravans.Count - 1; j >= 0; j--)
				{
					if (CaravanMergeUtility.CloseToEachOther(CaravanMergeUtility.tmpSelectedPlayerCaravans[j], caravan2))
					{
						CaravanMergeUtility.tmpCaravansOnSameTile.Add(CaravanMergeUtility.tmpSelectedPlayerCaravans[j]);
						CaravanMergeUtility.tmpSelectedPlayerCaravans.RemoveAt(j);
					}
				}
				if (CaravanMergeUtility.tmpCaravansOnSameTile.Count >= 2)
				{
					CaravanMergeUtility.MergeCaravans(CaravanMergeUtility.tmpCaravansOnSameTile);
				}
			}
		}

		private static bool CloseToEachOther(Caravan c1, Caravan c2)
		{
			if (c1.Tile == c2.Tile)
			{
				return true;
			}
			Vector3 drawPos = c1.DrawPos;
			Vector3 drawPos2 = c2.DrawPos;
			float num = Find.WorldGrid.averageTileSize * 0.5f;
			return (drawPos - drawPos2).sqrMagnitude < num * num;
		}

		private static void MergeCaravans(List<Caravan> caravans)
		{
			Caravan caravan = caravans.MaxBy((Caravan x) => x.PawnsListForReading.Count);
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan2 = caravans[i];
				if (caravan2 != caravan)
				{
					List<Pawn> pawnsListForReading = caravan2.PawnsListForReading;
					for (int j = 0; j < pawnsListForReading.Count; j++)
					{
						caravan.AddPawn(pawnsListForReading[j], true);
					}
					caravan2.RemoveAllPawns();
					Find.WorldObjects.Remove(caravan2);
				}
			}
		}
	}
}
