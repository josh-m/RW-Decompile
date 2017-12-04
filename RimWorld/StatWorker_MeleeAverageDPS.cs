using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class StatWorker_MeleeAverageDPS : StatWorker
	{
		public override bool ShouldShowFor(BuildableDef def)
		{
			ThingDef thingDef = def as ThingDef;
			return thingDef != null && thingDef.IsWeapon && !thingDef.tools.NullOrEmpty<Tool>();
		}

		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			Thing thing = req.Thing;
			IThingHolder thingHolder = (thing != null) ? thing.ParentHolder : null;
			List<Verb_MeleeAttack> list = null;
			if (req.HasThing && req.Thing.TryGetComp<CompEquippable>() != null)
			{
				list = req.Thing.TryGetComp<CompEquippable>().AllVerbs.OfType<Verb_MeleeAttack>().ToList<Verb_MeleeAttack>();
			}
			if (list == null && req.Def is ThingDef)
			{
				list = (req.Def as ThingDef).GetConcreteExample(req.StuffDef).TryGetComp<CompEquippable>().AllVerbs.OfType<Verb_MeleeAttack>().ToList<Verb_MeleeAttack>();
			}
			float num = list.AverageWeighted((Verb_MeleeAttack verb) => verb.verbProps.AdjustedMeleeSelectionWeight(verb, thingHolder as Pawn, thing), (Verb_MeleeAttack verb) => verb.verbProps.AdjustedMeleeDamageAmount(verb, thingHolder as Pawn, thing));
			float num2 = list.AverageWeighted((Verb_MeleeAttack verb) => verb.verbProps.AdjustedMeleeSelectionWeight(verb, thingHolder as Pawn, thing), (Verb_MeleeAttack verb) => verb.verbProps.AdjustedCooldown(verb, thingHolder as Pawn, thing));
			return num / num2;
		}

		public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
		{
			StringBuilder stringBuilder = new StringBuilder();
			ThingDef thingDef = req.Def as ThingDef;
			for (int i = 0; i < thingDef.tools.Count; i++)
			{
				Tool tool = thingDef.tools[i];
				for (int j = 0; j < tool.capacities.Count; j++)
				{
					ToolCapacityDef capacity = tool.capacities[j];
					IEnumerable<ManeuverDef> source = from maneuver in DefDatabase<ManeuverDef>.AllDefsListForReading
					where maneuver.requiredCapacity == capacity
					select maneuver;
					if (source.Count<ManeuverDef>() != 1)
					{
						Log.ErrorOnce(string.Format("{0} maneuvers when trying to get dps for weapon {1} tool {2} capacity {3}; average DPS explanation may be incorrect", new object[]
						{
							source.Count<ManeuverDef>(),
							thingDef.label,
							tool.Id,
							capacity.label
						}), 40417826);
					}
					ManeuverDef maneuverDef = source.FirstOrDefault<ManeuverDef>();
					if (maneuverDef != null)
					{
						stringBuilder.AppendLine(string.Format("  Tool: {0} ({1})", tool.Id, capacity.label));
						stringBuilder.AppendLine(string.Format("    {0} damage", tool.AdjustedMeleeDamageAmount(req.Thing, maneuverDef.verb.meleeDamageDef).ToString("F1")));
						stringBuilder.AppendLine(string.Format("    {0} seconds per attack", tool.AdjustedCooldown(req.Thing).ToString("F2")));
						stringBuilder.AppendLine();
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
