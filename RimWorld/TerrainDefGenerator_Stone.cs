using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	internal static class TerrainDefGenerator_Stone
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
				rough.pathCost = 1;
				StatUtility.SetStatValueInList(ref rough.statBases, StatDefOf.Beauty, -1f);
				rough.scatterType = "Rocky";
				rough.affordances = new List<TerrainAffordance>();
				rough.affordances.Add(TerrainAffordance.Light);
				rough.affordances.Add(TerrainAffordance.Heavy);
				rough.affordances.Add(TerrainAffordance.SmoothableStone);
				rough.fertility = 0f;
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
				rock.naturalTerrain = rough;
				hewn.texturePath = "Terrain/Surfaces/RoughHewnRock";
				hewn.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				hewn.pathCost = 1;
				StatUtility.SetStatValueInList(ref hewn.statBases, StatDefOf.Beauty, -1f);
				hewn.scatterType = "Rocky";
				hewn.affordances = new List<TerrainAffordance>();
				hewn.affordances.Add(TerrainAffordance.Light);
				hewn.affordances.Add(TerrainAffordance.Heavy);
				hewn.affordances.Add(TerrainAffordance.SmoothableStone);
				hewn.fertility = 0f;
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
				rock.leaveTerrain = hewn;
				smooth.texturePath = "Terrain/Surfaces/SmoothStone";
				smooth.edgeType = TerrainDef.TerrainEdgeType.FadeRough;
				smooth.pathCost = 0;
				StatUtility.SetStatValueInList(ref smooth.statBases, StatDefOf.Beauty, 3f);
				smooth.scatterType = "Rocky";
				smooth.affordances = new List<TerrainAffordance>();
				smooth.affordances.Add(TerrainAffordance.Light);
				smooth.affordances.Add(TerrainAffordance.Heavy);
				smooth.affordances.Add(TerrainAffordance.SmoothHard);
				smooth.fertility = 0f;
				smooth.acceptTerrainSourceFilth = true;
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
