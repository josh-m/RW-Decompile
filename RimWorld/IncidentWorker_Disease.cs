using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Disease : IncidentWorker
	{
		private IEnumerable<Pawn> PotentialVictims(Map map)
		{
			return map.mapPawns.FreeColonistsAndPrisoners.Where(delegate(Pawn p)
			{
				if (p.holdingContainer != null && p.holdingContainer.owner is Building_CryptosleepCasket)
				{
					return false;
				}
				if (!this.def.diseasePartsToAffect.NullOrEmpty<BodyPartDef>())
				{
					for (int i = 0; i < this.def.diseasePartsToAffect.Count; i++)
					{
						if (IncidentWorker_Disease.CanAddHediffToAnyPartOfDef(p, this.def.diseaseIncident, this.def.diseasePartsToAffect[i]))
						{
							goto IL_86;
						}
					}
					return false;
				}
				IL_86:
				return p.health.immunity.DiseaseContractChanceFactor(this.def.diseaseIncident, null) > 0f;
			});
		}

		private static bool CanAddHediffToAnyPartOfDef(Pawn pawn, HediffDef hediffDef, BodyPartDef partDef)
		{
			List<BodyPartRecord> allParts = pawn.def.race.body.AllParts;
			for (int i = 0; i < allParts.Count; i++)
			{
				BodyPartRecord bodyPartRecord = allParts[i];
				if (bodyPartRecord.def == partDef && !pawn.health.hediffSet.PartIsMissing(bodyPartRecord) && !pawn.health.hediffSet.HasHediff(hediffDef, bodyPartRecord))
				{
					return true;
				}
			}
			return false;
		}

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			return this.PotentialVictims((Map)target).Any<Pawn>();
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			int num = map.mapPawns.FreeColonistsAndPrisoners.Count<Pawn>();
			IntRange intRange = new IntRange(Mathf.RoundToInt((float)num * this.def.diseaseVictimFractionRange.min), Mathf.RoundToInt((float)num * this.def.diseaseVictimFractionRange.max));
			int num2 = intRange.RandomInRange;
			num2 = Mathf.Clamp(num2, 1, this.def.diseaseMaxVictims);
			for (int i = 0; i < num2; i++)
			{
				if (!this.PotentialVictims(map).Any<Pawn>())
				{
					break;
				}
				Pawn pawn = this.PotentialVictims(map).RandomElementByWeight((Pawn x) => x.health.immunity.DiseaseContractChanceFactor(this.def.diseaseIncident, null));
				HediffGiveUtility.TryApply(pawn, this.def.diseaseIncident, this.def.diseasePartsToAffect, false, 1, null);
			}
			return true;
		}
	}
}
