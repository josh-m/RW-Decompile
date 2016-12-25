using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class PawnInventoryOption
	{
		public ThingDef thingDef;

		public IntRange countRange = IntRange.one;

		public float choiceChance = 1f;

		public float skipChance;

		public List<PawnInventoryOption> subOptionsTakeAll;

		public List<PawnInventoryOption> subOptionsChooseOne;

		[DebuggerHidden]
		public IEnumerable<Thing> GenerateThings()
		{
			if (Rand.Value >= this.skipChance)
			{
				if (this.thingDef != null && this.countRange.max > 0)
				{
					Thing thing = ThingMaker.MakeThing(this.thingDef, null);
					thing.stackCount = this.countRange.RandomInRange;
					yield return thing;
				}
				if (this.subOptionsTakeAll != null)
				{
					foreach (PawnInventoryOption opt in this.subOptionsTakeAll)
					{
						foreach (Thing subThing in opt.GenerateThings())
						{
							yield return subThing;
						}
					}
				}
				if (this.subOptionsChooseOne != null)
				{
					PawnInventoryOption chosen = this.subOptionsChooseOne.RandomElementByWeight((PawnInventoryOption o) => o.choiceChance);
					foreach (Thing subThing2 in chosen.GenerateThings())
					{
						yield return subThing2;
					}
				}
			}
		}
	}
}
