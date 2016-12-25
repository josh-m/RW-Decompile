using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace Verse
{
	public sealed class ListerBuildings
	{
		public List<Building> allBuildingsColonist = new List<Building>();

		public HashSet<Building> allBuildingsColonistCombatTargets = new HashSet<Building>();

		public HashSet<Building> allBuildingsColonistElecFire = new HashSet<Building>();

		public void Add(Building b)
		{
			if (b.def.building != null && b.def.building.isNaturalRock)
			{
				return;
			}
			if (b.Faction == Faction.OfPlayer)
			{
				this.allBuildingsColonist.Add(b);
				if (b is IAttackTarget)
				{
					this.allBuildingsColonistCombatTargets.Add(b);
				}
			}
			CompProperties_Power compProperties = b.def.GetCompProperties<CompProperties_Power>();
			if (compProperties != null && compProperties.startElectricalFires)
			{
				this.allBuildingsColonistElecFire.Add(b);
			}
		}

		public void Remove(Building b)
		{
			if (b.Faction == Faction.OfPlayer)
			{
				this.allBuildingsColonist.Remove(b);
				if (b is IAttackTarget)
				{
					this.allBuildingsColonistCombatTargets.Remove(b);
				}
			}
			CompProperties_Power compProperties = b.def.GetCompProperties<CompProperties_Power>();
			if (compProperties != null && compProperties.startElectricalFires)
			{
				this.allBuildingsColonistElecFire.Remove(b);
			}
		}

		public bool ColonistsHaveBuilding(ThingDef def)
		{
			for (int i = 0; i < this.allBuildingsColonist.Count; i++)
			{
				if (this.allBuildingsColonist[i].def == def)
				{
					return true;
				}
			}
			return false;
		}

		public bool ColonistsHaveBuilding(Func<Thing, bool> predicate)
		{
			for (int i = 0; i < this.allBuildingsColonist.Count; i++)
			{
				if (predicate(this.allBuildingsColonist[i]))
				{
					return true;
				}
			}
			return false;
		}

		public bool ColonistsHaveResearchBench()
		{
			for (int i = 0; i < this.allBuildingsColonist.Count; i++)
			{
				if (this.allBuildingsColonist[i] is Building_ResearchBench)
				{
					return true;
				}
			}
			return false;
		}

		public bool ColonistsHaveBuildingWithPowerOn(ThingDef def)
		{
			for (int i = 0; i < this.allBuildingsColonist.Count; i++)
			{
				if (this.allBuildingsColonist[i].def == def)
				{
					CompPowerTrader compPowerTrader = this.allBuildingsColonist[i].TryGetComp<CompPowerTrader>();
					if (compPowerTrader == null || compPowerTrader.PowerOn)
					{
						return true;
					}
				}
			}
			return false;
		}

		[DebuggerHidden]
		public IEnumerable<Building> AllBuildingsColonistOfDef(ThingDef def)
		{
			for (int i = 0; i < this.allBuildingsColonist.Count; i++)
			{
				if (this.allBuildingsColonist[i].def == def)
				{
					yield return this.allBuildingsColonist[i];
				}
			}
		}

		[DebuggerHidden]
		public IEnumerable<T> AllBuildingsColonistOfClass<T>() where T : Building
		{
			for (int i = 0; i < this.allBuildingsColonist.Count; i++)
			{
				T casted = this.allBuildingsColonist[i] as T;
				if (casted != null)
				{
					yield return casted;
				}
			}
		}
	}
}
