using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public class MentalStateDef : Def
	{
		public Type stateClass = typeof(MentalState);

		public Type workerClass = typeof(MentalStateWorker);

		public MentalStateCategory category;

		public bool prisonersCanDo = true;

		public bool unspawnedCanDo;

		public bool colonistsOnly;

		public bool blockNormalThoughts;

		public EffecterDef stateEffecter;

		public TaleDef tale;

		public bool allowBeatfire;

		public DrugCategory drugCategory = DrugCategory.Any;

		public float recoveryMtbDays = 1f;

		public int minTicksBeforeRecovery = 500;

		public int maxTicksBeforeRecovery = 99999999;

		public bool recoverFromSleep;

		public bool recoverFromDowned;

		public ThoughtDef moodRecoveryThought;

		[MustTranslate]
		public string beginLetter;

		[MustTranslate]
		public string beginLetterLabel;

		public LetterType beginLetterType = LetterType.BadUrgent;

		public Color nameColor = Color.green;

		[MustTranslate]
		public string recoveryMessage;

		[MustTranslate]
		public string baseInspectLine;

		private MentalStateWorker workerInt;

		public MentalStateWorker Worker
		{
			get
			{
				if (this.workerInt == null && this.workerClass != null)
				{
					this.workerInt = (MentalStateWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}

		public bool IsAggro
		{
			get
			{
				return this.category == MentalStateCategory.Aggro;
			}
		}

		public bool IsExtreme
		{
			get
			{
				List<MentalBreakDef> allDefsListForReading = DefDatabase<MentalBreakDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (allDefsListForReading[i].intensity == MentalBreakIntensity.Extreme && allDefsListForReading[i].mentalState == this)
					{
						return true;
					}
				}
				return false;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in base.ConfigErrors())
			{
				yield return e;
			}
			if (!this.beginLetter.NullOrEmpty() && this.beginLetterLabel.NullOrEmpty())
			{
				yield return "no beginLetter or beginLetterLabel";
			}
		}
	}
}
