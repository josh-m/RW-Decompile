using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Verse
{
	public class HediffCompProperties_VerbGiver : HediffCompProperties
	{
		public List<VerbProperties> verbs;

		public List<Tool> tools;

		public HediffCompProperties_VerbGiver()
		{
			this.compClass = typeof(HediffComp_VerbGiver);
		}

		public override void PostLoad()
		{
			base.PostLoad();
			if (this.tools != null)
			{
				for (int i = 0; i < this.tools.Count; i++)
				{
					this.tools[i].id = i.ToString();
				}
			}
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (string err in base.ConfigErrors(parentDef))
			{
				yield return err;
			}
			if (this.tools != null)
			{
				Tool dupeTool = this.tools.SelectMany(delegate(Tool lhs)
				{
					HediffCompProperties_VerbGiver $this = this.$this;
					return from rhs in this.$this.tools
					where lhs != rhs && lhs.id == rhs.id
					select rhs;
				}).FirstOrDefault<Tool>();
				if (dupeTool != null)
				{
					yield return string.Format("duplicate hediff tool id {0}", dupeTool.id);
				}
				foreach (Tool t in this.tools)
				{
					foreach (string e in t.ConfigErrors())
					{
						yield return e;
					}
				}
			}
		}
	}
}
