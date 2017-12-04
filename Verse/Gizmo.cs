using System;
using UnityEngine;

namespace Verse
{
	public abstract class Gizmo
	{
		public bool disabled;

		public string disabledReason;

		public bool alsoClickIfOtherInGroupClicked = true;

		public const float Height = 75f;

		public abstract float Width
		{
			get;
		}

		public virtual bool Visible
		{
			get
			{
				return true;
			}
		}

		public abstract GizmoResult GizmoOnGUI(Vector2 topLeft);

		public virtual void ProcessInput(Event ev)
		{
		}

		public virtual bool GroupsWith(Gizmo other)
		{
			return false;
		}

		public virtual bool InheritInteractionsFrom(Gizmo other)
		{
			return this.alsoClickIfOtherInGroupClicked;
		}

		public void Disable(string reason = null)
		{
			this.disabled = true;
			this.disabledReason = reason;
		}
	}
}
