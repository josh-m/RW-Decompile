using System;
using UnityEngine;

namespace Verse
{
	public class HediffComp_GetsOld : HediffComp
	{
		public float oldDamageThreshold = 9999f;

		public bool isOldInt;

		public float painFactor = 1f;

		public HediffCompProperties_GetsOld Props
		{
			get
			{
				return (HediffCompProperties_GetsOld)this.props;
			}
		}

		public bool IsOld
		{
			get
			{
				return this.isOldInt;
			}
			set
			{
				if (value == this.isOldInt)
				{
					return;
				}
				this.isOldInt = value;
				if (this.isOldInt)
				{
					this.painFactor = OldInjuryUtility.GetRandomPainFactor();
				}
			}
		}

		private bool Active
		{
			get
			{
				return this.oldDamageThreshold < 9000f;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.isOldInt, "isOld", false, false);
			Scribe_Values.LookValue<float>(ref this.oldDamageThreshold, "oldDamageThreshold", 9999f, false);
			Scribe_Values.LookValue<float>(ref this.painFactor, "painFactor", 1f, false);
		}

		public override void CompPostInjuryHeal(float amount)
		{
			if (!this.Active || this.IsOld)
			{
				return;
			}
			if (this.parent.Severity <= this.oldDamageThreshold && this.parent.Severity >= this.oldDamageThreshold - amount)
			{
				float num = 0.2f;
				HediffComp_TendDuration hediffComp_TendDuration = this.parent.TryGetComp<HediffComp_TendDuration>();
				if (hediffComp_TendDuration != null)
				{
					num *= Mathf.Clamp01(1f - hediffComp_TendDuration.tendQuality);
				}
				if (Rand.Value < num)
				{
					this.parent.Severity = this.oldDamageThreshold;
					this.IsOld = true;
				}
				else
				{
					this.oldDamageThreshold = 9999f;
				}
			}
		}

		public override string CompDebugString()
		{
			return string.Concat(new object[]
			{
				"isOld: ",
				this.isOldInt,
				"\noldDamageThreshold: ",
				this.oldDamageThreshold,
				"\npainFactor: ",
				this.painFactor
			});
		}
	}
}
