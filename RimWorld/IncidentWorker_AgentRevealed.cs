using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_AgentRevealed : IncidentWorker
	{
		protected override bool CanFireNowSub()
		{
			return Find.MapPawns.FreeColonistsCount > 7 && (from p in Find.MapPawns.FreeColonists
			where this.CanBeAgent(p)
			select p).Count<Pawn>() > 6 && this.FindAgentFaction() != null;
		}

		private bool CanBeAgent(Pawn p)
		{
			return p.Faction == Faction.OfPlayer && !p.Downed && p.Awake() && p.health.summaryHealth.SummaryHealthPercent > 0.6f;
		}

		private Faction FindAgentFaction()
		{
			IEnumerable<Faction> source = from fac in Find.FactionManager.AllFactions
			where fac.def.humanlikeFaction && fac.HostileTo(Faction.OfPlayer)
			select fac;
			if (!source.Any<Faction>())
			{
				return null;
			}
			return source.RandomElement<Faction>();
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Faction faction = this.FindAgentFaction();
			if (faction == null)
			{
				return false;
			}
			IEnumerable<Pawn> source = from p in Find.MapPawns.FreeColonists
			where this.CanBeAgent(p)
			select p;
			Pawn pawn = source.ElementAt(Rand.Range(3, source.Count<Pawn>() - 1));
			if (pawn == null)
			{
				return false;
			}
			pawn.SetFaction(faction, null);
			List<Pawn> list = new List<Pawn>();
			list.Add(pawn);
			LordJob_ExitMapBest lordJob = new LordJob_ExitMapBest(LocomotionUrgency.Jog, false);
			LordMaker.MakeNewLord(faction, lordJob, list);
			string text = "LetterAgentRevealed".Translate(new object[]
			{
				pawn.Name,
				faction.Name
			});
			Find.LetterStack.ReceiveLetter("LetterLabelAgentRevealed".Translate(), text, LetterType.BadUrgent, pawn, null);
			return true;
		}
	}
}
