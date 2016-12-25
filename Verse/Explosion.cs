using RimWorld;
using System;
using System.Collections.Generic;
using Verse.Sound;

namespace Verse
{
	public class Explosion : IExposable
	{
		public ExplosionManager explosionManager;

		public IntVec3 position;

		public float radius;

		public DamageDef damType;

		public int damAmount;

		public Thing instigator;

		public ThingDef weaponGear;

		public bool applyDamageToExplosionCellsNeighbors;

		public ThingDef preExplosionSpawnThingDef;

		public float preExplosionSpawnChance;

		public int preExplosionSpawnThingCount = 1;

		public ThingDef postExplosionSpawnThingDef;

		public float postExplosionSpawnChance;

		public int postExplosionSpawnThingCount = 1;

		public bool finished;

		private int startTick;

		private List<IntVec3> cellsToAffect;

		private List<Thing> damagedThings;

		private HashSet<IntVec3> addedCellsAffectedOnlyByDamage;

		private static HashSet<IntVec3> tmpCells = new HashSet<IntVec3>();

		public Map Map
		{
			get
			{
				return this.explosionManager.map;
			}
		}

		public void StartExplosion(SoundDef explosionSound)
		{
			this.startTick = Find.TickManager.TicksGame;
			this.cellsToAffect = SimplePool<List<IntVec3>>.Get();
			this.cellsToAffect.Clear();
			this.damagedThings = SimplePool<List<Thing>>.Get();
			this.damagedThings.Clear();
			this.addedCellsAffectedOnlyByDamage = SimplePool<HashSet<IntVec3>>.Get();
			this.addedCellsAffectedOnlyByDamage.Clear();
			this.cellsToAffect.AddRange(this.damType.Worker.ExplosionCellsToHit(this));
			if (this.applyDamageToExplosionCellsNeighbors)
			{
				this.AddCellsNeighbors(this.cellsToAffect);
			}
			this.damType.Worker.ExplosionStart(this, this.cellsToAffect);
			this.PlayExplosionSound(explosionSound);
			this.cellsToAffect.Sort((IntVec3 a, IntVec3 b) => this.GetCellAffectTick(b).CompareTo(this.GetCellAffectTick(a)));
		}

		public void Tick()
		{
			int ticksGame = Find.TickManager.TicksGame;
			int count = this.cellsToAffect.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				if (ticksGame < this.GetCellAffectTick(this.cellsToAffect[i]))
				{
					break;
				}
				this.AffectCell(this.cellsToAffect[i]);
				this.cellsToAffect.RemoveAt(i);
			}
			if (!this.cellsToAffect.Any<IntVec3>())
			{
				this.Finished();
			}
		}

		public void Finished()
		{
			if (this.finished)
			{
				return;
			}
			this.cellsToAffect.Clear();
			SimplePool<List<IntVec3>>.Return(this.cellsToAffect);
			this.cellsToAffect = null;
			this.damagedThings.Clear();
			SimplePool<List<Thing>>.Return(this.damagedThings);
			this.damagedThings = null;
			this.addedCellsAffectedOnlyByDamage.Clear();
			SimplePool<HashSet<IntVec3>>.Return(this.addedCellsAffectedOnlyByDamage);
			this.addedCellsAffectedOnlyByDamage = null;
			this.finished = true;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<IntVec3>(ref this.position, "position", default(IntVec3), false);
			Scribe_Values.LookValue<float>(ref this.radius, "radius", 0f, false);
			Scribe_Defs.LookDef<DamageDef>(ref this.damType, "damType");
			Scribe_Values.LookValue<int>(ref this.damAmount, "damAmount", 0, false);
			Scribe_References.LookReference<Thing>(ref this.instigator, "instigator", false);
			Scribe_Defs.LookDef<ThingDef>(ref this.weaponGear, "weaponGear");
			Scribe_Values.LookValue<bool>(ref this.applyDamageToExplosionCellsNeighbors, "applyDamageToExplosionCellsNeighbors", false, false);
			Scribe_Defs.LookDef<ThingDef>(ref this.preExplosionSpawnThingDef, "preExplosionSpawnThingDef");
			Scribe_Values.LookValue<float>(ref this.preExplosionSpawnChance, "preExplosionSpawnChance", 0f, false);
			Scribe_Values.LookValue<int>(ref this.preExplosionSpawnThingCount, "preExplosionSpawnThingCount", 1, false);
			Scribe_Defs.LookDef<ThingDef>(ref this.postExplosionSpawnThingDef, "postExplosionSpawnThingDef");
			Scribe_Values.LookValue<float>(ref this.postExplosionSpawnChance, "postExplosionSpawnChance", 0f, false);
			Scribe_Values.LookValue<int>(ref this.postExplosionSpawnThingCount, "postExplosionSpawnThingCount", 1, false);
			Scribe_Values.LookValue<bool>(ref this.finished, "finished", false, false);
			Scribe_Values.LookValue<int>(ref this.startTick, "startTick", 0, false);
			Scribe_Collections.LookList<IntVec3>(ref this.cellsToAffect, "cellsToAffect", LookMode.Value, new object[0]);
			Scribe_Collections.LookList<Thing>(ref this.damagedThings, "damagedThings", LookMode.Reference, new object[0]);
			Scribe_Collections.LookHashSet<IntVec3>(ref this.addedCellsAffectedOnlyByDamage, "addedCellsAffectedOnlyByDamage", LookMode.Value);
		}

