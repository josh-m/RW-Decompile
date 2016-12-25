using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			if (!myPawn.CanReserve(this.parent, 1))
			{
				yield return new FloatMenuOption(this.FloatMenuOptionLabel + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Medium, null, null, 0f, null);
			}
			else
			{
				FloatMenuOption useopt = new FloatMenuOption(this.FloatMenuOptionLabel, delegate
				{
					if (this.myPawn.CanReserveAndReach(this.<>f__this.parent, PathEndMode.Touch, Danger.Deadly, 1))
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
				}, MenuOptionPriority.Medium, null, null, 0f, null);
				yield return useopt;
			}
		}

		public void TryStartUseJob(Pawn user)
		{
			if (!user.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1))
			{
				return;
			}
			Job newJob = new Job(this.Props.useJob, this.parent);
			user.drafter.TakeOrderedJob(newJob);
		}

		public void UsedBy(Pawn p)
		{
			foreach (CompUseEffect current in this.parent.GetComps<CompUseEffect>())
			{
				current.DoEffect(p);
			}
		}
	}
}
