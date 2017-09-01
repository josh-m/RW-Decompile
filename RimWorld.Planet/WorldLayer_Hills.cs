using System;
using System.Collections;
using System.Diagnostics;
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

		[DebuggerHidden]
		public override IEnumerable Regenerate()
		{
			foreach (object result in base.Regenerate())
			{
				yield return result;
			}
			Rand.PushState();
			Rand.Seed = Find.World.info.Seed;
			WorldGrid grid = Find.WorldGrid;
			int tilesCount = grid.TilesCount;
			int i = 0;
			while (i < tilesCount)
			{
				Tile tile = grid[i];
				Material mat;
				FloatRange basePosOffsetRange;
				switch (tile.hilliness)
				{
				case Hilliness.SmallHills:
					mat = WorldMaterials.SmallHills;
					basePosOffsetRange = WorldLayer_Hills.BasePosOffsetRange_SmallHills;
					goto IL_19A;
				case Hilliness.LargeHills:
					mat = WorldMaterials.LargeHills;
					basePosOffsetRange = WorldLayer_Hills.BasePosOffsetRange_LargeHills;
					goto IL_19A;
				case Hilliness.Mountainous:
					mat = WorldMaterials.Mountains;
					basePosOffsetRange = WorldLayer_Hills.BasePosOffsetRange_Mountains;
					goto IL_19A;
				case Hilliness.Impassable:
					mat = WorldMaterials.ImpassableMountains;
					basePosOffsetRange = WorldLayer_Hills.BasePosOffsetRange_ImpassableMountains;
					goto IL_19A;
				}
				IL_2BF:
				i++;
				continue;
				IL_19A:
				LayerSubMesh subMesh = base.GetSubMesh(mat);
				Vector3 pos = grid.GetTileCenter(i);
				Vector3 origPos = pos;
				float length = pos.magnitude;
				pos += Rand.PointOnSphere * basePosOffsetRange.RandomInRange * grid.averageTileSize;
				pos = pos.normalized * length;
				WorldRendererUtility.PrintQuadTangentialToPlanet(pos, origPos, WorldLayer_Hills.BaseSizeRange.RandomInRange * grid.averageTileSize, 0.005f, subMesh, false, true, false);
				WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, WorldLayer_Hills.TexturesInAtlas.x), Rand.Range(0, WorldLayer_Hills.TexturesInAtlas.z), WorldLayer_Hills.TexturesInAtlas.x, WorldLayer_Hills.TexturesInAtlas.z, subMesh);
				goto IL_2BF;
			}
			Rand.PopState();
			base.FinalizeMesh(MeshParts.All, true);
		}
	}
}
