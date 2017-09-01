using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class ImmunityHandler : IExposable
	{
		public Pawn pawn;

		private List<ImmunityRecord> immunityList = new List<ImmunityRecord>();

		private static List<HediffDef> tmpNeededImmunitiesNow = new List<HediffDef>();

		public ImmunityHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<ImmunityRecord>(ref this.immunityList, "imList", LookMode.Deep, new object[0]);
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
			List<HediffDef> list = this.NeededImmunitiesNow();
			for (int i = 0; i < list.Count; i++)
			{
				this.TryAddImmunityRecord(list[i]);
			}
			for (int j = 0; j < this.immunityList.Count; j++)
			{
				ImmunityRecord immunityRecord = this.immunityList[j];
				Hediff firstHediffOfDef = this.pawn.health.hediffSet.GetFirstHediffOfDef(immunityRecord.hediffDef, false);
				immunityRecord.ImmunityTick(this.pawn, firstHediffOfDef != null, firstHediffOfDef);
				if (firstHediffOfDef == null && this.AnyHediffMakesFullyImmuneTo(immunityRecord.hediffDef))
				{
					immunityRecord.immunity = Mathf.Clamp(0.650000036f, immunityRecord.immunity, 1f);
				}
			}
			for (int k = this.immunityList.Count - 1; k >= 0; k--)
			{
				if (this.immunityList[k].immunity <= 0f && !list.Contains(this.immunityList[k].hediffDef))
				{
					this.immunityList.RemoveAt(k);
				}
			}
		}

		private List<HediffDef> NeededImmunitiesNow()
		{
			ImmunityHandler.tmpNeededImmunitiesNow.Clear();
			List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def.PossibleToDevelopImmunityNaturally())
				{
					ImmunityHandler.tmpNeededImmunitiesNow.Add(hediffs[i].def);
				}
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null && curStage.makeImmuneTo != null)
				{
					for (int j = 0; j < curStage.makeImmuneTo.Count; j++)
					{
						ImmunityHandler.tmpNeededImmunitiesNow.Add(curStage.makeImmuneTo[j]);
					}
				}
			}
			return ImmunityHandler.tmpNeededImmunitiesNow;
		}

		private bool AnyHediffMakesFullyImmuneTo(HediffDef def)
		{
			List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null && curStage.makeImmuneTo != null)
				{
					for (int j = 0; j < curStage.makeImmuneTo.Count; j++)
					{
						if (curStage.makeImmuneTo[j] == def)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private void TryAddImmunityRecord(HediffDef def)
		{
			if (def.CompProps<HediffCompProperties_Immunizable>() == null)
			{
				return;
			}
			if (this.ImmunityRecordExists(def))
			{
				return;
			}
			ImmunityRecord immunityRecord = new ImmunityRecord();
			immunityRecord.hediffDef = def;
			this.immunityList.Add(immunityRecord);
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
