using System;
using System.Text;
using Verse;

namespace RimWorld
{
	public abstract class StatWorker_MeleeDamageAmount : StatWorker
	{
		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			float num = base.GetValueUnfinalized(req, true);
			ThingDef def = (ThingDef)req.Def;
			if (req.StuffDef != null)
			{
				StatDef statDef = null;
				DamageArmorCategory damageArmorCategory = this.CategoryOfDamage(def);
				if (damageArmorCategory != DamageArmorCategory.Blunt)
				{
					if (damageArmorCategory == DamageArmorCategory.Sharp)
					{
						statDef = StatDefOf.SharpDamageMultiplier;
					}
				}
				else
				{
					statDef = StatDefOf.BluntDamageMultiplier;
				}
				if (statDef != null)
				{
					num *= req.StuffDef.GetStatValueAbstract(statDef, null);
				}
			}
			return num;
		}

		public override string GetExplanation(StatRequest req, ToStringNumberSense numberSense)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetExplanation(req, numberSense));
			stringBuilder.AppendLine();
			ThingDef def = (ThingDef)req.Def;
			if (req.StuffDef != null)
			{
				StatDef statDef = null;
				DamageArmorCategory damageArmorCategory = this.CategoryOfDamage(def);
				if (damageArmorCategory != DamageArmorCategory.Blunt)
				{
					if (damageArmorCategory == DamageArmorCategory.Sharp)
					{
						statDef = StatDefOf.SharpDamageMultiplier;
					}
				}
				else
				{
					statDef = StatDefOf.BluntDamageMultiplier;
				}
				if (statDef != null)
				{
					stringBuilder.AppendLine(req.StuffDef.LabelCap + ": x" + req.StuffDef.GetStatValueAbstract(statDef, null).ToStringPercent());
				}
			}
			return stringBuilder.ToString();
		}

		protected abstract DamageArmorCategory CategoryOfDamage(ThingDef def);
	}
}
