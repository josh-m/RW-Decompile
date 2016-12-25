using RimWorld.Planet;
using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class FactionGenerator
	{
		private const int MinStartVisibleFactions = 5;

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
			if (!facDef.isPlayer)
			{
				if (facDef.fixedName != null)
				{
					faction.Name = facDef.fixedName;
				}
				else
				{
					faction.Name = NameGenerator.GenerateName(facDef.factionNameMaker, from fac in Find.FactionManager.AllFactionsVisible
					select fac.Name);
				}
			}
			foreach (Faction current in Find.FactionManager.AllFactionsListForReading)
			{
				faction.TryMakeInitialRelationsWith(current);
			}
			if (!facDef.hidden)
			{
				faction.homeSquare = FactionGenerator.RandomHomeSquareFor(faction);
			}
			faction.GenerateNewLeader();
			return faction;
		}

		private static IntVec2 RandomHomeSquareFor(Faction faction)
		{
			for (int i = 0; i < 2000; i++)
			{
				IntVec2 intVec = new IntVec2(Rand.Range(0, Find.World.Size.x), Rand.Range(0, Find.World.Size.z));
				WorldSquare worldSquare = Find.World.grid.Get(intVec);
				if (worldSquare.biome.canBuildBase)
				{
					if (Find.FactionManager.FactionInWorldSquare(intVec) == null)
					{
						return intVec;
					}
				}
			}
			Log.Error("Failed to find home square for " + faction);
			return new IntVec2(0, 0);
		}
	}
}
