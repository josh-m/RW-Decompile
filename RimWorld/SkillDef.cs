using System;
using Verse;

namespace RimWorld
{
	public class SkillDef : Def
	{
		public string skillLabel;

		public string pawnLabel;

		public bool usuallyDefinedInBackstories = true;

		public WorkTags disablingWorkTags;

		public override void PostLoad()
		{
			if (this.label == null)
			{
				this.label = this.skillLabel;
			}
		}
	}
}
