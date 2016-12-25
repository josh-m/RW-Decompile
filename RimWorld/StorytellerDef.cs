using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StorytellerDef : Def
	{
		public int listOrder = 9999;

		public bool listVisible = true;

		public bool tutorialMode;

		public bool disableAdaptiveTraining;

		public bool disableAlerts;

		public bool disablePermadeath;

		public DifficultyDef forcedDifficulty;

		[NoTranslate]
		private string portraitLarge;

		[NoTranslate]
		private string portraitTiny;

		public List<StorytellerCompProperties> comps = new List<StorytellerCompProperties>();

		public float desiredPopulationMin = 3f;

		public float desiredPopulationMax = 10f;

		public float desiredPopulationCritical = 13f;

		public int desiredPopulationGainIntervalMinDays = 3;

		public int desiredPopulationGainIntervalMaxDays = 10;

		[Unsaved]
		public Texture2D portraitLargeTex;

		[Unsaved]
		public Texture2D portraitTinyTex;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (!this.portraitTiny.NullOrEmpty())
				{
					this.portraitTinyTex = ContentFinder<Texture2D>.Get(this.portraitTiny, true);
					this.portraitLargeTex = ContentFinder<Texture2D>.Get(this.portraitLarge, true);
				}
			});
			for (int i = 0; i < this.comps.Count; i++)
			{
				this.comps[i].ResolveReferences(this);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in base.ConfigErrors())
			{
				yield return e;
			}
			for (int i = 0; i < this.comps.Count; i++)
			{
				foreach (string e2 in this.comps[i].ConfigErrors(this))
				{
					yield return e2;
				}
			}
		}
	}
}
