using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class ListerFilthInHomeArea
	{
		private Map map;

		private List<Thing> filthInHomeArea = new List<Thing>();

		public List<Thing> FilthInHomeArea
		{
			get
			{
				return this.filthInHomeArea;
			}
		}

		public ListerFilthInHomeArea(Map map)
		{
			this.map = map;
		}

		public void RebuildAll()
		{
			this.filthInHomeArea.Clear();
			foreach (IntVec3 current in this.map.AllCells)
			{
				this.Notify_HomeAreaChanged(current);
			}
		}

		public void Notify_FilthSpawned(Filth f)
		{
			if (this.map.areaManager.Home[f.Position])
			{
				this.filthInHomeArea.Add(f);
			}
		}

		public void Notify_FilthDespawned(Filth f)
		{
			for (int i = 0; i < this.filthInHomeArea.Count; i++)
			{
				if (this.filthInHomeArea[i] == f)
				{
					this.filthInHomeArea.RemoveAt(i);
					return;
				}
			}
		}

		public void Notify_HomeAreaChanged(IntVec3 c)
		{
			if (this.map.areaManager.Home[c])
			{
				List<Thing> thingList = c.GetThingList(this.map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Filth filth = thingList[i] as Filth;
					if (filth != null)
					{
						this.filthInHomeArea.Add(filth);
					}
				}
			}
			else
			{
				for (int j = this.filthInHomeArea.Count - 1; j >= 0; j--)
				{
					if (this.filthInHomeArea[j].Position == c)
					{
						this.filthInHomeArea.RemoveAt(j);
					}
				}
			}
		}

		internal string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= Filth in home area");
			foreach (Thing current in this.filthInHomeArea)
			{
				stringBuilder.AppendLine(current.ThingID + " " + current.Position);
			}
			return stringBuilder.ToString();
		}
	}
}
