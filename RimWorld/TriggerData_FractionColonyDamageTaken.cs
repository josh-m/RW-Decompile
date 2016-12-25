using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class TriggerData_FractionColonyDamageTaken : TriggerData
	{
		public float startColonyDamage;

		public override void ExposeData()
		{
			Scribe_Values.LookValue<float>(ref this.startColonyDamage, "startColonyDamage", 0f, false);
		}
	}
}
