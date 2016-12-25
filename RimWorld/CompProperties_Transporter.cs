using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_Transporter : CompProperties
	{
		public float massCapacity = 150f;

		public CompProperties_Transporter()
		{
			this.compClass = typeof(CompTransporter);
		}
	}
}
