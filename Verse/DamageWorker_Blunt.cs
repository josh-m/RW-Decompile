using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	[HasDebugOutput]
	public class DamageWorker_Blunt : DamageWorker_AddInjury
	{
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside, null);
		}

		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
		{
			bool flag = Rand.Chance(this.def.bluntInnerHitChance);
			float num = (!flag) ? 0f : this.def.bluntInnerHitDamageFractionToConvert.RandomInRange;
			float num2 = totalDamage * (1f - num);
			DamageInfo lastInfo = dinfo;
			while (true)
			{
				num2 -= base.FinalizeAndAddInjury(pawn, num2, lastInfo, result);
				if (!pawn.health.hediffSet.PartIsMissing(lastInfo.HitPart))
				{
					break;
				}
				if (num2 <= 1f)
				{
					break;
				}
				BodyPartRecord parent = lastInfo.HitPart.parent;
				if (parent == null)
				{
					break;
				}
				lastInfo.SetHitPart(parent);
			}
			if (flag && !lastInfo.HitPart.def.IsSolid(lastInfo.HitPart, pawn.health.hediffSet.hediffs) && lastInfo.HitPart.depth == BodyPartDepth.Outside)
			{
				IEnumerable<BodyPartRecord> source = from x in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
				where x.parent == lastInfo.HitPart && x.def.IsSolid(x, pawn.health.hediffSet.hediffs) && x.depth == BodyPartDepth.Inside
				select x;
				BodyPartRecord hitPart;
				if (source.TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs, out hitPart))
				{
					DamageInfo lastInfo2 = lastInfo;
					lastInfo2.SetHitPart(hitPart);
					float totalDamage2 = totalDamage * num + totalDamage * this.def.bluntInnerHitDamageFractionToAdd.RandomInRange;
					base.FinalizeAndAddInjury(pawn, totalDamage2, lastInfo2, result);
				}
			}
			if (!pawn.Dead)
			{
				SimpleCurve simpleCurve = null;
				if (lastInfo.HitPart.parent == null)
				{
					simpleCurve = this.def.bluntStunChancePerDamagePctOfCorePartToBodyCurve;
				}
				else
				{
					foreach (BodyPartRecord current in pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource))
					{
						if (this.InSameBranch(current, lastInfo.HitPart))
						{
							simpleCurve = this.def.bluntStunChancePerDamagePctOfCorePartToHeadCurve;
							break;
						}
					}
				}
				if (simpleCurve != null)
				{
					float x2 = totalDamage / pawn.def.race.body.corePart.def.GetMaxHealth(pawn);
					if (Rand.Chance(simpleCurve.Evaluate(x2)))
					{
						DamageInfo dinfo2 = dinfo;
						dinfo2.Def = DamageDefOf.Stun;
						dinfo2.SetAmount((float)this.def.bluntStunDuration.SecondsToTicks() / 30f);
						pawn.TakeDamage(dinfo2);
					}
				}
			}
		}

		[DebugOutput]
		public static void StunChances()
		{
			Func<ThingDef, float, bool, string> bluntBodyStunChance = delegate(ThingDef d, float dam, bool onHead)
			{
				SimpleCurve simpleCurve = (!onHead) ? DamageDefOf.Blunt.bluntStunChancePerDamagePctOfCorePartToBodyCurve : DamageDefOf.Blunt.bluntStunChancePerDamagePctOfCorePartToHeadCurve;
				PawnGenerationRequest request = new PawnGenerationRequest(d.race.AnyPawnKind, Find.FactionManager.FirstFactionOfDef(d.race.AnyPawnKind.defaultFactionType), PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null, null);
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				float x = dam / d.race.body.corePart.def.GetMaxHealth(pawn);
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return Mathf.Clamp01(simpleCurve.Evaluate(x)).ToStringPercent();
			};
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName));
			list.Add(new TableDataGetter<ThingDef>("body size", (ThingDef d) => d.race.baseBodySize.ToString("F2")));
			list.Add(new TableDataGetter<ThingDef>("health scale", (ThingDef d) => d.race.baseHealthScale.ToString("F2")));
			list.Add(new TableDataGetter<ThingDef>("body size\n* health scale", (ThingDef d) => (d.race.baseHealthScale * d.race.baseBodySize).ToString("F2")));
			list.Add(new TableDataGetter<ThingDef>("core part\nhealth", delegate(ThingDef d)
			{
				PawnGenerationRequest request = new PawnGenerationRequest(d.race.AnyPawnKind, Find.FactionManager.FirstFactionOfDef(d.race.AnyPawnKind.defaultFactionType), PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null, null);
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				float maxHealth = d.race.body.corePart.def.GetMaxHealth(pawn);
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return maxHealth;
			}));
			list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n5", (ThingDef d) => bluntBodyStunChance(d, 5f, false)));
			list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n10", (ThingDef d) => bluntBodyStunChance(d, 10f, false)));
			list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n15", (ThingDef d) => bluntBodyStunChance(d, 15f, false)));
			list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n20", (ThingDef d) => bluntBodyStunChance(d, 20f, false)));
			list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n5", (ThingDef d) => bluntBodyStunChance(d, 5f, true)));
			list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n10", (ThingDef d) => bluntBodyStunChance(d, 10f, true)));
			list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n15", (ThingDef d) => bluntBodyStunChance(d, 15f, true)));
			list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n20", (ThingDef d) => bluntBodyStunChance(d, 20f, true)));
			DebugTables.MakeTablesDialog<ThingDef>(from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Pawn
			select d, list.ToArray());
		}

		private bool InSameBranch(BodyPartRecord lhs, BodyPartRecord rhs)
		{
			while (lhs.parent != null && lhs.parent.parent != null)
			{
				lhs = lhs.parent;
			}
			while (rhs.parent != null && rhs.parent.parent != null)
			{
				rhs = rhs.parent;
			}
			return lhs == rhs;
		}
	}
}
