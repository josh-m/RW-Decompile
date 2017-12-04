using System;
using Verse;

namespace RimWorld
{
	public class JobDriver_SmoothFloor : JobDriver_AffectFloor
	{
		protected override int BaseWorkAmount
		{
			get
			{
				return 1400;
			}
		}

		protected override DesignationDef DesDef
		{
			get
			{
				return DesignationDefOf.SmoothFloor;
			}
		}

		protected override StatDef SpeedStat
		{
			get
			{
				return StatDefOf.SmoothingSpeed;
			}
		}

		public JobDriver_SmoothFloor()
		{
			this.clearSnow = true;
		}

		protected override void DoEffect(IntVec3 c)
		{
			TerrainDef smoothedTerrain = base.TargetLocA.GetTerrain(base.Map).smoothedTerrain;
			base.Map.terrainGrid.SetTerrain(base.TargetLocA, smoothedTerrain);
			FilthMaker.RemoveAllFilth(base.TargetLocA, base.Map);
		}
	}
}
