using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class CustomCursor
	{
		private static readonly Texture2D CursorTex = ContentFinder<Texture2D>.Get("UI/Cursors/CursorCustom", true);

		private static Vector2 CursorHotspot = new Vector2(3f, 3f);

		public static void Activate()
		{
			Cursor.SetCursor(CustomCursor.CursorTex, CustomCursor.CursorHotspot, CursorMode.Auto);
		}

		public static void Deactivate()
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
	}
}
