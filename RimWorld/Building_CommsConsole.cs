using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Building_CommsConsole : Building
	{
		private CompPowerTrader powerComp;

		public bool CanUseCommsNow
		{
			get
			{
				return (!base.Spawned || !base.Map.mapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare)) && this.powerComp.PowerOn;
			}
		}

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			this.powerComp = base.GetComp<CompPowerTrader>();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.GoodToKnow);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.OpeningComms, OpportunityType.GoodToKnow);
		}

		private void UseAct(Pawn myPawn, ICommunicable commTarget)
		{
			Job job = new Job(JobDefOf.UseCommsConsole, this);
			job.commTarget = commTarget;
			myPawn.jobs.TryTakeOrderedJob(job);
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			if (!myPawn.CanReserve(this, 1))
			{
				FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
				return new List<FloatMenuOption>
				{
					item
				};
			}
			if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, TraverseMode.ByPawn))
			{
				FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
				return new List<FloatMenuOption>
				{
					item2
				};
			}
			if (base.Spawned && base.Map.mapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare))
			{
				FloatMenuOption item3 = new FloatMenuOption("CannotUseSolarFlare".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
				return new List<FloatMenuOption>
				{
					item3
				};
			}
			if (!this.powerComp.PowerOn)
			{
				FloatMenuOption item4 = new FloatMenuOption("CannotUseNoPower".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
				return new List<FloatMenuOption>
				{
					item4
				};
			}
			if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
			{
				FloatMenuOption item5 = new FloatMenuOption("CannotUseReason".Translate(new object[]
				{
					"IncapableOfCapacity".Translate(new object[]
					{
						PawnCapacityDefOf.Talking.label
					})
				}), null, MenuOptionPriority.Default, null, null, 0f, null, null);
				return new List<FloatMenuOption>
				{
					item5
				};
			}
			if (!this.CanUseCommsNow)
			{
				Log.Error(myPawn + " could not use comm console for unknown reason.");
				FloatMenuOption item6 = new FloatMenuOption("Cannot use now", null, MenuOptionPriority.Default, null, null, 0f, null, null);
				return new List<FloatMenuOption>
				{
					item6
				};
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			IEnumerable<ICommunicable> enumerable = myPawn.Map.passingShipManager.passingShips.Cast<ICommunicable>().Concat(Find.FactionManager.AllFactionsInViewOrder.Cast<ICommunicable>());
			foreach (ICommunicable commTarget in enumerable)
			{
				ICommunicable localCommTarget = commTarget;
				string text = "CallOnRadio".Translate(new object[]
				{
					localCommTarget.GetCallLabel()
				});
				Faction faction = localCommTarget as Faction;
				if (faction != null)
				{
					if (faction.IsPlayer)
					{
						continue;
					}
					if (!Building_CommsConsole.LeaderIsAvailableToTalk(faction))
					{
						list.Add(new FloatMenuOption(text + " (" + "LeaderUnavailable".Translate(new object[]
						{
							faction.leader.LabelShort
						}) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
						continue;
					}
				}
				Action action = delegate
				{
					ICommunicable localCommTarget = localCommTarget;
					if (commTarget is TradeShip && !Building_OrbitalTradeBeacon.AllPowered(this.Map).Any<Building_OrbitalTradeBeacon>())
					{
						Messages.Message("MessageNeedBeaconToTradeWithShip".Translate(), this, MessageSound.RejectInput);
						return;
					}
					Job job = new Job(JobDefOf.UseCommsConsole, this);
					job.commTarget = localCommTarget;
					myPawn.jobs.TryTakeOrderedJob(job);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
				};
				list.Add(new FloatMenuOption(text, action, MenuOptionPriority.InitiateSocial, null, null, 0f, null, null));
			}
			return list;
		}

		public static bool LeaderIsAvailableToTalk(Faction fac)
		{
			return fac.leader != null && (!fac.leader.Spawned || (!fac.leader.Downed && !fac.leader.IsPrisoner && fac.leader.Awake() && !fac.leader.InMentalState));
		}
	}
}
