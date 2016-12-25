using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_Home : Area
	{
		public override string Label
		{
			get
			{
				return "Home".Translate();
			}
		}

		public override Color Color
		{
			get
			{
				return new Color(0.3f, 0.3f, 0.9f);
			}
		}

		public override int ListPriority
		{
			get
			{
				return 10000;
			}
		}

		public override bool AssignableAsAllowed(AllowedAreaMode mode)
		{
			return (byte)(mode & AllowedAreaMode.Any) != 0;
		}

		public override string GetUniqueLoadID()
		{
			return "Area_Home";
		}
	}
}
