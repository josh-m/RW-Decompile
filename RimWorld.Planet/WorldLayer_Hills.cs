using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Hills : WorldLayer
	{
		private static readonly FloatRange BaseSizeRange = new FloatRange(0.9f, 1.1f);

		private static readonly IntVec2 TexturesInAtlas = new IntVec2(2, 2);

		private static readonly FloatRange BasePosOffsetRange_SmallHills = new FloatRange(0f, 0.37f);

		private static readonly FloatRange BasePosOffsetRange_LargeHills = new FloatRange(0f, 0.2f);

		private static readonly FloatRange BasePosOffsetRange_Mountains = new FloatRange(0f, 0.08f);

		private static readonly FloatRange BasePosOffsetRange_ImpassableMountains = new FloatRange(0f, 0.08f);

		protected override void Regenerate()
		{
			base.Regenerate();
			Rand.PushSeed();
			Rand.Seed = Find.World.info.Seed;
			WorldGrid worldGrid = Find.WorldGrid;
			int tilesCount = worldGrid.TilesCount;
			int i = 0;
			while (i < tilesCount)
			{
				Tile tile = worldGrid[i];
				Material material;
				FloatRange floatRange;
				switch (tile.hilliness)
				{
				case Hilliness.SmallHills:
					material = WorldMaterials.SmallHills;
					floatRange = WorldLayer_Hills.BasePosOffsetRange_SmallHills;
					goto IL_B2;
				case Hilliness.LargeHills:
					material = WorldMaterials.LargeHills;
					floatRange = WorldLayer_Hills.BasePosOffsetRange_LargeHills;
					goto IL_B2;
				case Hilliness.Mountainous:
					material = WorldMaterials.Mountains;
					floatRange = WorldLayer_Hills.BasePosOffsetRange_Mountains;
					goto IL_B2;
				case Hilliness.Impassable:
					material = WorldMaterials.ImpassableMountains;
					floatRange = WorldLayer_Hills.BasePosOffsetRange_ImpassableMountains;
					goto IL_B2;
				}
				IL_17A:
				i++;
				continue;
				IL_B2:
				LayerSubMesh subMesh = base.GetSubMesh(material);
				Vector3 vector = worldGrid.GetTileCenter(i);
				Vector3 posForTangents = vector;
				float magnitude = vector.magnitude;
				vector = (vector + Rand.PointOnSphere * floatRange.RandomInRange * worldGrid.averageTileSize).normalized * magnitude;
				WorldRendererUtility.PrintQuadTangentialToPlanet(vector, posForTangents, WorldLayer_Hills.BaseSizeRange.RandomInRange * worldGrid.averageTileSize, 0.003f, subMesh, false, true, false);
				WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, WorldLayer_Hills.TexturesInAtlas.x), Rand.Range(0, WorldLayer_Hills.TexturesInAtlas.z), WorldLayer_Hills.TexturesInAtlas.x, WorldLayer_Hills.TexturesInAtlas.z, subMesh);
				goto IL_17A;
			}
			Rand.PopSeed();
			base.FinalizeMesh(MeshParts.All, true);
		}
	}
}
