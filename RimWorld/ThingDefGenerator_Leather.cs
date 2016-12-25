using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ThingDefGenerator_Leather
	{
		private const float HumanlikeLeatherCommonalityFactor = 0.01f;

		private static bool GeneratesLeather(ThingDef sourceDef)
		{
			return sourceDef.category == ThingCategory.Pawn && sourceDef.GetStatValueAbstract(StatDefOf.LeatherAmount, null) > 0f;
		}

		[DebuggerHidden]
		public static IEnumerable<ThingDef> ImpliedLeatherDefs()
		{
			int numLeathers = (from def in DefDatabase<ThingDef>.AllDefs
			where ThingDefGenerator_Leather.GeneratesLeather(def)
			select def).Count<ThingDef>();
			float eachLeatherCommonality = 1f / (float)numLeathers;
			foreach (ThingDef sourceDef in DefDatabase<ThingDef>.AllDefs.ToList<ThingDef>())
			{
				if (ThingDefGenerator_Leather.GeneratesLeather(sourceDef))
				{
					if (sourceDef.race.useLeatherFrom == null)
					{
						ThingDef d = new ThingDef();
						d.resourceReadoutPriority = ResourceCountPriority.Middle;
						d.category = ThingCategory.Item;
						d.thingClass = typeof(ThingWithComps);
						d.graphicData = new GraphicData();
						d.graphicData.graphicClass = typeof(Graphic_Single);
						d.useHitPoints = true;
						d.selectable = true;
						d.SetStatBaseValue(StatDefOf.MaxHitPoints, 100f);
						d.altitudeLayer = AltitudeLayer.Item;
						d.stackLimit = 75;
						d.comps.Add(new CompProperties_Forbiddable());
						d.SetStatBaseValue(StatDefOf.Beauty, -30f);
						d.alwaysHaulable = true;
						d.drawGUIOverlay = true;
						d.rotatable = false;
						d.pathCost = 15;
						d.category = ThingCategory.Item;
						d.description = "LeatherDesc".Translate(new object[]
						{
							sourceDef.label
						});
						d.useHitPoints = true;
						d.SetStatBaseValue(StatDefOf.MaxHitPoints, 100f);
						d.SetStatBaseValue(StatDefOf.MarketValue, sourceDef.race.leatherMarketValueFactor * 3.1f);
						if (d.thingCategories == null)
						{
							d.thingCategories = new List<ThingCategoryDef>();
						}
						CrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, "Leathers");
						d.graphicData.texPath = "Things/Item/Resource/Cloth";
						d.stuffProps = new StuffProperties();
						CrossRefLoader.RegisterListWantsCrossRef<StuffCategoryDef>(d.stuffProps.categories, "Leathery");
						StatUtility.SetStatValueInList(ref d.stuffProps.statFactors, StatDefOf.MarketValue, 1.3f);
						StatUtility.SetStatValueInList(ref d.stuffProps.statFactors, StatDefOf.ArmorRating_Blunt, 1.5f);
						StatUtility.SetStatValueInList(ref d.stuffProps.statFactors, StatDefOf.ArmorRating_Sharp, 1.5f);
						StatUtility.SetStatValueInList(ref d.stuffProps.statFactors, StatDefOf.ArmorRating_Heat, 1.7f);
						StatUtility.SetStatValueInList(ref d.stuffProps.statFactors, StatDefOf.ArmorRating_Electric, 4f);
						d.defName = sourceDef.defName + "_Leather";
						if (!sourceDef.race.leatherLabel.NullOrEmpty())
						{
							d.label = sourceDef.race.leatherLabel;
						}
						else
						{
							d.label = "LeatherLabel".Translate(new object[]
							{
								sourceDef.label
							});
						}
						d.stuffProps.color = sourceDef.race.leatherColor;
						d.graphicData.color = sourceDef.race.leatherColor;
						d.graphicData.colorTwo = sourceDef.race.leatherColor;
						d.stuffProps.commonality = eachLeatherCommonality * sourceDef.race.leatherCommonalityFactor;
						StatUtility.SetStatValueInList(ref d.stuffProps.statFactors, StatDefOf.Insulation_Cold, sourceDef.race.leatherInsulation);
						StatUtility.SetStatValueInList(ref d.stuffProps.statFactors, StatDefOf.Insulation_Heat, sourceDef.race.leatherInsulation);
						List<StatModifier> sfos = sourceDef.race.leatherStatFactors;
						if (sfos != null)
						{
							foreach (StatModifier fo in sfos)
							{
								StatUtility.SetStatValueInList(ref d.stuffProps.statFactors, fo.stat, fo.value);
							}
						}
						sourceDef.race.leatherDef = d;
						yield return d;
					}
				}
			}
		}
	}
}
