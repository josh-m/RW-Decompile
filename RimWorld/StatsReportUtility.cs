using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class StatsReportUtility
	{
		private static StatDrawEntry selectedEntry;

		private static Vector2 scrollPosition;

		private static float listHeight;

		private static List<StatDrawEntry> cachedDrawEntries = new List<StatDrawEntry>();

		public static void Reset()
		{
			StatsReportUtility.scrollPosition = default(Vector2);
			StatsReportUtility.selectedEntry = null;
			StatsReportUtility.cachedDrawEntries.Clear();
		}

		public static void DrawStatsReport(Rect rect, Def def, ThingDef stuff)
		{
			if (StatsReportUtility.cachedDrawEntries.NullOrEmpty<StatDrawEntry>())
			{
				StatsReportUtility.cachedDrawEntries.AddRange(def.SpecialDisplayStats());
				StatsReportUtility.cachedDrawEntries.AddRange(from r in StatsReportUtility.StatsToDraw(def, stuff)
				where r.ShouldDisplay
				select r);
				StatsReportUtility.FinalizeCachedDrawEntries(StatsReportUtility.cachedDrawEntries);
			}
			StatsReportUtility.DrawStatsWorker(rect, null, null);
		}

		public static void DrawStatsReport(Rect rect, Thing thing)
		{
			if (StatsReportUtility.cachedDrawEntries.NullOrEmpty<StatDrawEntry>())
			{
				StatsReportUtility.cachedDrawEntries.AddRange(thing.def.SpecialDisplayStats());
				StatsReportUtility.cachedDrawEntries.AddRange(from r in StatsReportUtility.StatsToDraw(thing)
				where r.ShouldDisplay
				select r);
				StatsReportUtility.cachedDrawEntries.RemoveAll((StatDrawEntry de) => de.stat != null && !de.stat.showNonAbstract);
				StatsReportUtility.FinalizeCachedDrawEntries(StatsReportUtility.cachedDrawEntries);
			}
			StatsReportUtility.DrawStatsWorker(rect, thing, null);
		}

		public static void DrawStatsReport(Rect rect, WorldObject worldObject)
		{
			if (StatsReportUtility.cachedDrawEntries.NullOrEmpty<StatDrawEntry>())
			{
				StatsReportUtility.cachedDrawEntries.AddRange(worldObject.def.SpecialDisplayStats());
				StatsReportUtility.cachedDrawEntries.AddRange(from r in StatsReportUtility.StatsToDraw(worldObject)
				where r.ShouldDisplay
				select r);
				StatsReportUtility.cachedDrawEntries.RemoveAll((StatDrawEntry de) => de.stat != null && !de.stat.showNonAbstract);
				StatsReportUtility.FinalizeCachedDrawEntries(StatsReportUtility.cachedDrawEntries);
			}
			StatsReportUtility.DrawStatsWorker(rect, null, worldObject);
		}

		[DebuggerHidden]
		private static IEnumerable<StatDrawEntry> StatsToDraw(Def def, ThingDef stuff)
		{
			yield return StatsReportUtility.DescriptionEntry(def);
			BuildableDef eDef = def as BuildableDef;
			if (eDef != null)
			{
				foreach (StatDef stat in from st in DefDatabase<StatDef>.AllDefs
				where st.Worker.ShouldShowFor(this.<eDef>__0)
				select st)
				{
					yield return new StatDrawEntry(stat.category, stat, eDef.GetStatValueAbstract(stat, stuff), StatRequest.For(eDef, stuff, QualityCategory.Normal), ToStringNumberSense.Undefined);
				}
			}
		}

		[DebuggerHidden]
		private static IEnumerable<StatDrawEntry> StatsToDraw(Thing thing)
		{
			yield return StatsReportUtility.DescriptionEntry(thing);
			StatDrawEntry qe = StatsReportUtility.QualityEntry(thing);
			if (qe != null)
			{
				yield return qe;
			}
			foreach (StatDef stat in from st in DefDatabase<StatDef>.AllDefs
			where st.Worker.ShouldShowFor(this.thing.def)
			select st)
			{
				yield return new StatDrawEntry(stat.category, stat, thing.GetStatValue(stat, true), StatRequest.For(thing), ToStringNumberSense.Undefined);
			}
			if (thing.def.useHitPoints)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "HitPointsBasic".Translate().CapitalizeFirst(), thing.HitPoints.ToString() + " / " + thing.MaxHitPoints.ToString(), 0)
				{
					overrideReportText = string.Concat(new string[]
					{
						"HitPointsBasic".Translate().CapitalizeFirst(),
						":\n\n",
						thing.HitPoints.ToString(),
						"\n\n",
						StatDefOf.MaxHitPoints.LabelCap,
						":\n\n",
						StatDefOf.MaxHitPoints.Worker.GetExplanation(StatRequest.For(thing), ToStringNumberSense.Absolute)
					})
				};
			}
			foreach (StatDrawEntry stat2 in thing.SpecialDisplayStats)
			{
				yield return stat2;
			}
			if (!thing.def.equippedStatOffsets.NullOrEmpty<StatModifier>())
			{
				for (int i = 0; i < thing.def.equippedStatOffsets.Count; i++)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.EquippedStatOffsets, thing.def.equippedStatOffsets[i].stat, thing.def.equippedStatOffsets[i].value, StatRequest.ForEmpty(), ToStringNumberSense.Offset);
				}
			}
			if (thing.def.IsStuff)
			{
				if (!thing.def.stuffProps.statFactors.NullOrEmpty<StatModifier>())
				{
					for (int j = 0; j < thing.def.stuffProps.statFactors.Count; j++)
					{
						yield return new StatDrawEntry(StatCategoryDefOf.StuffStatFactors, thing.def.stuffProps.statFactors[j].stat, thing.def.stuffProps.statFactors[j].value, StatRequest.ForEmpty(), ToStringNumberSense.Factor);
					}
				}
				if (!thing.def.stuffProps.statOffsets.NullOrEmpty<StatModifier>())
				{
					for (int k = 0; k < thing.def.stuffProps.statOffsets.Count; k++)
					{
						yield return new StatDrawEntry(StatCategoryDefOf.StuffStatOffsets, thing.def.stuffProps.statOffsets[k].stat, thing.def.stuffProps.statOffsets[k].value, StatRequest.ForEmpty(), ToStringNumberSense.Offset);
					}
				}
			}
		}

		[DebuggerHidden]
		private static IEnumerable<StatDrawEntry> StatsToDraw(WorldObject worldObject)
		{
			yield return StatsReportUtility.DescriptionEntry(worldObject);
			foreach (StatDrawEntry stat in worldObject.SpecialDisplayStats)
			{
				yield return stat;
			}
		}

		private static void FinalizeCachedDrawEntries(IEnumerable<StatDrawEntry> original)
		{
			StatsReportUtility.cachedDrawEntries = (from sd in original
			orderby sd.category.displayOrder, sd.DisplayPriorityWithinCategory descending, sd.LabelCap
			select sd).ToList<StatDrawEntry>();
			if (StatsReportUtility.cachedDrawEntries.Count > 0)
			{
				StatsReportUtility.SelectEntry(StatsReportUtility.cachedDrawEntries[0], false);
			}
		}

		private static StatDrawEntry DescriptionEntry(Def def)
		{
			return new StatDrawEntry(StatCategoryDefOf.Basics, "Description".Translate(), string.Empty, 99999)
			{
				overrideReportText = def.description
			};
		}

		private static StatDrawEntry DescriptionEntry(Thing thing)
		{
			return new StatDrawEntry(StatCategoryDefOf.Basics, "Description".Translate(), string.Empty, 99999)
			{
				overrideReportText = thing.GetDescription()
			};
		}

		private static StatDrawEntry DescriptionEntry(WorldObject worldObject)
		{
			return new StatDrawEntry(StatCategoryDefOf.Basics, "Description".Translate(), string.Empty, 99999)
			{
				overrideReportText = worldObject.GetDescription()
			};
		}

		private static StatDrawEntry QualityEntry(Thing t)
		{
			QualityCategory cat;
			if (!t.TryGetQuality(out cat))
			{
				return null;
			}
			return new StatDrawEntry(StatCategoryDefOf.Basics, "Quality".Translate(), cat.GetLabel().CapitalizeFirst(), 99999)
			{
				overrideReportText = "QualityDescription".Translate()
			};
		}

		private static void SelectEntry(StatDrawEntry rec, bool playSound = true)
		{
			StatsReportUtility.selectedEntry = rec;
			if (playSound)
			{
				SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
			}
		}

		private static void DrawStatsWorker(Rect rect, Thing optionalThing, WorldObject optionalWorldObject)
		{
			Rect outRect = new Rect(rect);
			outRect.width *= 0.5f;
			Rect rect2 = new Rect(rect);
			rect2.x = outRect.xMax;
			rect2.width = rect.xMax - rect2.x;
			Text.Font = GameFont.Small;
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, StatsReportUtility.listHeight);
			Widgets.BeginScrollView(outRect, ref StatsReportUtility.scrollPosition, viewRect, true);
			float num = 0f;
			string b = null;
			foreach (StatDrawEntry ent in StatsReportUtility.cachedDrawEntries)
			{
				if (ent.category.LabelCap != b)
				{
					Widgets.ListSeparator(ref num, viewRect.width, ent.category.LabelCap);
					b = ent.category.LabelCap;
				}
				num += ent.Draw(8f, num, viewRect.width - 8f, StatsReportUtility.selectedEntry == ent, delegate
				{
					StatsReportUtility.SelectEntry(ent, true);
				});
			}
			StatsReportUtility.listHeight = num + 100f;
			Widgets.EndScrollView();
			Rect rect3 = rect2.ContractedBy(10f);
			GUI.BeginGroup(rect3);
			if (StatsReportUtility.selectedEntry != null)
			{
				StatRequest optionalReq;
				if (StatsReportUtility.selectedEntry.hasOptionalReq)
				{
					optionalReq = StatsReportUtility.selectedEntry.optionalReq;
				}
				else if (optionalThing != null)
				{
					optionalReq = StatRequest.For(optionalThing);
				}
				else
				{
					optionalReq = StatRequest.ForEmpty();
				}
				string explanationText = StatsReportUtility.selectedEntry.GetExplanationText(optionalReq);
				Rect rect4 = rect3.AtZero();
				Widgets.Label(rect4, explanationText);
			}
			GUI.EndGroup();
		}
	}
}
