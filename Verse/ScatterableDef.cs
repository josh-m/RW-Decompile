using System;
using UnityEngine;

namespace Verse
{
	public class ScatterableDef : Def
	{
		[NoTranslate]
		public string texturePath;

		public float minSize;

		public float maxSize;

		public float selectionWeight = 100f;

		[NoTranslate]
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
