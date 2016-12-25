using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldGenerator
	{
		private const float WorldAspectRatio = 0.75f;

		public static readonly IntVec2 DefaultSize = new IntVec2(200, 150);

		public static IntVec2 WorldSizeVectorFromInt(int intSize)
		{
			return new IntVec2(intSize, Mathf.RoundToInt((float)intSize * 0.75f));
		}

		public static World GenerateWorld(IntVec2 size, string seedString)
		{
			Rand.Seed = (GenText.StableStringHash(seedString) ^ 4323276);
			Current.CreatingWorld = new World();
			Current.CreatingWorld.info.size = size;
			Current.CreatingWorld.info.seedString = seedString.ToLowerInvariant();
			Current.CreatingWorld.info.name = NameGenerator.GenerateName(RulePackDefOf.NamerWorld, null);
			WorldGenerator_Grid.GenerateGridIntoWorld(seedString.ToLowerInvariant());
			FactionGenerator.GenerateFactionsIntoWorld();
			Current.CreatingWorld.renderer.Notify_WorldChanged();
			World creatingWorld = Current.CreatingWorld;
			Current.CreatingWorld = null;
			return creatingWorld;
		}
	}
}
