using System;
using Verse;

namespace RimWorld
{
	public class Spark : Projectile
	{
		protected override void Impact(Thing hitThing)
		{
			base.Impact(hitThing);
			FireUtility.TryStartFireIn(base.Position, 0.1f);
		}
	}
}
