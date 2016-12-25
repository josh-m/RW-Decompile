using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_NoRoof : Area
	{
		public override string Label
		{
			get
			{
				return "NoRoof".Translate();
			}
		}

		public override Color Color
		{
			get
			{
				return new Color(0.9f, 0.5f, 0.1f);
			}
		}

		public override int ListPriority
		{
			get
			{
				return 8000;
			}
		}

		public Area_NoRoof()
		{
		}

		public Area_NoRoof(AreaManager areaManager) : base(areaManager)
		{
		}

		public override string GetUniqueLoadID()
		{
			return "Area_" + this.ID + "_NoRoof";
		}
	}
}
