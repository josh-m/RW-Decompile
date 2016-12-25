using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_PrisonBreak : LordJob
	{
		private IntVec3 groupUpLoc;

		private IntVec3 exitPoint;

		private int sapperThingID = -1;

		public LordJob_PrisonBreak()
		{
		}

		public LordJob_PrisonBreak(IntVec3 groupUpLoc, IntVec3 exitPoint, int sapperThingID)
		{
			this.groupUpLoc = groupUpLoc;
			this.exitPoint = exitPoint;
			this.sapperThingID = sapperThingID;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Travel lordToil_Travel = new LordToil_Travel(this.groupUpLoc);
			lordToil_Travel.maxDanger = Danger.Deadly;
			lordToil_Travel.avoidGridMode = AvoidGridMode.Smart;
			stateGraph.StartingToil = lordToil_Travel;
			LordToil_PrisonerEscape lordToil_PrisonerEscape = new LordToil_PrisonerEscape(this.exitPoint, this.sapperThingID);
			lordToil_PrisonerEscape.avoidGridMode = AvoidGridMode.Smart;
			stateGraph.AddToil(lordToil_PrisonerEscape);
			LordToil_ExitMapBest lordToil_ExitMapBest = new LordToil_ExitMapBest(LocomotionUrgency.Jog, false);
			lordToil_ExitMapBest.avoidGridMode = AvoidGridMode.Smart;
			stateGraph.AddToil(lordToil_ExitMapBest);
			LordToil_ExitMapBest lordToil_ExitMapBest2 = new LordToil_ExitMapBest(LocomotionUrgency.Jog, true);
			stateGraph.AddToil(lordToil_ExitMapBest2);
			Transition transition = new Transition(lordToil_Travel, lordToil_ExitMapBest2);
			transition.AddSources(new LordToil[]
			{
				lordToil_PrisonerEscape,
				lordToil_ExitMapBest
			});
			transition.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_ExitMapBest2, lordToil_ExitMapBest);
			transition2.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition2.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_Travel, lordToil_PrisonerEscape);
			transition3.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_Travel, lordToil_PrisonerEscape);
			transition4.AddTrigger(new Trigger_PawnLost());
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_PrisonerEscape, lordToil_PrisonerEscape);
			transition5.canMoveToSameState = true;
			transition5.AddTrigger(new Trigger_PawnLost());
			transition5.AddTrigger(new Trigger_PawnHarmed());
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(lordToil_PrisonerEscape, lordToil_ExitMapBest);
			transition6.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition6);
			return stateGraph;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<IntVec3>(ref this.groupUpLoc, "groupUpLoc", default(IntVec3), false);
			Scribe_Values.LookValue<IntVec3>(ref this.exitPoint, "exitPoint", default(IntVec3), false);
			Scribe_Values.LookValue<int>(ref this.sapperThingID, "sapperThingID", -1, false);
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
