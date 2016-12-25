using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_ShortCircuit : IncidentWorker
	{
		private IEnumerable<Building_Battery> FullBatteries()
		{
			return from Building_Battery bat in Find.ListerBuildings.AllBuildingsColonistOfDef(ThingDefOf.Battery)
			where bat.GetComp<CompPowerBattery>().StoredEnergy > 50f
			select bat;
		}

		protected override bool CanFireNowSub()
		{
			return this.FullBatteries().Any<Building_Battery>();
		}

		public override bool TryExecute(IncidentParms parms)
		{
			List<Building_Battery> source = this.FullBatteries().ToList<Building_Battery>();
			if (source.Count<Building_Battery>() == 0)
			{
				return false;
			}
			PowerNet powerNet = source.RandomElement<Building_Battery>().PowerComp.PowerNet;
			List<CompPower> list = (from trans in powerNet.transmitters
			where trans.parent.def == ThingDefOf.PowerConduit
			select trans).ToList<CompPower>();
			if (list.Count == 0)
			{
				return false;
			}
			float num = 0f;
			foreach (CompPowerBattery current in powerNet.batteryComps)
			{
				num += current.StoredEnergy;
				current.DrawPower(current.StoredEnergy);
			}
			float num2 = Mathf.Sqrt(num) * 0.05f;
			if (num2 > 14.9f)
			{
				num2 = 14.9f;
			}
			Thing parent = list.RandomElement<CompPower>().parent;
			GenExplosion.DoExplosion(parent.Position, num2, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			if (num2 > 3.5f)
			{
				GenExplosion.DoExplosion(parent.Position, num2 * 0.3f, DamageDefOf.Bomb, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
			}
			if (!parent.Destroyed)
			{
				parent.TakeDamage(new DamageInfo(DamageDefOf.Bomb, 200, null, null, null));
			}
			string text = "something";
			if (parent.def == ThingDefOf.PowerConduit)
			{
				text = "AnElectricalConduit".Translate();
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ShortCircuit".Translate(new object[]
			{
				text,
				num.ToString("F0")
			}));
			if (num2 > 5f)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("ShortCircuitWasLarge".Translate());
			}
			if (num2 > 8f)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("ShortCircuitWasHuge".Translate());
			}
			Find.LetterStack.ReceiveLetter("LetterLabelShortCircuit".Translate(), stringBuilder.ToString(), LetterType.BadNonUrgent, parent.Position, null);
			return true;
		}
	}
}
