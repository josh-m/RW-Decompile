using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class Verb_MeleeAttackBase : Verb
	{
		private const int TargetCooldown = 50;

		public override bool IsMeleeAttack
		{
			get
			{
				return true;
			}
		}

		protected override bool TryCastShot()
		{
			Pawn casterPawn = base.CasterPawn;
			if (casterPawn.stances.FullBodyBusy)
			{
				return false;
			}
			Thing thing = this.currentTarget.Thing;
			if (!base.CanHitTarget(thing))
			{
				Log.Warning(string.Concat(new object[]
				{
					casterPawn,
					" meleed ",
					thing,
					" from out of melee position."
				}));
			}
			casterPawn.rotationTracker.Face(thing.DrawPos);
			if (!this.IsTargetImmobile(this.currentTarget) && casterPawn.skills != null)
			{
				casterPawn.skills.Learn(SkillDefOf.Melee, 250f, false);
			}
			bool result;
			SoundDef soundDef;
			if (Rand.Value < this.GetNonMissChance(thing))
			{
				if (Rand.Value > this.GetDodgeChance(thing))
				{
					BattleLogEntry_MeleeCombat log = this.CreateCombatLog(RulePackDefOf.Combat_Hit);
					result = true;
					this.ApplyMeleeDamageToTarget(this.currentTarget).InsertIntoLog(log);
					if (thing.def.category == ThingCategory.Building)
					{
						soundDef = this.SoundHitBuilding();
					}
					else
					{
						soundDef = this.SoundHitPawn();
					}
				}
				else
				{
					result = false;
					soundDef = this.SoundMiss();
					MoteMaker.ThrowText(thing.DrawPos, thing.Map, "TextMote_Dodge".Translate(), 1.9f);
					this.CreateCombatLog(RulePackDefOf.Combat_Dodge);
				}
			}
			else
			{
				result = false;
				soundDef = this.SoundMiss();
				this.CreateCombatLog(RulePackDefOf.Combat_Miss);
			}
			soundDef.PlayOneShot(new TargetInfo(thing.Position, casterPawn.Map, false));
			casterPawn.Drawer.Notify_MeleeAttackOn(thing);
			Pawn pawn = thing as Pawn;
			if (pawn != null && !pawn.Dead)
			{
				pawn.stances.StaggerFor(95);
				if (casterPawn.MentalStateDef != MentalStateDefOf.SocialFighting || pawn.MentalStateDef != MentalStateDefOf.SocialFighting)
				{
					pawn.mindState.meleeThreat = casterPawn;
					pawn.mindState.lastMeleeThreatHarmTick = Find.TickManager.TicksGame;
				}
			}
			casterPawn.rotationTracker.FaceCell(thing.Position);
			if (casterPawn.caller != null)
			{
				casterPawn.caller.Notify_DidMeleeAttack();
			}
			return result;
		}

		public BattleLogEntry_MeleeCombat CreateCombatLog(RulePackDef rulePack)
		{
			if (this.maneuver == null)
			{
				return null;
			}
			if (this.tool == null)
			{
				return null;
			}
			BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = new BattleLogEntry_MeleeCombat(rulePack, this.maneuver.combatLogRules, base.CasterPawn, this.currentTarget.Thing, this.implementOwnerType, (!this.tool.labelUsedInLogging) ? string.Empty : this.tool.label, (this.ownerEquipment != null) ? this.ownerEquipment.def : null, (this.ownerHediffComp != null) ? this.ownerHediffComp.Def : null);
			Find.BattleLog.Add(battleLogEntry_MeleeCombat);
			return battleLogEntry_MeleeCombat;
		}

		private float GetNonMissChance(LocalTargetInfo target)
		{
			if (this.surpriseAttack)
			{
				return 1f;
			}
			if (this.IsTargetImmobile(target))
			{
				return 1f;
			}
			return base.CasterPawn.GetStatValue(StatDefOf.MeleeHitChance, true);
		}

		private float GetDodgeChance(LocalTargetInfo target)
		{
			if (this.surpriseAttack)
			{
				return 0f;
			}
			if (this.IsTargetImmobile(target))
			{
				return 0f;
			}
			Pawn pawn = target.Thing as Pawn;
			if (pawn == null)
			{
				return 0f;
			}
			Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
			if (stance_Busy != null && stance_Busy.verb != null && !stance_Busy.verb.verbProps.MeleeRange)
			{
				return 0f;
			}
			return pawn.GetStatValue(StatDefOf.MeleeDodgeChance, true);
		}

		private bool IsTargetImmobile(LocalTargetInfo target)
		{
			Thing thing = target.Thing;
			Pawn pawn = thing as Pawn;
			return thing.def.category != ThingCategory.Pawn || pawn.Downed || pawn.GetPosture() != PawnPosture.Standing;
		}

		protected abstract DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target);

		private SoundDef SoundHitPawn()
		{
			if (this.ownerEquipment != null && this.ownerEquipment.Stuff != null)
			{
				if (this.verbProps.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
				{
					if (!this.ownerEquipment.Stuff.stuffProps.soundMeleeHitSharp.NullOrUndefined())
					{
						return this.ownerEquipment.Stuff.stuffProps.soundMeleeHitSharp;
					}
				}
				else if (!this.ownerEquipment.Stuff.stuffProps.soundMeleeHitBlunt.NullOrUndefined())
				{
					return this.ownerEquipment.Stuff.stuffProps.soundMeleeHitBlunt;
				}
			}
			if (base.CasterPawn != null && !base.CasterPawn.def.race.soundMeleeHitPawn.NullOrUndefined())
			{
				return base.CasterPawn.def.race.soundMeleeHitPawn;
			}
			return SoundDefOf.Pawn_Melee_Punch_HitPawn;
		}

		private SoundDef SoundHitBuilding()
		{
			if (this.ownerEquipment != null && this.ownerEquipment.Stuff != null)
			{
				if (this.verbProps.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
				{
					if (!this.ownerEquipment.Stuff.stuffProps.soundMeleeHitSharp.NullOrUndefined())
					{
						return this.ownerEquipment.Stuff.stuffProps.soundMeleeHitSharp;
					}
				}
				else if (!this.ownerEquipment.Stuff.stuffProps.soundMeleeHitBlunt.NullOrUndefined())
				{
					return this.ownerEquipment.Stuff.stuffProps.soundMeleeHitBlunt;
				}
			}
			if (base.CasterPawn != null && !base.CasterPawn.def.race.soundMeleeHitBuilding.NullOrUndefined())
			{
				return base.CasterPawn.def.race.soundMeleeHitBuilding;
			}
			return SoundDefOf.Pawn_Melee_Punch_HitBuilding;
		}

		private SoundDef SoundMiss()
		{
			if (base.CasterPawn != null && !base.CasterPawn.def.race.soundMeleeMiss.NullOrUndefined())
			{
				return base.CasterPawn.def.race.soundMeleeMiss;
			}
			return SoundDefOf.Pawn_Melee_Punch_Miss;
		}
	}
}
