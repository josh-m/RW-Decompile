using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ThingDefGenerator_Seeds
	{
		[DebuggerHidden]
		public static IEnumerable<ThingDef> ImpliedSeedDefs()
		{
			foreach (ThingDef plantDef in DefDatabase<ThingDef>.AllDefs.ToList<ThingDef>())
			{
				if (plantDef.category == ThingCategory.Plant && plantDef.plant.shootsSeeds)
				{
					ThingDef seedDef = new ThingDef();
					seedDef.category = ThingCategory.Projectile;
					seedDef.category = ThingCategory.Projectile;
					seedDef.tickerType = TickerType.Normal;
					seedDef.label = "Unspecified seed";
					seedDef.description = "Seed lacks desc.";
					seedDef.thingClass = typeof(Seed);
					seedDef.graphicData = new GraphicData();
					seedDef.graphicData.texPath = "Things/Plant/Seed_Default";
					seedDef.graphicData.graphicClass = typeof(Graphic_Single);
					seedDef.graphicData.shaderType = ShaderType.Transparent;
					seedDef.altitudeLayer = AltitudeLayer.Projectile;
					seedDef.neverMultiSelect = true;
					seedDef.selectable = true;
					seedDef.useHitPoints = true;
					seedDef.SetStatBaseValue(StatDefOf.MaxHitPoints, 10f);
					seedDef.projectile = new ProjectileProperties();
					seedDef.projectile.speed = 1f;
					seedDef.defName = plantDef.defName + "_Seed";
					seedDef.label = "SeedLabel".Translate(new object[]
					{
						plantDef.label
					});
					seedDef.seed_PlantDefToMake = plantDef;
					plantDef.plant.seedDef = seedDef;
					yield return seedDef;
				}
			}
		}
	}
}
