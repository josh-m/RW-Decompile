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
				WorldObject o = allObjects[i];
				if (!o.def.useDynamicDrawer)
				{
					if (!this.ShouldSkip(o))
					{
						Material mat = o.Material;
						if (mat == null)
						{
							Log.ErrorOnce("World object " + o + " returned null material.", Gen.HashCombineInt(1948576891, o.ID));
						}
						else
						{
							LayerSubMesh subMesh = base.GetSubMesh(mat);
							Rand.PushState();
							Rand.Seed = o.ID;
							o.Print(subMesh);
							Rand.PopState();
						}
					}
				}
			}
			base.FinalizeMesh(MeshParts.All, false);
		}
	}
}
