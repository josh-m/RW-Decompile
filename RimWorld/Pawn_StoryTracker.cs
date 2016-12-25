using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_StoryTracker : IExposable
	{
		private const float PassionChancePerLevel = 0.11f;

		private const float PassionMajorChance = 0.2f;

		private Pawn pawn;

		public Backstory childhood;

		public Backstory adulthood;

		public float skinWhiteness;

		public Color hairColor = Color.white;

		public CrownType crownType;

		private string headGraphicPath;

		public HairDef hairDef;

		public TraitSet traits;

		private List<WorkTypeDef> cachedDisabledWorkTypes;

		private static readonly SimpleCurve AgeSkillMaxFactorCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(10f, 0.7f),
			new CurvePoint(35f, 1f),
			new CurvePoint(60f, 1.6f)
		};

		private static readonly SimpleCurve LevelFinalAdjustmentCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(10f, 10f),
			new CurvePoint(20f, 16f),
			new CurvePoint(30f, 22f)
		};

		private static readonly SimpleCurve LevelRandomCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(3f, 200f),
			new CurvePoint(7f, 25f),
			new CurvePoint(10f, 25f),
			new CurvePoint(15f, 5f),
			new CurvePoint(20f, 0f)
		};

		public string HeadGraphicPath
		{
			get
			{
				if (this.headGraphicPath == null)
				{
					this.headGraphicPath = GraphicDatabaseHeadRecords.GetHeadRandom(this.pawn.gender, this.pawn.story.SkinColor, this.pawn.story.crownType).GraphicPath;
				}
				return this.headGraphicPath;
			}
		}

		public BodyType BodyType
		{
			get
			{
				return this.adulthood.BodyTypeFor(this.pawn.gender);
			}
		}

		public IEnumerable<Backstory> AllBackstories
		{
			get
			{
				yield return this.childhood;
				yield return this.adulthood;
			}
		}

		public List<WorkTypeDef> DisabledWorkTypes
		{
			get
			{
				if (this.cachedDisabledWorkTypes == null)
				{
					this.cachedDisabledWorkTypes = new List<WorkTypeDef>();
					foreach (Backstory current in this.AllBackstories)
					{
						if (current == null)
						{
							Log.Error("DisabledWorkTypes on pawn with missing backstory: " + this.pawn);
							this.cachedDisabledWorkTypes = null;
							return new List<WorkTypeDef>();
						}
						foreach (WorkTypeDef current2 in current.DisabledWorkTypes)
						{
							if (!this.cachedDisabledWorkTypes.Contains(current2))
							{
								this.cachedDisabledWorkTypes.Add(current2);
							}
						}
					}
					for (int i = 0; i < this.traits.allTraits.Count; i++)
					{
						foreach (WorkTypeDef current3 in this.traits.allTraits[i].DisabledWorkTypes)
						{
							if (!this.cachedDisabledWorkTypes.Contains(current3))
							{
								this.cachedDisabledWorkTypes.Add(current3);
							}
						}
					}
				}
				return this.cachedDisabledWorkTypes;
			}
		}

		public WorkTags CombinedDisabledWorkTags
		{
			get
			{
				WorkTags workTags = WorkTags.None;
				foreach (Backstory current in this.AllBackstories)
				{
					if (current != null)
					{
						workTags |= current.workDisables;
					}
				}
				foreach (Trait current2 in this.traits.allTraits)
				{
					workTags |= current2.def.disabledWorkTags;
				}
				return workTags;
			}
		}

		public IEnumerable<WorkTags> DisabledWorkTags
		{
			get
			{
				WorkTags disabledTags = this.CombinedDisabledWorkTags;
				foreach (WorkTags workTag in disabledTags.GetAllSelectedItems<WorkTags>())
				{
					if (workTag != WorkTags.None)
					{
						yield return workTag;
					}
				}
			}
		}

		public Color SkinColor
		{
			get
			{
				return PawnSkinColors.GetSkinColor(this.skinWhiteness);
			}
		}

		public int NumStorySlots
		{
			get
			{
				return Enum.GetValues(typeof(BackstorySlot)).Length;
			}
		}

		public Pawn_StoryTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.traits = new TraitSet(pawn);
		}

		public void ExposeData()
		{
			string saveKey = (this.childhood == null) ? null : this.childhood.uniqueSaveKey;
			Scribe_Values.LookValue<string>(ref saveKey, "childhood", null, false);
			this.childhood = BackstoryDatabase.GetWithKey(saveKey);
			string saveKey2 = (this.adulthood == null) ? null : this.adulthood.uniqueSaveKey;
			Scribe_Values.LookValue<string>(ref saveKey2, "adulthood", null, false);
			this.adulthood = BackstoryDatabase.GetWithKey(saveKey2);
			Scribe_Values.LookValue<float>(ref this.skinWhiteness, "skinWhiteness", 0f, false);
			Scribe_Values.LookValue<Color>(ref this.hairColor, "hairColor", default(Color), false);
			Scribe_Values.LookValue<CrownType>(ref this.crownType, "crownType", CrownType.Undefined, false);
			Scribe_Values.LookValue<string>(ref this.headGraphicPath, "headGraphicPath", null, false);
			Scribe_Defs.LookDef<HairDef>(ref this.hairDef, "hairDef");
			Scribe_Deep.LookDeep<TraitSet>(ref this.traits, "traits", new object[]
			{
				this.pawn
			});
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.hairDef == null)
			{
				this.hairDef = DefDatabase<HairDef>.AllDefs.RandomElement<HairDef>();
			}
		}

		public bool WorkTypeIsDisabled(WorkTypeDef w)
		{
			return this.DisabledWorkTypes.Contains(w);
		}

		public bool WorkTagIsDisabled(WorkTags w)
		{
			return (this.CombinedDisabledWorkTags & w) != WorkTags.None;
		}

		public Backstory GetBackstory(BackstorySlot slot)
		{
			if (slot == BackstorySlot.Childhood)
			{
				return this.childhood;
			}
			return this.adulthood;
		}

		public void GenerateSkillsFromBackstory()
		{
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				SkillDef skillDef = allDefsListForReading[i];
				int num = this.FinalLevelOfSkill(skillDef);
				SkillRecord skill = this.pawn.skills.GetSkill(skillDef);
				skill.level = num;
				if (!skill.TotallyDisabled)
				{
					float num2 = (float)num * 0.11f;
					float value = Rand.Value;
					if (value < num2)
					{
						if (value < num2 * 0.2f)
						{
							skill.passion = Passion.Major;
						}
						else
						{
							skill.passion = Passion.Minor;
						}
					}
					skill.xpSinceLastLevel = Rand.Range(skill.XpRequiredForLevelUp * 0.1f, skill.XpRequiredForLevelUp * 0.9f);
				}
			}
		}

		private int FinalLevelOfSkill(SkillDef sk)
		{
			float num;
			if (sk.definedInBackstories)
			{
				num = (float)Rand.RangeInclusive(1, 4);
				foreach (Backstory current in from bs in this.AllBackstories
				where bs != null
				select bs)
				{
					foreach (KeyValuePair<SkillDef, int> current2 in current.skillGainsResolved)
					{
						if (current2.Key == sk)
						{
							num += (float)current2.Value * Rand.Range(1f, 1.4f);
						}
					}
				}
			}
			else
			{
				num = Rand.ByCurve(Pawn_StoryTracker.LevelRandomCurve, 100);
			}
			float num2 = Rand.Range(1f, Pawn_StoryTracker.AgeSkillMaxFactorCurve.Evaluate((float)this.pawn.ageTracker.AgeBiologicalYears));
			num *= num2;
			for (int i = 0; i < this.traits.allTraits.Count; i++)
			{
				int num3 = 0;
				if (this.traits.allTraits[i].CurrentData.skillGains.TryGetValue(sk, out num3))
				{
					num += (float)num3;
				}
			}
			num = Pawn_StoryTracker.LevelFinalAdjustmentCurve.Evaluate(num);
			int value = Mathf.RoundToInt(num);
			return Mathf.Clamp(value, 0, 20);
		}

		public bool HasOneOfBackstory(IEnumerable<Backstory> backstoryList)
		{
			return backstoryList.Contains(this.childhood) || backstoryList.Contains(this.adulthood);
		}

		public bool OneOfWorkTypesIsDisabled(List<WorkTypeDef> wts)
		{
			for (int i = 0; i < wts.Count; i++)
			{
				if (this.WorkTypeIsDisabled(wts[i]))
				{
					return true;
				}
			}
			return false;
		}

		internal void Notify_TraitChanged()
		{
			this.cachedDisabledWorkTypes = null;
		}
	}
}
