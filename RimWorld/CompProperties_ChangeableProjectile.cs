using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_ChangeableProjectile : CompProperties
	{
		public CompProperties_ChangeableProjectile()
		{
			this.compClass = typeof(CompChangeableProjectile);
		}
	}
}
