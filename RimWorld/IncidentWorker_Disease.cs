using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public abstract class IncidentWorker_Disease : IncidentWorker
	{
		protected abstract IEnumerable<Pawn> PotentialVictimCandidates(IIncidentTarget target);

		protected IEnumerable<Pawn> PotentialVictims(IIncidentTarget target)
		{
			return this.PotentialVictimCandidates(target).Where(delegate(Pawn p)
			{
				if (p.ParentHolder is Building_CryptosleepCasket)
				{
					return false;
				}
				if (!this.def.diseasePartsToAffect.NullOrEmpty<BodyPartDef>())
				{
					bool flag = false;
					for (int i = 0; i < this.def.diseasePartsToAffect.Count; i++)
					{
						if (IncidentWorker_Disease.CanAddHediffToAnyPartOfDef(p, this.def.diseaseIncident, this.def.diseasePartsToAffect[i]))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
				return p.RaceProps.IsFlesh;
			});
		}

		protected abstract IEnumerable<Pawn> ActualVictims(IncidentParms parms);

		private static bool CanAddHediffToAnyPartOfDef(Pawn pawn, HediffDef hediffDef, BodyPartDef partDef)
		{
			List<BodyPartRecord> allParts = pawn.def.race.body.AllParts;
			for (int i = 0; i < allParts.Count; i++)
			{
				BodyPartRecord bodyPartRecord = allParts[i];
				if (bodyPartRecord.def == partDef && !pawn.health.hediffSet.PartIsMissing(bodyPartRecord) && !pawn.health.hediffSet.HasHediff(hediffDef, bodyPartRecord, false))
				{
					return true;
				}
			}
			return false;
		}

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			return this.PotentialVictims(parms.target).Any<Pawn>();
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			string text;
			List<Pawn> list = this.ApplyToPawns(this.ActualVictims(parms).ToList<Pawn>(), out text);
			if (!list.Any<Pawn>() && text.NullOrEmpty())
			{
				return false;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("  - " + list[i].LabelCap);
			}
			string text2;
			if (list.Any<Pawn>())
			{
				text2 = string.Format(this.def.letterText, new object[]
				{
					list.Count.ToString(),
					Faction.OfPlayer.def.pawnsPlural,
					this.def.diseaseIncident.label,
					stringBuilder.ToString()
				});
			}
			else
			{
				text2 = string.Empty;
			}
			if (!text.NullOrEmpty())
			{
				if (!text2.NullOrEmpty())
				{
					text2 += "\n\n";
				}
				text2 += text;
			}
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, text2, this.def.letterDef, list, null, null);
			return true;
		}

		public List<Pawn> ApplyToPawns(IEnumerable<Pawn> pawns, out string blockedInfo)
		{
			List<Pawn> list = new List<Pawn>();
			Dictionary<HediffDef, List<Pawn>> dictionary = new Dictionary<HediffDef, List<Pawn>>();
			foreach (Pawn current in pawns)
			{
				HediffDef hediffDef = null;
				if (Rand.Chance(current.health.immunity.DiseaseContractChanceFactor(this.def.diseaseIncident, out hediffDef, null)))
				{
					HediffGiverUtility.TryApply(current, this.def.diseaseIncident, this.def.diseasePartsToAffect, false, 1, null);
					TaleRecorder.RecordTale(TaleDefOf.IllnessRevealed, new object[]
					{
						current,
						this.def.diseaseIncident
					});
					list.Add(current);
				}
				else if (hediffDef != null)
				{
					if (!dictionary.ContainsKey(hediffDef))
					{
						dictionary[hediffDef] = new List<Pawn>();
					}
					dictionary[hediffDef].Add(current);
				}
			}
			blockedInfo = string.Empty;
			foreach (KeyValuePair<HediffDef, List<Pawn>> current2 in dictionary)
			{
				if (current2.Key != this.def.diseaseIncident)
				{
					if (blockedInfo.Length != 0)
					{
						blockedInfo += "\n\n";
					}
					blockedInfo += "LetterDisease_Blocked".Translate(current2.Key.LabelCap, this.def.diseaseIncident.label, (from victim in current2.Value
					select victim.LabelShort).ToCommaList(true));
				}
			}
			return list;
		}
	}
}
