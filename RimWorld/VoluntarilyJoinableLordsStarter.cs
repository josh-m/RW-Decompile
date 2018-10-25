using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class VoluntarilyJoinableLordsStarter : IExposable
	{
		private Map map;

		private int lastLordStartTick = -999999;

		private bool startPartyASAP;

		private const int CheckStartPartyIntervalTicks = 5000;

		private const float StartPartyMTBDays = 40f;

		public VoluntarilyJoinableLordsStarter(Map map)
		{
			this.map = map;
		}

		public bool TryStartMarriageCeremony(Pawn firstFiance, Pawn secondFiance)
		{
			IntVec3 intVec;
			if (!RCellFinder.TryFindMarriageSite(firstFiance, secondFiance, out intVec))
			{
				return false;
			}
			LordMaker.MakeNewLord(firstFiance.Faction, new LordJob_Joinable_MarriageCeremony(firstFiance, secondFiance, intVec), this.map, null);
			Messages.Message("MessageNewMarriageCeremony".Translate(firstFiance.LabelShort, secondFiance.LabelShort, firstFiance.Named("PAWN1"), secondFiance.Named("PAWN2")), new TargetInfo(intVec, this.map, false), MessageTypeDefOf.PositiveEvent, true);
			this.lastLordStartTick = Find.TickManager.TicksGame;
			return true;
		}

		public bool TryStartParty()
		{
			Pawn pawn = PartyUtility.FindRandomPartyOrganizer(Faction.OfPlayer, this.map);
			if (pawn == null)
			{
				return false;
			}
			IntVec3 intVec;
			if (!RCellFinder.TryFindPartySpot(pawn, out intVec))
			{
				return false;
			}
			LordMaker.MakeNewLord(pawn.Faction, new LordJob_Joinable_Party(intVec, pawn), this.map, null);
			Find.LetterStack.ReceiveLetter("LetterLabelNewParty".Translate(), "LetterNewParty".Translate(pawn.LabelShort, pawn), LetterDefOf.PositiveEvent, new TargetInfo(intVec, this.map, false), null, null);
			this.lastLordStartTick = Find.TickManager.TicksGame;
			this.startPartyASAP = false;
			return true;
		}

		public void VoluntarilyJoinableLordsStarterTick()
		{
			this.Tick_TryStartParty();
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.lastLordStartTick, "lastLordStartTick", 0, false);
			Scribe_Values.Look<bool>(ref this.startPartyASAP, "startPartyASAP", false, false);
		}

		private void Tick_TryStartParty()
		{
			if (!this.map.IsPlayerHome)
			{
				return;
			}
			if (Find.TickManager.TicksGame % 5000 == 0)
			{
				if (Rand.MTBEventOccurs(40f, 60000f, 5000f))
				{
					this.startPartyASAP = true;
				}
				if (this.startPartyASAP && Find.TickManager.TicksGame - this.lastLordStartTick >= 600000 && PartyUtility.AcceptableGameConditionsToStartParty(this.map))
				{
					this.TryStartParty();
				}
			}
		}
	}
}
