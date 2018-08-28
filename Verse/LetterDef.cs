using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class LetterDef : Def
	{
		public Type letterClass = typeof(StandardLetter);

		public Color color = Color.white;

		public Color flashColor = Color.white;

		public float flashInterval = 90f;

		public bool bounce;

		public SoundDef arriveSound;

		[NoTranslate]
		public string icon = "UI/Letters/LetterUnopened";

		public bool pauseIfPauseOnUrgentLetter;

		public bool forcedSlowdown;

		[Unsaved]
		private Texture2D iconTex;

		public Texture2D Icon
		{
			get
			{
				if (this.iconTex == null && !this.icon.NullOrEmpty())
				{
					this.iconTex = ContentFinder<Texture2D>.Get(this.icon, true);
				}
				return this.iconTex;
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (this.arriveSound == null)
			{
				this.arriveSound = SoundDefOf.LetterArrive;
			}
		}
	}
}
