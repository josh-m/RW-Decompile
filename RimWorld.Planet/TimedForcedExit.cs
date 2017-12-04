using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public class TimedForcedExit : WorldObjectComp
	{
		private int ticksLeftToForceExitAndRemoveMap = -1;

		public const int DefaultForceExitAndRemoveMapCountdownHours = 24;

		private static List<Pawn> tmpPawns = new List<Pawn>();

		public bool ForceExitAndRemoveMapCountdownActive
		{
			get
			{
				return this.ticksLeftToForceExitAndRemoveMap >= 0;
			}
		}

		public string ForceExitAndRemoveMapCountdownTimeLeftString
		{
			get
			{
				if (!this.ForceExitAndRemoveMapCountdownActive)
				{
					return string.Empty;
				}
				return TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(this.ticksLeftToForceExitAndRemoveMap);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.ticksLeftToForceExitAndRemoveMap, "ticksLeftToForceExitAndRemoveMap", -1, false);
		}

		public void StartForceExitAndRemoveMapCountdown()
		{
			this.StartForceExitAndRemoveMapCountdown(60000);
		}

		public void StartForceExitAndRemoveMapCountdown(int duration)
		{
			this.ticksLeftToForceExitAndRemoveMap = duration;
		}

		public override string CompInspectStringExtra()
		{
			if (this.ForceExitAndRemoveMapCountdownActive)
			{
				return "ForceExitAndRemoveMapCountdown".Translate(new object[]
				{
					this.ForceExitAndRemoveMapCountdownTimeLeftString
				}) + ".";
			}
			return null;
		}

		public override void CompTick()
		{
			MapParent mapParent = this.parent as MapParent;
			if (this.ForceExitAndRemoveMapCountdownActive)
			{
				if (mapParent.HasMap)
				{
					this.ticksLeftToForceExitAndRemoveMap--;
					if (this.ticksLeftToForceExitAndRemoveMap == 0)
					{
						if (Dialog_FormCaravan.AllSendablePawns(mapParent.Map, true).Any((Pawn x) => x.IsColonist))
						{
							Messages.Message("MessageYouHaveToReformCaravanNow".Translate(), new GlobalTargetInfo(mapParent.Tile), MessageTypeDefOf.NeutralEvent);
							Current.Game.VisibleMap = mapParent.Map;
							Dialog_FormCaravan window = new Dialog_FormCaravan(mapParent.Map, true, delegate
							{
								if (mapParent.HasMap)
								{
									this.ShowWorldViewIfVisibleMapAboutToBeRemoved(mapParent.Map);
									Find.WorldObjects.Remove(mapParent);
								}
							}, false, true);
							Find.WindowStack.Add(window);
						}
						else
						{
							TimedForcedExit.tmpPawns.Clear();
							TimedForcedExit.tmpPawns.AddRange(from x in mapParent.Map.mapPawns.AllPawns
							where x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer
							select x);
							if (TimedForcedExit.tmpPawns.Any<Pawn>())
							{
								if (TimedForcedExit.tmpPawns.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer)))
								{
									Caravan o = CaravanExitMapUtility.ExitMapAndCreateCaravan(TimedForcedExit.tmpPawns, Faction.OfPlayer, mapParent.Tile);
									Messages.Message("MessageAutomaticallyReformedCaravan".Translate(), o, MessageTypeDefOf.NeutralEvent);
								}
								else
								{
									StringBuilder stringBuilder = new StringBuilder();
									for (int i = 0; i < TimedForcedExit.tmpPawns.Count; i++)
									{
										stringBuilder.AppendLine("    " + TimedForcedExit.tmpPawns[i].LabelCap);
									}
									Find.LetterStack.ReceiveLetter("LetterLabelPawnsLostDueToMapCountdown".Translate(), "LetterPawnsLostDueToMapCountdown".Translate(new object[]
									{
										stringBuilder.ToString().TrimEndNewlines()
									}), LetterDefOf.NegativeEvent, new GlobalTargetInfo(mapParent.Tile), null);
								}
								TimedForcedExit.tmpPawns.Clear();
							}
							this.ShowWorldViewIfVisibleMapAboutToBeRemoved(mapParent.Map);
							Find.WorldObjects.Remove(mapParent);
						}
					}
				}
				else
				{
					this.ticksLeftToForceExitAndRemoveMap = -1;
				}
			}
		}

		public static string GetForceExitAndRemoveMapCountdownTimeLeftString(int ticksLeft)
		{
			if (ticksLeft < 0)
			{
				return string.Empty;
			}
			return ticksLeft.ToStringTicksToPeriod(true, true, true);
		}

		private void ShowWorldViewIfVisibleMapAboutToBeRemoved(Map map)
		{
			if (map == Find.VisibleMap)
			{
				Find.World.renderer.wantedMode = WorldRenderMode.Planet;
			}
		}
	}
}
