using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class FactionGenerator
	{
		private const int MinStartVisibleFactions = 5;

		private static readonly FloatRange FactionBasesPer100kTiles = new FloatRange(75f, 85f);

		public static void GenerateFactionsIntoWorld()
		{
			int i = 0;
			foreach (FactionDef current in DefDatabase<FactionDef>.AllDefs)
			{
				for (int j = 0; j < current.requiredCountAtGameStart; j++)
				{
					Faction faction = FactionGenerator.NewGeneratedFaction(current);
					Find.FactionManager.Add(faction);
					if (!current.hidden)
					{
						i++;
					}
				}
			}
			while (i < 5)
			{
				FactionDef facDef = (from fa in DefDatabase<FactionDef>.AllDefs
				where fa.canMakeRandomly && Find.FactionManager.AllFactions.Count((Faction f) => f.def == fa) < fa.maxCountAtGameStart
				select fa).RandomElement<FactionDef>();
				Faction faction2 = FactionGenerator.NewGeneratedFaction(facDef);
				Find.World.factionManager.Add(faction2);
				i++;
			}
			int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * FactionGenerator.FactionBasesPer100kTiles.RandomInRange);
			num -= Find.WorldObjects.FactionBases.Count;
			for (int k = 0; k < num; k++)
			{
				Faction faction3 = (from x in Find.World.factionManager.AllFactionsListForReading
				where !x.def.isPlayer && !x.def.hidden
				select x).RandomElementByWeight((Faction x) => x.def.baseSelectionWeight);
				FactionBase factionBase = (FactionBase)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.FactionBase);
				factionBase.SetFaction(faction3);
				factionBase.Tile = TileFinder.RandomFactionBaseTileFor(faction3, false);
				factionBase.Name = FactionBaseNameGenerator.GenerateFactionBaseName(factionBase);
				Find.WorldObjects.Add(factionBase);
			}
		}

		public static void EnsureRequiredEnemies(Faction player)
		{
			foreach (FactionDef facDef in DefDatabase<FactionDef>.AllDefs)
			{
				if (facDef.mustStartOneEnemy && Find.World.factionManager.AllFactions.Any((Faction f) => f.def == facDef) && !Find.World.factionManager.AllFactions.Any((Faction f) => f.def == facDef && f.HostileTo(player)))
				{
					Faction faction = (from f in Find.World.factionManager.AllFactions
					where f.def == facDef
					select f).RandomElement<Faction>();
					float goodwillChange = -(faction.GoodwillWith(player) + 100f) * Rand.Range(0.8f, 0.9f);
					faction.AffectGoodwillWith(player, goodwillChange);
					faction.SetHostileTo(player, true);
				}
			}
		}

		public static Faction NewGeneratedFaction()
		{
			return FactionGenerator.NewGeneratedFaction(DefDatabase<FactionDef>.GetRandom());
		}

		public static Faction NewGeneratedFaction(FactionDef facDef)
		{
			Faction faction = new Faction();
			faction.def = facDef;
			faction.loadID = Find.World.uniqueIDsManager.GetNextFactionID();
			faction.colorFromSpectrum = FactionGenerator.NewRandomColorFromSpectrum(faction);
			if (!facDef.isPlayer)
			{
				if (facDef.fixedName != null)
				{
					faction.Name = facDef.fixedName;
				}
				else
				{
					faction.Name = NameGenerator.GenerateName(facDef.factionNameMaker, from fac in Find.FactionManager.AllFactionsVisible
					select fac.Name, false);
				}
			}
			foreach (Faction current in Find.FactionManager.AllFactionsListForReading)
			{
				faction.TryMakeInitialRelationsWith(current);
			}
			if (!facDef.hidden && !facDef.isPlayer)
			{
				FactionBase factionBase = (FactionBase)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.FactionBase);
				factionBase.SetFaction(faction);
				factionBase.Tile = TileFinder.RandomFactionBaseTileFor(faction, false);
				factionBase.Name = FactionBaseNameGenerator.GenerateFactionBaseName(factionBase);
				Find.WorldObjects.Add(factionBase);
			}
			faction.GenerateNewLeader();
			return faction;
		}

		private static float NewRandomColorFromSpectrum(Faction faction)
		{
			float num = -1f;
			float result = 0f;
			for (int i = 0; i < 10; i++)
			{
				float value = Rand.Value;
				float num2 = 1f;
				List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
				for (int j = 0; j < allFactionsListForReading.Count; j++)
				{
					Faction faction2 = allFactionsListForReading[j];
					if (faction2 != faction && faction2.def == faction.def)
					{
						float num3 = Mathf.Abs(value - faction2.colorFromSpectrum);
						if (num3 < num2)
						{
							num2 = num3;
						}
					}
				}
				if (num2 > num)
				{
					num = num2;
					result = value;
				}
			}
			return result;
		}
	}
}
