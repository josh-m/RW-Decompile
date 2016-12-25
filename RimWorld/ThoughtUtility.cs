using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class ThoughtUtility
	{
		public static void GiveThoughtsForPawnDied(Pawn victim, DamageInfo? dinfo, Hediff hediff)
		{
			if (PawnGenerator.IsBeingGenerated(victim))
			{
				return;
			}
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			bool flag = (dinfo.HasValue && dinfo.Value.Def == DamageDefOf.ExecutionCut) || (hediff != null && (hediff.def == HediffDefOf.Euthanasia || hediff.def == HediffDefOf.ShutDown));
			bool flag2 = victim.IsPrisonerOfColony && !PrisonBreakUtility.IsPrisonBreaking(victim) && !victim.InAggroMentalState;
			if (victim.RaceProps.Humanlike)
			{
				if (dinfo.HasValue && dinfo.Value.Def.externalViolence && dinfo.Value.Instigator != null)
				{
					Pawn pawn = dinfo.Value.Instigator as Pawn;
					if (pawn != null && !pawn.Dead && pawn.needs.mood != null && pawn.story != null)
					{
						pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KilledHumanlikeBloodlust, null);
					}
				}
				List<Pawn> allPawnsSpawned = Find.MapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					Pawn pawn2 = allPawnsSpawned[i];
					if (pawn2.needs.mood != null)
					{
						if (!flag)
						{
							if (pawn2.MentalStateDef != MentalStateDefOf.SocialFighting || ((MentalState_SocialFighting)pawn2.MentalState).otherPawn != victim)
							{
								if (victim.Spawned && pawn2.Position.InHorDistOf(victim.Position, 12f) && GenSight.LineOfSight(victim.Position, pawn2.Position, false) && pawn2.Awake() && pawn2.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
								{
									if (pawn2.Faction == victim.Faction)
									{
										pawn2.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.WitnessedDeathAlly, null);
									}
									else
									{
										pawn2.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.WitnessedDeathNonAlly, null);
									}
									if (pawn2.relations.FamilyByBlood.Contains(victim))
									{
										pawn2.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.WitnessedDeathFamily, null);
									}
									pawn2.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.WitnessedDeathBloodlust, null);
								}
								else if (victim.Faction == Faction.OfPlayer && victim.Faction == pawn2.Faction && victim.HostFaction != pawn2.Faction)
								{
									pawn2.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KnowColonistDied, null);
								}
								if (flag2 && pawn2.Faction == Faction.OfPlayer && !pawn2.IsPrisoner)
								{
									pawn2.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KnowPrisonerDiedInnocent, null);
								}
							}
						}
					}
				}
			}
		}

		public static void GiveThoughtsForPawnExecuted(Pawn victim, PawnExecutionKind kind)
		{
			if (!victim.RaceProps.Humanlike)
			{
				return;
			}
			int forcedStage = 0;
			switch (kind)
			{
			case PawnExecutionKind.GenericBrutal:
				forcedStage = 1;
				break;
			case PawnExecutionKind.GenericHumane:
				forcedStage = 0;
				break;
			case PawnExecutionKind.OrganHarvesting:
				forcedStage = 2;
				break;
			}
			ThoughtDef def;
			if (victim.IsColonist)
			{
				def = ThoughtDefOf.KnowColonistExecuted;
			}
			else
			{
				def = ThoughtDefOf.KnowGuestExecuted;
			}
			foreach (Pawn current in Find.MapPawns.FreeColonistsAndPrisoners)
			{
				current.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtMaker.MakeThought(def, forcedStage), null);
			}
		}

		public static void GiveThoughtsForPawnOrganHarvested(Pawn victim)
		{
			if (!victim.RaceProps.Humanlike)
			{
				return;
			}
			ThoughtDef thoughtDef = null;
			if (victim.IsColonist)
			{
				thoughtDef = ThoughtDefOf.KnowColonistOrganHarvested;
			}
			else if (victim.HostFaction == Faction.OfPlayer)
			{
				thoughtDef = ThoughtDefOf.KnowGuestOrganHarvested;
			}
			foreach (Pawn current in Find.MapPawns.FreeColonistsAndPrisoners)
			{
				if (current == victim)
				{
					current.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.MyOrganHarvested, null);
				}
				else if (thoughtDef != null)
				{
					current.needs.mood.thoughts.memories.TryGainMemoryThought(thoughtDef, null);
				}
			}
		}

		public static bool IsSituationalThoughtNullifiedByHediffs(ThoughtDef def, Pawn pawn)
		{
			if (def.IsMemory)
			{
				return false;
			}
			float num = 0f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null && curStage.pctConditionalThoughtsNullified > num)
				{
					num = curStage.pctConditionalThoughtsNullified;
				}
			}
			if (num == 0f)
			{
				return false;
			}
			Rand.PushSeed();
			Rand.Seed = pawn.thingIDNumber * 31 + (int)(def.index * 139);
			bool result = Rand.Value < num;
			Rand.PopSeed();
			return result;
		}

		public static bool IsThoughtNullifiedByOwnTales(ThoughtDef def, Pawn pawn)
		{
			if (def.nullifyingOwnTales != null)
			{
				for (int i = 0; i < def.nullifyingOwnTales.Count; i++)
				{
					if (Find.TaleManager.GetLatestTale(def.nullifyingOwnTales[i], pawn) != null)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
