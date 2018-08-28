using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TerrainDefGenerator_Stone
	{
		[DebuggerHidden]
		public static IEnumerable<TerrainDef> ImpliedTerrainDefs()
		{
			int i = 0;
			foreach (ThingDef rock in from def in DefDatabase<ThingDef>.AllDefs
			where def.building != null && def.building.isNaturalRock && !def.building.isResourceRock
			select def)
			{
				TerrainDef rough = new TerrainDef();
				TerrainDef hewn = new TerrainDef();
				TerrainDef smooth = new TerrainDef();
				rough.texturePath = "Terrain/Surfaces/RoughStone";
				rough.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				rough.pathCost = 2;
				StatUtility.SetStatValueInList(ref rough.statBases, StatDefOf.Beauty, -1f);
				rough.scatterType = "Rocky";
				rough.affordances = new List<TerrainAffordanceDef>();
				rough.affordances.Add(TerrainAffordanceDefOf.Light);
				rough.affordances.Add(TerrainAffordanceDefOf.Medium);
				rough.affordances.Add(TerrainAffordanceDefOf.Heavy);
				rough.affordances.Add(TerrainAffordanceDefOf.SmoothableStone);
				rough.fertility = 0f;
				rough.acceptFilth = false;
				rough.acceptTerrainSourceFilth = false;
				rough.modContentPack = rock.modContentPack;
				rough.renderPrecedence = 190 + i;
				rough.defName = rock.defName + "_Rough";
				rough.label = "RoughStoneTerrainLabel".Translate(new object[]
				{
					rock.label
				});
				rough.description = "RoughStoneTerrainDesc".Translate(new object[]
				{
					rock.label
				});
				rough.color = rock.graphicData.color;
				rock.building.naturalTerrain = rough;
				hewn.texturePath = "Terrain/Surfaces/RoughHewnRock";
				hewn.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				hewn.pathCost = 1;
				StatUtility.SetStatValueInList(ref hewn.statBases, StatDefOf.Beauty, -1f);
				hewn.scatterType = "Rocky";
				hewn.affordances = new List<TerrainAffordanceDef>();
				hewn.affordances.Add(TerrainAffordanceDefOf.Light);
				hewn.affordances.Add(TerrainAffordanceDefOf.Medium);
				hewn.affordances.Add(TerrainAffordanceDefOf.Heavy);
				hewn.affordances.Add(TerrainAffordanceDefOf.SmoothableStone);
				hewn.fertility = 0f;
				hewn.acceptFilth = true;
				hewn.acceptTerrainSourceFilth = true;
				hewn.modContentPack = rock.modContentPack;
				hewn.renderPrecedence = 50 + i;
				hewn.defName = rock.defName + "_RoughHewn";
				hewn.label = "RoughHewnStoneTerrainLabel".Translate(new object[]
				{
					rock.label
				});
				hewn.description = "RoughHewnStoneTerrainDesc".Translate(new object[]
				{
					rock.label
				});
				hewn.color = rock.graphicData.color;
				rock.building.leaveTerrain = hewn;
				smooth.texturePath = "Terrain/Surfaces/SmoothStone";
				smooth.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				smooth.pathCost = 0;
				StatUtility.SetStatValueInList(ref smooth.statBases, StatDefOf.Beauty, 2f);
				StatUtility.SetStatValueInList(ref smooth.statBases, StatDefOf.MarketValue, 8f);
				smooth.scatterType = "Rocky";
				smooth.affordances = new List<TerrainAffordanceDef>();
				smooth.affordances.Add(TerrainAffordanceDefOf.Light);
				smooth.affordances.Add(TerrainAffordanceDefOf.Medium);
				smooth.affordances.Add(TerrainAffordanceDefOf.Heavy);
				smooth.fertility = 0f;
				smooth.acceptFilth = true;
				smooth.acceptTerrainSourceFilth = true;
				smooth.modContentPack = rock.modContentPack;
				smooth.renderPrecedence = 140 + i;
				smooth.defName = rock.defName + "_Smooth";
				smooth.label = "SmoothStoneTerrainLabel".Translate(new object[]
				{
					rock.label
				});
				smooth.description = "SmoothStoneTerrainDesc".Translate(new object[]
				{
					rock.label
				});
				smooth.color = rock.graphicData.color;
				rough.smoothedTerrain = smooth;
				hewn.smoothedTerrain = smooth;
				yield return rough;
			}
		}
	}
}
