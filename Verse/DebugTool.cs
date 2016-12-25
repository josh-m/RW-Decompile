using System;
using UnityEngine;

namespace Verse
{
	public class DebugTool
	{
		private string label;

		private Action clickAction;

		private Action onGUIAction;

		public DebugTool(string label, Action clickAction, Action onGUIAction = null)
		{
			this.label = label;
			this.clickAction = clickAction;
			this.onGUIAction = onGUIAction;
		}

		public DebugTool(string label, Action clickAction, IntVec3 firstRectCorner)
		{
			this.label = label;
			this.clickAction = clickAction;
			this.onGUIAction = delegate
			{
				IntVec3 intVec = UI.MouseCell();
				Vector3 position = firstRectCorner.ToVector3Shifted();
				Vector3 position2 = intVec.ToVector3Shifted();
				if (position.x < position2.x)
				{
					position.x -= 0.5f;
					position2.x += 0.5f;
				}
				else
				{
					position.x += 0.5f;
					position2.x -= 0.5f;
				}
				if (position.z < position2.z)
				{
					position.z -= 0.5f;
					position2.z += 0.5f;
				}
				else
				{
					position.z += 0.5f;
					position2.z -= 0.5f;
				}
				Vector3 vector = Find.Camera.WorldToScreenPoint(position) / Prefs.UIScale;
				Vector3 vector2 = Find.Camera.WorldToScreenPoint(position2) / Prefs.UIScale;
				Vector2 vector3 = new Vector2(vector.x, (float)UI.screenHeight - vector.y);
				Vector2 vector4 = new Vector2(vector2.x, (float)UI.screenHeight - vector2.y);
				Widgets.DrawBox(new Rect(vector3.x, vector3.y, vector4.x - vector3.x, vector4.y - vector3.y), 3);
			};
		}

		public void DebugToolOnGUI()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					this.clickAction();
				}
				if (Event.current.button == 1)
				{
					DebugTools.curTool = null;
				}
				Event.current.Use();
			}
			Vector2 vector = Event.current.mousePosition + new Vector2(15f, 15f);
			Rect rect = new Rect(vector.x, vector.y, 999f, 999f);
			Text.Font = GameFont.Small;
			Widgets.Label(rect, this.label);
			if (this.onGUIAction != null)
			{
				this.onGUIAction();
			}
		}
	}
}
