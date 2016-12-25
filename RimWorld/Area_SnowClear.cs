using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_SnowClear : Area
	{
		public override string Label
		{
			get
			{
				return "SnowClear".Translate();
			}
		}

		public override Color Color
		{
			get
			{
				return new Color(0.8f, 0.1f, 0.1f);
			}
		}

		public override int ListPriority
		{
			get
			{
				return 5000;
			}
		}

		public Area_SnowClear()
		{
		}

		public Area_SnowClear(AreaManager areaManager) : base(areaManager)
		{
		}

		public override string GetUniqueLoadID()
		{
			return "Area_" + this.ID + "_SnowClear";
		}
	}
}
