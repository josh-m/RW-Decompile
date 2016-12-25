using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_DefendBase : LordJob
	{
		private Faction faction;

		private IntVec3 baseCenter;

		public LordJob_DefendBase()
		{
		}

		public LordJob_DefendBase(Faction faction, IntVec3 baseCenter)
		{
			this.faction = faction;
			this.baseCenter = baseCenter;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_DefendBase lordToil_DefendBase = new LordToil_DefendBase(this.baseCenter);
			stateGraph.StartingToil = lordToil_DefendBase;
			LordToil_DefendBase lordToil_DefendBase2 = new LordToil_DefendBase(this.baseCenter);
			stateGraph.AddToil(lordToil_DefendBase2);
			LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
			lordToil_AssaultColony.avoidGridMode = AvoidGridMode.Smart;
			stateGraph.AddToil(lordToil_AssaultColony);
			Transition transition = new Transition(lordToil_DefendBase, lordToil_DefendBase2);
			transition.AddSource(lordToil_AssaultColony);
			transition.AddTrigger(new Trigger_BecameColonyAlly());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_DefendBase2, lordToil_DefendBase);
			transition2.AddTrigger(new Trigger_BecameColonyEnemy());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_DefendBase, lordToil_AssaultColony);
			transition3.AddTrigger(new Trigger_FractionPawnsLost(0.2f));
			transition3.AddTrigger(new Trigger_ChanceOnSignal(TriggerSignalType.PawnLost, 0.5f));
			transition3.AddTrigger(new Trigger_ChanceOnTickInteval(2500, 0.03f));
			transition3.AddTrigger(new Trigger_TicksPassed(251999));
			transition3.AddTrigger(new Trigger_UrgentlyHungry());
			transition3.AddPreAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition3);
			return stateGraph;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.LookReference<Faction>(ref this.faction, "faction", false);
			Scribe_Values.LookValue<IntVec3>(ref this.baseCenter, "baseCenter", default(IntVec3), false);
		}
	}
}
