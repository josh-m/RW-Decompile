using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Designator_AreaAllowed : Designator
	{
		private static Area selectedArea;

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

		public static Area SelectedArea
		{
			get
			{
				return Designator_AreaAllowed.selectedArea;
			}
		}

		public Designator_AreaAllowed(DesignateMode mode)
		{
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.useMouseIcon = true;
		}

		public static void ClearSelectedArea()
		{
			Designator_AreaAllowed.selectedArea = null;
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			if (Designator_AreaAllowed.selectedArea != null && Find.WindowStack.FloatMenu == null)
			{
				Designator_AreaAllowed.selectedArea.MarkForDraw();
			}
		}

		public override void ProcessInput(Event ev)
		{
			if (!base.CheckCanInteract())
			{
				return;
			}
			if (Designator_AreaAllowed.selectedArea != null)
			{
				base.ProcessInput(ev);
			}
			AreaUtility.MakeAllowedAreaListFloatMenu(delegate(Area a)
			{
				Designator_AreaAllowed.selectedArea = a;
				this.ProcessInput(ev);
			}, AllowedAreaMode.Any, false, true, base.Map);
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AllowedAreas, KnowledgeAmount.SpecificInteraction);
		}
	}
}
