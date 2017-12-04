using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_BuildRoof : JobDriver_AffectRoof
	{
		private static List<IntVec3> builtRoofs = new List<IntVec3>();

		protected override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !this.$this.Map.areaManager.BuildRoof[this.$this.Cell]);
			this.FailOn(() => !RoofCollapseUtility.WithinRangeOfRoofHolder(this.$this.Cell, this.$this.Map));
			this.FailOn(() => !RoofCollapseUtility.ConnectedToRoofHolder(this.$this.Cell, this.$this.Map, true));
			foreach (Toil t in base.MakeNewToils())
			{
				yield return t;
			}
		}

		protected override void DoEffect()
		{
			JobDriver_BuildRoof.builtRoofs.Clear();
			for (int i = 0; i < 9; i++)
			{
				IntVec3 intVec = base.Cell + GenAdj.AdjacentCellsAndInside[i];
				if (intVec.InBounds(base.Map))
				{
					if (base.Map.areaManager.BuildRoof[intVec] && !intVec.Roofed(base.Map) && RoofCollapseUtility.WithinRangeOfRoofHolder(intVec, base.Map))
					{
						base.Map.roofGrid.SetRoof(intVec, RoofDefOf.RoofConstructed);
						MoteMaker.PlaceTempRoof(intVec, base.Map);
						JobDriver_BuildRoof.builtRoofs.Add(intVec);
					}
				}
			}
			JobDriver_BuildRoof.builtRoofs.Clear();
		}

		protected override bool DoWorkFailOn()
		{
			return base.Cell.Roofed(base.Map);
		}
	}
}
