using System;

namespace RimWorld
{
	public static class StoryDangerUtility
	{
		public static float Scale(this StoryDanger d)
		{
			if (d == StoryDanger.None)
			{
				return 0f;
			}
			if (d == StoryDanger.Low)
			{
				return 1f;
			}
			if (d != StoryDanger.High)
			{
				return 0f;
			}
			return 2f;
		}
	}
}
