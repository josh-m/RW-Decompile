using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class Hive : ThingWithComps
	{
		private const int InitialPawnSpawnDelay = 960;

		private const int PawnSpawnRadius = 5;

		private const float MaxSpawnedPawnsPoints = 500f;

		private const int InitialPawnsPoints = 260;

		public bool active = true;

		public int nextPawnSpawnTick = -1;

		private List<Pawn> spawnedPawns = new List<Pawn>();

		private int ticksToSpawnInitialPawns = -1;

		private static readonly FloatRange PawnSpawnIntervalDays = new FloatRange(0.85f, 1.1f);

		private Lord Lord
		{
			get
			{
				Predicate<Pawn> hasDefendHiveLord = delegate(Pawn x)
				{
					Lord lord = x.GetLord();
					return lord != null && lord.LordJob is LordJob_DefendAndExpandHive;
				};
				Pawn foundPawn = this.spawnedPawns.Find(hasDefendHiveLord);
				if (foundPawn == null)
				{
					RegionTraverser.BreadthFirstTraverse(base.Position.GetRegion(), (Region from, Region to) => true, delegate(Region r)
					{
						List<Thing> list = r.ListerThings.ThingsOfDef(ThingDefOf.Hive);
						for (int i = 0; i < list.Count; i++)
						{
							if (list[i] != this)
							{
								if (list[i].Faction == this.Faction)
								{
									foundPawn = ((Hive)list[i]).spawnedPawns.Find(hasDefendHiveLord);
									if (foundPawn != null)
									{
										return true;
									}
								}
							}
						}
						return false;
					}, 20);
				}
				if (foundPawn != null)
				{
					return foundPawn.GetLord();
				}
				return null;
			}
		}

		private float SpawnedPawnsPoints
		{
			get
			{
				this.FilterOutUnspawnedPawns();
				float num = 0f;
				for (int i = 0; i < this.spawnedPawns.Count; i++)
				{
					num += this.spawnedPawns[i].kindDef.combatPower;
				}
				return num;
			}
		}

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			if (base.Faction == null)
			{
				this.SetFaction(Faction.OfInsects, null);
			}
		}

		public void StartInitialPawnSpawnCountdown()
		{
			this.ticksToSpawnInitialPawns = 960;
		}

		private void SpawnInitialPawnsNow()
		{
			this.ticksToSpawnInitialPawns = -1;
			while (this.SpawnedPawnsPoints < 260f)
			{
				Pawn pawn;
				if (!this.TrySpawnPawn(out pawn))
				{
					return;
				}
			}
			this.CalculateNextPawnSpawnTick();
		}

		public override void TickRare()
		{
			base.TickRare();
			this.FilterOutUnspawnedPawns();
			if (!this.active && !base.Position.Fogged())
			{
				this.Activate();
			}
			if (this.active)
			{
				if (this.ticksToSpawnInitialPawns > 0)
				{
					this.ticksToSpawnInitialPawns -= 250;
					if (this.ticksToSpawnInitialPawns <= 0)
					{
						this.SpawnInitialPawnsNow();
					}
				}
				if (Find.TickManager.TicksGame >= this.nextPawnSpawnTick)
				{
					if (this.SpawnedPawnsPoints < 500f)
					{
						Pawn pawn;
						bool flag = this.TrySpawnPawn(out pawn);
						if (flag && pawn.caller != null)
						{
							pawn.caller.DoCall();
						}
					}
					this.CalculateNextPawnSpawnTick();
				}
			}
		}

		public override void DeSpawn()
		{
			base.DeSpawn();
			List<Lord> lords = Find.LordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				lords[i].ReceiveMemo("HiveDestroyed");
			}
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (dinfo.Def.externalViolence && dinfo.Instigator != null)
			{
				if (this.ticksToSpawnInitialPawns > 0)
				{
					this.SpawnInitialPawnsNow();
				}
				Lord lord = this.Lord;
				if (lord != null)
				{
					lord.ReceiveMemo("HiveAttacked");
				}
			}
			base.PostApplyDamage(dinfo, totalDamageDealt);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<bool>(ref this.active, "active", false, false);
			Scribe_Values.LookValue<int>(ref this.nextPawnSpawnTick, "nextPawnSpawnTick", 0, false);
			Scribe_Collections.LookList<Pawn>(ref this.spawnedPawns, "spawnedPawns", LookMode.MapReference, new object[0]);
			Scribe_Values.LookValue<int>(ref this.ticksToSpawnInitialPawns, "ticksToSpawnInitialPawns", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.spawnedPawns.RemoveAll((Pawn x) => x == null);
			}
		}

		private void Activate()
		{
			this.active = true;
			this.nextPawnSpawnTick = Find.TickManager.TicksGame + Rand.Range(200, 400);
			CompSpawnerHives comp = base.GetComp<CompSpawnerHives>();
			if (comp != null)
			{
				comp.CalculateNextHiveSpawnTick();
			}
		}

		private void CalculateNextPawnSpawnTick()
		{
			float num = GenMath.LerpDouble(0f, 5f, 1f, 0.5f, (float)this.spawnedPawns.Count);
			this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)(Hive.PawnSpawnIntervalDays.RandomInRange * 60000f / (num * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}

		private void FilterOutUnspawnedPawns()
		{
			this.spawnedPawns.RemoveAll((Pawn x) => !x.Spawned);
		}

		private bool TrySpawnPawn(out Pawn pawn)
		{
			List<PawnKindDef> list = new List<PawnKindDef>();
			list.Add(PawnKindDefOf.Megascarab);
			list.Add(PawnKindDefOf.Spelopede);
			list.Add(PawnKindDefOf.Megaspider);
			float curPoints = this.SpawnedPawnsPoints;
			IEnumerable<PawnKindDef> source = from x in list
			where curPoints + x.combatPower <= 500f
			select x;
			PawnKindDef kindDef;
			if (!source.TryRandomElement(out kindDef))
			{
				pawn = null;
				return false;
			}
			pawn = PawnGenerator.GeneratePawn(kindDef, base.Faction);
			GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(base.Position, 5));
			this.spawnedPawns.Add(pawn);
			Lord lord = this.Lord;
			if (lord == null)
			{
				lord = this.CreateNewLord();
			}
			lord.AddPawn(pawn);
			return true;
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEBUG: Spawn pawn",
					icon = TexCommand.ReleaseAnimals,
					action = delegate
					{
						Pawn pawn;
						this.<>f__this.TrySpawnPawn(out pawn);
					}
				};
			}
		}

		public override bool PreventPlayerSellingThingsNearby(out string reason)
		{
			if (this.spawnedPawns.Count > 0)
			{
				if (this.spawnedPawns.Any((Pawn p) => !p.Downed))
				{
					reason = this.def.label;
					return true;
				}
			}
			reason = null;
			return false;
		}

		private Lord CreateNewLord()
		{
			return LordMaker.MakeNewLord(base.Faction, new LordJob_DefendAndExpandHive(), null);
		}
	}
}
