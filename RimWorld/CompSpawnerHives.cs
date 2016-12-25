using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompSpawnerHives : ThingComp
	{
		public const int MaxHivesPerMap = 30;

		private int nextHiveSpawnTick = -1;

		private CompProperties_SpawnerHives Props
		{
			get
			{
				return (CompProperties_SpawnerHives)this.props;
			}
		}

		private bool CanSpawnChildHive
		{
			get
			{
				return HivesUtility.TotalSpawnedHivesCount < 30;
			}
		}

		public override void PostSpawnSetup()
		{
			this.CalculateNextHiveSpawnTick();
		}

		public override void CompTickRare()
		{
			Hive hive = this.parent as Hive;
			if ((hive == null || hive.active) && Find.TickManager.TicksGame >= this.nextHiveSpawnTick)
			{
				Hive hive2;
				if (this.TrySpawnChildHive(false, out hive2))
				{
					hive2.nextPawnSpawnTick = Find.TickManager.TicksGame + Rand.Range(150, 350);
					Messages.Message("MessageHiveReproduced".Translate(), hive2, MessageSound.Negative);
				}
				else
				{
					this.CalculateNextHiveSpawnTick();
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = null;
			if (this.CanSpawnChildHive)
			{
				text = text + "HiveReproducesIn".Translate() + ": " + (this.nextHiveSpawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(true);
			}
			return text;
		}

		public void CalculateNextHiveSpawnTick()
		{
			Room room = this.parent.GetRoom();
			int num = 0;
			int num2 = GenRadial.NumCellsInRadius(9f);
			for (int i = 0; i < num2; i++)
			{
				IntVec3 intVec = this.parent.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds())
				{
					if (intVec.GetRoom() == room)
					{
						if (intVec.GetThingList().Any((Thing t) => t is Hive))
						{
							num++;
						}
					}
				}
			}
			float num3 = GenMath.LerpDouble(0f, 7f, 1f, 0.35f, (float)Mathf.Clamp(num, 0, 7));
			this.nextHiveSpawnTick = Find.TickManager.TicksGame + (int)(this.Props.HiveSpawnIntervalDays.RandomInRange * 60000f / (num3 * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}

		public bool TrySpawnChildHive(bool ignoreRoofedRequirement, out Hive newHive)
		{
			if (!this.CanSpawnChildHive)
			{
				newHive = null;
				return false;
			}
			IntVec3 invalid = IntVec3.Invalid;
			for (int i = 0; i < 3; i++)
			{
				float minDist = this.Props.HiveSpawnPreferredMinDist;
				if (i == 1)
				{
					minDist = 0f;
				}
				else if (i == 2)
				{
					newHive = null;
					return false;
				}
				if (CellFinder.TryFindRandomReachableCellNear(this.parent.Position, this.Props.HiveSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => this.CanSpawnHiveAt(c, minDist, ignoreRoofedRequirement), null, out invalid, 999999))
				{
					break;
				}
			}
			newHive = (Hive)GenSpawn.Spawn(this.parent.def, invalid);
			if (newHive.Faction != this.parent.Faction)
			{
				newHive.SetFaction(this.parent.Faction, null);
			}
			Hive hive = this.parent as Hive;
			if (hive != null)
			{
				newHive.active = hive.active;
			}
			this.CalculateNextHiveSpawnTick();
			return true;
		}

		private bool CanSpawnHiveAt(IntVec3 c, float minDist, bool ignoreRoofedRequirement)
		{
			if ((!ignoreRoofedRequirement && !c.Roofed()) || !c.Standable() || (minDist != 0f && c.DistanceToSquared(this.parent.Position) < minDist * minDist))
			{
				return false;
			}
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				if (c2.InBounds())
				{
					List<Thing> thingList = c2.GetThingList();
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j] is Hive)
						{
							return false;
						}
					}
				}
			}
			return c.GetThingList().Find((Thing x) => x.def.category == ThingCategory.Building || x.def.category == ThingCategory.Item) == null;
		}

		[DebuggerHidden]
		public override IEnumerable<Command> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEBUG: Reproduce",
					icon = TexCommand.GatherSpotActive,
					action = delegate
					{
						Hive hive;
						this.<>f__this.TrySpawnChildHive(false, out hive);
					}
				};
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.nextHiveSpawnTick, "nextHiveSpawnTick", 0, false);
		}
	}
}
