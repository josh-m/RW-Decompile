using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class ImmunityRecord : IExposable
	{
		public HediffDef hediffDef;

		public float immunity;

		public float ImmunityChangePerTick(Pawn pawn, bool sick, Hediff diseaseInstance)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return 0f;
			}
			HediffCompProperties_Immunizable hediffCompProperties_Immunizable = this.hediffDef.CompProps<HediffCompProperties_Immunizable>();
			float num = (!sick) ? hediffCompProperties_Immunizable.immunityPerDayNotSick : hediffCompProperties_Immunizable.immunityPerDaySick;
			num /= 60000f;
			float num2 = pawn.GetStatValue(StatDefOf.ImmunityGainSpeed, true);
			if (diseaseInstance != null)
			{
				int value = Gen.HashCombineInt(diseaseInstance.GetHashCode(), 156482735);
				num2 *= Mathf.Lerp(0.8f, 1.2f, (float)Mathf.Abs(value) / 2.14748365E+09f);
			}
			if (num > 0f)
			{
				return num * num2;
			}
			return num / num2;
		}

		public void ImmunityTick(Pawn pawn, bool sick, Hediff diseaseInstance)
		{
			this.immunity += this.ImmunityChangePerTick(pawn, sick, diseaseInstance);
			this.immunity = Mathf.Clamp01(this.immunity);
		}

		public void ExposeData()
		{
			Scribe_Defs.LookDef<HediffDef>(ref this.hediffDef, "hediffDef");
			Scribe_Values.LookValue<float>(ref this.immunity, "immunity", 0f, false);
		}
	}
}
