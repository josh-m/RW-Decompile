using System;
using Verse;

namespace RimWorld
{
	public class JobDriver_RemoveFloor : JobDriver_AffectFloor
	{
		protected override int BaseWorkAmount
		{
			get
			{
				return 200;
			}
		}

		protected override DesignationDef DesDef
		{
			get
			{
				return DesignationDefOf.RemoveFloor;
			}
		}

		protected override StatDef SpeedStat
		{
			get
			{
				return StatDefOf.ConstructionSpeed;
			}
		}

		protected override void DoEffect(IntVec3 c)
		{
			base.Map.terrainGrid.RemoveTopLayer(base.TargetLocA, true);
		}
	}
}
