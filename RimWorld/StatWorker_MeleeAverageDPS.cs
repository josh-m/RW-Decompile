using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class StatWorker_MeleeAverageDPS : StatWorker
	{
		public override bool ShouldShowFor(StatRequest req)
		{
			ThingDef thingDef = req.Def as ThingDef;
			if (thingDef == null)
			{
				return false;
			}
			if (!thingDef.IsWeapon && !thingDef.isTechHediff)
			{
				return false;
			}
			List<VerbProperties> list;
			List<Tool> list2;
			this.GetVerbsAndTools(thingDef, out list, out list2);
			if (!list2.NullOrEmpty<Tool>())
			{
				return true;
			}
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsMeleeAttack)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			ThingDef thingDef = req.Def as ThingDef;
			if (thingDef == null)
			{
				return 0f;
			}
			List<VerbProperties> verbProps;
			List<Tool> tools;
			this.GetVerbsAndTools(thingDef, out verbProps, out tools);
			Pawn attacker = StatWorker_MeleeAverageDPS.GetCurrentWeaponUser(req.Thing);
			float num = (from x in VerbUtility.GetAllVerbProperties(verbProps, tools)
			where x.verbProps.IsMeleeAttack
			select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, attacker, req.Thing, null, false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeDamageAmount(x.tool, attacker, req.Thing, null));
			float num2 = (from x in VerbUtility.GetAllVerbProperties(verbProps, tools)
			where x.verbProps.IsMeleeAttack
			select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, attacker, req.Thing, null, false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedCooldown(x.tool, attacker, req.Thing));
			if (num2 == 0f)
			{
				return 0f;
			}
			return num / num2;
		}

		public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
		{
			ThingDef thingDef = req.Def as ThingDef;
			if (thingDef == null)
			{
				return null;
			}
			List<VerbProperties> verbProps;
			List<Tool> tools;
			this.GetVerbsAndTools(thingDef, out verbProps, out tools);
			Pawn currentWeaponUser = StatWorker_MeleeAverageDPS.GetCurrentWeaponUser(req.Thing);
			IEnumerable<VerbUtility.VerbPropertiesWithSource> enumerable = from x in VerbUtility.GetAllVerbProperties(verbProps, tools)
			where x.verbProps.IsMeleeAttack
			select x;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (VerbUtility.VerbPropertiesWithSource current in enumerable)
			{
				float num = current.verbProps.AdjustedMeleeDamageAmount(current.tool, currentWeaponUser, req.Thing, null);
				float num2 = current.verbProps.AdjustedCooldown(current.tool, currentWeaponUser, req.Thing);
				if (current.tool != null)
				{
					stringBuilder.AppendLine(string.Format("  {0}: {1} ({2})", "Tool".Translate(), current.tool.LabelCap, current.ToolCapacity.defName));
				}
				else
				{
					stringBuilder.AppendLine(string.Format("  {0}:", "StatsReport_NonToolAttack".Translate()));
				}
				stringBuilder.AppendLine(string.Format("    {0} {1}", num.ToString("F1"), "DamageLower".Translate()));
				stringBuilder.AppendLine(string.Format("    {0} {1}", num2.ToString("F2"), "SecondsPerAttackLower".Translate()));
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		public static Pawn GetCurrentWeaponUser(Thing weapon)
		{
			if (weapon == null)
			{
				return null;
			}
			Pawn_EquipmentTracker pawn_EquipmentTracker = weapon.ParentHolder as Pawn_EquipmentTracker;
			if (pawn_EquipmentTracker != null)
			{
				return pawn_EquipmentTracker.pawn;
			}
			Pawn_ApparelTracker pawn_ApparelTracker = weapon.ParentHolder as Pawn_ApparelTracker;
			if (pawn_ApparelTracker != null)
			{
				return pawn_ApparelTracker.pawn;
			}
			return null;
		}

		private void GetVerbsAndTools(ThingDef def, out List<VerbProperties> verbs, out List<Tool> tools)
		{
			if (def.isTechHediff)
			{
				HediffDef hediffDef = this.FindTechHediffHediff(def);
				if (hediffDef == null)
				{
					verbs = null;
					tools = null;
					return;
				}
				HediffCompProperties_VerbGiver hediffCompProperties_VerbGiver = hediffDef.CompProps<HediffCompProperties_VerbGiver>();
				if (hediffCompProperties_VerbGiver == null)
				{
					verbs = null;
					tools = null;
					return;
				}
				verbs = hediffCompProperties_VerbGiver.verbs;
				tools = hediffCompProperties_VerbGiver.tools;
			}
			else
			{
				verbs = def.Verbs;
				tools = def.tools;
			}
		}

		private HediffDef FindTechHediffHediff(ThingDef techHediff)
		{
			List<RecipeDef> allDefsListForReading = DefDatabase<RecipeDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].addsHediff != null && allDefsListForReading[i].IsIngredient(techHediff))
				{
					return allDefsListForReading[i].addsHediff;
				}
			}
			return null;
		}
	}
}
