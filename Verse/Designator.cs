using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Designator : Command
	{
		protected bool useMouseIcon;

		public SoundDef soundDragSustain;

		public SoundDef soundDragChanged;

		protected SoundDef soundSucceeded;

		protected SoundDef soundFailed = SoundDefOf.DesignateFailed;

		private string cachedTutorTagSelect;

		private string cachedTutorTagDesignate;

		protected string cachedHighlightTag;

		protected Map Map
		{
			get
			{
				return Find.VisibleMap;
			}
		}

		public virtual int DraggableDimensions
		{
			get
			{
				return 0;
			}
		}

		public virtual bool DragDrawMeasurements
		{
			get
			{
				return false;
			}
		}

		protected override bool DoTooltip
		{
			get
			{
				return false;
			}
		}

		public override string TutorTagSelect
		{
			get
			{
				if (this.tutorTag == null)
				{
					return null;
				}
				if (this.cachedTutorTagSelect == null)
				{
					this.cachedTutorTagSelect = "SelectDesignator-" + this.tutorTag;
				}
				return this.cachedTutorTagSelect;
			}
		}

		public string TutorTagDesignate
		{
			get
			{
				if (this.tutorTag == null)
				{
					return null;
				}
				if (this.cachedTutorTagDesignate == null)
				{
					this.cachedTutorTagDesignate = "Designate-" + this.tutorTag;
				}
				return this.cachedTutorTagDesignate;
			}
		}

		public override string HighlightTag
		{
			get
			{
				if (this.cachedHighlightTag == null && this.tutorTag != null)
				{
					this.cachedHighlightTag = "Designator-" + this.tutorTag;
				}
				return this.cachedHighlightTag;
			}
		}

		public Designator()
		{
			this.activateSound = SoundDefOf.SelectDesignator;
		}

		protected bool CheckCanInteract()
		{
			return !TutorSystem.TutorialMode || TutorSystem.AllowAction(this.TutorTagSelect);
		}

		public override void ProcessInput(Event ev)
		{
			if (!this.CheckCanInteract())
			{
				return;
			}
			base.ProcessInput(ev);
			Find.DesignatorManager.Select(this);
		}

		public virtual AcceptanceReport CanDesignateThing(Thing t)
		{
			throw new NotImplementedException();
		}

		public virtual void DesignateThing(Thing t)
		{
			throw new NotImplementedException();
		}

		public abstract AcceptanceReport CanDesignateCell(IntVec3 loc);

		public virtual void DesignateMultiCell(IEnumerable<IntVec3> cells)
		{
			if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(this.TutorTagDesignate, cells)))
			{
				return;
			}
			bool somethingSucceeded = false;
			foreach (IntVec3 current in cells)
			{
				if (this.CanDesignateCell(current).Accepted)
				{
					this.DesignateSingleCell(current);
					somethingSucceeded = true;
				}
			}
			this.Finalize(somethingSucceeded);
			if (TutorSystem.TutorialMode)
			{
				TutorSystem.Notify_Event(new EventPack(this.TutorTagDesignate, cells));
			}
		}

		public virtual void DesignateSingleCell(IntVec3 c)
		{
			throw new NotImplementedException();
		}

		public void Finalize(bool somethingSucceeded)
		{
			if (somethingSucceeded)
			{
				this.FinalizeDesignationSucceeded();
			}
			else
			{
				this.FinalizeDesignationFailed();
			}
		}

		protected virtual void FinalizeDesignationSucceeded()
		{
			if (this.soundSucceeded != null)
			{
				this.soundSucceeded.PlayOneShotOnCamera(null);
			}
		}

		protected virtual void FinalizeDesignationFailed()
		{
			if (this.soundFailed != null)
			{
				this.soundFailed.PlayOneShotOnCamera(null);
			}
			if (Find.DesignatorManager.Dragger.FailureReason != null)
			{
				Messages.Message(Find.DesignatorManager.Dragger.FailureReason, MessageTypeDefOf.RejectInput);
			}
		}

		public virtual string LabelCapReverseDesignating(Thing t)
		{
			return this.LabelCap;
		}

		public virtual string DescReverseDesignating(Thing t)
		{
			return this.Desc;
		}

		public virtual Texture2D IconReverseDesignating(Thing t, out float angle)
		{
			angle = this.iconAngle;
			return this.icon;
		}

		public virtual void DrawMouseAttachments()
		{
			if (this.useMouseIcon)
			{
				GenUI.DrawMouseAttachment(this.icon, string.Empty, this.iconAngle);
			}
		}

		public virtual void DrawPanelReadout(ref float curY, float width)
		{
		}

		public virtual void DoExtraGuiControls(float leftX, float bottomY)
		{
		}

		public virtual void SelectedUpdate()
		{
		}

		public virtual void SelectedProcessInput(Event ev)
		{
		}

		public virtual void Rotate(RotationDirection rotDir)
		{
		}

		public virtual bool CanRemainSelected()
		{
			return true;
		}

		public virtual void Selected()
		{
		}
	}
}
