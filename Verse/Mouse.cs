using System;
using UnityEngine;

namespace Verse
{
	public static class Mouse
	{
		public static bool IsInputBlockedNow
		{
			get
			{
				WindowStack windowStack = Find.WindowStack;
				return windowStack.MouseObscuredNow || !windowStack.CurrentWindowGetsInput;
			}
		}

		public static bool IsOver(Rect rect)
		{
			return rect.Contains(Event.current.mousePosition) && !Mouse.IsInputBlockedNow;
		}
	}
}
