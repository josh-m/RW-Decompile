using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_RaidEnemy : IncidentWorker_Raid
	{
		protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			return base.FactionCanBeGroupSource(f, map, desperate) && f.HostileTo(Faction.OfPlayer) && (desperate || (float)GenDate.DaysPassed >= f.def.earliestRaidDays);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!base.TryExecuteWorker(parms))
			{
				return false;
			}
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			Find.StoryWatcher.statsRecord.numRaidsEnemy++;
			return true;
		}

		protected override bool TryResolveRaidFaction(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (parms.faction != null)
			{
				return true;
			}
			float num = parms.points;
			if (num <= 0f)
			{
				num = 999999f;
			}
			return PawnGroupMakerUtility.TryGetRandomFactionForNormalPawnGroup(num, out parms.faction, (Faction f) => this.FactionCanBeGroupSource(f, map, false), true, true, true, true) || PawnGroupMakerUtility.TryGetRandomFactionForNormalPawnGroup(num, out parms.faction, (Faction f) => this.FactionCanBeGroupSource(f, map, true), true, true, true, true);
		}

		protected override void ResolveRaidPoints(IncidentParms parms)
		{
			if (parms.points <= 0f)
			{
				Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.");
				parms.points = (float)Rand.Range(50, 300);
			}
		}

		protected override void ResolveRaidStrategy(IncidentParms parms)
		{
			if (parms.raidStrategy != null)
			{
				return;
			}
			Map map = (Map)parms.target;
			parms.raidStrategy = (from d in DefDatabase<RaidStrategyDef>.AllDefs
			where d.Worker.CanUseWith(parms)
			select d).RandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionChance(map));
		}

		protected override string GetLetterLabel(IncidentParms parms)
		{
			return parms.raidStrategy.letterLabelEnemy;
		}

		protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
		{
			string text = null;
			PawnsArriveMode raidArrivalMode = parms.raidArrivalMode;
			if (raidArrivalMode != PawnsArriveMode.EdgeWalkIn)
			{
				if (raidArrivalMode != PawnsArriveMode.EdgeDrop)
				{
					if (raidArrivalMode == PawnsArriveMode.CenterDrop)
					{
						text = "EnemyRaidCenterDrop".Translate(new object[]
						{
							parms.faction.def.pawnsPlural,
							parms.faction.Name
						});
					}
				}
				else
				{
					text = "EnemyRaidEdgeDrop".Translate(new object[]
					{
						parms.faction.def.pawnsPlural,
						parms.faction.Name
					});
				}
			}
			else
			{
				text = "EnemyRaidWalkIn".Translate(new object[]
				{
					parms.faction.def.pawnsPlural,
					parms.faction.Name
				});
			}
			text += "\n\n";
			text += parms.raidStrategy.arrivalTextEnemy;
			Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
			if (pawn != null)
			{
				text += "\n\n";
				text += "EnemyRaidLeaderPresent".Translate(new object[]
				{
					pawn.Faction.def.pawnsPlural,
					pawn.LabelShort
				});
			}
			return text;
		}

		protected override LetterDef GetLetterDef()
		{
			return LetterDefOf.ThreatBig;
		}

		protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
		{
			return "LetterRelatedPawnsRaidEnemy".Translate(new object[]
			{
				parms.faction.def.pawnsPlural
			});
		}
	}
}
