using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public static class GenAI
	{
		public static bool CanInteractPawn(Pawn assister, Pawn assistee)
		{
			return assistee.Spawned && assister.CanReach(assistee, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn);
		}

		public static bool MachinesLike(Faction machineFaction, Pawn p)
		{
			return p.Faction != null && (!p.IsPrisoner || p.HostFaction != machineFaction) && !p.Faction.HostileTo(machineFaction);
		}

		public static bool CanUseItemForWork(Pawn p, Thing item)
		{
			return !item.IsForbidden(p) && p.CanReserveAndReach(item, PathEndMode.ClosestTouch, p.NormalMaxDanger(), 1);
		}

		public static bool CanBeArrested(this Pawn pawn)
		{
			return pawn.RaceProps.Humanlike && !pawn.InAggroMentalState && !pawn.HostileTo(Faction.OfPlayer) && (!pawn.IsPrisonerOfColony || !pawn.Position.IsInPrisonCell(pawn.Map));
		}

		public static bool InDangerousCombat(Pawn pawn)
		{
			Region root = pawn.GetRegion();
			bool found = false;
			RegionTraverser.BreadthFirstTraverse(root, (Region r1, Region r2) => r2.Room == root.Room, (Region r) => r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn).Any(delegate(Thing t)
			{
				Pawn pawn2 = t as Pawn;
				if (pawn2 != null && !pawn2.Downed && (pawn.Position - pawn2.Position).LengthHorizontalSquared < 144f && pawn2.HostileTo(pawn.Faction))
				{
					found = true;
					return true;
				}
				return false;
			}), 9);
			return found;
		}

		public static IntVec3 RandomRaidDest(Map map)
		{
			List<ThingDef> allBedDefBestToWorst = RestUtility.AllBedDefBestToWorst;
			List<Building> list = new List<Building>(map.mapPawns.FreeColonistsAndPrisonersSpawnedCount);
			for (int i = 0; i < allBedDefBestToWorst.Count; i++)
			{
				foreach (Building current in map.listerBuildings.AllBuildingsColonistOfDef(allBedDefBestToWorst[i]))
				{
					if (((Building_Bed)current).owners.Any<Pawn>())
					{
						list.Add(current);
					}
				}
			}
			Building building;
			if (list.TryRandomElement(out building))
			{
				return building.Position;
			}
			IEnumerable<Building> source = from b in map.listerBuildings.allBuildingsColonist
			where !b.def.building.ai_combatDangerous && !b.def.building.isInert
			select b;
			if (source.Any<Building>())
			{
				for (int j = 0; j < 500; j++)
				{
					Building t = source.RandomElement<Building>();
					IntVec3 intVec = t.RandomAdjacentCell8Way();
					if (intVec.Walkable(map))
					{
						return intVec;
					}
				}
			}
			if (map.mapPawns.FreeColonistsSpawnedCount > 0)
			{
				return map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>().Position;
			}
			return map.Center;
		}

		public static bool EnemyIsNear(Pawn p, float radius)
		{
			if (!p.Spawned)
			{
				return false;
			}
			List<IAttackTarget> potentialTargetsFor = p.Map.attackTargetsCache.GetPotentialTargetsFor(p);
			for (int i = 0; i < potentialTargetsFor.Count; i++)
			{
				IAttackTarget attackTarget = potentialTargetsFor[i];
				if (!attackTarget.ThreatDisabled())
				{
					if (p.Position.InHorDistOf(((Thing)attackTarget).Position, radius))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
