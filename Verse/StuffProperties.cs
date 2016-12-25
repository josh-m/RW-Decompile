using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class StuffProperties
	{
		public string stuffAdjective;

		public float commonality = 1f;

		public List<StuffCategoryDef> categories = new List<StuffCategoryDef>();

		public bool smeltable;

		public List<StatModifier> statOffsets;

		public List<StatModifier> statFactors;

		public Color color = new Color(0.8f, 0.8f, 0.8f);

		public EffecterDef constructEffect;

		public StuffAppearance appearance;

		public bool allowColorGenerators;

		public SoundDef soundImpactStuff;

		public SoundDef soundMeleeHitSharp;

		public SoundDef soundMeleeHitBlunt;

		public bool CanMake(ThingDef t)
		{
			for (int i = 0; i < t.stuffCategories.Count; i++)
			{
				for (int j = 0; j < this.categories.Count; j++)
				{
					if (t.stuffCategories[i] == this.categories[j])
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
