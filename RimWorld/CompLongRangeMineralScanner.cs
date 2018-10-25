using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompLongRangeMineralScanner : ThingComp
	{
		private ThingDef targetMineable;

		private float daysWorkingSinceLastMinerals;

		private CompPowerTrader powerComp;

		public CompProperties_LongRangeMineralScanner Props
		{
			get
			{
				return (CompProperties_LongRangeMineralScanner)this.props;
			}
		}

		public bool CanUseNow
		{
			get
			{
				return this.parent.Spawned && (this.powerComp == null || this.powerComp.PowerOn) && this.parent.Faction == Faction.OfPlayer;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Defs.Look<ThingDef>(ref this.targetMineable, "targetMineable");
			Scribe_Values.Look<float>(ref this.daysWorkingSinceLastMinerals, "daysWorkingSinceLastMinerals", 0f, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.targetMineable == null)
			{
				this.SetDefaultTargetMineral();
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			this.SetDefaultTargetMineral();
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			this.powerComp = this.parent.GetComp<CompPowerTrader>();
		}

		private void SetDefaultTargetMineral()
		{
			this.targetMineable = ThingDefOf.MineableGold;
		}

		public void Used(Pawn worker)
		{
			if (!this.CanUseNow)
			{
				Log.Error("Used while CanUseNow is false.", false);
			}
			float statValue = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
			this.daysWorkingSinceLastMinerals += statValue / 60000f;
			if (Find.TickManager.TicksGame % 59 == 0)
			{
				float mtb = this.Props.mtbDays / statValue;
				if (this.daysWorkingSinceLastMinerals >= this.Props.guaranteedToFindLumpAfterDaysWorking || Rand.MTBEventOccurs(mtb, 60000f, 59f))
				{
					this.FoundMinerals(worker);
				}
			}
		}

		private void FoundMinerals(Pawn worker)
		{
			this.daysWorkingSinceLastMinerals = 0f;
			IntRange preciousLumpSiteDistanceRange = SiteTuning.PreciousLumpSiteDistanceRange;
			int min = preciousLumpSiteDistanceRange.min;
			int max = preciousLumpSiteDistanceRange.max;
			int tile = this.parent.Tile;
			int tile2;
			if (!TileFinder.TryFindNewSiteTile(out tile2, min, max, false, true, tile))
			{
				return;
			}
			Site site = SiteMaker.TryMakeSite_SingleSitePart(SiteCoreDefOf.PreciousLump, (!Rand.Chance(0.6f)) ? "MineralScannerPreciousLumpThreat" : null, tile2, null, true, null, true, null);
			if (site != null)
			{
				site.sitePartsKnown = true;
				site.core.parms.preciousLumpResources = this.targetMineable;
				int randomInRange = SiteTuning.MineralScannerPreciousLumpTimeoutDaysRange.RandomInRange;
				site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
				Find.WorldObjects.Add(site);
				Find.LetterStack.ReceiveLetter("LetterLabelFoundPreciousLump".Translate(), "LetterFoundPreciousLump".Translate(this.targetMineable.label, randomInRange, SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault<SitePart>()).CapitalizeFirst(), worker.LabelShort, worker.Named("WORKER")), LetterDefOf.PositiveEvent, site, null, null);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (this.parent.Faction == Faction.OfPlayer)
			{
				ThingDef resource = this.targetMineable.building.mineableThing;
				Command_Action setTarg = new Command_Action();
				setTarg.defaultLabel = "CommandSelectMineralToScanFor".Translate() + ": " + resource.LabelCap;
				setTarg.icon = resource.uiIcon;
				setTarg.iconAngle = resource.uiIconAngle;
				setTarg.iconOffset = resource.uiIconOffset;
				setTarg.action = delegate
				{
					List<ThingDef> mineables = ((GenStep_PreciousLump)GenStepDefOf.PreciousLump.genStep).mineables;
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (ThingDef current in mineables)
					{
						ThingDef localD = current;
						FloatMenuOption item = new FloatMenuOption(localD.building.mineableThing.LabelCap, delegate
						{
							foreach (object current2 in Find.Selector.SelectedObjects)
							{
								Thing thing = current2 as Thing;
								if (thing != null)
								{
									CompLongRangeMineralScanner compLongRangeMineralScanner = thing.TryGetComp<CompLongRangeMineralScanner>();
									if (compLongRangeMineralScanner != null)
									{
										compLongRangeMineralScanner.targetMineable = localD;
									}
								}
							}
						}, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, localD.building.mineableThing), null);
						list.Add(item);
					}
					Find.WindowStack.Add(new FloatMenu(list));
				};
				yield return setTarg;
			}
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "Dev: Find resources now",
					action = delegate
					{
						this.$this.FoundMinerals(PawnsFinder.AllMaps_FreeColonists.FirstOrDefault<Pawn>());
					}
				};
			}
		}
	}
}
