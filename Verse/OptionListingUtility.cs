using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class OptionListingUtility
	{
		private const float OptionSpacing = 7f;

		public static float DrawOptionListing(Rect rect, List<ListableOption> optList)
		{
			float num = 0f;
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Small;
			foreach (ListableOption current in optList)
			{
				num += current.DrawOption(new Vector2(0f, num), rect.width) + 7f;
			}
			GUI.EndGroup();
			return num;
		}
	}
}
