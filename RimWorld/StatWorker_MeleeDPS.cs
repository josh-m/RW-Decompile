using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class StatWorker_MeleeDPS : StatWorker
	{
		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			return this.GetMeleeDamage(req, applyPostProcess) * this.GetMeleeHitChance(req, applyPostProcess) / this.GetMeleeCooldown(req, applyPostProcess);
		}

		public override string GetExplanation(StatRequest req, ToStringNumberSense numberSense)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("StatsReport_MeleeDPSExplanation".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_MeleeDamage".Translate() + " (" + "AverageOfAllAttacks".Translate() + ")");
			stringBuilder.AppendLine("  " + this.GetMeleeDamage(req, true).ToString("0.##"));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_Cooldown".Translate() + " (" + "AverageOfAllAttacks".Translate() + ")");
			stringBuilder.AppendLine("  " + "StatsReport_CooldownFormat".Translate(new object[]
			{
				this.GetMeleeCooldown(req, true).ToString("0.##")
			}));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_MeleeHitChance".Translate());
			stringBuilder.AppendLine();
			stringBuilder.Append(this.GetMeleeHitChanceExplanation(req));
			return stringBuilder.ToString();
		}

		public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq)
		{
			return string.Format("{0} ( {1} / {2} / {3} )", new object[]
			{
				value.ToStringByStyle(stat.toStringStyle, numberSense),
				this.GetMeleeDamage(optionalReq, true).ToString("0.##"),
				this.GetMeleeCooldown(optionalReq, true).ToString("0.##"),
				StatDefOf.MeleeHitChance.ValueToString(this.GetMeleeHitChance(optionalReq, true), ToStringNumberSense.Absolute)
			});
		}

		private float GetMeleeDamage(StatRequest req, bool applyPostProcess = true)
		{
			Pawn pawn = req.Thing as Pawn;
			if (pawn == null)
			{
				return 0f;
			}
			List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList();
			if (updatedAvailableVerbsList.Count == 0)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
			{
				num += updatedAvailableVerbsList[i].SelectionWeight;
			}
			float num2 = 0f;
			for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
			{
				ThingWithComps ownerEquipment = updatedAvailableVerbsList[j].verb.ownerEquipment;
				num2 += updatedAvailableVerbsList[j].SelectionWeight / num * (float)updatedAvailableVerbsList[j].verb.verbProps.AdjustedMeleeDamageAmount(updatedAvailableVerbsList[j].verb, pawn, ownerEquipment);
			}
			return num2;
		}

		private float GetMeleeHitChance(StatRequest req, bool applyPostProcess = true)
		{
			if (req.HasThing)
			{
				return req.Thing.GetStatValue(StatDefOf.MeleeHitChance, applyPostProcess);
			}
			return req.Def.GetStatValueAbstract(StatDefOf.MeleeHitChance, null);
		}

		private float GetMeleeCooldown(StatRequest req, bool applyPostProcess = true)
		{
			Pawn pawn = req.Thing as Pawn;
			if (pawn == null)
			{
				return 1f;
			}
			List<VerbEntry> updatedAvailableVerbsList = pawn.meleeVerbs.GetUpdatedAvailableVerbsList();
			if (updatedAvailableVerbsList.Count == 0)
			{
				return 1f;
			}
			float num = 0f;
			for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
			{
				num += updatedAvailableVerbsList[i].SelectionWeight;
			}
			float num2 = 0f;
			for (int j = 0; j < updatedAvailableVerbsList.Count; j++)
			{
				ThingWithComps ownerEquipment = updatedAvailableVerbsList[j].verb.ownerEquipment;
				num2 += updatedAvailableVerbsList[j].SelectionWeight / num * (float)updatedAvailableVerbsList[j].verb.verbProps.AdjustedCooldownTicks(ownerEquipment);
			}
			return num2 / 60f;
		}

		private string GetMeleeHitChanceExplanation(StatRequest req)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(StatDefOf.MeleeHitChance.Worker.GetExplanation(req, StatDefOf.MeleeHitChance.toStringNumberSense));
			StatDefOf.MeleeHitChance.Worker.FinalizeExplanation(stringBuilder, req, StatDefOf.MeleeHitChance.toStringNumberSense, this.GetMeleeHitChance(req, true));
			StringBuilder stringBuilder2 = new StringBuilder();
			string[] array = stringBuilder.ToString().Split(new char[]
			{
				'\n'
			});
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder2.Append("  ");
				stringBuilder2.AppendLine(array[i]);
			}
			return stringBuilder2.ToString();
		}
	}
}
