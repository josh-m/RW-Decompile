using System;
using UnityEngine;

namespace Verse
{
	public struct GizmoResult
	{
		private GizmoState stateInt;

		private Event interactEventInt;

		public GizmoState State
		{
			get
			{
				return this.stateInt;
			}
		}

		public Event InteractEvent
		{
			get
			{
				return this.interactEventInt;
			}
		}

		public GizmoResult(GizmoState state)
		{
			this.stateInt = state;
			this.interactEventInt = null;
		}

		public GizmoResult(GizmoState state, Event interactEvent)
		{
			this.stateInt = state;
			this.interactEventInt = interactEvent;
		}
	}
}
