using System;

namespace Verse
{
	public static class HediffUtility
	{
		public static T TryGetComp<T>(this Hediff hd) where T : HediffComp
		{
			HediffWithComps hediffWithComps = hd as HediffWithComps;
			if (hediffWithComps == null)
			{
				return (T)((object)null);
			}
			if (hediffWithComps.comps != null)
			{
				for (int i = 0; i < hediffWithComps.comps.Count; i++)
				{
					T t = hediffWithComps.comps[i] as T;
					if (t != null)
					{
						return t;
					}
				}
			}
			return (T)((object)null);
		}

		public static bool IsTended(this Hediff hd)
		{
			HediffWithComps hediffWithComps = hd as HediffWithComps;
			if (hediffWithComps == null)
			{
				return false;
			}
			HediffComp_Tendable hediffComp_Tendable = hediffWithComps.TryGetComp<HediffComp_Tendable>();
			return hediffComp_Tendable != null && hediffComp_Tendable.IsTended;
		}

		public static bool IsTendedWell(this Hediff hd)
		{
			HediffWithComps hediffWithComps = hd as HediffWithComps;
			if (hediffWithComps == null)
			{
				return false;
			}
			HediffComp_Tendable hediffComp_Tendable = hediffWithComps.TryGetComp<HediffComp_Tendable>();
			return hediffComp_Tendable != null && hediffComp_Tendable.IsTendedWell;
		}

		public static bool IsOld(this Hediff hd)
		{
			HediffWithComps hediffWithComps = hd as HediffWithComps;
			if (hediffWithComps == null)
			{
				return false;
			}
			HediffComp_GetsOld hediffComp_GetsOld = hediffWithComps.TryGetComp<HediffComp_GetsOld>();
			return hediffComp_GetsOld != null && hediffComp_GetsOld.IsOld;
		}

		public static bool FullyImmune(this Hediff hd)
		{
			HediffWithComps hediffWithComps = hd as HediffWithComps;
			if (hediffWithComps == null)
			{
				return false;
			}
			HediffComp_Immunizable hediffComp_Immunizable = hediffWithComps.TryGetComp<HediffComp_Immunizable>();
			return hediffComp_Immunizable != null && hediffComp_Immunizable.FullyImmune;
		}

		public static bool IsTendedAndHealing(this Hediff hd)
		{
			return hd.IsTended() && !hd.IsOld();
		}

		public static bool NotNaturallyHealingBecauseNeedsTending(this Hediff hd)
		{
			return hd.FullyHealableOnlyByTend() && hd.Severity <= 2f;
		}

		public static bool FullyHealableOnlyByTend(this Hediff hd)
		{
			return hd.def.naturallyHealed && !hd.IsTended() && hd.def.injuryProps.fullyHealableOnlyByTend;
		}

		public static bool IsNaturallyHealing(this Hediff hd)
		{
			return !hd.NotNaturallyHealingBecauseNeedsTending() && hd.def.naturallyHealed && !hd.IsOld();
		}
	}
}
