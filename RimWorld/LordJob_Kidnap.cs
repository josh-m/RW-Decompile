using System;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_Kidnap : LordJob
	{
		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_KidnapCover lordToil_KidnapCover = new LordToil_KidnapCover();
			lordToil_KidnapCover.avoidGridMode = AvoidGridMode.Smart;
			stateGraph.AddToil(lordToil_KidnapCover);
			LordToil_KidnapCover lordToil_KidnapCover2 = new LordToil_KidnapCover();
			lordToil_KidnapCover2.cover = false;
			lordToil_KidnapCover2.avoidGridMode = AvoidGridMode.Smart;
			stateGraph.AddToil(lordToil_KidnapCover2);
			Transition transition = new Transition(lordToil_KidnapCover, lordToil_KidnapCover2);
			transition.AddTrigger(new Trigger_TicksPassed(1200));
			stateGraph.AddTransition(transition);
			return stateGraph;
		}

		public override void ExposeData()
		{
		}
	}
}
