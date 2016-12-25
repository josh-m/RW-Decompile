using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PlantHarvest : JobDriver_PlantWork
	{
		protected override void Init()
		{
			this.xpPerTick = 0.11f;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil toil in base.MakeNewToils())
			{
				yield return toil;
			}
			yield return Toils_General.RemoveDesignationsOnThing(TargetIndex.A, DesignationDefOf.HarvestPlant);
		}
	}
}
