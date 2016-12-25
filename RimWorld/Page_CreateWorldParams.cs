using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Page_CreateWorldParams : Page
	{
		public static IntVec2 size;

		public static string seedString;

		private static readonly int[] WorldSizes = new int[]
		{
			150,
			200,
			250,
			300,
			350,
			400
		};

		public override string PageTitle
		{
			get
			{
				return "CreateWorld".Translate();
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			Page_CreateWorldParams.Reset();
		}

		public override void PostOpen()
		{
			base.PostOpen();
			TutorSystem.Notify_Event("PageStart-CreateWorldParams");
		}

		public static void Reset()
		{
			Page_CreateWorldParams.size = WorldGenerator.DefaultSize;
			Page_CreateWorldParams.seedString = GenText.RandomSeedString();
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DrawPageTitle(rect);
			GUI.BeginGroup(base.GetMainRect(rect, 0f, false));
			Text.Font = GameFont.Small;
			float num = 0f;
			Widgets.Label(new Rect(0f, num, 200f, 30f), "WorldSeed".Translate());
			Rect rect2 = new Rect(200f, num, 200f, 30f);
			Page_CreateWorldParams.seedString = Widgets.TextField(rect2, Page_CreateWorldParams.seedString);
			num += 40f;
			Rect rect3 = new Rect(200f, num, 200f, 30f);
			if (Widgets.ButtonText(rect3, "RandomizeSeed".Translate(), true, false, true))
			{
				SoundDefOf.TickTiny.PlayOneShotOnCamera();
				Page_CreateWorldParams.seedString = GenText.RandomSeedString();
			}
			num += 40f;
			Widgets.Label(new Rect(0f, num, 200f, 30f), "WorldSize".Translate());
			Rect rect4 = new Rect(200f, num, 200f, 30f);
			if (Widgets.ButtonText(rect4, Page_CreateWorldParams.size.ToStringCross(), true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				int[] worldSizes = Page_CreateWorldParams.WorldSizes;
				for (int i = 0; i < worldSizes.Length; i++)
				{
					int intSize = worldSizes[i];
					IntVec2 sizeVec = WorldGenerator.WorldSizeVectorFromInt(intSize);
					FloatMenuOption item = new FloatMenuOption(sizeVec.ToStringCross(), delegate
					{
						Page_CreateWorldParams.size = sizeVec;
					}, MenuOptionPriority.Medium, null, null, 0f, null);
					list.Add(item);
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			GUI.EndGroup();
			base.DoBottomButtons(rect, "WorldGenerate".Translate(), "Reset".Translate(), new Action(Page_CreateWorldParams.Reset), true);
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			LongEventHandler.QueueLongEvent(delegate
			{
				Find.GameInitData.ResetWorldRelatedMapInitData();
				Current.Game.World = WorldGenerator.GenerateWorld(Page_CreateWorldParams.size, Page_CreateWorldParams.seedString);
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					Find.Scenario.PostWorldLoad();
					if (this.next != null)
					{
						Find.WindowStack.Add(this.next);
					}
					this.Close(true);
				});
			}, "GeneratingWorld", true, null);
			return false;
		}
	}
}
