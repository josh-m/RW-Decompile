using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WorldLayer_WorldObjects : WorldLayer
	{
		protected abstract bool ShouldSkip(WorldObject worldObject);

		[DebuggerHidden]
		public override IEnumerable Regenerate()
		{
			foreach (object result in base.Regenerate())
			{
				yield return result;
			}
			List<WorldObject> allObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allObjects.Count; i++)
			{
				WorldObject worldObject = allObjects[i];
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
							Rand.PushState();
							Rand.Seed = worldObject.ID;
							worldObject.Print(subMesh);
							Rand.PopState();
						}
					}
				}
			}
			base.FinalizeMesh(MeshParts.All);
		}
	}
}
