using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class PawnNeedsUIUtility
	{
		public static void SortInDisplayOrder(List<Need> needs)
		{
			needs.Sort((Need a, Need b) => b.def.listPriority.CompareTo(a.def.listPriority));
		}

		public static Thought GetLeadingThoughtInGroup(List<Thought> thoughtsInGroup)
		{
			Thought result = null;
			int num = -1;
			for (int i = 0; i < thoughtsInGroup.Count; i++)
			{
				if (thoughtsInGroup[i].CurStageIndex > num)
				{
					num = thoughtsInGroup[i].CurStageIndex;
					result = thoughtsInGroup[i];
				}
			}
			return result;
		}

		public static void GetThoughtGroupsInDisplayOrder(Need_Mood mood, List<Thought> outThoughtGroupsPresent)
		{
			outThoughtGroupsPresent.Clear();
			outThoughtGroupsPresent.AddRange(mood.thoughts.DistinctThoughtGroups());
			outThoughtGroupsPresent.SortByDescending((Thought th) => mood.thoughts.MoodOffsetOfThoughtGroup(th));
		}
	}
}
