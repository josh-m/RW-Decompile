using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale_SinglePawnAndDef : Tale_SinglePawn
	{
		public TaleData_Def defData;

		public Tale_SinglePawnAndDef()
		{
		}

		public Tale_SinglePawnAndDef(Pawn pawn, Def def) : base(pawn)
		{
			this.defData = TaleData_Def.GenerateFrom(def);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look<TaleData_Def>(ref this.defData, "defData", new object[0]);
		}

		[DebuggerHidden]
		protected override IEnumerable<Rule> SpecialTextGenerationRules()
		{
			foreach (Rule r in base.SpecialTextGenerationRules())
			{
				yield return r;
			}
			foreach (Rule r2 in this.defData.GetRules(this.def.defSymbol))
			{
				yield return r2;
			}
		}

		public override void GenerateTestData()
		{
			base.GenerateTestData();
			this.defData = TaleData_Def.GenerateFrom((Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), this.def.defType, "GetRandom"));
		}
	}
}
