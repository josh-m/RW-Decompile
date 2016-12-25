using System;
using Verse;

namespace RimWorld
{
	public class DrugPolicyEntry : IExposable
	{
		public ThingDef drug;

		public bool allowedForAddiction;

		public bool allowedForJoy;

		public bool allowScheduled;

		public float daysFrequency = 1f;

		public float onlyIfMoodBelow = 1f;

		public float onlyIfJoyBelow = 1f;

		public int takeToInventory;

		public string takeToInventoryTempBuffer;

		public void ExposeData()
		{
			Scribe_Defs.LookDef<ThingDef>(ref this.drug, "drug");
			Scribe_Values.LookValue<bool>(ref this.allowedForAddiction, "allowedForAddiction", false, false);
			Scribe_Values.LookValue<bool>(ref this.allowedForJoy, "allowedForJoy", false, false);
			Scribe_Values.LookValue<bool>(ref this.allowScheduled, "allowScheduled", false, false);
			Scribe_Values.LookValue<float>(ref this.daysFrequency, "daysFrequency", 1f, false);
			Scribe_Values.LookValue<float>(ref this.onlyIfMoodBelow, "onlyIfMoodBelow", 1f, false);
			Scribe_Values.LookValue<float>(ref this.onlyIfJoyBelow, "onlyIfJoyBelow", 1f, false);
			Scribe_Values.LookValue<int>(ref this.takeToInventory, "takeToInventory", 0, false);
		}
	}
}
