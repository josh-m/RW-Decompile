using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Verse
{
	public class WorkTypeDef : Def
	{
		public WorkTags workTags;

		public string labelShort;

		public string pawnLabel;

		public string gerundLabel;

		public string verb;

		public bool visible = true;

		public int naturalPriority;

		public bool alwaysStartActive;

		public bool requireCapableColonist;

		public List<SkillDef> relevantSkills = new List<SkillDef>();

		[Unsaved]
		public List<WorkGiverDef> workGiversByPriority = new List<WorkGiverDef>();

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in base.ConfigErrors())
			{
				yield return e;
			}
			if (this.naturalPriority < 0 || this.naturalPriority > 10000)
			{
				yield return "naturalPriority is " + this.naturalPriority + ", but it must be between 0 and 10000";
			}
		}

		public override void ResolveReferences()
		{
			foreach (WorkGiverDef current in from d in DefDatabase<WorkGiverDef>.AllDefs
			where d.workType == this
			orderby d.priorityInType descending
			select d)
			{
				this.workGiversByPriority.Add(current);
			}
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine<string>(this.defName.GetHashCode(), this.gerundLabel);
		}
	}
}
