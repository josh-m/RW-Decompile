using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldRenderModeDatabase
	{
		private static List<WorldRenderMode> allModes;

		public static IEnumerable<WorldRenderMode> AllModes
		{
			get
			{
				return WorldRenderModeDatabase.allModes;
			}
		}

		static WorldRenderModeDatabase()
		{
			WorldRenderModeDatabase.allModes = new List<WorldRenderMode>();
			WorldRenderModeDatabase.Reset();
		}

		public static void Reset()
		{
			for (int i = 0; i < WorldRenderModeDatabase.allModes.Count; i++)
			{
				WorldRenderModeDatabase.allModes[i].CleanUp();
			}
			WorldRenderModeDatabase.allModes.Clear();
			foreach (Type current in typeof(WorldRenderMode).AllLeafSubclasses())
			{
				WorldRenderMode item = (WorldRenderMode)Activator.CreateInstance(current);
				WorldRenderModeDatabase.allModes.Add(item);
			}
		}

		public static T ModeOfType<T>() where T : WorldRenderMode
		{
			foreach (WorldRenderMode current in WorldRenderModeDatabase.allModes)
			{
				if (current.GetType() == typeof(T))
				{
					return (T)((object)current);
				}
			}
			return (T)((object)null);
		}
	}
}
