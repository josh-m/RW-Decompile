using System;
using Verse;

namespace RimWorld
{
	public class Alert_LowMedicine : Alert_High
	{
		private const float MedicinePerColonistThreshold = 2f;

		private int MedicineCount
		{
			get
			{
				return Find.ResourceCounter.GetCountIn(ThingRequestGroup.Medicine);
			}
		}

		public override string FullExplanation
		{
			get
			{
				int medicineCount = this.MedicineCount;
				if (medicineCount == 0)
				{
					return string.Format("NoMedicineDesc".Translate(), new object[0]);
				}
				return string.Format("LowMedicineDesc".Translate(), medicineCount);
			}
		}

		public override AlertReport Report
		{
			get
			{
				if (Find.TickManager.TicksGame < 150000)
				{
					return false;
				}
				return (float)this.MedicineCount < 2f * (float)Find.MapPawns.FreeColonistsSpawnedCount;
			}
		}

		public Alert_LowMedicine()
		{
			this.baseLabel = "LowMedicine".Translate();
		}
	}
}
