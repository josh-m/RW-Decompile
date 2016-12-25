using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class JoyToleranceSet : IExposable
	{
		private const float ToleranceGainRate = 0.4f;

		private const float ToleranceDropPerDay = 0.0833333358f;

		private DefMap<JoyKindDef, float> tolerances = new DefMap<JoyKindDef, float>();

		public float this[JoyKindDef d]
		{
			get
			{
				return this.tolerances[d];
			}
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<DefMap<JoyKindDef, float>>(ref this.tolerances, "tolerances", new object[0]);
		}

		public void Notify_JoyGained(float amount, JoyKindDef joyKind)
		{
			this.tolerances[joyKind] = Mathf.Min(this.tolerances[joyKind] + amount * 0.4f, 1f);
		}

		public float JoyFactorFromTolerance(JoyKindDef joyKind)
		{
			return 1f - this.tolerances[joyKind];
		}

		public void NeedInterval()
		{
			for (int i = 0; i < this.tolerances.Count; i++)
			{
				float num = this.tolerances[i];
				num -= 0.000208333338f;
				if (num < 0f)
				{
					num = 0f;
				}
				this.tolerances[i] = num;
			}
		}

		public string TolerancesString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("\n" + "JoyTolerances".Translate() + ":");
			List<JoyKindDef> allDefsListForReading = DefDatabase<JoyKindDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				JoyKindDef joyKindDef = allDefsListForReading[i];
				float num = this.tolerances[joyKindDef];
				if (num > 0.01f)
				{
					stringBuilder.AppendLine("   -" + joyKindDef.label + ": " + num.ToStringPercent());
				}
			}
			return stringBuilder.ToString();
		}
	}
}
