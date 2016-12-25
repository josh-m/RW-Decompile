using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class TrainableDef : Def
	{
		public float difficulty = -1f;

		public float minBodySize;

		public List<TrainableDef> prerequisites;

		[NoTranslate]
		public List<string> tags = new List<string>();

		public bool defaultTrainable;

		public TrainableIntelligence requiredTrainableIntelligence = TrainableIntelligence.Simple;

		public int steps = 1;

		public float listPriority;

		private string icon;

		[Unsaved]
		public int indent;

		[Unsaved]
		private Texture2D iconTex;

		public Texture2D Icon
		{
			get
			{
				if (this.iconTex == null)
				{
					this.iconTex = ContentFinder<Texture2D>.Get(this.icon, true);
				}
				return this.iconTex;
			}
		}

		public bool MatchesTag(string tag)
		{
			if (tag == this.defName)
			{
				return true;
			}
			for (int i = 0; i < this.tags.Count; i++)
			{
				if (this.tags[i] == tag)
				{
					return true;
				}
			}
			return false;
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in base.ConfigErrors())
			{
				yield return e;
			}
			if (this.difficulty < 0f)
			{
				yield return "difficulty not set";
			}
		}
	}
}
