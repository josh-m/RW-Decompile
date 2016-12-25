using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class ShipCountdown
	{
		private const float InitialTime = 7.2f;

		private static float timeLeft = -1000f;

		private static Building shipRoot;

		private static int journeyDestinationTile;

		private static List<Caravan> caravans = new List<Caravan>();

		public static bool CountingDown
		{
			get
			{
				return ShipCountdown.timeLeft >= 0f;
			}
		}

		public static void InitiateCountdown(Building launchingShipRoot, int journeyDestinationTile)
		{
			SoundDef.Named("ShipTakeoff").PlayOneShotOnCamera();
			ShipCountdown.shipRoot = launchingShipRoot;
			ShipCountdown.journeyDestinationTile = journeyDestinationTile;
			ShipCountdown.timeLeft = 7.2f;
			ScreenFader.StartFade(Color.white, 7.2f);
		}

		public static void ShipCountdownUpdate()
		{
			if (ShipCountdown.timeLeft > 0f)
			{
				ShipCountdown.timeLeft -= Time.deltaTime;
				if (ShipCountdown.timeLeft <= 0f)
				{
					ShipCountdown.CountdownEnded();
				}
			}
		}

		public static void CancelCountdown()
		{
			ShipCountdown.timeLeft = -1000f;
		}

		private static void CountdownEnded()
		{
			if (ShipCountdown.journeyDestinationTile >= 0)
			{
				CaravanJourneyDestinationUtility.PlayerCaravansAt(ShipCountdown.journeyDestinationTile, ShipCountdown.caravans);
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < ShipCountdown.caravans.Count; i++)
				{
					Caravan caravan = ShipCountdown.caravans[i];
					List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
					for (int j = 0; j < pawnsListForReading.Count; j++)
					{
						stringBuilder.Append("   ");
						stringBuilder.AppendLine(pawnsListForReading[j].LabelCap);
						pawnsListForReading[j].Destroy(DestroyMode.Vanish);
					}
					pawnsListForReading.Clear();
					Find.WorldObjects.Remove(caravan);
				}
				WorldObject worldObject = CaravanJourneyDestinationUtility.JurneyDestinationAt(ShipCountdown.journeyDestinationTile);
				if (worldObject != null)
				{
					Find.WorldObjects.Remove(worldObject);
				}
				string victoryText = "GameOverArrivedAtJourneyDestination".Translate(new object[]
				{
					stringBuilder.ToString(),
					GameVictoryUtility.PawnsLeftBehind()
				});
				GameVictoryUtility.ShowCredits(victoryText);
			}
			else
			{
				List<Building> list = ShipUtility.ShipBuildingsAttachedTo(ShipCountdown.shipRoot).ToList<Building>();
				StringBuilder stringBuilder2 = new StringBuilder();
				foreach (Building current in list)
				{
					Building_CryptosleepCasket building_CryptosleepCasket = current as Building_CryptosleepCasket;
					if (building_CryptosleepCasket != null && building_CryptosleepCasket.HasAnyContents)
					{
						stringBuilder2.AppendLine("   " + building_CryptosleepCasket.ContainedThing.LabelCap);
						Find.StoryWatcher.statsRecord.colonistsLaunched++;
					}
					current.Destroy(DestroyMode.Vanish);
				}
				string victoryText2 = "GameOverShipLaunched".Translate(new object[]
				{
					stringBuilder2.ToString(),
					GameVictoryUtility.PawnsLeftBehind()
				});
				GameVictoryUtility.ShowCredits(victoryText2);
			}
		}
	}
}
