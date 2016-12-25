using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class RectTrigger : Thing
	{
		private CellRect rect;

		public Letter letter;

		public bool destroyIfUnfogged;

		public CellRect Rect
		{
			get
			{
				return this.rect;
			}
			set
			{
				this.rect = value;
				if (base.Spawned)
				{
					this.rect.ClipInsideMap(base.Map);
				}
			}
		}

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			this.rect.ClipInsideMap(base.Map);
		}

		public override void Tick()
		{
			if (this.destroyIfUnfogged && !this.rect.CenterCell.Fogged(base.Map))
			{
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			if (this.IsHashIntervalTick(60))
			{
				Map map = base.Map;
				for (int i = this.rect.minZ; i <= this.rect.maxZ; i++)
				{
					for (int j = this.rect.minX; j <= this.rect.maxX; j++)
					{
						IntVec3 c = new IntVec3(j, 0, i);
						List<Thing> thingList = c.GetThingList(map);
						for (int k = 0; k < thingList.Count; k++)
						{
							if (thingList[k].def.category == ThingCategory.Pawn && thingList[k].def.race.intelligence == Intelligence.Humanlike && thingList[k].Faction == Faction.OfPlayer)
							{
								this.ActivatedBy((Pawn)thingList[k]);
								return;
							}
						}
					}
				}
			}
		}

		private void ActivatedBy(Pawn p)
		{
			if (this.letter != null)
			{
				this.letter.text = string.Format(this.letter.text, p.NameStringShort).AdjustedFor(p);
				Find.LetterStack.ReceiveLetter(this.letter, null);
			}
			if (!base.Destroyed)
			{
				this.Destroy(DestroyMode.Vanish);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<CellRect>(ref this.rect, "rect", default(CellRect), false);
			Scribe_Deep.LookDeep<Letter>(ref this.letter, "letter", new object[0]);
		}
	}
}
