using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class LifeStageDef : Def
	{
		private string adjective;

		public bool visible = true;

		public bool reproductive;

		public bool milkable;

		public bool shearable;

		public float voxPitch = 1f;

		public float voxVolume = 1f;

		public string icon;

		[Unsaved]
		public Texture2D iconTex;

		public List<StatModifier> statFactors = new List<StatModifier>();

		public float bodySizeFactor = 1f;

		public float healthScaleFactor = 1f;

		public float hungerRateFactor = 1f;

		public float marketValueFactor = 1f;

		public string Adjective
		{
			get
			{
				return this.adjective ?? this.label;
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (!this.icon.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					this.iconTex = ContentFinder<Texture2D>.Get(this.icon, true);
				});
			}
		}
	}
}
