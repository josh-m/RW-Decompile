using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class ImmunityHandler : IExposable
	{
		public Pawn pawn;

		private List<ImmunityRecord> immunityList = new List<ImmunityRecord>();

		public ImmunityHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<ImmunityRecord>(ref this.immunityList, "imList", LookMode.Deep, new object[0]);
		}

		public float DiseaseContractChanceFactor(HediffDef diseaseDef, BodyPartRecord part = null)
		{
			if (!this.pawn.RaceProps.IsFlesh)
			{
				return 0f;
			}
			List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def == diseaseDef && hediffs[i].Part == part)
				{
					return 0f;
				}
			}
			for (int j = 0; j < this.immunityList.Count; j++)
			{
				if (this.immunityList[j].hediffDef == diseaseDef)
				{
					return Mathf.Lerp(1f, 0f, this.immunityList[j].immunity / 0.6f);
				}
			}
			return 1f;
		}

		public float GetImmunity(HediffDef def)
		{
			for (int i = 0; i < this.immunityList.Count; i++)
			{
				ImmunityRecord immunityRecord = this.immunityList[i];
				if (immunityRecord.hediffDef == def)
				{
					return immunityRecord.immunity;
				}
			}
			return 0f;
		}

		internal void ImmunityHandlerTick()
		{
			List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].TryGetComp<HediffComp_Immunizable>() != null)
				{
					if (!this.ImmunityRecordExists(hediffs[i].def))
					{
						if (hediffs[i].def.PossibleToDevelopImmunity())
						{
							ImmunityRecord immunityRecord = new ImmunityRecord();
							immunityRecord.hediffDef = hediffs[i].def;
							this.immunityList.Add(immunityRecord);
						}
					}
				}
			}
			for (int j = 0; j < this.immunityList.Count; j++)
			{
				ImmunityRecord immunityRecord2 = this.immunityList[j];
				bool flag = false;
				Hediff hediff = null;
				for (int k = 0; k < hediffs.Count; k++)
				{
					if (hediffs[k].def == immunityRecord2.hediffDef)
					{
						hediff = hediffs[k];
					}
					HediffStage curStage = hediffs[k].CurStage;
					if (curStage != null && curStage.makeImmuneTo != null)
					{
						for (int l = 0; l < curStage.makeImmuneTo.Count; l++)
						{
							if (curStage.makeImmuneTo[l] == immunityRecord2.hediffDef)
							{
								flag = true;
								break;
							}
						}
					}
				}
				immunityRecord2.ImmunityTick(this.pawn, hediff != null, hediff);
				if (flag)
				{
					immunityRecord2.immunity = Mathf.Clamp(0.61f, immunityRecord2.immunity, 1f);
				}
			}
			this.immunityList.RemoveAll((ImmunityRecord x) => x.immunity <= 0f);
		}

		public ImmunityRecord GetImmunityRecord(HediffDef def)
		{
			for (int i = 0; i < this.immunityList.Count; i++)
			{
				if (this.immunityList[i].hediffDef == def)
				{
					return this.immunityList[i];
				}
			}
			return null;
		}

		public bool ImmunityRecordExists(HediffDef def)
		{
			return this.GetImmunityRecord(def) != null;
		}
	}
}
