using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GameVictoryUtility
	{
		public static void ShowCredits(string victoryText)
		{
			Screen_Credits screen_Credits = new Screen_Credits(victoryText);
			screen_Credits.wonGame = true;
			Find.WindowStack.Add(screen_Credits);
			Find.MusicManagerPlay.ForceSilenceFor(999f);
			ScreenFader.StartFade(Color.clear, 3f);
		}

		public static string PawnsLeftBehind()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn current in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				stringBuilder.AppendLine("   " + current.LabelCap);
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan = caravans[i];
				if (caravan.IsPlayerControlled)
				{
					List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
					for (int j = 0; j < pawnsListForReading.Count; j++)
					{
						Pawn pawn = pawnsListForReading[j];
						if (pawn.IsColonist && pawn.HostFaction == null)
						{
							stringBuilder.AppendLine("   " + pawn.LabelCap);
						}
					}
				}
			}
			if (stringBuilder.Length == 0)
			{
				stringBuilder.AppendLine("Nobody".Translate().ToLower());
			}
			return stringBuilder.ToString();
		}
	}
}
