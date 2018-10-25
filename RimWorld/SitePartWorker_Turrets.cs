using RimWorld.Planet;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SitePartWorker_Turrets : SitePartWorker
	{
		private const float TurretPoints = 250f;

		private const int MinTurrets = 2;

		private const int MaxTurrets = 11;

		public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
			Thing arg_6C_0;
			if ((arg_6C_0 = map.listerThings.AllThings.FirstOrDefault((Thing x) => x is Building_TurretGun && x.HostileTo(Faction.OfPlayer))) == null)
			{
				arg_6C_0 = map.listerThings.AllThings.FirstOrDefault((Thing x) => x is Building_TurretGun);
			}
			lookTargets = arg_6C_0;
			return arrivedLetterPart;
		}

		public override SiteCoreOrPartParams GenerateDefaultParams(Site site, float myThreatPoints)
		{
			SiteCoreOrPartParams siteCoreOrPartParams = base.GenerateDefaultParams(site, myThreatPoints);
			siteCoreOrPartParams.mortarsCount = Rand.RangeInclusive(0, 1);
			siteCoreOrPartParams.turretsCount = Mathf.Clamp(Mathf.RoundToInt(siteCoreOrPartParams.threatPoints / 250f), 2, 11);
			return siteCoreOrPartParams;
		}

		public override string GetPostProcessedDescriptionDialogue(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			return string.Format(base.GetPostProcessedDescriptionDialogue(site, siteCoreOrPart), this.GetThreatsInfo(siteCoreOrPart.parms));
		}

		public override string GetPostProcessedThreatLabel(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			return base.GetPostProcessedThreatLabel(site, siteCoreOrPart) + " (" + this.GetThreatsInfo(siteCoreOrPart.parms) + ")";
		}

		private string GetThreatsInfo(SiteCoreOrPartParams parms)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = parms.mortarsCount + 1;
			if (parms.turretsCount != 0)
			{
				stringBuilder.Append(parms.turretsCount + " ");
				if (parms.turretsCount == 1)
				{
					stringBuilder.Append("Turret".Translate());
				}
				else
				{
					stringBuilder.Append("Turrets".Translate());
				}
			}
			if (parms.mortarsCount != 0)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(parms.mortarsCount + " ");
				if (parms.mortarsCount == 1)
				{
					stringBuilder.Append("Mortar".Translate());
				}
				else
				{
					stringBuilder.Append("Mortars".Translate());
				}
			}
			if (num != 0)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(", " + "AndLower".Translate() + " ");
				}
				stringBuilder.Append(num + " ");
				if (num == 1)
				{
					stringBuilder.Append("Enemy".Translate());
				}
				else
				{
					stringBuilder.Append("Enemies".Translate());
				}
			}
			return stringBuilder.ToString();
		}
	}
}
