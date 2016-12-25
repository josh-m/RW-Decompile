using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class Dialog_InfoCard : Window
	{
		private enum InfoCardTab : byte
		{
			Stats,
			Character,
			Health,
			Records
		}

		private Def def;

		private Thing thing;

		private ThingDef stuff;

		private Dialog_InfoCard.InfoCardTab tab;

		private Def Def
		{
			get
			{
				if (this.thing != null)
				{
					return this.thing.def;
				}
				return this.def;
			}
		}

		private Pawn ThingPawn
		{
			get
			{
				return this.thing as Pawn;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(950f, 760f);
			}
		}

		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		public Dialog_InfoCard(Thing thing)
		{
			this.thing = thing;
			this.tab = Dialog_InfoCard.InfoCardTab.Stats;
			this.Setup();
		}

		public Dialog_InfoCard(Def onlyDef)
		{
			this.def = onlyDef;
			this.Setup();
		}

		public Dialog_InfoCard(ThingDef thingDef, ThingDef stuff)
		{
			this.def = thingDef;
			this.stuff = stuff;
			this.Setup();
		}

		private void Setup()
		{
			this.forcePause = true;
			this.closeOnEscapeKey = true;
			this.doCloseButton = true;
			this.doCloseX = true;
			this.absorbInputAroundWindow = true;
			this.soundAppear = SoundDef.Named("InfoCard_Open");
			this.soundClose = SoundDef.Named("InfoCard_Close");
			StatsReportUtility.Reset();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InfoCard, KnowledgeAmount.Total);
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(inRect);
			rect = rect.ContractedBy(18f);
			rect.height = 34f;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, this.GetTitle());
			Rect rect2 = new Rect(inRect);
			rect2.yMin = rect.yMax;
			rect2.yMax -= 38f;
			Rect rect3 = rect2;
			rect3.yMin += 45f;
			List<TabRecord> list = new List<TabRecord>();
			TabRecord item = new TabRecord("TabStats".Translate(), delegate
			{
				this.tab = Dialog_InfoCard.InfoCardTab.Stats;
			}, this.tab == Dialog_InfoCard.InfoCardTab.Stats);
			list.Add(item);
			if (this.ThingPawn != null)
			{
				if (this.ThingPawn.RaceProps.Humanlike)
				{
					TabRecord item2 = new TabRecord("TabCharacter".Translate(), delegate
					{
						this.tab = Dialog_InfoCard.InfoCardTab.Character;
					}, this.tab == Dialog_InfoCard.InfoCardTab.Character);
					list.Add(item2);
				}
				TabRecord item3 = new TabRecord("TabHealth".Translate(), delegate
				{
					this.tab = Dialog_InfoCard.InfoCardTab.Health;
				}, this.tab == Dialog_InfoCard.InfoCardTab.Health);
				list.Add(item3);
				TabRecord item4 = new TabRecord("TabRecords".Translate(), delegate
				{
					this.tab = Dialog_InfoCard.InfoCardTab.Records;
				}, this.tab == Dialog_InfoCard.InfoCardTab.Records);
				list.Add(item4);
			}
			TabDrawer.DrawTabs(rect3, list);
			this.FillCard(rect3.ContractedBy(18f));
		}

		protected void FillCard(Rect cardRect)
		{
			if (this.tab == Dialog_InfoCard.InfoCardTab.Stats)
			{
				if (this.thing != null)
				{
					Thing innerThing = this.thing;
					MinifiedThing minifiedThing = this.thing as MinifiedThing;
					if (minifiedThing != null)
					{
						innerThing = minifiedThing.InnerThing;
					}
					StatsReportUtility.DrawStatsReport(cardRect, innerThing);
				}
				else
				{
					StatsReportUtility.DrawStatsReport(cardRect, this.def, this.stuff);
				}
			}
			else if (this.tab == Dialog_InfoCard.InfoCardTab.Character)
			{
				CharacterCardUtility.DrawCharacterCard(cardRect, (Pawn)this.thing);
			}
			else if (this.tab == Dialog_InfoCard.InfoCardTab.Health)
			{
				cardRect.yMin += 8f;
				HealthCardUtility.DrawPawnHealthCard(cardRect, (Pawn)this.thing, false, false, null);
			}
			else if (this.tab == Dialog_InfoCard.InfoCardTab.Records)
			{
				RecordsCardUtility.DrawRecordsCard(cardRect, (Pawn)this.thing);
			}
		}

		private string GetTitle()
		{
			if (this.thing != null)
			{
				return this.thing.LabelCapNoCount;
			}
			ThingDef thingDef = this.Def as ThingDef;
			if (thingDef != null)
			{
				return GenLabel.ThingLabel(thingDef, this.stuff, 1).CapitalizeFirst();
			}
			return this.Def.LabelCap;
		}

		private string GetBasicDescription()
		{
			return this.Def.description;
		}
	}
}
