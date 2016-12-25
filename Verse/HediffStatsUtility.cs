using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class HediffStatsUtility
	{
		[DebuggerHidden]
		public static IEnumerable<StatDrawEntry> SpecialDisplayStats(HediffStage stage, Hediff instance)
		{
			float painOffsetToDisplay = 0f;
			if (instance != null)
			{
				painOffsetToDisplay = instance.PainOffset;
			}
			else if (stage != null)
			{
				painOffsetToDisplay = stage.painOffset;
			}
			if (painOffsetToDisplay != 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Pain".Translate(), (painOffsetToDisplay * 100f).ToString("+#;-#") + "%", 0);
			}
			float painFactorToDisplay = 1f;
			if (instance != null)
			{
				painFactorToDisplay = instance.PainFactor;
			}
			else if (stage != null)
			{
				painFactorToDisplay = stage.painFactor;
			}
			if (painFactorToDisplay != 1f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Pain".Translate(), "x" + painFactorToDisplay.ToStringPercent(), 0);
			}
			List<PawnCapacityModifier> capModsToDisplay = null;
			if (instance != null)
			{
				capModsToDisplay = instance.CapMods;
			}
			else if (stage != null)
			{
				capModsToDisplay = stage.capMods;
			}
			if (capModsToDisplay != null)
			{
				for (int i = 0; i < capModsToDisplay.Count; i++)
				{
					if (!Mathf.Approximately(capModsToDisplay[i].offset, 0f))
					{
						yield return new StatDrawEntry(StatCategoryDefOf.Basics, capModsToDisplay[i].capacity.GetLabelFor(true, true).CapitalizeFirst(), (capModsToDisplay[i].offset * 100f).ToString("+#;-#") + "%", 0);
					}
					if (capModsToDisplay[i].SetMaxDefined)
					{
						yield return new StatDrawEntry(StatCategoryDefOf.Basics, capModsToDisplay[i].capacity.GetLabelFor(true, true).CapitalizeFirst(), "max".Translate() + " " + capModsToDisplay[i].setMax.ToStringPercent(), 0);
					}
				}
			}
			if (stage != null)
			{
				if (stage.AffectsMemory || stage.AffectsSocialInteractions)
				{
					StringBuilder affectsSb = new StringBuilder();
					if (stage.AffectsMemory)
					{
						if (affectsSb.Length != 0)
						{
							affectsSb.Append(", ");
						}
						affectsSb.Append("MemoryLower".Translate());
					}
					if (stage.AffectsSocialInteractions)
					{
						if (affectsSb.Length != 0)
						{
							affectsSb.Append(", ");
						}
						affectsSb.Append("SocialInteractionsLower".Translate());
					}
					yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Affects".Translate(), affectsSb.ToString(), 0);
				}
				if (stage.hungerRateFactor != 1f)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Basics, "HungerRate".Translate(), "x" + stage.hungerRateFactor.ToStringPercent(), 0);
				}
				if (stage.restFallFactor != 1f)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Tiredness".Translate(), "x" + stage.restFallFactor.ToStringPercent(), 0);
				}
				if (stage.makeImmuneTo != null)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Basics, "PreventsInfection".Translate(), stage.makeImmuneTo.LabelCap, 0);
				}
				if (stage.statOffsets != null)
				{
					for (int j = 0; j < stage.statOffsets.Count; j++)
					{
						StatModifier sm = stage.statOffsets[j];
						yield return new StatDrawEntry(StatCategoryDefOf.Basics, sm.stat.LabelCap, sm.ToStringAsOffset, 0);
					}
				}
			}
			if (instance != null && instance.BleedRate > 0.001f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "BleedingRate".Translate(), (instance.BleedRate / instance.MaxBleeding).ToStringPercent(), 0);
			}
		}
	}
}
