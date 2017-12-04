using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Mine : JobDriver
	{
		private int ticksToPickHit = -1000;

		private Effecter effecter;

		public const int BaseTicksBetweenPickHits = 120;

		private const int BaseDamagePerPickHit = 80;

		private const float MinMiningSpeedForNPCs = 0.5f;

		private Thing MineTarget
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.MineTarget, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnCellMissingDesignation(TargetIndex.A, DesignationDefOf.Mine);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil mine = new Toil();
			mine.tickAction = delegate
			{
				Pawn actor = mine.actor;
				Thing mineTarget = this.$this.MineTarget;
				if (this.$this.ticksToPickHit < -100)
				{
					this.$this.ResetTicksToPickHit();
				}
				if (actor.skills != null)
				{
					actor.skills.Learn(SkillDefOf.Mining, 0.11f, false);
				}
				this.$this.ticksToPickHit--;
				if (this.$this.ticksToPickHit <= 0)
				{
					IntVec3 position = mineTarget.Position;
					if (this.$this.effecter == null)
					{
						this.$this.effecter = EffecterDefOf.Mine.Spawn();
					}
					this.$this.effecter.Trigger(actor, mineTarget);
					int num = 80;
					Mineable mineable = mineTarget as Mineable;
					if (mineable == null || mineTarget.HitPoints > num)
					{
						DamageDef mining = DamageDefOf.Mining;
						int amount = num;
						Pawn actor2 = mine.actor;
						DamageInfo dinfo = new DamageInfo(mining, amount, -1f, actor2, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
						mineTarget.TakeDamage(dinfo);
					}
					else
					{
						mineable.Notify_TookMiningDamage(mineTarget.HitPoints, mine.actor);
						mineable.HitPoints = 0;
						mineable.DestroyMined(actor);
					}
					if (mineTarget.Destroyed)
					{
						actor.Map.mineStrikeManager.CheckStruckOre(position, mineTarget.def, actor);
						actor.records.Increment(RecordDefOf.CellsMined);
						if (this.$this.pawn.Faction != Faction.OfPlayer)
						{
							List<Thing> thingList = position.GetThingList(this.$this.Map);
							for (int i = 0; i < thingList.Count; i++)
							{
								thingList[i].SetForbidden(true, false);
							}
						}
						if (this.$this.pawn.Faction == Faction.OfPlayer && MineStrikeManager.MineableIsVeryValuable(mineTarget.def))
						{
							TaleRecorder.RecordTale(TaleDefOf.MinedValuable, new object[]
							{
								this.$this.pawn,
								mineTarget.def.building.mineableThing
							});
						}
						if (this.$this.pawn.Faction == Faction.OfPlayer && MineStrikeManager.MineableIsValuable(mineTarget.def) && !this.$this.pawn.Map.IsPlayerHome)
						{
							TaleRecorder.RecordTale(TaleDefOf.CaravanRemoteMining, new object[]
							{
								this.$this.pawn,
								mineTarget.def.building.mineableThing
							});
						}
						this.$this.ReadyForNextToil();
						return;
					}
					this.$this.ResetTicksToPickHit();
				}
			};
			mine.defaultCompleteMode = ToilCompleteMode.Never;
			mine.WithProgressBar(TargetIndex.A, () => 1f - (float)this.$this.MineTarget.HitPoints / (float)this.$this.MineTarget.MaxHitPoints, false, -0.5f);
			mine.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return mine;
		}

		private void ResetTicksToPickHit()
		{
			float num = this.pawn.GetStatValue(StatDefOf.MiningSpeed, true);
			if (num < 0.5f && this.pawn.Faction != Faction.OfPlayer)
			{
				num = 0.5f;
			}
			this.ticksToPickHit = (int)Math.Round((double)(120f / num));
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksToPickHit, "ticksToPickHit", 0, false);
		}
	}
}
