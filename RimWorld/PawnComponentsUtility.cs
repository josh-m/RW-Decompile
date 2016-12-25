using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class PawnComponentsUtility
	{
		public static void CreateInitialComponents(Pawn pawn)
		{
			pawn.ageTracker = new Pawn_AgeTracker(pawn);
			pawn.health = new Pawn_HealthTracker(pawn);
			pawn.records = new Pawn_RecordsTracker(pawn);
			pawn.inventory = new Pawn_InventoryTracker(pawn);
			pawn.meleeVerbs = new Pawn_MeleeVerbs(pawn);
			pawn.verbTracker = new VerbTracker(pawn);
			pawn.carryTracker = new Pawn_CarryTracker(pawn);
			pawn.needs = new Pawn_NeedsTracker(pawn);
			pawn.mindState = new Pawn_MindState(pawn);
			if (pawn.RaceProps.ToolUser)
			{
				pawn.equipment = new Pawn_EquipmentTracker(pawn);
				pawn.apparel = new Pawn_ApparelTracker(pawn);
			}
			if (pawn.RaceProps.Humanlike)
			{
				pawn.ownership = new Pawn_Ownership(pawn);
				pawn.skills = new Pawn_SkillTracker(pawn);
				pawn.story = new Pawn_StoryTracker(pawn);
				pawn.guest = new Pawn_GuestTracker(pawn);
				pawn.guilt = new Pawn_GuiltTracker();
				pawn.workSettings = new Pawn_WorkSettings(pawn);
			}
			if (pawn.RaceProps.IsFlesh)
			{
				pawn.relations = new Pawn_RelationsTracker(pawn);
			}
			PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, false);
		}

		public static void AddComponentsForSpawn(Pawn pawn)
		{
			if (pawn.pather == null)
			{
				pawn.pather = new Pawn_PathFollower(pawn);
			}
			if (pawn.thinker == null)
			{
				pawn.thinker = new Pawn_Thinker(pawn);
			}
			if (pawn.jobs == null)
			{
				pawn.jobs = new Pawn_JobTracker(pawn);
			}
			if (pawn.stances == null)
			{
				pawn.stances = new Pawn_StanceTracker(pawn);
			}
			if (pawn.natives == null)
			{
				pawn.natives = new Pawn_NativeVerbs(pawn);
			}
			if (pawn.filth == null)
			{
				pawn.filth = new Pawn_FilthTracker(pawn);
			}
			if (pawn.RaceProps.intelligence <= Intelligence.ToolUser && pawn.caller == null)
			{
				pawn.caller = new Pawn_CallTracker(pawn);
			}
			if (pawn.RaceProps.IsFlesh && pawn.interactions == null)
			{
				pawn.interactions = new Pawn_InteractionsTracker(pawn);
			}
			PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
		}

		public static void RemoveComponentsOnKilled(Pawn pawn)
		{
			pawn.carryTracker = null;
			pawn.needs = null;
			pawn.mindState = null;
			pawn.workSettings = null;
			pawn.trader = null;
		}

		public static void RemoveComponentsOnDespawned(Pawn pawn)
		{
			pawn.pather = null;
			pawn.thinker = null;
			pawn.jobs = null;
			pawn.stances = null;
			pawn.natives = null;
			pawn.filth = null;
			pawn.caller = null;
			pawn.interactions = null;
			pawn.drafter = null;
		}

		public static void AddAndRemoveDynamicComponents(Pawn pawn, bool actAsIfSpawned = false)
		{
			bool flag = pawn.Faction != null && pawn.Faction.IsPlayer;
			bool flag2 = pawn.HostFaction != null && pawn.HostFaction.IsPlayer;
			if (pawn.RaceProps.Humanlike && !pawn.Dead)
			{
				if (pawn.mindState.wantsToTradeWithColony)
				{
					if (pawn.trader == null)
					{
						pawn.trader = new Pawn_TraderTracker(pawn);
					}
				}
				else
				{
					pawn.trader = null;
				}
			}
			if (pawn.RaceProps.Humanlike)
			{
				if (flag)
				{
					if (pawn.outfits == null)
					{
						pawn.outfits = new Pawn_OutfitTracker(pawn);
					}
					if (pawn.drugs == null)
					{
						pawn.drugs = new Pawn_DrugPolicyTracker(pawn);
					}
					if (pawn.timetable == null)
					{
						pawn.timetable = new Pawn_TimetableTracker(pawn);
					}
					if ((pawn.Spawned || actAsIfSpawned) && pawn.drafter == null)
					{
						pawn.drafter = new Pawn_DraftController(pawn);
					}
				}
				else
				{
					pawn.drafter = null;
				}
			}
			if (flag || flag2)
			{
				if (pawn.playerSettings == null)
				{
					pawn.playerSettings = new Pawn_PlayerSettings(pawn);
				}
			}
			if (pawn.RaceProps.intelligence <= Intelligence.ToolUser && pawn.Faction != null && !pawn.RaceProps.IsMechanoid && pawn.training == null)
			{
				pawn.training = new Pawn_TrainingTracker(pawn);
			}
			if (pawn.needs != null)
			{
				pawn.needs.AddOrRemoveNeedsAsAppropriate();
			}
		}
	}
}
