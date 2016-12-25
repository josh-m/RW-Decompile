using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public static class DesignatorManager
	{
		private static Designator selectedDesignator;

		private static DesignationDragger dragger = new DesignationDragger();

		public static Designator SelectedDesignator
		{
			get
			{
				return DesignatorManager.selectedDesignator;
			}
		}

		public static DesignationDragger Dragger
		{
			get
			{
				return DesignatorManager.dragger;
			}
		}

		public static void Reinit()
		{
			DesignatorManager.selectedDesignator = null;
		}

		public static void Select(Designator des)
		{
			DesignatorManager.Deselect();
			DesignatorManager.selectedDesignator = des;
			DesignatorManager.selectedDesignator.Selected();
		}

		public static void Deselect()
		{
			if (DesignatorManager.selectedDesignator != null)
			{
				DesignatorManager.selectedDesignator = null;
			}
		}

		private static bool CheckSelectedDesignatorValid()
		{
			if (DesignatorManager.selectedDesignator == null)
			{
				return false;
			}
			if (!DesignatorManager.selectedDesignator.CanRemainSelected())
			{
				DesignatorManager.Deselect();
				return false;
			}
			return true;
		}

		public static void ProcessInputEvents()
		{
			if (!DesignatorManager.CheckSelectedDesignatorValid())
			{
				return;
			}
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				if (DesignatorManager.selectedDesignator.DraggableDimensions == 0)
				{
					Designator designator = DesignatorManager.selectedDesignator;
					AcceptanceReport acceptanceReport = DesignatorManager.selectedDesignator.CanDesignateCell(Gen.MouseCell());
					if (acceptanceReport.Accepted)
					{
						designator.DesignateSingleCell(Gen.MouseCell());
						designator.Finalize(true);
					}
					else
					{
						Messages.Message(acceptanceReport.Reason, MessageSound.Silent);
						DesignatorManager.selectedDesignator.Finalize(false);
					}
				}
				else
				{
					DesignatorManager.dragger.StartDrag();
				}
				Event.current.Use();
			}
			if ((Event.current.type == EventType.MouseDown && Event.current.button == 1) || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape))
			{
				SoundDefOf.CancelMode.PlayOneShotOnCamera();
				DesignatorManager.Deselect();
				DesignatorManager.dragger.EndDrag();
				Event.current.Use();
				TutorSystem.Notify_Event("ClearDesignatorSelection");
			}
			if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && DesignatorManager.dragger.Dragging)
			{
				DesignatorManager.selectedDesignator.DesignateMultiCell(DesignatorManager.dragger.DragCells);
				DesignatorManager.dragger.EndDrag();
				Event.current.Use();
			}
		}

		public static void DesignationManagerOnGUI()
		{
			DesignatorManager.dragger.DraggerOnGUI();
			if (DesignatorManager.CheckSelectedDesignatorValid())
			{
				DesignatorManager.selectedDesignator.DrawMouseAttachments();
			}
		}

		public static void DesignatorManagerUpdate()
		{
			DesignatorManager.dragger.DraggerUpdate();
			if (DesignatorManager.CheckSelectedDesignatorValid())
			{
				DesignatorManager.selectedDesignator.SelectedUpdate();
			}
		}
	}
}
