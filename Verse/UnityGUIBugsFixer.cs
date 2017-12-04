using System;
using UnityEngine;

namespace Verse
{
	public static class UnityGUIBugsFixer
	{
		private const float ScrollFactor = -6f;

		public static void OnGUI()
		{
			UnityGUIBugsFixer.FixScrolling();
			UnityGUIBugsFixer.FixShift();
		}

		private static void FixScrolling()
		{
			if (Event.current.type == EventType.ScrollWheel && (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer))
			{
				Vector2 delta = Event.current.delta;
				Event.current.delta = new Vector2(delta.x, delta.y * -6f);
			}
		}

		private static void FixShift()
		{
			if ((Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer) && !Event.current.shift)
			{
				Event.current.shift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
			}
		}
	}
}
