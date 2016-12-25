using System;
using UnityEngine;

namespace Verse
{
	public static class UI
	{
		public static int screenWidth;

		public static int screenHeight;

		public static Vector2 MousePositionOnUI
		{
			get
			{
				return Input.mousePosition / Prefs.UIScale;
			}
		}

		public static Vector2 MousePositionOnUIInverted
		{
			get
			{
				Vector2 mousePositionOnUI = UI.MousePositionOnUI;
				mousePositionOnUI.y = (float)UI.screenHeight - mousePositionOnUI.y;
				return mousePositionOnUI;
			}
		}

		public static void ApplyPixelScale()
		{
			if (Prefs.UIScale == 1f)
			{
				UI.screenWidth = Screen.width;
				UI.screenHeight = Screen.height;
			}
			else
			{
				UI.screenWidth = Mathf.RoundToInt((float)Screen.width / Prefs.UIScale);
				UI.screenHeight = Mathf.RoundToInt((float)Screen.height / Prefs.UIScale);
				float uIScale = Prefs.UIScale;
				float uIScale2 = Prefs.UIScale;
				GUI.matrix = Matrix4x4.TRS(new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(uIScale, uIScale2, 1f));
			}
		}

		public static Vector2 GUIToScreenPoint(Vector2 guiPoint)
		{
			return GUIUtility.GUIToScreenPoint(guiPoint / Prefs.UIScale);
		}

		public static void RotateAroundPivot(float angle, Vector2 center)
		{
			GUIUtility.RotateAroundPivot(angle, center * Prefs.UIScale);
		}

		public static Vector2 MapToUIPosition(this Vector3 v)
		{
			Vector3 vector = Find.Camera.WorldToScreenPoint(v) / Prefs.UIScale;
			return new Vector2(vector.x, (float)UI.screenHeight - vector.y);
		}

		public static Vector3 UIToMapPosition(float x, float y)
		{
			return UI.UIToMapPosition(new Vector2(x, y));
		}

		public static Vector3 UIToMapPosition(Vector2 screenLoc)
		{
			Ray ray = Find.Camera.ScreenPointToRay(screenLoc * Prefs.UIScale);
			return new Vector3(ray.origin.x, 0f, ray.origin.z);
		}

		public static Vector3 MouseMapPosition()
		{
			return UI.UIToMapPosition(UI.MousePositionOnUI);
		}

		public static IntVec3 MouseCell()
		{
			return UI.UIToMapPosition(UI.MousePositionOnUI).ToIntVec3();
		}
	}
}
