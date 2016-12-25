using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class CompTargetable : CompUseEffect
	{
		private Thing target;

		private CompProperties_Targetable Props
		{
			get
			{
				return (CompProperties_Targetable)this.props;
			}
		}

		protected abstract bool PlayerChoosesTarget
		{
			get;
		}

		public override bool SelectedUseOption(Pawn p)
		{
			if (this.PlayerChoosesTarget)
			{
				Find.Targeter.BeginTargeting(this.GetTargetingParameters(), delegate(LocalTargetInfo t)
				{
					this.target = t.Thing;
					this.parent.GetComp<CompUsable>().TryStartUseJob(p);
				}, p, null, null);
				return true;
			}
			this.target = null;
			return false;
		}

		public override void DoEffect(Pawn usedBy)
		{
			if (this.PlayerChoosesTarget && this.target == null)
			{
				return;
			}
			if (this.target != null && !this.GetTargetingParameters().CanTarget(this.target))
			{
				return;
			}
			base.DoEffect(usedBy);
			foreach (Thing current in this.GetTargets(this.target))
			{
				foreach (CompTargetEffect current2 in this.parent.GetComps<CompTargetEffect>())
				{
					current2.DoEffectOn(usedBy, current);
				}
			}
		}

		protected abstract TargetingParameters GetTargetingParameters();

		public abstract IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null);

		public bool BaseTargetValidator(Thing t)
		{
			if (this.Props.psychicSensitiveTargetsOnly)
			{
				Pawn pawn = t as Pawn;
				if (pawn != null && pawn.GetStatValue(StatDefOf.PsychicSensitivity, true) <= 0f)
				{
					return false;
				}
			}
			return true;
		}
	}
}
