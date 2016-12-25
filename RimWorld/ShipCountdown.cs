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

		public static bool CountingDown
		{
			get
			{
				return ShipCountdown.timeLeft >= 0f;
			}
		}

		public static void InitiateCountdown(Building launchingShipRoot)
		{
			SoundDef.Named("ShipTakeoff").PlayOneShotOnCamera();
			ShipCountdown.shipRoot = launchingShipRoot;
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
			List<Building> list = ShipUtility.ShipBuildingsAttachedTo(ShipCountdown.shipRoot).ToList<Building>();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Building current in list)
			{
				Building_CryptosleepCasket building_CryptosleepCasket = current as Building_CryptosleepCasket;
				if (building_CryptosleepCasket != null && building_CryptosleepCasket.HasAnyContents)
				{
					stringBuilder.AppendLine("   " + building_CryptosleepCasket.ContainedThing.LabelCap);
					Find.StoryWatcher.statsRecord.colonistsLaunched++;
				}
				current.Destroy(DestroyMode.Vanish);
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (Pawn current2 in Find.MapPawns.FreeColonists)
			{
				if (current2.Spawned)
				{
					stringBuilder2.AppendLine("   " + current2.LabelCap);
				}
			}
			if (stringBuilder2.Length == 0)
			{
				stringBuilder2.AppendLine("Nobody".Translate().ToLower());
			}
			string preCreditsMessage = "GameOverShipLaunched".Translate(new object[]
			{
				stringBuilder.ToString(),
				stringBuilder2.ToString()
			});
			Screen_Credits screen_Credits = new Screen_Credits(preCreditsMessage);
			screen_Credits.wonGame = true;
			Find.WindowStack.Add(screen_Credits);
			Find.MusicManagerMap.ForceSilenceFor(999f);
			ScreenFader.StartFade(Color.clear, 3f);
		}
	}
}
