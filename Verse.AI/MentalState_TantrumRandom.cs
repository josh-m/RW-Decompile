using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse.AI
{
	public abstract class MentalState_TantrumRandom : MentalState_Tantrum
	{
		private int targetFoundTicks;

		private const int CheckChooseNewTargetIntervalTicks = 500;

		private const int MaxSameTargetAttackTicks = 1250;

		private static List<Thing> candidates = new List<Thing>();

		protected abstract void GetPotentialTargets(List<Thing> outThings);

		protected virtual Predicate<Thing> GetCustomValidator()
		{
			return null;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.targetFoundTicks, "targetFoundTicks", 0, false);
		}

		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			this.ChooseNextTarget();
		}

		public override void MentalStateTick()
		{
			if (this.target != null && (!this.target.Spawned || !this.pawn.CanReach(this.target, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn) || (this.target is Pawn && ((Pawn)this.target).Downed)))
			{
				this.ChooseNextTarget();
			}
			if (this.pawn.IsHashIntervalTick(500) && (this.target == null || this.hitTargetAtLeastOnce))
			{
				this.ChooseNextTarget();
			}
			base.MentalStateTick();
		}

		private void ChooseNextTarget()
		{
			MentalState_TantrumRandom.candidates.Clear();
			this.GetPotentialTargets(MentalState_TantrumRandom.candidates);
			if (!MentalState_TantrumRandom.candidates.Any<Thing>())
			{
				this.target = null;
				this.hitTargetAtLeastOnce = false;
				this.targetFoundTicks = -1;
			}
			else
			{
				Thing thing;
				if (this.target != null && Find.TickManager.TicksGame - this.targetFoundTicks > 1250 && MentalState_TantrumRandom.candidates.Any((Thing x) => x != this.target))
				{
					thing = (from x in MentalState_TantrumRandom.candidates
					where x != this.target
					select x).RandomElementByWeight((Thing x) => this.GetCandidateWeight(x));
				}
				else
				{
					thing = MentalState_TantrumRandom.candidates.RandomElementByWeight((Thing x) => this.GetCandidateWeight(x));
				}
				if (thing != this.target)
				{
					this.target = thing;
					this.hitTargetAtLeastOnce = false;
					this.targetFoundTicks = Find.TickManager.TicksGame;
				}
			}
			MentalState_TantrumRandom.candidates.Clear();
		}

		private float GetCandidateWeight(Thing candidate)
		{
			float num = this.pawn.Position.DistanceTo(candidate.Position);
			float num2 = Mathf.Min(num / 40f, 1f);
			return (1f - num2) * (1f - num2) + 0.01f;
		}
	}
}
