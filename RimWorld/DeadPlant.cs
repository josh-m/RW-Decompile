using System;

namespace RimWorld
{
	public class DeadPlant : Plant
	{
		protected override bool Resting
		{
			get
			{
				return false;
			}
		}

		public override float GrowthRate
		{
			get
			{
				return 0f;
			}
		}

		public override float CurrentDyingDamagePerTick
		{
			get
			{
				return 0f;
			}
		}

		public override string LabelMouseover
		{
			get
			{
				return this.LabelCap;
			}
		}

		public override PlantLifeStage LifeStage
		{
			get
			{
				return PlantLifeStage.Mature;
			}
		}

		public override string GetInspectStringLowPriority()
		{
			return null;
		}

		public override string GetInspectString()
		{
			return string.Empty;
		}

		public override void CropBlighted()
		{
		}
	}
}
