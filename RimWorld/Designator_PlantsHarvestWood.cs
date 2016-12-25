using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlantsHarvestWood : Designator_Plants
	{
		public Designator_PlantsHarvestWood()
		{
			this.defaultLabel = "DesignatorHarvestWood".Translate();
			this.defaultDesc = "DesignatorHarvestWoodDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/HarvestWood", true);
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.useMouseIcon = true;
			this.soundSucceeded = SoundDefOf.DesignateHarvest;
			this.hotKey = KeyBindingDefOf.Misc1;
			this.designationDef = DesignationDefOf.HarvestPlant;
			this.tutorTag = "PlantsHarvestWood";
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			AcceptanceReport result = base.CanDesignateThing(t);
			if (!result.Accepted)
			{
				return result;
			}
			Plant plant = (Plant)t;
			if (!plant.HarvestableNow || plant.def.plant.harvestTag != "Wood")
			{
				return "MessageMustDesignateHarvestableWood".Translate();
			}
			return true;
		}
	}
}
