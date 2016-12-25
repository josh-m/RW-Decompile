using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class Pawn_AgeTracker : IExposable
	{
		private const float BornAtLongitude = 0f;

		private Pawn pawn;

		private long ageBiologicalTicksInt = -1L;

		private long birthAbsTicksInt = -1L;

		private int cachedLifeStageIndex = -1;

		private int nextLifeStageChangeTick = -1;

		public long BirthAbsTicks
		{
			get
			{
				return this.birthAbsTicksInt;
			}
			set
			{
				this.birthAbsTicksInt = value;
			}
		}

		public int AgeBiologicalYears
		{
			get
			{
				return (int)(this.ageBiologicalTicksInt / 3600000L);
			}
		}

		public float AgeBiologicalYearsFloat
		{
			get
			{
				return (float)this.ageBiologicalTicksInt / 3600000f;
			}
		}

		public long AgeBiologicalTicks
		{
			get
			{
				return this.ageBiologicalTicksInt;
			}
			set
			{
				this.ageBiologicalTicksInt = value;
				this.cachedLifeStageIndex = -1;
			}
		}

		public long AgeChronologicalTicks
		{
			get
			{
				return (long)GenTicks.TicksAbs - this.birthAbsTicksInt;
			}
			set
			{
				this.BirthAbsTicks = (long)GenTicks.TicksAbs - value;
			}
		}

		public int AgeChronologicalYears
		{
			get
			{
				return (int)(this.AgeChronologicalTicks / 3600000L);
			}
		}

		public float AgeChronologicalYearsFloat
		{
			get
			{
				return (float)this.AgeChronologicalTicks / 3600000f;
			}
		}

		public int BirthYear
		{
			get
			{
				return GenDate.Year(this.birthAbsTicksInt, 0f);
			}
		}

		public int BirthDayOfSeasonZeroBased
		{
			get
			{
				return GenDate.DayOfSeason(this.birthAbsTicksInt, 0f);
			}
		}

		public int BirthDayOfYear
		{
			get
			{
				return GenDate.DayOfYear(this.birthAbsTicksInt, 0f);
			}
		}

		public Season BirthSeason
		{
			get
			{
				return GenDate.Season(this.birthAbsTicksInt, 0f);
			}
		}

		public string AgeNumberString
		{
			get
			{
				string text = this.AgeBiologicalYearsFloat.ToStringApproxAge();
				if (this.AgeChronologicalYears != this.AgeBiologicalYears)
				{
					string text2 = text;
					text = string.Concat(new object[]
					{
						text2,
						" (",
						this.AgeChronologicalYears,
						")"
					});
				}
				return text;
			}
		}

		public string AgeTooltipString
		{
			get
			{
				int num;
				int num2;
				int num3;
				float num4;
				this.ageBiologicalTicksInt.TicksToPeriod(out num, out num2, out num3, out num4);
				long numTicks = (long)GenTicks.TicksAbs - this.birthAbsTicksInt;
				int num5;
				int num6;
				int num7;
				numTicks.TicksToPeriod(out num5, out num6, out num7, out num4);
				string text = "FullDate".Translate(new object[]
				{
					Find.ActiveLanguageWorker.OrdinalNumber(this.BirthDayOfSeasonZeroBased + 1),
					this.BirthSeason.Label(),
					this.BirthYear
				});
				string text2 = string.Concat(new string[]
				{
					"Born".Translate(new object[]
					{
						text
					}),
					"\n",
					"AgeChronological".Translate(new object[]
					{
						num5,
						num6,
						num7
					}),
					"\n",
					"AgeBiological".Translate(new object[]
					{
						num,
						num2,
						num3
					})
				});
				if (Prefs.DevMode)
				{
					text2 += "\n\nDev mode info:";
					text2 = text2 + "\nageBiologicalTicksInt: " + this.ageBiologicalTicksInt;
					text2 = text2 + "\nbirthAbsTicksInt: " + this.birthAbsTicksInt;
					text2 = text2 + "\nnextLifeStageChangeTick: " + this.nextLifeStageChangeTick;
				}
				return text2;
			}
		}

		public int CurLifeStageIndex
		{
			get
			{
				if (this.cachedLifeStageIndex < 0)
				{
					this.RecalculateLifeStageIndex();
				}
				return this.cachedLifeStageIndex;
			}
		}

		public LifeStageDef CurLifeStage
		{
			get
			{
				return this.CurLifeStageRace.def;
			}
		}

		public LifeStageAge CurLifeStageRace
		{
			get
			{
				return this.pawn.RaceProps.lifeStageAges[this.CurLifeStageIndex];
			}
		}

		public PawnKindLifeStage CurKindLifeStage
		{
			get
			{
				if (this.pawn.RaceProps.Humanlike)
				{
					Log.ErrorOnce("Tried to get CurKindLifeStage from humanlike pawn " + this.pawn, 8888811);
					return null;
				}
				return this.pawn.kindDef.lifeStages[this.CurLifeStageIndex];
			}
		}

		public Pawn_AgeTracker(Pawn newPawn)
		{
			this.pawn = newPawn;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<long>(ref this.ageBiologicalTicksInt, "ageBiologicalTicks", 0L, false);
			Scribe_Values.LookValue<long>(ref this.birthAbsTicksInt, "birthAbsTicks", 0L, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.cachedLifeStageIndex = -1;
			}
		}

		public void AgeTick()
		{
			this.ageBiologicalTicksInt += 1L;
			if (Find.TickManager.TicksGame == this.nextLifeStageChangeTick)
			{
				this.RecalculateLifeStageIndex();
			}
			if (Find.TickManager.TicksGame % 60000 == 20000)
			{
				if (GenLocalDate.DayOfYear(this.pawn) == this.BirthDayOfYear)
				{
					this.BirthdayChronological();
				}
				if (this.ageBiologicalTicksInt % 3600000L < 60000L)
				{
					this.BirthdayBiological();
				}
			}
		}

		private void RecalculateLifeStageIndex()
		{
			int num = -1;
			List<LifeStageAge> lifeStageAges = this.pawn.RaceProps.lifeStageAges;
			for (int i = lifeStageAges.Count - 1; i >= 0; i--)
			{
				if (lifeStageAges[i].minAge <= this.AgeBiologicalYearsFloat + 1E-06f)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				num = 0;
			}
			bool flag = this.cachedLifeStageIndex != num;
			this.cachedLifeStageIndex = num;
			if (flag && !this.pawn.RaceProps.Humanlike)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					this.pawn.Drawer.renderer.graphics.ResolveAllGraphics();
				});
				this.CheckChangePawnKindName();
			}
			if (this.cachedLifeStageIndex < lifeStageAges.Count - 1)
			{
				float num2 = lifeStageAges[this.cachedLifeStageIndex + 1].minAge - this.AgeBiologicalYearsFloat;
				int num3 = (Current.ProgramState != ProgramState.Playing) ? 0 : Find.TickManager.TicksGame;
				this.nextLifeStageChangeTick = num3 + Mathf.CeilToInt(num2 * 3600000f);
			}
		}

		private void BirthdayBiological()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (HediffGiver_Birthday current in AgeInjuryUtility.RandomHediffsToGainOnBirthday(this.pawn, this.AgeBiologicalYears))
			{
				if (current.TryApply(this.pawn, null))
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append("    - " + current.hediff.LabelCap);
				}
			}
			if (this.pawn.RaceProps.Humanlike && PawnUtility.ShouldSendNotificationAbout(this.pawn) && stringBuilder.Length > 0)
			{
				string text = "BirthdayBiologicalAgeInjuries".Translate(new object[]
				{
					this.pawn,
					this.AgeBiologicalYears,
					stringBuilder
				}).AdjustedFor(this.pawn);
				Find.LetterStack.ReceiveLetter(new Letter("LetterLabelBirthday".Translate(), text, LetterType.BadNonUrgent, this.pawn), null);
			}
		}

		private void BirthdayChronological()
		{
		}

		public void DebugForceBirthdayBiological()
		{
			this.BirthdayBiological();
		}

		private void CheckChangePawnKindName()
		{
			NameSingle nameSingle = this.pawn.Name as NameSingle;
			if (nameSingle == null || !nameSingle.Numerical)
			{
				return;
			}
			string kindLabel = this.pawn.KindLabel;
			if (nameSingle.NameWithoutNumber == kindLabel)
			{
				return;
			}
			int number = nameSingle.Number;
			string text = this.pawn.KindLabel + " " + number;
			if (!NameUseChecker.NameSingleIsUsedOnAnyMap(text))
			{
				this.pawn.Name = new NameSingle(text, true);
				return;
			}
			this.pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(this.pawn, NameStyle.Numeric, null);
		}

		public void DebugMake1YearOlder()
		{
			this.ageBiologicalTicksInt += 3600000L;
			this.birthAbsTicksInt -= 3600000L;
			this.RecalculateLifeStageIndex();
		}
	}
}
