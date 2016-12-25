using System;
using System.Collections.Generic;

namespace Verse
{
	public class BodyPartRecord
	{
		public BodyPartDef def;

		public List<BodyPartRecord> parts = new List<BodyPartRecord>();

		public BodyPartHeight height;

		public BodyPartDepth depth;

		public float coverage = 1f;

		public List<BodyPartGroupDef> groups = new List<BodyPartGroupDef>();

		[Unsaved]
		public BodyPartRecord parent;

		[Unsaved]
		public float fleshCoverage;

		[Unsaved]
		public float absoluteCoverage;

		[Unsaved]
		public float absoluteFleshCoverage;

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"BodyPartRecord(",
				(this.def == null) ? "NULL_DEF" : this.def.defName,
				" parts.Count=",
				this.parts.Count,
				")"
			});
		}

		public bool IsInGroup(BodyPartGroupDef group)
		{
			for (int i = 0; i < this.groups.Count; i++)
			{
				if (this.groups[i] == group)
				{
					return true;
				}
			}
			return false;
		}
	}
}
