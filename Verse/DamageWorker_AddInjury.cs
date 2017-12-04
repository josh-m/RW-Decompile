using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class DamageWorker_AddInjury : DamageWorker
	{
		private const float SpreadDamageChance = 0.5f;

		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing thing)
		{
			Pawn pawn = thing as Pawn;
			if (pawn == null)
			{
				return base.Apply(dinfo, thing);
			}
			return this.ApplyToPawn(dinfo, pawn);
		}

		private DamageWorker.DamageResult ApplyToPawn(DamageInfo dinfo, Pawn pawn)
		{
			DamageWorker.DamageResult result = DamageWorker.DamageResult.MakeNew();
			if (dinfo.Amount <= 0)
			{
				return result;
			}
			if (!DebugSettings.enablePlayerDamage && pawn.Faction == Faction.OfPlayer)
			{
				return result;
			}
			Map mapHeld = pawn.MapHeld;
			bool spawnedOrAnyParentSpawned = pawn.SpawnedOrAnyParentSpawned;
			if (dinfo.Def.spreadOut)
			{
				if (pawn.apparel != null)
				{
					List<Apparel> wornApparel = pawn.apparel.WornApparel;
					for (int i = wornApparel.Count - 1; i >= 0; i--)
					{
						this.CheckApplySpreadDamage(dinfo, wornApparel[i]);
					}
				}
				if (pawn.equipment != null && pawn.equipment.Primary != null)
				{
					this.CheckApplySpreadDamage(dinfo, pawn.equipment.Primary);
				}
				if (pawn.inventory != null)
				{
					ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
					for (int j = innerContainer.Count - 1; j >= 0; j--)
					{
						this.CheckApplySpreadDamage(dinfo, innerContainer[j]);
					}
				}
			}
			if (this.ShouldFragmentDamageForDamageType(dinfo))
			{
				int num = Rand.RangeInclusive(3, 4);
				for (int k = 0; k < num; k++)
				{
					DamageInfo dinfo2 = dinfo;
					dinfo2.SetAmount(dinfo.Amount / num);
					this.ApplyDamageToPart(dinfo2, pawn, ref result);
				}
			}
			else
			{
				this.ApplyDamageToPart(dinfo, pawn, ref result);
				this.CheckDuplicateSmallPawnDamageToPartParent(dinfo, pawn, ref result);
			}
			if (result.wounded)
			{
				DamageWorker_AddInjury.PlayWoundedVoiceSound(dinfo, pawn);
				pawn.Drawer.Notify_DamageApplied(dinfo);
				DamageWorker_AddInjury.InformPsychology(dinfo, pawn);
			}
			if (result.headshot && pawn.Spawned)
			{
				MoteMaker.ThrowText(new Vector3((float)pawn.Position.x + 1f, (float)pawn.Position.y, (float)pawn.Position.z + 1f), pawn.Map, "Headshot".Translate(), Color.white, -1f);
				if (dinfo.Instigator != null)
				{
					Pawn pawn2 = dinfo.Instigator as Pawn;
					if (pawn2 != null)
					{
						pawn2.records.Increment(RecordDefOf.Headshots);
					}
				}
			}
			if (result.deflected)
			{
				if (pawn.health.deflectionEffecter == null)
				{
					pawn.health.deflectionEffecter = EffecterDefOf.ArmorDeflect.Spawn();
				}
				pawn.health.deflectionEffecter.Trigger(pawn, pawn);
			}
			else if (spawnedOrAnyParentSpawned)
			{
				ImpactSoundUtility.PlayImpactSound(pawn, dinfo.Def.impactSoundType, mapHeld);
			}
			return result;
		}

		private void CheckApplySpreadDamage(DamageInfo dinfo, Thing t)
		{
			if (dinfo.Def == DamageDefOf.Flame && !t.FlammableNow)
			{
				return;
			}
			if (UnityEngine.Random.value < 0.5f)
			{
				dinfo.SetAmount(Mathf.CeilToInt((float)dinfo.Amount * Rand.Range(0.35f, 0.7f)));
				t.TakeDamage(dinfo);
			}
		}

		private bool ShouldFragmentDamageForDamageType(DamageInfo dinfo)
		{
			return dinfo.AllowDamagePropagation && dinfo.Amount >= 9 && dinfo.Def.spreadOut;
		}

		private void CheckDuplicateSmallPawnDamageToPartParent(DamageInfo dinfo, Pawn pawn, ref DamageWorker.DamageResult result)
		{
			if (!dinfo.AllowDamagePropagation)
			{
				return;
			}
			if (result.LastHitPart != null && dinfo.Def.harmsHealth && result.LastHitPart != pawn.RaceProps.body.corePart && result.LastHitPart.parent != null && pawn.health.hediffSet.GetPartHealth(result.LastHitPart.parent) > 0f && dinfo.Amount >= 10 && pawn.HealthScale <= 0.5001f)
			{
				DamageInfo dinfo2 = dinfo;
				dinfo2.SetHitPart(result.LastHitPart.parent);
				this.ApplyDamageToPart(dinfo2, pawn, ref result);
			}
		}

		private void ApplyDamageToPart(DamageInfo dinfo, Pawn pawn, ref DamageWorker.DamageResult result)
		{
			BodyPartRecord exactPartFromDamageInfo = this.GetExactPartFromDamageInfo(dinfo, pawn);
			if (exactPartFromDamageInfo == null)
			{
				return;
			}
			dinfo.SetHitPart(exactPartFromDamageInfo);
			int num = dinfo.Amount;
			bool flag = !dinfo.InstantOldInjury;
			if (flag)
			{
				num = ArmorUtility.GetPostArmorDamage(pawn, dinfo.Amount, dinfo.HitPart, dinfo.Def);
			}
			if (num <= 0)
			{
				result.deflected = true;
				return;
			}
			if (DamageWorker_AddInjury.IsHeadshot(dinfo, pawn))
			{
				result.headshot = true;
			}
			if (dinfo.InstantOldInjury)
			{
				HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, dinfo.HitPart);
				if (hediffDefFromDamage.CompPropsFor(typeof(HediffComp_GetsOld)) == null || dinfo.HitPart.def.oldInjuryBaseChance == 0f || dinfo.HitPart.def.IsSolid(dinfo.HitPart, pawn.health.hediffSet.hediffs) || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(dinfo.HitPart))
				{
					return;
				}
			}
			if (!dinfo.AllowDamagePropagation)
			{
				this.FinalizeAndAddInjury(pawn, (float)num, dinfo, ref result);
				return;
			}
			this.ApplySpecialEffectsToPart(pawn, (float)num, dinfo, ref result);
		}

		protected virtual void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, ref DamageWorker.DamageResult result)
		{
			totalDamage = this.ReduceDamageToPreserveOutsideParts(totalDamage, dinfo, pawn);
			this.FinalizeAndAddInjury(pawn, totalDamage, dinfo, ref result);
			this.CheckDuplicateDamageToOuterParts(dinfo, pawn, totalDamage, ref result);
		}

		protected float FinalizeAndAddInjury(Pawn pawn, float totalDamage, DamageInfo dinfo, ref DamageWorker.DamageResult result)
		{
			if (pawn.health.hediffSet.PartIsMissing(dinfo.HitPart))
			{
				return 0f;
			}
			HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, dinfo.HitPart);
			Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, pawn, null);
			hediff_Injury.Part = dinfo.HitPart;
			hediff_Injury.source = dinfo.Weapon;
			hediff_Injury.sourceBodyPartGroup = dinfo.WeaponBodyPartGroup;
			hediff_Injury.sourceHediffDef = dinfo.WeaponLinkedHediff;
			hediff_Injury.Severity = totalDamage;
			if (dinfo.InstantOldInjury)
			{
				HediffComp_GetsOld hediffComp_GetsOld = hediff_Injury.TryGetComp<HediffComp_GetsOld>();
				if (hediffComp_GetsOld != null)
				{
					hediffComp_GetsOld.IsOld = true;
				}
				else
				{
					Log.Error(string.Concat(new object[]
					{
						"Tried to create instant old injury on Hediff without a GetsOld comp: ",
						hediffDefFromDamage,
						" on ",
						pawn
					}));
				}
			}
			return this.FinalizeAndAddInjury(pawn, hediff_Injury, dinfo, ref result);
		}

		protected float FinalizeAndAddInjury(Pawn pawn, Hediff_Injury injury, DamageInfo dinfo, ref DamageWorker.DamageResult result)
		{
			this.CalculateOldInjuryDamageThreshold(pawn, injury);
			pawn.health.AddHediff(injury, null, new DamageInfo?(dinfo));
			float num = Mathf.Min(injury.Severity, pawn.health.hediffSet.GetPartHealth(injury.Part));
			result.totalDamageDealt += num;
			result.wounded = true;
			result.AddPart(pawn, injury.Part);
			return num;
		}

		private void CalculateOldInjuryDamageThreshold(Pawn pawn, Hediff_Injury injury)
		{
			HediffCompProperties_GetsOld hediffCompProperties_GetsOld = injury.def.CompProps<HediffCompProperties_GetsOld>();
			if (hediffCompProperties_GetsOld == null)
			{
				return;
			}
			if (injury.Part.def.IsSolid(injury.Part, pawn.health.hediffSet.hediffs) || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(injury.Part) || injury.IsOld() || injury.Part.def.oldInjuryBaseChance < 1E-05f)
			{
				return;
			}
			bool isDelicate = injury.Part.def.IsDelicate;
			if ((Rand.Value <= injury.Part.def.oldInjuryBaseChance * hediffCompProperties_GetsOld.becomeOldChance && injury.Severity >= injury.Part.def.GetMaxHealth(pawn) * 0.25f && injury.Severity >= 7f) || isDelicate)
			{
				HediffComp_GetsOld hediffComp_GetsOld = injury.TryGetComp<HediffComp_GetsOld>();
				float num = 1f;
				float num2 = injury.Severity / 2f;
				if (num <= num2)
				{
					hediffComp_GetsOld.oldDamageThreshold = Rand.Range(num, num2);
				}
				if (isDelicate)
				{
					hediffComp_GetsOld.oldDamageThreshold = injury.Severity;
					hediffComp_GetsOld.IsOld = true;
				}
			}
		}

		private void CheckDuplicateDamageToOuterParts(DamageInfo dinfo, Pawn pawn, float totalDamage, ref DamageWorker.DamageResult result)
		{
			if (!dinfo.AllowDamagePropagation)
			{
				return;
			}
			if (dinfo.Def.harmAllLayersUntilOutside && dinfo.HitPart.depth == BodyPartDepth.Inside)
			{
				BodyPartRecord parent = dinfo.HitPart.parent;
				do
				{
					if (pawn.health.hediffSet.GetPartHealth(parent) != 0f)
					{
						HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, parent);
						Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, pawn, null);
						hediff_Injury.Part = parent;
						hediff_Injury.source = dinfo.Weapon;
						hediff_Injury.sourceBodyPartGroup = dinfo.WeaponBodyPartGroup;
						hediff_Injury.Severity = totalDamage;
						if (hediff_Injury.Severity <= 0f)
						{
							hediff_Injury.Severity = 1f;
						}
						this.FinalizeAndAddInjury(pawn, hediff_Injury, dinfo, ref result);
					}
					if (parent.depth == BodyPartDepth.Outside)
					{
						break;
					}
					parent = parent.parent;
				}
				while (parent != null);
			}
		}

		private static void InformPsychology(DamageInfo dinfo, Pawn pawn)
		{
		}

		private static bool IsHeadshot(DamageInfo dinfo, Pawn pawn)
		{
			return !dinfo.InstantOldInjury && dinfo.HitPart.groups.Contains(BodyPartGroupDefOf.FullHead) && dinfo.Def == DamageDefOf.Bullet;
		}

		private BodyPartRecord GetExactPartFromDamageInfo(DamageInfo dinfo, Pawn pawn)
		{
			if (dinfo.HitPart != null)
			{
				return (!pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).Any((BodyPartRecord x) => x == dinfo.HitPart)) ? null : dinfo.HitPart;
			}
			BodyPartRecord bodyPartRecord = this.ChooseHitPart(dinfo, pawn);
			if (bodyPartRecord == null)
			{
				Log.Warning("GetRandomNotMissingPart returned null (any part).");
			}
			return bodyPartRecord;
		}

		protected virtual BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, dinfo.Depth);
		}

		private static void PlayWoundedVoiceSound(DamageInfo dinfo, Pawn pawn)
		{
			if (pawn.Dead)
			{
				return;
			}
			if (dinfo.InstantOldInjury)
			{
				return;
			}
			if (!pawn.SpawnedOrAnyParentSpawned)
			{
				return;
			}
			if (dinfo.Def.externalViolence)
			{
				LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge ls) => ls.soundWounded, 1f);
			}
		}

		protected float ReduceDamageToPreserveOutsideParts(float postArmorDamage, DamageInfo dinfo, Pawn pawn)
		{
			if (DamageWorker_AddInjury.ShouldReduceDamageToPreservePart(dinfo.HitPart))
			{
				int num = Mathf.FloorToInt(pawn.health.hediffSet.GetPartHealth(dinfo.HitPart));
				if ((float)num >= 6f && postArmorDamage >= (float)num && postArmorDamage < (float)num * 1.9f)
				{
					postArmorDamage = (float)(num - 1);
				}
			}
			return postArmorDamage;
		}

		public static bool ShouldReduceDamageToPreservePart(BodyPartRecord bodyPart)
		{
			return bodyPart.depth == BodyPartDepth.Outside && !bodyPart.IsCorePart;
		}
	}
}
