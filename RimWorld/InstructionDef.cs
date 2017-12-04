using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class InstructionDef : Def
	{
		public Type instructionClass = typeof(Instruction_Basic);

		public string text;

		public bool startCentered;

		public bool tutorialModeOnly = true;

		public string eventTagInitiate;

		public InstructionDef eventTagInitiateSource;

		public List<string> eventTagsEnd;

		public List<string> actionTagsAllowed;

		public string rejectInputMessage;

		public ConceptDef concept;

		public List<string> highlightTags;

		public string onMapInstruction;

		public int targetCount;

		public ThingDef thingDef;

		public RecipeDef recipeDef;

		public int recipeTargetCount = 1;

		public ThingDef giveOnActivateDef;

		public int giveOnActivateCount;

		public bool endTutorial;

		public bool resetBuildDesignatorStuffs;

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
		}
	}
}
