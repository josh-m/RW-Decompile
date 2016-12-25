using System;
using System.Collections.Generic;

namespace Verse
{
	public class HediffComp_VerbGiver : HediffComp, IVerbOwner
	{
		public VerbTracker verbTracker;

		public HediffCompProperties_VerbGiver Props
		{
			get
			{
				return (HediffCompProperties_VerbGiver)this.props;
			}
		}

		public VerbTracker VerbTracker
		{
			get
			{
				return this.verbTracker;
			}
		}

		public List<VerbProperties> VerbProperties
		{
			get
			{
				return this.Props.verbs;
			}
		}

		public HediffComp_VerbGiver()
		{
			this.verbTracker = new VerbTracker(this);
		}

		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Deep.LookDeep<VerbTracker>(ref this.verbTracker, "verbTracker", new object[]
			{
				this
			});
		}

		public override void CompPostTick()
		{
			base.CompPostTick();
			this.verbTracker.VerbsTick();
		}
	}
}
