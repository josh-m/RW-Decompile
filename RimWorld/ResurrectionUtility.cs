using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class ResurrectionUtility
	{
		private const float BrainDamageChancePerDaySinceDeath = 0.2f;

		private const float MinBrainDamageChance = 0.08f;

		private const float BlindnessChancePerDaySinceDeath = 0.12f;

		private const float MinBlindnessChance = 0.04f;

		private const float ResurrectionPsychosisChance = 0.3f;

		public static void Resurrect(Pawn pawn)
		{
			if (!pawn.Dead)
			{
				Log.Error("Tried to resurrect a pawn who is not dead: " + pawn.ToStringSafe<Pawn>());
				return;
			}
			if (pawn.Discarded)
			{
				Log.Error("Tried to resurrect a discarded pawn: " + pawn.ToStringSafe<Pawn>());
				return;
			}
			Corpse corpse = pawn.Corpse;
			bool flag = false;
			IntVec3 loc = IntVec3.Invalid;
			Map map = null;
			if (corpse != null)
			{
				flag = corpse.Spawned;
				loc = corpse.Position;
				map = corpse.Map;
				corpse.InnerPawn = null;
				corpse.Destroy(DestroyMode.Vanish);
			}
			if (flag && pawn.IsWorldPawn())
			{
				Find.WorldPawns.RemovePawn(pawn);
			}
			pawn.ForceSetStateToUnspawned();
			PawnComponentsUtility.CreateInitialComponents(pawn);
			pawn.health.Notify_Resurrected();
			if (pawn.Faction != null && pawn.Faction.IsPlayer)
			{
				if (pawn.workSettings != null)
				{
					pawn.workSettings.EnableAndInitialize();
				}
				Find.Storyteller.intenderPopulation.Notify_PopulationGained();
			}
			if (flag)
			{
				GenSpawn.Spawn(pawn, loc, map);
				for (int i = 0; i < 10; i++)
				{
					MoteMaker.ThrowAirPuffUp(pawn.DrawPos, map);
				}
				if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer))
				{
					LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssaultColony(pawn.Faction, true, true, false, false, true), pawn.Map, Gen.YieldSingle<Pawn>(pawn));
				}
				if (pawn.apparel != null)
				{
					List<Apparel> wornApparel = pawn.apparel.WornApparel;
					for (int j = 0; j < wornApparel.Count; j++)
					{
						wornApparel[j].Notify_PawnResurrected();
					}
				}
			}
		}

		public static void ResurrectWithSideEffects(Pawn pawn)
		{
			Corpse corpse = pawn.Corpse;
			float num;
			bool flag;
			if (corpse != null)
			{
				CompRottable comp = corpse.GetComp<CompRottable>();
				num = comp.RotProgress / 60000f;
				flag = (comp.Stage == RotStage.Fresh);
			}
			else
			{
				num = 0f;
				flag = true;
			}
			ResurrectionUtility.Resurrect(pawn);
			BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
			float chance = Mathf.Max(0.2f * num, 0.08f);
			if (Rand.Chance(chance) && brain != null)
			{
				int num2 = Rand.RangeInclusive(1, 5);
				int b = Mathf.FloorToInt(pawn.health.hediffSet.GetPartHealth(brain)) - 1;
				num2 = Mathf.Min(num2, b);
				if (num2 > 0 && !pawn.health.WouldDieAfterAddingHediff(HediffDefOf.Burn, brain, (float)num2))
				{
					DamageDef burn = DamageDefOf.Burn;
					int amount = num2;
					BodyPartRecord hitPart = brain;
					pawn.TakeDamage(new DamageInfo(burn, amount, -1f, null, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown));
				}
			}
			float chance2 = Mathf.Max(0.12f * num, 0.04f);
			if (Rand.Chance(chance2))
			{
				IEnumerable<BodyPartRecord> enumerable = from x in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
				where x.def == BodyPartDefOf.LeftEye || x.def == BodyPartDefOf.RightEye
				select x;
				foreach (BodyPartRecord current in enumerable)
				{
					Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Blindness, pawn, current);
					pawn.health.AddHediff(hediff, null, null);
				}
			}
			Hediff hediff2 = HediffMaker.MakeHediff(HediffDefOf.ResurrectionSickness, pawn, null);
			if (!pawn.health.WouldDieAfterAddingHediff(hediff2))
			{
				pawn.health.AddHediff(hediff2, null, null);
			}
			if ((!flag || Rand.Chance(0.3f)) && brain != null)
			{
				Hediff hediff3 = HediffMaker.MakeHediff(HediffDefOf.ResurrectionPsychosis, pawn, brain);
				if (!pawn.health.WouldDieAfterAddingHediff(hediff3))
				{
					pawn.health.AddHediff(hediff3, null, null);
				}
			}
			if (pawn.Dead)
			{
				Log.Error("The pawn has died while being resurrected.");
				ResurrectionUtility.Resurrect(pawn);
			}
		}
	}
}
