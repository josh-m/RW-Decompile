using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class ItemStashContentsComp : WorldObjectComp, IThingHolder
	{
		public ThingOwner contents;

		private static List<Thing> tmpContents = new List<Thing>();

		private static List<string> tmpContentsStr = new List<string>();

		public ItemStashContentsComp()
		{
			this.contents = new ThingOwner<Thing>(this);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Deep.Look<ThingOwner>(ref this.contents, "contents", new object[]
			{
				this
			});
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.contents;
		}

		public override void PostPostRemove()
		{
			base.PostPostRemove();
			this.contents.ClearAndDestroyContents(DestroyMode.Vanish);
		}

		public override string CompInspectStringExtra()
		{
			if (!this.contents.Any)
			{
				return null;
			}
			ItemStashContentsComp.tmpContents.Clear();
			ItemStashContentsComp.tmpContents.AddRange(this.contents);
			ItemStashContentsComp.tmpContents.SortByDescending((Thing x) => x.MarketValue * (float)x.stackCount);
			ItemStashContentsComp.tmpContentsStr.Clear();
			for (int i = 0; i < Mathf.Min(5, ItemStashContentsComp.tmpContents.Count); i++)
			{
				ItemStashContentsComp.tmpContentsStr.Add(ItemStashContentsComp.tmpContents[i].LabelShort);
			}
			string text = GenText.ToCommaList(ItemStashContentsComp.tmpContentsStr, true);
			int count = ItemStashContentsComp.tmpContents.Count;
			ItemStashContentsComp.tmpContents.Clear();
			ItemStashContentsComp.tmpContentsStr.Clear();
			if (count > 5)
			{
				return "SomeItemStashContents".Translate(new object[]
				{
					text
				});
			}
			return "ItemStashContents".Translate(new object[]
			{
				text
			});
		}
	}
}
