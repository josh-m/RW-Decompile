using System;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_DefendAndExpandHive : LordJob
	{
		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_DefendAndExpandHive lordToil_DefendAndExpandHive = new LordToil_DefendAndExpandHive();
			lordToil_DefendAndExpandHive.distToHiveToAttack = 10f;
			stateGraph.StartingToil = lordToil_DefendAndExpandHive;
			LordToil_DefendAndExpandHive lordToil_DefendAndExpandHive2 = new LordToil_DefendAndExpandHive();
			lordToil_DefendAndExpandHive2.distToHiveToAttack = 32f;
			stateGraph.AddToil(lordToil_DefendAndExpandHive2);
			Transition transition = new Transition(lordToil_DefendAndExpandHive, lordToil_DefendAndExpandHive2);
			transition.AddTrigger(new Trigger_PawnHarmed());
			transition.AddTrigger(new Trigger_Memo("HiveAttacked"));
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_DefendAndExpandHive, lordToil_DefendAndExpandHive2);
			transition2.canMoveToSameState = true;
			transition2.AddSource(lordToil_DefendAndExpandHive2);
			transition2.AddTrigger(new Trigger_Memo("HiveDestroyed"));
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_DefendAndExpandHive2, lordToil_DefendAndExpandHive);
			transition3.AddTrigger(new Trigger_TicksPassedWithoutHarm(500));
			stateGraph.AddTransition(transition3);
			return stateGraph;
		}
	}
}
