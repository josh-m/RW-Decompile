using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlantsHarvest : Designator_Plants
	{
		public Designator_PlantsHarvest()
		{
			this.defaultLabel = "DesignatorHarvest".Translate();
			this.defaultDesc = "DesignatorHarvestDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Harvest", true);
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.useMouseIcon = true;
			this.soundSucceeded = SoundDefOf.DesignateHarvest;
			this.hotKey = KeyBindingDefOf.Misc2;
			this.designationDef = DesignationDefOf.HarvestPlant;
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			AcceptanceReport result = base.CanDesignateThing(t);
			if (!result.Accepted)
			{
				return result;
			}
			Plant plant = (Plant)t;
			if (!plant.HarvestableNow || plant.def.plant.harvestTag != "Standard")
			{
				return "MessageMustDesignateHarvestable".Translate();
			}
			return true;
		}
	}
}
