using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_PlantsCut : Designator_Plants
	{
		public Designator_PlantsCut()
		{
			this.defaultLabel = "DesignatorCutPlants".Translate();
			this.defaultDesc = "DesignatorCutPlantsDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/CutPlants", true);
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.useMouseIcon = true;
			this.soundSucceeded = SoundDefOf.DesignateCutPlants;
			this.hotKey = KeyBindingDefOf.Misc3;
			this.designationDef = DesignationDefOf.CutPlant;
		}

		public override Texture2D IconReverseDesignating(Thing t, out float angle)
		{
			if (!t.def.plant.IsTree)
			{
				return base.IconReverseDesignating(t, out angle);
			}
			angle = 0f;
			return TexCommand.TreeChop;
		}

		public override string LabelCapReverseDesignating(Thing t)
		{
			if (!t.def.plant.IsTree)
			{
				return base.LabelCapReverseDesignating(t);
			}
			return "DesignatorHarvestWood".Translate();
		}

		public override string DescReverseDesignating(Thing t)
		{
			if (!t.def.plant.IsTree)
			{
				return base.DescReverseDesignating(t);
			}
			return "DesignatorHarvestWoodDesc".Translate();
		}
	}
}
