using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PlantCut : JobDriver_PlantWork
	{
		protected override void Init()
		{
			if (base.Plant.def.plant.harvestedThingDef != null && base.Plant.YieldNow() > 0)
			{
				this.xpPerTick = 0.11f;
			}
			else
			{
				this.xpPerTick = 0f;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil toil in base.MakeNewToils())
			{
				yield return toil;
			}
			yield return Toils_Interact.DestroyThing(TargetIndex.A);
		}
	}
}
