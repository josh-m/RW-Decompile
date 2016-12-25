using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SkillRecord : IExposable
	{
		public const int IntervalTicks = 200;

		public const int MinLevel = 0;

		public const int MaxLevel = 20;

		public const int MaxFullRateXpPerDay = 4000;

		public const float SaturatedLearningFactor = 0.2f;

		public const float LearnFactorPassionNone = 0.333f;

		public const float LearnFactorPassionMinor = 1f;

		public const float LearnFactorPassionMajor = 1.5f;

		private Pawn pawn;

		public SkillDef def;

		public int levelInt;

		public Passion passion;

		public float xpSinceLastLevel;

		public float xpSinceMidnight;

		private BoolUnknown cachedTotallyDisabled = BoolUnknown.Unknown;

		public int Level
		{
			get
			{
				if (this.TotallyDisabled)
				{
					return 0;
				}
				return this.levelInt;
			}
			set
			{
				this.levelInt = value;
			}
		}

		public float XpRequiredForLevelUp
		{
			get
			{
				return this.XpRequiredToLevelUpFrom(this.levelInt);
			}
		}

		public float XpProgressPercent
		{
			get
			{
				return this.xpSinceLastLevel / this.XpRequiredForLevelUp;
			}
		}

		public float XpTotalEarned
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < this.levelInt; i++)
				{
					num += this.XpRequiredToLevelUpFrom(i);
				}
				return num;
			}
		}

		public bool TotallyDisabled
		{
			get
			{
				if (this.cachedTotallyDisabled == BoolUnknown.Unknown)
				{
					this.cachedTotallyDisabled = ((!this.CalculateTotallyDisabled()) ? BoolUnknown.False : BoolUnknown.True);
				}
				return this.cachedTotallyDisabled == BoolUnknown.True;
			}
		}

		public string LevelDescriptor
		{
			get
			{
				switch (this.levelInt)
				{
				case 0:
					return "Skill0".Translate();
				case 1:
					return "Skill1".Translate();
				case 2:
					return "Skill2".Translate();
				case 3:
					return "Skill3".Translate();
				case 4:
					return "Skill4".Translate();
				case 5:
					return "Skill5".Translate();
				case 6:
					return "Skill6".Translate();
				case 7:
					return "Skill7".Translate();
				case 8:
					return "Skill8".Translate();
				case 9:
					return "Skill9".Translate();
				case 10:
					return "Skill10".Translate();
				case 11:
					return "Skill11".Translate();
				case 12:
					return "Skill12".Translate();
				case 13:
					return "Skill13".Translate();
				case 14:
					return "Skill14".Translate();
				case 15:
					return "Skill15".Translate();
				case 16:
					return "Skill16".Translate();
				case 17:
					return "Skill17".Translate();
				case 18:
					return "Skill18".Translate();
				case 19:
					return "Skill19".Translate();
				case 20:
					return "Skill20".Translate();
				default:
					return "Unknown";
				}
			}
		}

		public bool LearningSaturatedToday
		{
			get
			{
				return this.xpSinceMidnight > 4000f;
			}
		}

		public SkillRecord()
		{
		}

		public SkillRecord(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public SkillRecord(Pawn pawn, SkillDef def)
		{
			this.pawn = pawn;
			this.def = def;
		}

		public void ExposeData()
		{
			Scribe_Defs.LookDef<SkillDef>(ref this.def, "def");
			Scribe_Values.LookValue<int>(ref this.levelInt, "level", 0, false);
			Scribe_Values.LookValue<float>(ref this.xpSinceLastLevel, "xpSinceLastLevel", 0f, false);
			Scribe_Values.LookValue<Passion>(ref this.passion, "passion", Passion.None, false);
			Scribe_Values.LookValue<float>(ref this.xpSinceMidnight, "xpSinceMidnight", 0f, false);
		}

		public void Interval()
		{
			if (Find.TickManager.TicksAbs % 60000 <= 200)
			{
				this.xpSinceMidnight = 0f;
			}
			switch (this.levelInt)
			{
			case 10:
				this.Learn(-0.1f, false);
				break;
			case 11:
				this.Learn(-0.2f, false);
				break;
			case 12:
				this.Learn(-0.4f, false);
				break;
			case 13:
				this.Learn(-0.65f, false);
				break;
			case 14:
				this.Learn(-1f, false);
				break;
			case 15:
				this.Learn(-1.5f, false);
				break;
			case 16:
				this.Learn(-2f, false);
				break;
			case 17:
				this.Learn(-3f, false);
				break;
			case 18:
				this.Learn(-4f, false);
				break;
			case 19:
				this.Learn(-6f, false);
				break;
			case 20:
				this.Learn(-8f, false);
				break;
			}
		}

		public float XpRequiredToLevelUpFrom(int startingLevel)
		{
			return (float)(1000 + startingLevel * 1000);
		}

		public void Learn(float xp, bool direct = false)
		{
			if (this.TotallyDisabled)
			{
				return;
			}
			if (xp < 0f && this.levelInt == 0)
			{
				return;
			}
			if (xp > 0f)
			{
				if (this.pawn.needs.joy != null)
				{
					float amount = 0f;
					switch (this.passion)
					{
					case Passion.None:
						amount = 0f * xp;
						break;
					case Passion.Minor:
						amount = 2E-05f * xp;
						break;
					case Passion.Major:
						amount = 4E-05f * xp;
						break;
					}
					this.pawn.needs.joy.GainJoy(amount, JoyKindDefOf.Work);
				}
				xp *= this.LearnRateFactor(direct);
			}
			this.xpSinceLastLevel += xp;
			if (!direct)
			{
				this.xpSinceMidnight += xp;
			}
			if (this.levelInt == 20 && this.xpSinceLastLevel > this.XpRequiredForLevelUp - 1f)
			{
				this.xpSinceLastLevel = this.XpRequiredForLevelUp - 1f;
			}
			while (this.xpSinceLastLevel >= this.XpRequiredForLevelUp)
			{
				this.xpSinceLastLevel -= this.XpRequiredForLevelUp;
				this.levelInt++;
				if (this.levelInt >= 20)
				{
					this.levelInt = 20;
					this.xpSinceLastLevel = Mathf.Clamp(this.xpSinceLastLevel, 0f, this.XpRequiredForLevelUp - 1f);
					break;
				}
			}
			while (this.xpSinceLastLevel < 0f)
			{
				this.levelInt--;
				this.xpSinceLastLevel += this.XpRequiredForLevelUp;
				if (this.levelInt <= 0)
				{
					this.levelInt = 0;
					this.xpSinceLastLevel = 0f;
					break;
				}
			}
		}

		public float LearnRateFactor(bool direct = false)
		{
			if (DebugSettings.fastLearning)
			{
				return 200f;
			}
			float num;
			switch (this.passion)
			{
			case Passion.None:
				num = 0.333f;
				break;
			case Passion.Minor:
				num = 1f;
				break;
			case Passion.Major:
				num = 1.5f;
				break;
			default:
				throw new NotImplementedException("Passion level " + this.passion);
			}
			if (!direct)
			{
				num *= this.pawn.GetStatValue(StatDefOf.GlobalLearningFactor, true);
				if (this.LearningSaturatedToday)
				{
					num *= 0.2f;
				}
			}
			return num;
		}

		public void Notify_SkillDisablesChanged()
		{
			this.cachedTotallyDisabled = BoolUnknown.Unknown;
		}

		private bool CalculateTotallyDisabled()
		{
			if (this.pawn.story.WorkTagIsDisabled(this.def.disablingWorkTags))
			{
				return true;
			}
			List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			bool result = false;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkTypeDef workTypeDef = allDefsListForReading[i];
				for (int j = 0; j < workTypeDef.relevantSkills.Count; j++)
				{
					if (workTypeDef.relevantSkills[j] == this.def)
					{
						if (!this.pawn.story.WorkTypeIsDisabled(workTypeDef))
						{
							return false;
						}
						result = true;
					}
				}
			}
			return result;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				this.def.defName,
				": ",
				this.levelInt,
				" (",
				this.xpSinceLastLevel,
				"xp)"
			});
		}
	}
}
