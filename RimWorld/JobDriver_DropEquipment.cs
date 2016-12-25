using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_DropEquipment : JobDriver
	{
		private const int DurationTicks = 30;

		private ThingWithComps TargetEquipment
		{
			get
			{
				return (ThingWithComps)base.TargetA.Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				initAction = delegate
				{
					this.<>f__this.pawn.pather.StopDead();
				},
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 30
			};
			yield return new Toil
			{
				initAction = delegate
				{
					ThingWithComps thingWithComps;
					if (!this.<>f__this.pawn.equipment.TryDropEquipment(this.<>f__this.TargetEquipment, out thingWithComps, this.<>f__this.pawn.Position, true))
					{
						this.<>f__this.EndJobWith(JobCondition.Incompletable);
					}
				}
			};
		}
	}
}
