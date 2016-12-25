using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Mine : Designator
	{
		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}

		public override bool DragDrawMeasurements
		{
			get
			{
				return true;
			}
		}

		public Designator_Mine()
		{
			this.defaultLabel = "DesignatorMine".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Mine", true);
			this.defaultDesc = "DesignatorMineDesc".Translate();
			this.useMouseIcon = true;
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.soundSucceeded = SoundDefOf.DesignateMine;
			this.hotKey = KeyBindingDefOf.Misc10;
			this.tutorTag = "Mine";
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds())
			{
				return false;
			}
			if (Find.DesignationManager.DesignationAt(c, DesignationDefOf.Mine) != null)
			{
				return AcceptanceReport.WasRejected;
			}
			if (c.Fogged())
			{
				return true;
			}
			Thing thing = MineUtility.MineableInCell(c);
			if (thing == null)
			{
				return "MessageMustDesignateMineable".Translate();
			}
			AcceptanceReport result = this.CanDesignateThing(thing);
			if (!result.Accepted)
			{
				return result;
			}
			return AcceptanceReport.WasAccepted;
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (!t.def.mineable)
			{
				return false;
			}
			if (Find.DesignationManager.DesignationAt(t.Position, DesignationDefOf.Mine) != null)
			{
				return AcceptanceReport.WasRejected;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc)
		{
			Find.DesignationManager.AddDesignation(new Designation(loc, DesignationDefOf.Mine));
		}

		public override void DesignateThing(Thing t)
		{
			this.DesignateSingleCell(t.Position);
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Mining, KnowledgeAmount.SpecificInteraction);
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}
	}
}
