using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public abstract class Gizmo
	{
		public bool disabled;

		public string disabledReason;

		public bool alsoClickIfOtherInGroupClicked = true;

		public float order;

		public const float Height = 75f;

		public virtual bool Visible
		{
			get
			{
				return true;
			}
		}

		public virtual IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
		{
			get
			{
			}
		}

		public abstract GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth);

		public virtual void GizmoUpdateOnMouseover()
		{
		}

		public abstract float GetWidth(float maxWidth);

		public virtual void ProcessInput(Event ev)
		{
		}

		public virtual bool GroupsWith(Gizmo other)
		{
			return false;
		}

		public virtual void MergeWith(Gizmo other)
		{
		}

		public virtual bool InheritInteractionsFrom(Gizmo other)
		{
			return this.alsoClickIfOtherInGroupClicked;
		}

		public virtual bool InheritFloatMenuInteractionsFrom(Gizmo other)
		{
			return this.InheritInteractionsFrom(other);
		}

		public void Disable(string reason = null)
		{
			this.disabled = true;
			this.disabledReason = reason;
		}
	}
}
