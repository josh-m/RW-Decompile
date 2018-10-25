using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_FormAndSendCaravan : LordJob
	{
		public List<TransferableOneWay> transferables;

		public List<Pawn> downedPawns;

		private IntVec3 meetingPoint;

		private IntVec3 exitSpot;

		private int startingTile;

		private int destinationTile;

		private bool caravanSent;

		private LordToil gatherAnimals;

		private LordToil gatherAnimals_pause;

		private LordToil gatherItems;

		private LordToil gatherItems_pause;

		private LordToil gatherSlaves;

		private LordToil gatherSlaves_pause;

		private LordToil gatherDownedPawns;

		private LordToil gatherDownedPawns_pause;

		private LordToil leave;

		private LordToil leave_pause;

		public const float CustomWakeThreshold = 0.5f;

		public bool GatheringItemsNow
		{
			get
			{
				return this.lord.CurLordToil == this.gatherItems;
			}
		}

		public override bool AllowStartNewGatherings
		{
			get
			{
				return false;
			}
		}

		public override bool NeverInRestraints
		{
			get
			{
				return true;
			}
		}

		public override bool AddFleeToil
		{
			get
			{
				return false;
			}
		}

		public string Status
		{
			get
			{
				LordToil curLordToil = this.lord.CurLordToil;
				if (curLordToil == this.gatherAnimals)
				{
					return "FormingCaravanStatus_GatheringAnimals".Translate();
				}
				if (curLordToil == this.gatherAnimals_pause)
				{
					return "FormingCaravanStatus_GatheringAnimals_Pause".Translate();
				}
				if (curLordToil == this.gatherItems)
				{
					return "FormingCaravanStatus_GatheringItems".Translate();
				}
				if (curLordToil == this.gatherItems_pause)
				{
					return "FormingCaravanStatus_GatheringItems_Pause".Translate();
				}
				if (curLordToil == this.gatherSlaves)
				{
					return "FormingCaravanStatus_GatheringSlaves".Translate();
				}
				if (curLordToil == this.gatherSlaves_pause)
				{
					return "FormingCaravanStatus_GatheringSlaves_Pause".Translate();
				}
				if (curLordToil == this.gatherDownedPawns)
				{
					return "FormingCaravanStatus_GatheringDownedPawns".Translate();
				}
				if (curLordToil == this.gatherDownedPawns_pause)
				{
					return "FormingCaravanStatus_GatheringDownedPawns_Pause".Translate();
				}
				if (curLordToil == this.leave)
				{
					return "FormingCaravanStatus_Leaving".Translate();
				}
				if (curLordToil == this.leave_pause)
				{
					return "FormingCaravanStatus_Leaving_Pause".Translate();
				}
				return "FormingCaravanStatus_Waiting".Translate();
			}
		}

		public LordJob_FormAndSendCaravan()
		{
		}

		public LordJob_FormAndSendCaravan(List<TransferableOneWay> transferables, List<Pawn> downedPawns, IntVec3 meetingPoint, IntVec3 exitSpot, int startingTile, int destinationTile)
		{
			this.transferables = transferables;
			this.downedPawns = downedPawns;
			this.meetingPoint = meetingPoint;
			this.exitSpot = exitSpot;
			this.startingTile = startingTile;
			this.destinationTile = destinationTile;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			this.gatherAnimals = new LordToil_PrepareCaravan_GatherAnimals(this.meetingPoint);
			stateGraph.AddToil(this.gatherAnimals);
			this.gatherAnimals_pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(this.gatherAnimals_pause);
			this.gatherItems = new LordToil_PrepareCaravan_GatherItems(this.meetingPoint);
			stateGraph.AddToil(this.gatherItems);
			this.gatherItems_pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(this.gatherItems_pause);
			this.gatherSlaves = new LordToil_PrepareCaravan_GatherSlaves(this.meetingPoint);
			stateGraph.AddToil(this.gatherSlaves);
			this.gatherSlaves_pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(this.gatherSlaves_pause);
			this.gatherDownedPawns = new LordToil_PrepareCaravan_GatherDownedPawns(this.meetingPoint, this.exitSpot);
			stateGraph.AddToil(this.gatherDownedPawns);
			this.gatherDownedPawns_pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(this.gatherDownedPawns_pause);
			LordToil_PrepareCaravan_Wait lordToil_PrepareCaravan_Wait = new LordToil_PrepareCaravan_Wait(this.meetingPoint);
			stateGraph.AddToil(lordToil_PrepareCaravan_Wait);
			LordToil_PrepareCaravan_Pause lordToil_PrepareCaravan_Pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(lordToil_PrepareCaravan_Pause);
			this.leave = new LordToil_PrepareCaravan_Leave(this.exitSpot);
			stateGraph.AddToil(this.leave);
			this.leave_pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(this.leave_pause);
			LordToil_End lordToil_End = new LordToil_End();
			stateGraph.AddToil(lordToil_End);
			Transition transition = new Transition(this.gatherAnimals, this.gatherItems, false, true);
			transition.AddTrigger(new Trigger_Memo("AllAnimalsGathered"));
			stateGraph.AddTransition(transition, false);
			Transition transition2 = new Transition(this.gatherItems, this.gatherSlaves, false, true);
			transition2.AddTrigger(new Trigger_Memo("AllItemsGathered"));
			transition2.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition2, false);
			Transition transition3 = new Transition(this.gatherSlaves, this.gatherDownedPawns, false, true);
			transition3.AddTrigger(new Trigger_Memo("AllSlavesGathered"));
			transition3.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition3, false);
			Transition transition4 = new Transition(this.gatherDownedPawns, lordToil_PrepareCaravan_Wait, false, true);
			transition4.AddTrigger(new Trigger_Memo("AllDownedPawnsGathered"));
			transition4.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition4, false);
			Transition transition5 = new Transition(lordToil_PrepareCaravan_Wait, this.leave, false, true);
			transition5.AddTrigger(new Trigger_NoPawnsVeryTiredAndSleeping(0f));
			transition5.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition5, false);
			Transition transition6 = new Transition(this.leave, lordToil_End, false, true);
			transition6.AddTrigger(new Trigger_Memo("ReadyToExitMap"));
			transition6.AddPreAction(new TransitionAction_Custom(new Action(this.SendCaravan)));
			stateGraph.AddTransition(transition6, false);
			Transition transition7 = this.PauseTransition(this.gatherAnimals, this.gatherAnimals_pause);
			stateGraph.AddTransition(transition7, false);
			Transition transition8 = this.UnpauseTransition(this.gatherAnimals_pause, this.gatherAnimals);
			stateGraph.AddTransition(transition8, false);
			Transition transition9 = this.PauseTransition(this.gatherItems, this.gatherItems_pause);
			stateGraph.AddTransition(transition9, false);
			Transition transition10 = this.UnpauseTransition(this.gatherItems_pause, this.gatherItems);
			stateGraph.AddTransition(transition10, false);
			Transition transition11 = this.PauseTransition(this.gatherSlaves, this.gatherSlaves_pause);
			stateGraph.AddTransition(transition11, false);
			Transition transition12 = this.UnpauseTransition(this.gatherSlaves_pause, this.gatherSlaves);
			stateGraph.AddTransition(transition12, false);
			Transition transition13 = this.PauseTransition(this.gatherDownedPawns, this.gatherDownedPawns_pause);
			stateGraph.AddTransition(transition13, false);
			Transition transition14 = this.UnpauseTransition(this.gatherDownedPawns_pause, this.gatherDownedPawns);
			stateGraph.AddTransition(transition14, false);
			Transition transition15 = this.PauseTransition(this.leave, this.leave_pause);
			stateGraph.AddTransition(transition15, false);
			Transition transition16 = this.UnpauseTransition(this.leave_pause, this.leave);
			stateGraph.AddTransition(transition16, false);
			Transition transition17 = this.PauseTransition(lordToil_PrepareCaravan_Wait, lordToil_PrepareCaravan_Pause);
			stateGraph.AddTransition(transition17, false);
			Transition transition18 = this.UnpauseTransition(lordToil_PrepareCaravan_Pause, lordToil_PrepareCaravan_Wait);
			stateGraph.AddTransition(transition18, false);
			return stateGraph;
		}

		public override void LordJobTick()
		{
			base.LordJobTick();
			for (int i = this.downedPawns.Count - 1; i >= 0; i--)
			{
				if (this.downedPawns[i].Destroyed)
				{
					this.downedPawns.RemoveAt(i);
				}
				else if (!this.downedPawns[i].Downed)
				{
					this.lord.AddPawn(this.downedPawns[i]);
					this.downedPawns.RemoveAt(i);
				}
			}
		}

		public override string GetReport()
		{
			return "LordReportFormingCaravan".Translate();
		}

		private Transition PauseTransition(LordToil from, LordToil to)
		{
			Transition transition = new Transition(from, to, false, true);
			transition.AddPreAction(new TransitionAction_Message("MessageCaravanFormationPaused".Translate(), MessageTypeDefOf.NegativeEvent, () => this.lord.ownedPawns.FirstOrDefault((Pawn x) => x.InMentalState), null, 1f));
			transition.AddTrigger(new Trigger_MentalState());
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			return transition;
		}

		private Transition UnpauseTransition(LordToil from, LordToil to)
		{
			Transition transition = new Transition(from, to, false, true);
			transition.AddPreAction(new TransitionAction_Message("MessageCaravanFormationUnpaused".Translate(), MessageTypeDefOf.SilentInput, null, 1f));
			transition.AddTrigger(new Trigger_NoMentalState());
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			return transition;
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look<TransferableOneWay>(ref this.transferables, "transferables", LookMode.Deep, new object[0]);
			Scribe_Collections.Look<Pawn>(ref this.downedPawns, "downedPawns", LookMode.Reference, new object[0]);
			Scribe_Values.Look<IntVec3>(ref this.meetingPoint, "meetingPoint", default(IntVec3), false);
			Scribe_Values.Look<IntVec3>(ref this.exitSpot, "exitSpot", default(IntVec3), false);
			Scribe_Values.Look<int>(ref this.startingTile, "startingTile", 0, false);
			Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.downedPawns.RemoveAll(new Predicate<Pawn>(ThingUtility.DestroyedOrNull));
			}
		}

		private void SendCaravan()
		{
			this.caravanSent = true;
			CaravanFormingUtility.FormAndCreateCaravan(this.lord.ownedPawns.Concat(from x in this.downedPawns
			where JobGiver_PrepareCaravan_GatherDownedPawns.IsDownedPawnNearExitPoint(x, this.exitSpot)
			select x), this.lord.faction, base.Map.Tile, this.startingTile, this.destinationTile);
		}

		public override void Notify_PawnAdded(Pawn p)
		{
			base.Notify_PawnAdded(p);
			ReachabilityUtility.ClearCacheFor(p);
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
			base.Notify_PawnLost(p, condition);
			ReachabilityUtility.ClearCacheFor(p);
			if (!this.caravanSent)
			{
				if (condition == PawnLostCondition.IncappedOrKilled && p.Downed)
				{
					this.downedPawns.Add(p);
				}
				CaravanFormingUtility.RemovePawnFromCaravan(p, this.lord, false);
			}
		}

		public override bool CanOpenAnyDoor(Pawn p)
		{
			return true;
		}
	}
}
