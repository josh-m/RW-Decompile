using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlanAdd : Designator_Plan
	{
		public Designator_PlanAdd() : base(DesignateMode.Add)
		{
			this.defaultLabel = "DesignatorPlan".Translate();
			this.defaultDesc = "DesignatorPlanDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/PlanOn", true);
			this.soundSucceeded = SoundDefOf.DesignatePlanAdd;
			this.hotKey = KeyBindingDefOf.Misc9;
		}
	}
}
