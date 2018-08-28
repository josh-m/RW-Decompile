using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_StackCount : ThingSetMaker
	{
		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			if (!this.AllowedThingDefs(parms).Any<ThingDef>())
			{
				return false;
			}
			IntRange? countRange = parms.countRange;
			if (countRange.HasValue && parms.countRange.Value.max <= 0)
			{
				return false;
			}
			float? maxTotalMass = parms.maxTotalMass;
			if (maxTotalMass.HasValue && parms.maxTotalMass != 3.40282347E+38f)
			{
				IEnumerable<ThingDef> arg_E4_0 = this.AllowedThingDefs(parms);
				TechLevel? techLevel = parms.techLevel;
				TechLevel arg_E4_1 = (!techLevel.HasValue) ? TechLevel.Undefined : techLevel.Value;
				float arg_E4_2 = parms.maxTotalMass.Value;
				IntRange? countRange2 = parms.countRange;
				if (!ThingSetMakerUtility.PossibleToWeighNoMoreThan(arg_E4_0, arg_E4_1, arg_E4_2, (!countRange2.HasValue) ? 1 : parms.countRange.Value.max))
				{
					return false;
				}
			}
			return true;
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			IEnumerable<ThingDef> enumerable = this.AllowedThingDefs(parms);
			if (!enumerable.Any<ThingDef>())
			{
				return;
			}
			TechLevel? techLevel = parms.techLevel;
			TechLevel stuffTechLevel = (!techLevel.HasValue) ? TechLevel.Undefined : techLevel.Value;
			IntRange? countRange = parms.countRange;
			IntRange intRange = (!countRange.HasValue) ? IntRange.one : countRange.Value;
			float? maxTotalMass = parms.maxTotalMass;
			float num = (!maxTotalMass.HasValue) ? 3.40282347E+38f : maxTotalMass.Value;
			int num2 = Mathf.Max(intRange.RandomInRange, 1);
			float num3 = 0f;
			int i = num2;
			while (i > 0)
			{
				ThingStuffPair thingStuffPair;
				if (!ThingSetMakerUtility.TryGetRandomThingWhichCanWeighNoMoreThan(enumerable, stuffTechLevel, (num != 3.40282347E+38f) ? (num - num3) : 3.40282347E+38f, out thingStuffPair))
				{
					break;
				}
				Thing thing = ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
				ThingSetMakerUtility.AssignQuality(thing, parms.qualityGenerator);
				int num4 = i;
				if (num != 3.40282347E+38f && !(thing is Pawn))
				{
					num4 = Mathf.Min(num4, Mathf.FloorToInt((num - num3) / thing.GetStatValue(StatDefOf.Mass, true)));
				}
				num4 = Mathf.Clamp(num4, 1, thing.def.stackLimit);
				thing.stackCount = num4;
				i -= num4;
				outThings.Add(thing);
				if (!(thing is Pawn))
				{
					num3 += thing.GetStatValue(StatDefOf.Mass, true) * (float)thing.stackCount;
				}
			}
		}

		protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
		{
			return ThingSetMakerUtility.GetAllowedThingDefs(parms);
		}

		[DebuggerHidden]
		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			TechLevel? techLevel2 = parms.techLevel;
			TechLevel techLevel = (!techLevel2.HasValue) ? TechLevel.Undefined : techLevel2.Value;
			foreach (ThingDef t in this.AllowedThingDefs(parms))
			{
				float? maxTotalMass = parms.maxTotalMass;
				if (maxTotalMass.HasValue && parms.maxTotalMass != 3.40282347E+38f)
				{
					float? maxTotalMass2 = parms.maxTotalMass;
					if (ThingSetMakerUtility.GetMinMass(t, techLevel) > maxTotalMass2)
					{
						continue;
					}
				}
				yield return t;
			}
		}
	}
}
