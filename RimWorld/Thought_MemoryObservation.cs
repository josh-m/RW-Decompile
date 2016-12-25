using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Thought_MemoryObservation : Thought_Memory
	{
		private int targetThingID;

		private static List<Thought> tmpThoughts = new List<Thought>();

		public Thing Target
		{
			set
			{
				this.targetThingID = value.thingIDNumber;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.targetThingID, "targetThingID", 0, false);
		}

		public override bool TryMergeWithExistingThought(out bool showBubble)
		{
			ThoughtHandler thoughts = this.pawn.needs.mood.thoughts;
			Thought_MemoryObservation thought_MemoryObservation = null;
			thoughts.GetMainThoughts(Thought_MemoryObservation.tmpThoughts);
			for (int i = 0; i < Thought_MemoryObservation.tmpThoughts.Count; i++)
			{
				Thought_MemoryObservation thought_MemoryObservation2 = Thought_MemoryObservation.tmpThoughts[i] as Thought_MemoryObservation;
				if (thought_MemoryObservation2 != null && thought_MemoryObservation2.def == this.def && thought_MemoryObservation2.targetThingID == this.targetThingID && (thought_MemoryObservation == null || thought_MemoryObservation2.age > thought_MemoryObservation.age))
				{
					thought_MemoryObservation = thought_MemoryObservation2;
				}
			}
			Thought_MemoryObservation.tmpThoughts.Clear();
			if (thought_MemoryObservation != null)
			{
				showBubble = (thought_MemoryObservation.age > thought_MemoryObservation.def.DurationTicks / 2);
				thought_MemoryObservation.Renew();
				return true;
			}
			showBubble = true;
			return false;
		}
	}
}