		private int GetCellAffectTick(IntVec3 cell)
		{
			return this.startTick + (int)((cell - this.position).LengthHorizontal * 1.5f);
		}

		private void AffectCell(IntVec3 c)
		{
			bool flag = this.ShouldCellBeAffectedOnlyByDamage(c);
			if (!flag && c.Walkable(this.Map) && Rand.Value < this.preExplosionSpawnChance)
			{
				this.TrySpawnExplosionThing(this.preExplosionSpawnThingDef, c, this.preExplosionSpawnThingCount);
			}
			this.damType.Worker.ExplosionAffectCell(this, c, this.damagedThings, !flag);
			if (!flag && c.Walkable(this.Map) && Rand.Value < this.postExplosionSpawnChance)
			{
				this.TrySpawnExplosionThing(this.postExplosionSpawnThingDef, c, this.postExplosionSpawnThingCount);
			}
		}

		private void TrySpawnExplosionThing(ThingDef thingDef, IntVec3 c, int count)
		{
			if (thingDef == null)
			{
				return;
			}
			if (thingDef.IsFilth)
			{
				FilthMaker.MakeFilth(c, this.Map, thingDef, count);
			}
			else
			{
				Thing thing = ThingMaker.MakeThing(thingDef, null);
				thing.stackCount = count;
				GenSpawn.Spawn(thing, c, this.Map);
			}
		}

		private void PlayExplosionSound(SoundDef explosionSound)
		{
			bool flag;
			if (Prefs.DevMode)
			{
				flag = (explosionSound != null);
			}
			else
			{
				flag = !explosionSound.NullOrUndefined();
			}
			if (flag)
			{
				explosionSound.PlayOneShot(new TargetInfo(this.position, this.Map, false));
			}
			else
			{
				this.damType.soundExplosion.PlayOneShot(new TargetInfo(this.position, this.Map, false));
			}
		}

		private void AddCellsNeighbors(List<IntVec3> cells)
		{
			Explosion.tmpCells.Clear();
			this.addedCellsAffectedOnlyByDamage.Clear();
			for (int i = 0; i < cells.Count; i++)
			{
				Explosion.tmpCells.Add(cells[i]);
			}
			for (int j = 0; j < cells.Count; j++)
			{
				if (cells[j].Walkable(this.Map))
				{
					for (int k = 0; k < GenAdj.AdjacentCells.Length; k++)
					{
						IntVec3 intVec = cells[j] + GenAdj.AdjacentCells[k];
						if (intVec.InBounds(this.Map))
						{
							bool flag = Explosion.tmpCells.Add(intVec);
							if (flag)
							{
								this.addedCellsAffectedOnlyByDamage.Add(intVec);
							}
						}
					}
				}
			}
			cells.Clear();
			foreach (IntVec3 current in Explosion.tmpCells)
			{
				cells.Add(current);
			}
			Explosion.tmpCells.Clear();
		}

		private bool ShouldCellBeAffectedOnlyByDamage(IntVec3 c)
		{
			return this.applyDamageToExplosionCellsNeighbors && this.addedCellsAffectedOnlyByDamage.Contains(c);
		}
	}
}
