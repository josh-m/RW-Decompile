using System;
using Verse;
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
			LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
			stateGraph.AddToil(lordToil_AssaultColony);
			Transition transition = new Transition(lordToil_DefendAndExpandHive, lordToil_AssaultColony);
			transition.AddTrigger(new Trigger_PawnHarmed(0.5f, true));
			transition.AddTrigger(new Trigger_PawnLostViolently());
			transition.AddTrigger(new Trigger_Memo(Hive.MemoAttackedByEnemy));
			transition.AddTrigger(new Trigger_Memo(Hive.MemoBurnedBadly));
			transition.AddTrigger(new Trigger_Memo(Hive.MemoDestroyed));
			transition.AddTrigger(new Trigger_Memo(HediffGiver_Heat.MemoPawnBurnedByAir));
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_DefendAndExpandHive, lordToil_AssaultColony);
			transition2.canMoveToSameState = true;
			transition2.AddSource(lordToil_AssaultColony);
			transition2.AddTrigger(new Trigger_Memo(Hive.MemoDestroyed));
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_AssaultColony, lordToil_DefendAndExpandHive);
			transition3.AddTrigger(new Trigger_TicksPassedWithoutHarmOrMemos(1200, new string[]
			{
				Hive.MemoAttackedByEnemy,
				Hive.MemoBurnedBadly,
				Hive.MemoDestroyed,
				HediffGiver_Heat.MemoPawnBurnedByAir
			}));
			transition3.AddPostAction(new TransitionAction_EndAttackBuildingJobs());
			stateGraph.AddTransition(transition3);
			return stateGraph;
		}
	}
}
