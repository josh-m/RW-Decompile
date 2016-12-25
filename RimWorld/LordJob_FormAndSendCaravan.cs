using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_FormAndSendCaravan : LordJob
	{
		public List<TransferableOneWay> transferables;

		private IntVec3 meetingPoint;

		private IntVec3 exitSpot;

		private int startingTile;

		private bool caravanSent;

		public LordJob_FormAndSendCaravan()
		{
		}

		public LordJob_FormAndSendCaravan(List<TransferableOneWay> transferables, IntVec3 meetingPoint, IntVec3 exitSpot, int startingTile)
		{
			this.transferables = transferables;
			this.meetingPoint = meetingPoint;
			this.exitSpot = exitSpot;
			this.startingTile = startingTile;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_PrepareCaravan_GatherAnimals lordToil_PrepareCaravan_GatherAnimals = new LordToil_PrepareCaravan_GatherAnimals(this.meetingPoint);
			stateGraph.AddToil(lordToil_PrepareCaravan_GatherAnimals);
			LordToil_PrepareCaravan_Pause lordToil_PrepareCaravan_Pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(lordToil_PrepareCaravan_Pause);
			LordToil_PrepareCaravan_GatherItems lordToil_PrepareCaravan_GatherItems = new LordToil_PrepareCaravan_GatherItems(this.meetingPoint);
			stateGraph.AddToil(lordToil_PrepareCaravan_GatherItems);
			LordToil_PrepareCaravan_Pause lordToil_PrepareCaravan_Pause2 = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(lordToil_PrepareCaravan_Pause2);
			LordToil_PrepareCaravan_GatherSlaves lordToil_PrepareCaravan_GatherSlaves = new LordToil_PrepareCaravan_GatherSlaves(this.meetingPoint);
			stateGraph.AddToil(lordToil_PrepareCaravan_GatherSlaves);
			LordToil_PrepareCaravan_Pause lordToil_PrepareCaravan_Pause3 = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(lordToil_PrepareCaravan_Pause3);
			LordToil_PrepareCaravan_Leave lordToil_PrepareCaravan_Leave = new LordToil_PrepareCaravan_Leave(this.exitSpot);
			stateGraph.AddToil(lordToil_PrepareCaravan_Leave);
			LordToil_PrepareCaravan_Pause lordToil_PrepareCaravan_Pause4 = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(lordToil_PrepareCaravan_Pause4);
			LordToil_End lordToil_End = new LordToil_End();
			stateGraph.AddToil(lordToil_End);
			Transition transition = new Transition(lordToil_PrepareCaravan_GatherAnimals, lordToil_End);
			transition.AddSource(lordToil_PrepareCaravan_GatherItems);
			transition.AddSource(lordToil_PrepareCaravan_GatherSlaves);
			transition.AddSource(lordToil_PrepareCaravan_Leave);
			transition.AddTrigger(new Trigger_Custom((TriggerSignal x) => x.type == TriggerSignalType.PawnLost && !this.caravanSent));
			transition.AddPreAction(new TransitionAction_Message("MessageFailedToSendCaravanBecausePawnLost".Translate(), MessageSound.Negative));
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_PrepareCaravan_GatherAnimals, lordToil_PrepareCaravan_GatherItems);
			transition2.AddTrigger(new Trigger_Memo("AllAnimalsGathered"));
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_PrepareCaravan_GatherItems, lordToil_PrepareCaravan_GatherSlaves);
			transition3.AddTrigger(new Trigger_Memo("AllItemsGathered"));
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_PrepareCaravan_GatherSlaves, lordToil_PrepareCaravan_Leave);
			transition4.AddTrigger(new Trigger_Memo("AllSlavesGathered"));
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_PrepareCaravan_Leave, lordToil_End);
			transition5.AddTrigger(new Trigger_Memo("ReadyToExitMap"));
			transition5.AddPreAction(new TransitionAction_Custom(new Action(this.SendCaravan)));
			stateGraph.AddTransition(transition5);
			Transition transition6 = this.PauseTransition(lordToil_PrepareCaravan_GatherAnimals, lordToil_PrepareCaravan_Pause);
			stateGraph.AddTransition(transition6);
			Transition transition7 = this.UnpauseTransition(lordToil_PrepareCaravan_Pause, lordToil_PrepareCaravan_GatherAnimals);
			stateGraph.AddTransition(transition7);
			Transition transition8 = this.PauseTransition(lordToil_PrepareCaravan_GatherItems, lordToil_PrepareCaravan_Pause2);
			stateGraph.AddTransition(transition8);
			Transition transition9 = this.UnpauseTransition(lordToil_PrepareCaravan_Pause2, lordToil_PrepareCaravan_GatherItems);
			stateGraph.AddTransition(transition9);
			Transition transition10 = this.PauseTransition(lordToil_PrepareCaravan_GatherSlaves, lordToil_PrepareCaravan_Pause3);
			stateGraph.AddTransition(transition10);
			Transition transition11 = this.UnpauseTransition(lordToil_PrepareCaravan_Pause3, lordToil_PrepareCaravan_GatherSlaves);
			stateGraph.AddTransition(transition11);
			Transition transition12 = this.PauseTransition(lordToil_PrepareCaravan_Leave, lordToil_PrepareCaravan_Pause4);
			stateGraph.AddTransition(transition12);
			Transition transition13 = this.UnpauseTransition(lordToil_PrepareCaravan_Pause4, lordToil_PrepareCaravan_Leave);
			stateGraph.AddTransition(transition13);
			return stateGraph;
		}

		private Transition PauseTransition(LordToil from, LordToil to)
		{
			Transition transition = new Transition(from, to);
			transition.AddPreAction(new TransitionAction_Message("MessageCaravanFormationPaused".Translate(), MessageSound.Standard));
			transition.AddTrigger(new Trigger_MentalState());
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			return transition;
		}

		private Transition UnpauseTransition(LordToil from, LordToil to)
		{
			Transition transition = new Transition(from, to);
			transition.AddPreAction(new TransitionAction_Message("MessageCaravanFormationUnpaused".Translate(), MessageSound.Benefit));
			transition.AddTrigger(new Trigger_NoMentalState());
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			return transition;
		}

		public override void ExposeData()
		{
			Scribe_Collections.LookList<TransferableOneWay>(ref this.transferables, "transferables", LookMode.Deep, new object[0]);
			Scribe_Values.LookValue<IntVec3>(ref this.meetingPoint, "meetingPoint", default(IntVec3), false);
			Scribe_Values.LookValue<IntVec3>(ref this.exitSpot, "exitSpot", default(IntVec3), false);
			Scribe_Values.LookValue<int>(ref this.startingTile, "startingTile", 0, false);
		}

		private void SendCaravan()
		{
			this.caravanSent = true;
			CaravanFormingUtility.FormAndCreateCaravan(this.lord.ownedPawns, this.lord.faction, this.startingTile);
		}

		public override void Notify_PawnAdded(Pawn p)
		{
			ReachabilityUtility.ClearCache();
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
			ReachabilityUtility.ClearCache();
		}

		public override bool CanOpenAnyDoor(Pawn p)
		{
			return true;
		}
	}
}
