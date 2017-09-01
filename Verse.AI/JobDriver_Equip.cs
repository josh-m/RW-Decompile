using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.Sound;

namespace Verse.AI
{
	public class JobDriver_Equip : JobDriver
	{
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					ThingWithComps thingWithComps = (ThingWithComps)this.<>f__this.CurJob.targetA.Thing;
					ThingWithComps thingWithComps2;
					if (thingWithComps.def.stackLimit > 1 && thingWithComps.stackCount > 1)
					{
						thingWithComps2 = (ThingWithComps)thingWithComps.SplitOff(1);
					}
					else
					{
						thingWithComps2 = thingWithComps;
						thingWithComps2.DeSpawn();
					}
					this.<>f__this.pawn.equipment.MakeRoomFor(thingWithComps2);
					this.<>f__this.pawn.equipment.AddEquipment(thingWithComps2);
					if (thingWithComps.def.soundInteract != null)
					{
						thingWithComps.def.soundInteract.PlayOneShot(new TargetInfo(this.<>f__this.pawn.Position, this.<>f__this.pawn.Map, false));
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
