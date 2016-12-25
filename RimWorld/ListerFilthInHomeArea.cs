using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class ListerFilthInHomeArea
	{
		private static List<Thing> filthInHomeArea = new List<Thing>();

		public static List<Thing> FilthInHomeArea
		{
			get
			{
				return ListerFilthInHomeArea.filthInHomeArea;
			}
		}

		public static void RebuildAll()
		{
			ListerFilthInHomeArea.filthInHomeArea.Clear();
			foreach (IntVec3 current in Find.Map.AllCells)
			{
				ListerFilthInHomeArea.Notify_HomeAreaChanged(current);
			}
		}

		public static void Notify_FilthSpawned(Filth f)
		{
			if (Find.AreaHome[f.Position])
			{
				ListerFilthInHomeArea.filthInHomeArea.Add(f);
			}
		}

		public static void Notify_FilthDespawned(Filth f)
		{
			for (int i = 0; i < ListerFilthInHomeArea.filthInHomeArea.Count; i++)
			{
				if (ListerFilthInHomeArea.filthInHomeArea[i] == f)
				{
					ListerFilthInHomeArea.filthInHomeArea.RemoveAt(i);
					return;
				}
			}
		}

		public static void Notify_HomeAreaChanged(IntVec3 c)
		{
			if (Find.AreaHome[c])
			{
				List<Thing> thingList = c.GetThingList();
				for (int i = 0; i < thingList.Count; i++)
				{
					Filth filth = thingList[i] as Filth;
					if (filth != null)
					{
						ListerFilthInHomeArea.filthInHomeArea.Add(filth);
					}
				}
			}
			else
			{
				for (int j = ListerFilthInHomeArea.filthInHomeArea.Count - 1; j >= 0; j--)
				{
					if (ListerFilthInHomeArea.filthInHomeArea[j].Position == c)
					{
						ListerFilthInHomeArea.filthInHomeArea.RemoveAt(j);
					}
				}
			}
		}

		internal static string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= Filth in home area");
			foreach (Thing current in ListerFilthInHomeArea.filthInHomeArea)
			{
				stringBuilder.AppendLine(current.ThingID + " " + current.Position);
			}
			return stringBuilder.ToString();
		}
	}
}
