using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ScenPart_ThingCount : ScenPart
	{
		protected ThingDef thingDef;

		protected ThingDef stuff;

		protected int count = 1;

		private string countBuf;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<ThingDef>(ref this.thingDef, "thingDef");
			Scribe_Defs.LookDef<ThingDef>(ref this.stuff, "stuff");
			Scribe_Values.LookValue<int>(ref this.count, "count", 1, false);
		}

		public override void Randomize()
		{
			this.thingDef = this.PossibleThingDefs().RandomElement<ThingDef>();
			this.stuff = GenStuff.RandomStuffFor(this.thingDef);
			if (this.thingDef.statBases.StatListContains(StatDefOf.MarketValue))
			{
				float num = (float)Rand.Range(200, 2000);
				float statValueAbstract = this.thingDef.GetStatValueAbstract(StatDefOf.MarketValue, this.stuff);
				this.count = Mathf.CeilToInt(num / statValueAbstract);
			}
			else
			{
				this.count = Rand.RangeInclusive(1, 100);
			}
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f);
			Rect rect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, scenPartRect.height / 3f);
			Rect rect2 = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height / 3f, scenPartRect.width, scenPartRect.height / 3f);
			Rect rect3 = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height * 2f / 3f, scenPartRect.width, scenPartRect.height / 3f);
			if (Widgets.ButtonText(rect, this.thingDef.LabelCap, true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (ThingDef current in from t in this.PossibleThingDefs()
				orderby t.label
				select t)
				{
					ThingDef localTd = current;
					list.Add(new FloatMenuOption(localTd.LabelCap, delegate
					{
						this.thingDef = localTd;
						this.stuff = GenStuff.DefaultStuffFor(localTd);
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			if (this.thingDef.MadeFromStuff && Widgets.ButtonText(rect2, this.stuff.LabelCap, true, false, true))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (ThingDef current2 in from t in GenStuff.AllowedStuffsFor(this.thingDef)
				orderby t.label
				select t)
				{
					ThingDef localSd = current2;
					list2.Add(new FloatMenuOption(localSd.LabelCap, delegate
					{
						this.stuff = localSd;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			Widgets.TextFieldNumeric<int>(rect3, ref this.count, ref this.countBuf, 1f, 1E+09f);
		}

		public override bool TryMerge(ScenPart other)
		{
			ScenPart_ThingCount scenPart_ThingCount = other as ScenPart_ThingCount;
			if (scenPart_ThingCount != null && base.GetType() == scenPart_ThingCount.GetType() && this.thingDef == scenPart_ThingCount.thingDef && this.stuff == scenPart_ThingCount.stuff && this.count >= 0 && scenPart_ThingCount.count >= 0)
			{
				this.count += scenPart_ThingCount.count;
				return true;
			}
			return false;
		}

		protected virtual IEnumerable<ThingDef> PossibleThingDefs()
		{
			return from d in DefDatabase<ThingDef>.AllDefs
			where (d.category == ThingCategory.Item && d.scatterableOnMapGen && !d.destroyOnDrop) || (d.category == ThingCategory.Building && d.Minifiable) || (d.category == ThingCategory.Building && d.scatterableOnMapGen)
			select d;
		}
	}
}
