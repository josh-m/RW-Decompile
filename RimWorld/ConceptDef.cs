using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class ConceptDef : Def
	{
		public float priority = 3.40282347E+38f;

		public bool noteTeaches;

		public bool needsOpportunity;

		public bool opportunityDecays = true;

		public ProgramState gameMode = ProgramState.MapPlaying;

		private string helpText;

		public List<string> highlightTags;

		public bool TriggeredDirect
		{
			get
			{
				return this.priority <= 0f;
			}
		}

		public string HelpTextAdjusted
		{
			get
			{
				return this.helpText.AdjustedForKeys();
			}
		}

		public override void PostLoad()
		{
			base.PostLoad();
			if (this.defName == "UnnamedDef")
			{
				this.defName = this.defName.ToString();
			}
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string str in base.ConfigErrors())
			{
				yield return str;
			}
			if (this.priority > 9999999f)
			{
				yield return "priority isn't set";
			}
			if (this.helpText.NullOrEmpty())
			{
				yield return "no help text";
			}
			if (this.TriggeredDirect && this.label.NullOrEmpty())
			{
				yield return "no label";
			}
		}

		public static ConceptDef Named(string defName)
		{
			return DefDatabase<ConceptDef>.GetNamed(defName, true);
		}

		public void HighlightAllTags()
		{
			if (this.highlightTags != null)
			{
				for (int i = 0; i < this.highlightTags.Count; i++)
				{
					UIHighlighter.HighlightTag(this.highlightTags[i]);
				}
			}
		}
	}
}
