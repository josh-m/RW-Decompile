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

		public ThoughtDef recoveryThought;

		public string beginLetter;

		public string beginLetterLabel;

		public LetterType beginLetterType = LetterType.BadUrgent;

		public Color nameColor = Color.green;

		public string recoveryMessage;

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
