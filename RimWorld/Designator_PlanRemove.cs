using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlanRemove : Designator_Plan
	{
		public Designator_PlanRemove() : base(DesignateMode.Remove)
		{
			this.defaultLabel = "DesignatorPlanRemove".Translate();
			this.defaultDesc = "DesignatorPlanRemoveDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOff", true);
			this.soundSucceeded = SoundDefOf.DesignatePlanRemove;
			this.hotKey = KeyBindingDefOf.Misc8;
		}
	}
}
