using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class FleshTypeDef : Def
	{
		public class Wound
		{
			[NoTranslate]
			public string texture;

			public Color color = Color.white;

			public Material GetMaterial()
			{
				return MaterialPool.MatFrom(this.texture, ShaderDatabase.Cutout, this.color);
			}
		}

		public ThoughtDef ateDirect;

		public ThoughtDef ateAsIngredient;

		public ThingCategoryDef corpseCategory;

		public List<FleshTypeDef.Wound> wounds;

		private List<Material> woundsResolved;

		public Material ChooseWoundOverlay()
		{
			if (this.wounds == null)
			{
				return null;
			}
			if (this.woundsResolved == null)
			{
				this.woundsResolved = (from wound in this.wounds
				select wound.GetMaterial()).ToList<Material>();
			}
			return this.woundsResolved.RandomElement<Material>();
		}
	}
}
