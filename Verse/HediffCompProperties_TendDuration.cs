using System;
using UnityEngine;

namespace Verse
{
	public class HediffCompProperties_TendDuration : HediffCompProperties
	{
		private float baseTendDurationHours = -1f;

		private float tendOverlapHours = 3f;

		public bool tendAllAtOnce;

		public int disappearsAtTotalTendQuality = -1;

		public float severityPerDayTended;

		public bool showTendQuality = true;

		[LoadAlias("labelTreatedWell")]
		public string labelTendedWell;

		[LoadAlias("labelTreatedWellInner")]
		public string labelTendedWellInner;

		[LoadAlias("labelSolidTreatedWell")]
		public string labelSolidTendedWell;

		public bool TendIsPermanent
		{
			get
			{
				return this.baseTendDurationHours < 0f;
			}
		}

		public int TendTicksFull
		{
			get
			{
				if (this.TendIsPermanent)
				{
					Log.ErrorOnce("Queried TendTicksFull on permanent-tend Hediff.", 6163263, false);
				}
				return Mathf.RoundToInt((this.baseTendDurationHours + this.tendOverlapHours) * 2500f);
			}
		}

		public int TendTicksBase
		{
			get
			{
				if (this.TendIsPermanent)
				{
					Log.ErrorOnce("Queried TendTicksBase on permanent-tend Hediff.", 61621263, false);
				}
				return Mathf.RoundToInt(this.baseTendDurationHours * 2500f);
			}
		}

		public int TendTicksOverlap
		{
			get
			{
				if (this.TendIsPermanent)
				{
					Log.ErrorOnce("Queried TendTicksOverlap on permanent-tend Hediff.", 1963263, false);
				}
				return Mathf.RoundToInt(this.tendOverlapHours * 2500f);
			}
		}

		public HediffCompProperties_TendDuration()
		{
			this.compClass = typeof(HediffComp_TendDuration);
		}
	}
}
