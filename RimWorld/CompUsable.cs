using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompUsable : ThingComp
	{
		protected CompProperties_Usable Props
		{
			get
			{
				return (CompProperties_Usable)this.props;
			}
		}

		protected virtual string FloatMenuOptionLabel
		{
			get
			{
				return this.Props.useLabel;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
		{
			if (!myPawn.CanReserve(this.parent, 1, -1, null, false))
			{
				yield return new FloatMenuOption(this.FloatMenuOptionLabel + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
			}
			else
			{
				FloatMenuOption useopt = new FloatMenuOption(this.FloatMenuOptionLabel, delegate
				{
					if (this.myPawn.CanReserveAndReach(this.<>f__this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
					{
						foreach (CompUseEffect current in this.<>f__this.parent.GetComps<CompUseEffect>())
						{
							if (current.SelectedUseOption(this.myPawn))
							{
								return;
							}
						}
						this.<>f__this.TryStartUseJob(this.myPawn);
					}
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				yield return useopt;
			}
		}

		public void TryStartUseJob(Pawn user)
		{
			if (!user.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
			{
				return;
			}
			Job job = new Job(this.Props.useJob, this.parent);
			user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}

		public void UsedBy(Pawn p)
		{
			foreach (CompUseEffect current in from x in this.parent.GetComps<CompUseEffect>()
			orderby x.OrderPriority descending
			select x)
			{
				try
				{
					current.DoEffect(p);
				}
				catch (Exception arg)
				{
					Log.Error("Error in CompUseEffect: " + arg);
				}
			}
		}
	}
}
