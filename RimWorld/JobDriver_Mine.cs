using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Mine : JobDriver
	{
		public const int BaseTicksBetweenPickHits = 120;

		private const int BaseDamagePerPickHit = 80;

		private int ticksToPickHit = -1000;

		private Effecter effecter;

		private Thing Mineable
		{
			get
			{
				return base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnCellMissingDesignation(TargetIndex.A, DesignationDefOf.Mine);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil mine = new Toil();
			mine.tickAction = delegate
			{
				Pawn actor = this.<mine>__0.actor;
				Thing mineable = this.<>f__this.Mineable;
				if (this.<>f__this.ticksToPickHit < -100)
				{
					this.<>f__this.ResetTicksToPickHit();
				}
				if (actor.skills != null)
				{
					actor.skills.Learn(SkillDefOf.Mining, 0.11f);
				}
				this.<>f__this.ticksToPickHit--;
				if (this.<>f__this.ticksToPickHit <= 0)
				{
					if (this.<>f__this.effecter == null)
					{
						this.<>f__this.effecter = EffecterDefOf.Mine.Spawn();
					}
					this.<>f__this.effecter.Trigger(actor, mineable);
					int amount = 80;
					BodyPartDamageInfo value = new BodyPartDamageInfo(new BodyPartHeight?(BodyPartHeight.Top), new BodyPartDepth?(BodyPartDepth.Outside));
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Mining, amount, this.<mine>__0.actor, new BodyPartDamageInfo?(value), null);
					IntVec3 position = mineable.Position;
					mineable.TakeDamage(dinfo);
					if (mineable.Destroyed)
					{
						MineStrikeManager.CheckStruckOre(position, mineable.def, actor);
						actor.records.Increment(RecordDefOf.CellsMined);
						if (this.<>f__this.pawn.Faction != Faction.OfPlayer)
						{
							List<Thing> thingList = position.GetThingList();
							for (int i = 0; i < thingList.Count; i++)
							{
								thingList[i].SetForbidden(true, false);
							}
						}
						this.<>f__this.ReadyForNextToil();
						return;
					}
					this.<>f__this.ResetTicksToPickHit();
				}
			};
			mine.defaultCompleteMode = ToilCompleteMode.Never;
			mine.WithProgressBar(TargetIndex.A, () => 1f - (float)this.<>f__this.Mineable.HitPoints / (float)this.<>f__this.Mineable.MaxHitPoints, false, -0.5f);
			yield return mine;
		}

		private void ResetTicksToPickHit()
		{
			float statValue = this.pawn.GetStatValue(StatDefOf.MiningSpeed, true);
			this.ticksToPickHit = (int)Math.Round((double)(120f / statValue));
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.ticksToPickHit, "ticksToPickHit", 0, false);
		}
	}
}
