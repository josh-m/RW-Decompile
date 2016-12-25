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
				return !Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare) && this.powerComp.PowerOn;
			}
		}

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			this.powerComp = base.GetComp<CompPowerTrader>();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.GoodToKnow);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.OpeningComms, OpportunityType.GoodToKnow);
		}

		private void UseAct(Pawn myPawn, ICommunicable commTarget)
		{
			Job job = new Job(JobDefOf.UseCommsConsole, this);
			job.commTarget = commTarget;
			myPawn.drafter.TakeOrderedJob(job);
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			if (!myPawn.CanReserve(this, 1))
			{
				FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Medium, null, null, 0f, null);
				return new List<FloatMenuOption>
				{
					item
				};
			}
			if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, TraverseMode.ByPawn))
			{
				FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Medium, null, null, 0f, null);
				return new List<FloatMenuOption>
				{
					item2
				};
			}
			if (Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare))
			{
				FloatMenuOption item3 = new FloatMenuOption("CannotUseSolarFlare".Translate(), null, MenuOptionPriority.Medium, null, null, 0f, null);
				return new List<FloatMenuOption>
				{
					item3
				};
			}
			if (!this.powerComp.PowerOn)
			{
				FloatMenuOption item4 = new FloatMenuOption("CannotUseNoPower".Translate(), null, MenuOptionPriority.Medium, null, null, 0f, null);
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
				}), null, MenuOptionPriority.Medium, null, null, 0f, null);
				return new List<FloatMenuOption>
				{
					item5
				};
			}
			if (!this.CanUseCommsNow)
			{
				Log.Error(myPawn + " could not use comm console for unknown reason.");
				FloatMenuOption item6 = new FloatMenuOption("Cannot use now", null, MenuOptionPriority.Medium, null, null, 0f, null);
				return new List<FloatMenuOption>
				{
					item6
				};
			}
			IEnumerable<ICommunicable> enumerable = Find.PassingShipManager.passingShips.Cast<ICommunicable>().Concat(Find.FactionManager.AllFactionsInViewOrder.Cast<ICommunicable>());
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (ICommunicable commTarget in enumerable)
			{
				Faction faction = commTarget as Faction;
				if (faction == null || !faction.IsPlayer)
				{
					ICommunicable localCommTarget = commTarget;
					Action action = delegate
					{
						ICommunicable localCommTarget = localCommTarget;
						if (commTarget is TradeShip && !Building_OrbitalTradeBeacon.AllPowered().Any<Building_OrbitalTradeBeacon>())
						{
							Messages.Message("MessageNeedBeaconToTradeWithShip".Translate(), this, MessageSound.RejectInput);
							return;
						}
						Job job = new Job(JobDefOf.UseCommsConsole, this);
						job.commTarget = localCommTarget;
						myPawn.drafter.TakeOrderedJob(job);
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
					};
					list.Add(new FloatMenuOption("CallOnRadio".Translate(new object[]
					{
						localCommTarget.GetCallLabel()
					}), action, MenuOptionPriority.Medium, null, null, 0f, null));
				}
			}
			return list;
		}
	}
}
