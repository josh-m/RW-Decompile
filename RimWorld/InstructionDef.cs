using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class InstructionDef : Def
	{
		public Type instructionClass = typeof(Instruction_Basic);

		[MustTranslate]
		public string text;

		public bool startCentered;

		public bool tutorialModeOnly = true;

		[NoTranslate]
		public string eventTagInitiate;

		public InstructionDef eventTagInitiateSource;

		[NoTranslate]
		public List<string> eventTagsEnd;

		[NoTranslate]
		public List<string> actionTagsAllowed;

		[MustTranslate]
		public string rejectInputMessage;

		public ConceptDef concept;

		[NoTranslate]
		public List<string> highlightTags;

		[MustTranslate]
		public string onMapInstruction;

		public int targetCount;

		public ThingDef thingDef;

		public RecipeDef recipeDef;

		public int recipeTargetCount = 1;

		public ThingDef giveOnActivateDef;

		public int giveOnActivateCount;

		public bool endTutorial;

		public bool resetBuildDesignatorStuffs;

		private static List<string> tmpParseErrors = new List<string>();

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in base.ConfigErrors())
			{
				yield return e;
			}
			if (this.instructionClass == null)
			{
				yield return "no instruction class";
			}
			if (this.text.NullOrEmpty())
			{
				yield return "no text";
			}
			if (this.eventTagInitiate.NullOrEmpty())
			{
				yield return "no eventTagInitiate";
			}
			InstructionDef.tmpParseErrors.Clear();
			this.text.AdjustedForKeys(InstructionDef.tmpParseErrors, false);
			for (int i = 0; i < InstructionDef.tmpParseErrors.Count; i++)
			{
				yield return "text error: " + InstructionDef.tmpParseErrors[i];
			}
		}
	}
}
