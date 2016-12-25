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
			this.FailOn(() => !Find.AreaBuildRoof[this.<>f__this.Cell]);
			this.FailOn(() => !RoofCollapseUtility.WithinRangeOfRoofHolder(this.<>f__this.Cell));
			this.FailOn(() => !RoofCollapseUtility.ConnectedToRoofHolder(this.<>f__this.Cell, true));
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
				if (intVec.InBounds())
				{
					if (Find.AreaBuildRoof[intVec] && !intVec.Roofed() && RoofCollapseUtility.WithinRangeOfRoofHolder(intVec))
					{
						Find.RoofGrid.SetRoof(intVec, RoofDefOf.RoofConstructed);
						MoteMaker.PlaceTempRoof(intVec);
						JobDriver_BuildRoof.builtRoofs.Add(intVec);
					}
				}
			}
			JobDriver_BuildRoof.builtRoofs.Clear();
		}

		protected override bool DoWorkFailOn()
		{
			return base.Cell.Roofed();
		}
	}
}
