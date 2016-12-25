using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WorldLayer_WorldObjects : WorldLayer
	{
		protected abstract bool ShouldSkip(WorldObject worldObject);

		protected override void Regenerate()
		{
			base.Regenerate();
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				WorldObject worldObject = allWorldObjects[i];
				if (!worldObject.def.useDynamicDrawer)
				{
					if (!this.ShouldSkip(worldObject))
					{
						Material material = worldObject.Material;
						if (material == null)
						{
							Log.ErrorOnce("World object " + worldObject + " returned null material.", Gen.HashCombineInt(1948576891, worldObject.ID));
						}
						else
						{
							LayerSubMesh subMesh = base.GetSubMesh(material);
							Rand.PushSeed();
							Rand.Seed = worldObject.ID;
							worldObject.Print(subMesh);
							Rand.PopSeed();
						}
					}
				}
			}
			base.FinalizeMesh(MeshParts.All, false);
		}
	}
}
