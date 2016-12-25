using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_DrugPolicyTracker : IExposable
	{
		private const float DangerousDrugOverdoseSeverity = 0.5f;

		public Pawn pawn;

		private DrugPolicy curPolicy;

		private List<DrugTakeRecord> drugTakeRecords = new List<DrugTakeRecord>();

		public DrugPolicy CurrentPolicy
		{
			get
			{
				if (this.curPolicy == null)
				{
					this.curPolicy = Current.Game.drugPolicyDatabase.DefaultDrugPolicy();
				}
				return this.curPolicy;
			}
			set
			{
				if (this.curPolicy == value)
				{
					return;
				}
				this.curPolicy = value;
			}
		}

		private float DayPercentPassedNotSleeping
		{
			get
			{
				if (this.pawn.timetable == null)
				{
					return GenLocalDate.DayPercent(this.pawn);
				}
				float num = 0f;
				int b = GenLocalDate.HourOfDay(this.pawn);
				float num2 = 0.0416666679f;
				for (int i = 0; i < Mathf.Min(this.pawn.timetable.times.Count, b); i++)
				{
					if (this.pawn.timetable.times[i] != TimeAssignmentDefOf.Sleep)
					{
						num += num2;
					}
				}
				TimeAssignmentDef currentAssignment = this.pawn.timetable.CurrentAssignment;
				if (currentAssignment != TimeAssignmentDefOf.Sleep)
				{
					float num3 = (float)(Find.TickManager.TicksAbs % 2500) / 2500f;
					num += num3 * num2;
				}
				return num;
			}
		}

		public Pawn_DrugPolicyTracker()
		{
		}

		public Pawn_DrugPolicyTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_References.LookReference<DrugPolicy>(ref this.curPolicy, "curAssignedDrugs", false);
			Scribe_Collections.LookList<DrugTakeRecord>(ref this.drugTakeRecords, "drugTakeRecords", LookMode.Deep, new object[0]);
		}

		public bool HasEverTaken(ThingDef drug)
		{
			if (!drug.IsDrug)
			{
				Log.Warning(drug + " is not a drug.");
				return false;
			}
			return this.drugTakeRecords.Any((DrugTakeRecord x) => x.drug == drug);
		}

		public bool AllowedToTakeScheduledEver(ThingDef thingDef)
		{
			if (!thingDef.IsIngestible)
			{
				Log.Error(thingDef + " is not ingestible.");
				return false;
			}
			if (!thingDef.IsDrug)
			{
				Log.Error("AllowedToTakeScheduledEver on non-drug " + thingDef);
				return false;
			}
			DrugPolicyEntry drugPolicyEntry = this.CurrentPolicy[thingDef];
			return drugPolicyEntry.allowScheduled && (!thingDef.IsPleasureDrug || !this.pawn.IsTeetotaler());
		}

		public bool AllowedToTakeScheduledNow(ThingDef thingDef)
		{
			if (!thingDef.IsIngestible)
			{
				Log.Error(thingDef + " is not ingestible.");
				return false;
			}
			if (!thingDef.IsDrug)
			{
				Log.Error("AllowedToTakeScheduledEver on non-drug " + thingDef);
				return false;
			}
			if (!this.AllowedToTakeScheduledEver(thingDef))
			{
				return false;
			}
			DrugPolicyEntry drugPolicyEntry = this.CurrentPolicy[thingDef];
			if (drugPolicyEntry.onlyIfMoodBelow < 1f && this.pawn.needs.mood != null && this.pawn.needs.mood.CurLevelPercentage >= drugPolicyEntry.onlyIfMoodBelow)
			{
				return false;
			}
			if (drugPolicyEntry.onlyIfJoyBelow < 1f && this.pawn.needs.joy != null && this.pawn.needs.joy.CurLevelPercentage >= drugPolicyEntry.onlyIfJoyBelow)
			{
				return false;
			}
			DrugTakeRecord drugTakeRecord = this.drugTakeRecords.Find((DrugTakeRecord x) => x.drug == thingDef);
			if (drugTakeRecord != null)
			{
				if (drugPolicyEntry.daysFrequency < 1f)
				{
					int num = Mathf.RoundToInt(1f / drugPolicyEntry.daysFrequency);
					if (drugTakeRecord.TimesTakenThisDay >= num)
					{
						return false;
					}
				}
				else
				{
					int num2 = Mathf.Abs(GenDate.DaysPassed - drugTakeRecord.LastTakenDays);
					int num3 = Mathf.RoundToInt(drugPolicyEntry.daysFrequency);
					if (num2 < num3)
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool ShouldTryToTakeScheduledNow(ThingDef ingestible)
		{
			if (!ingestible.IsDrug)
			{
				return false;
			}
			if (!this.AllowedToTakeScheduledNow(ingestible))
			{
				return false;
			}
			Hediff firstHediffOfDef = this.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DrugOverdose);
			if (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.5f && this.CanCauseOverdose(ingestible))
			{
				int num = this.LastTicksWhenTakenDrugWhichCanCauseOverdose();
				if (Find.TickManager.TicksGame - num < 1250)
				{
					return false;
				}
			}
			DrugTakeRecord drugTakeRecord = this.drugTakeRecords.Find((DrugTakeRecord x) => x.drug == ingestible);
			if (drugTakeRecord == null)
			{
				return true;
			}
			DrugPolicyEntry drugPolicyEntry = this.CurrentPolicy[ingestible];
			if (drugPolicyEntry.daysFrequency < 1f)
			{
				int num2 = Mathf.RoundToInt(1f / drugPolicyEntry.daysFrequency);
				float num3 = 1f / (float)(num2 + 1);
				int num4 = 0;
				float dayPercentPassedNotSleeping = this.DayPercentPassedNotSleeping;
				for (int i = 0; i < num2; i++)
				{
					if (dayPercentPassedNotSleeping > (float)(i + 1) * num3 - num3 * 0.5f)
					{
						num4++;
					}
				}
				return drugTakeRecord.TimesTakenThisDay < num4 && (drugTakeRecord.TimesTakenThisDay == 0 || (float)(Find.TickManager.TicksGame - drugTakeRecord.lastTakenTicks) / 60000f >= 0.6f * num3);
			}
			float dayPercentPassedNotSleeping2 = this.DayPercentPassedNotSleeping;
			Rand.PushSeed();
			Rand.Seed = Gen.HashCombineInt(GenDate.DaysPassed, this.pawn.thingIDNumber);
			bool result = dayPercentPassedNotSleeping2 >= Rand.Range(0.1f, 0.35f);
			Rand.PopSeed();
			return result;
		}

		public void Notify_DrugIngested(Thing drug)
		{
			DrugTakeRecord drugTakeRecord = this.drugTakeRecords.Find((DrugTakeRecord x) => x.drug == drug.def);
			if (drugTakeRecord == null)
			{
				drugTakeRecord = new DrugTakeRecord();
				drugTakeRecord.drug = drug.def;
				this.drugTakeRecords.Add(drugTakeRecord);
			}
			drugTakeRecord.lastTakenTicks = Find.TickManager.TicksGame;
			drugTakeRecord.TimesTakenThisDay++;
		}

		private int LastTicksWhenTakenDrugWhichCanCauseOverdose()
		{
			int num = -999999;
			for (int i = 0; i < this.drugTakeRecords.Count; i++)
			{
				if (this.CanCauseOverdose(this.drugTakeRecords[i].drug))
				{
					num = Mathf.Max(num, this.drugTakeRecords[i].lastTakenTicks);
				}
			}
			return num;
		}

		private bool CanCauseOverdose(ThingDef drug)
		{
			CompProperties_Drug compProperties = drug.GetCompProperties<CompProperties_Drug>();
			return compProperties != null && compProperties.CanCauseOverdose;
		}
	}
}
