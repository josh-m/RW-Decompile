using System;
using UnityEngine;

namespace Verse
{
	public class HediffComp_GetsOld : HediffComp
	{
		public float oldDamageThreshold = 9999f;

		public bool isOldInt;

		public float painFactor = 1f;

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

		public override void CompExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.isOldInt, "isOld", false, false);
			Scribe_Values.LookValue<float>(ref this.oldDamageThreshold, "oldDamageThreshold", 9999f, false);
			Scribe_Values.LookValue<float>(ref this.painFactor, "painFactor", 1f, false);
		}

		public override void CompPostDirectHeal(float amount)
		{
			if (!this.IsOld && this.oldDamageThreshold <= this.parent.Severity + amount && this.oldDamageThreshold >= this.parent.Severity)
			{
				HediffComp_Tendable hediffComp_Tendable = this.parent.TryGetComp<HediffComp_Tendable>();
				float num = (hediffComp_Tendable == null) ? 0f : hediffComp_Tendable.tendQuality;
				float num2 = Mathf.Clamp01(1f - num);
				if (num2 < 0.9f)
				{
					num2 *= 0.65f;
				}
				if (Rand.Value < num2)
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
