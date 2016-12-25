using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_Joinable_MarriageCeremony : LordJob_VoluntarilyJoinable
	{
		public Pawn firstPawn;

		public Pawn secondPawn;

		private IntVec3 spot;

		private Trigger_TicksPassed afterPartyTimeoutTrigger;

		public override bool LostImportantReferenceDuringLoading
		{
			get
			{
				return this.firstPawn == null || this.secondPawn == null;
			}
		}

		public LordJob_Joinable_MarriageCeremony()
		{
		}

		public LordJob_Joinable_MarriageCeremony(Pawn firstPawn, Pawn secondPawn, IntVec3 spot)
		{
			this.firstPawn = firstPawn;
			this.secondPawn = secondPawn;
			this.spot = spot;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Party lordToil_Party = new LordToil_Party(this.spot);
			stateGraph.AddToil(lordToil_Party);
			LordToil_MarriageCeremony lordToil_MarriageCeremony = new LordToil_MarriageCeremony(this.firstPawn, this.secondPawn, this.spot);
			stateGraph.AddToil(lordToil_MarriageCeremony);
			LordToil_Party lordToil_Party2 = new LordToil_Party(this.spot);
			stateGraph.AddToil(lordToil_Party2);
			LordToil_End lordToil_End = new LordToil_End();
			stateGraph.AddToil(lordToil_End);
			Transition transition = new Transition(lordToil_Party, lordToil_MarriageCeremony);
			transition.AddTrigger(new Trigger_TickCondition(() => this.lord.ticksInToil >= 5000 && this.AreFiancesInPartyArea()));
			transition.AddPreAction(new TransitionAction_Message("MessageMarriageCeremonyStarts".Translate(new object[]
			{
				this.firstPawn.LabelShort,
				this.secondPawn.LabelShort
			}), MessageSound.Benefit, this.firstPawn));
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_MarriageCeremony, lordToil_Party2);
			transition2.AddTrigger(new Trigger_TickCondition(() => this.firstPawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, this.secondPawn)));
			transition2.AddPreAction(new TransitionAction_Message("MessageNewlyMarried".Translate(new object[]
			{
				this.firstPawn.LabelShort,
				this.secondPawn.LabelShort
			}), MessageSound.Benefit, new TargetInfo(this.spot, base.Map, false)));
			transition2.AddPreAction(new TransitionAction_Custom(delegate
			{
				this.AddAttendedWeddingThoughts();
			}));
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_Party2, lordToil_End);
			transition3.AddTrigger(new Trigger_TickCondition(() => this.ShouldAfterPartyBeCalledOff()));
			transition3.AddTrigger(new Trigger_PawnLostViolently());
			transition3.AddPreAction(new TransitionAction_Message("MessageMarriageCeremonyCalledOff".Translate(new object[]
			{
				this.firstPawn.LabelShort,
				this.secondPawn.LabelShort
			}), MessageSound.Negative, new TargetInfo(this.spot, base.Map, false)));
			stateGraph.AddTransition(transition3);
			this.afterPartyTimeoutTrigger = new Trigger_TicksPassed(7500);
			Transition transition4 = new Transition(lordToil_Party2, lordToil_End);
			transition4.AddTrigger(this.afterPartyTimeoutTrigger);
			transition4.AddPreAction(new TransitionAction_Message("MessageMarriageCeremonyAfterPartyFinished".Translate(new object[]
			{
				this.firstPawn.LabelShort,
				this.secondPawn.LabelShort
			}), MessageSound.Benefit, this.firstPawn));
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_MarriageCeremony, lordToil_End);
			transition5.AddSource(lordToil_Party);
			transition5.AddTrigger(new Trigger_TickCondition(() => this.lord.ticksInToil >= 120000 && (this.firstPawn.Drafted || this.secondPawn.Drafted || !this.firstPawn.Position.InHorDistOf(this.spot, 4f) || !this.secondPawn.Position.InHorDistOf(this.spot, 4f))));
			transition5.AddPreAction(new TransitionAction_Message("MessageMarriageCeremonyCalledOff".Translate(new object[]
			{
				this.firstPawn.LabelShort,
				this.secondPawn.LabelShort
			}), MessageSound.Negative, new TargetInfo(this.spot, base.Map, false)));
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(lordToil_MarriageCeremony, lordToil_End);
			transition6.AddSource(lordToil_Party);
			transition6.AddTrigger(new Trigger_TickCondition(() => this.ShouldCeremonyBeCalledOff()));
			transition6.AddTrigger(new Trigger_PawnLostViolently());
			transition6.AddPreAction(new TransitionAction_Message("MessageMarriageCeremonyCalledOff".Translate(new object[]
			{
				this.firstPawn.LabelShort,
				this.secondPawn.LabelShort
			}), MessageSound.Negative, new TargetInfo(this.spot, base.Map, false)));
			stateGraph.AddTransition(transition6);
			return stateGraph;
		}

		private bool AreFiancesInPartyArea()
		{
			return this.lord.ownedPawns.Contains(this.firstPawn) && this.lord.ownedPawns.Contains(this.secondPawn) && this.firstPawn.Map == base.Map && PartyUtility.InPartyArea(this.firstPawn.Position, this.spot, base.Map) && this.secondPawn.Map == base.Map && PartyUtility.InPartyArea(this.secondPawn.Position, this.spot, base.Map);
		}

		private bool ShouldCeremonyBeCalledOff()
		{
			return this.firstPawn.Destroyed || this.secondPawn.Destroyed || !this.firstPawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, this.secondPawn) || (this.spot.GetDangerFor(this.firstPawn) != Danger.None || this.spot.GetDangerFor(this.secondPawn) != Danger.None) || (!MarriageCeremonyUtility.AcceptableMapConditionsToContinueCeremony(base.Map) || !MarriageCeremonyUtility.FianceCanContinueCeremony(this.firstPawn) || !MarriageCeremonyUtility.FianceCanContinueCeremony(this.secondPawn));
		}

		private bool ShouldAfterPartyBeCalledOff()
		{
			return this.firstPawn.Destroyed || this.secondPawn.Destroyed || (this.spot.GetDangerFor(this.firstPawn) != Danger.None || this.spot.GetDangerFor(this.secondPawn) != Danger.None) || !PartyUtility.AcceptableMapConditionsToContinueParty(base.Map);
		}

		public override float VoluntaryJoinPriorityFor(Pawn p)
		{
			if (this.IsFiance(p))
			{
				if (!MarriageCeremonyUtility.FianceCanContinueCeremony(p))
				{
					return 0f;
				}
				return 100f;
			}
			else
			{
				if (!this.IsGuest(p))
				{
					return 0f;
				}
				if (!MarriageCeremonyUtility.ShouldGuestKeepAttendingCeremony(p))
				{
					return 0f;
				}
				if (!this.lord.ownedPawns.Contains(p))
				{
					if (this.IsCeremonyAboutToEnd())
					{
						return 0f;
					}
					LordToil_MarriageCeremony lordToil_MarriageCeremony = this.lord.CurLordToil as LordToil_MarriageCeremony;
					IntVec3 intVec;
					if (lordToil_MarriageCeremony != null && !SpectatorCellFinder.TryFindSpectatorCellFor(p, lordToil_MarriageCeremony.Data.spectateRect, base.Map, out intVec, lordToil_MarriageCeremony.Data.spectateRectAllowedSides, 1, null))
					{
						return 0f;
					}
				}
				return 20f;
			}
		}

		public override void ExposeData()
		{
			Scribe_References.LookReference<Pawn>(ref this.firstPawn, "firstPawn", false);
			Scribe_References.LookReference<Pawn>(ref this.secondPawn, "secondPawn", false);
			Scribe_Values.LookValue<IntVec3>(ref this.spot, "spot", default(IntVec3), false);
		}

		public override string GetReport()
		{
			return "LordReportAttendingMarriageCeremony".Translate();
		}

		private bool IsCeremonyAboutToEnd()
		{
			return this.afterPartyTimeoutTrigger.TicksLeft < 1200;
		}

		private bool IsFiance(Pawn p)
		{
			return p == this.firstPawn || p == this.secondPawn;
		}

		private bool IsGuest(Pawn p)
		{
			return p.RaceProps.Humanlike && p != this.firstPawn && p != this.secondPawn && (p.Faction == this.firstPawn.Faction || p.Faction == this.secondPawn.Faction);
		}

		private void AddAttendedWeddingThoughts()
		{
			List<Pawn> ownedPawns = this.lord.ownedPawns;
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				if (ownedPawns[i] != this.firstPawn && ownedPawns[i] != this.secondPawn)
				{
					if (this.firstPawn.Position.InHorDistOf(ownedPawns[i].Position, 18f) || this.secondPawn.Position.InHorDistOf(ownedPawns[i].Position, 18f))
					{
						ownedPawns[i].needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.AttendedWedding, null);
					}
				}
			}
		}
	}
}
