using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleData_Def : TaleData
	{
		public Def def;

		private string tmpDefName;

		private Type tmpDefType;

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.tmpDefName = this.def.defName;
				this.tmpDefType = this.def.GetType();
			}
			Scribe_Values.LookValue<string>(ref this.tmpDefName, "defName", null, false);
			Scribe_Values.LookValue<Type>(ref this.tmpDefType, "defType", null, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.def = GenDefDatabase.GetDef(this.tmpDefType, this.tmpDefName, true);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Rule> GetRules(string prefix)
		{
			yield return new Rule_String(prefix + "_label", this.def.label);
			yield return new Rule_String(prefix + "_labelDefinite", Find.ActiveLanguageWorker.WithDefiniteArticle(this.def.label));
			yield return new Rule_String(prefix + "_labelIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(this.def.label));
		}

		public static TaleData_Def GenerateFrom(Def def)
		{
			return new TaleData_Def
			{
				def = def
			};
		}
	}
}
