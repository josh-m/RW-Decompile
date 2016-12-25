using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class SettleUtility
	{
		public static readonly Texture2D SettleCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/Settle", true);

		public static bool PlayerHomesCountLimitReached
		{
			get
			{
				int num = 0;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].IsPlayerHome)
					{
						num++;
					}
				}
				return num >= Prefs.MaxNumberOfPlayerHomes;
			}
		}

		public static FactionBase AddNewHome(int tile, Faction faction)
		{
			FactionBase factionBase = (FactionBase)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.FactionBase);
			factionBase.Tile = tile;
			factionBase.SetFaction(faction);
			factionBase.Name = FactionBaseNameGenerator.GenerateFactionBaseName(factionBase);
			Find.WorldObjects.Add(factionBase);
			return factionBase;
		}
	}
}
