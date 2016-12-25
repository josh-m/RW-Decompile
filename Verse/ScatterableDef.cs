using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class ScatterableDef : Def
	{
		public string texturePath;

		public List<string> relevantTerrains = new List<string>();

		public float minSize;

		public float maxSize;

		public float selectionWeight = 100f;

		public string scatterType = string.Empty;

		public Material mat;

		public override void PostLoad()
		{
			base.PostLoad();
			if (this.defName == "UnnamedDef")
			{
				this.defName = "Scatterable_" + this.texturePath;
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				this.mat = MaterialPool.MatFrom(this.texturePath, ShaderDatabase.Transparent);
			});
		}
	}
}
