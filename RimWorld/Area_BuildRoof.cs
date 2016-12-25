using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_BuildRoof : Area
	{
		public override string Label
		{
			get
			{
				return "BuildRoof".Translate();
			}
		}

		public override Color Color
		{
			get
			{
				return new Color(0.9f, 0.9f, 0.5f);
			}
		}

		public override int ListPriority
		{
			get
			{
				return 9000;
			}
		}

		public Area_BuildRoof()
		{
		}

		public Area_BuildRoof(AreaManager areaManager) : base(areaManager)
		{
		}

		public override string GetUniqueLoadID()
		{
			return "Area_" + this.ID + "_BuildRoof";
		}
	}
}
