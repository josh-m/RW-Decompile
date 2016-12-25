using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_StoryTracker : IExposable
	{
		private Pawn pawn;

		public Backstory childhood;

		public Backstory adulthood;

		public float melanin;

		public Color hairColor = Color.white;

		public CrownType crownType;

		public BodyType bodyType;

		private string headGraphicPath;

		public HairDef hairDef;

		public TraitSet traits;

		private List<WorkTypeDef> cachedDisabledWorkTypes;

		public string Title
		{
			get
			{
				if (this.adulthood != null)
				{
					return this.adulthood.Title;
				}
				return this.childhood.Title;
			}
		}

		public string TitleShort
		{
			get
			{
				if (this.adulthood != null)
				{
					return this.adulthood.TitleShort;
				}
				return this.childhood.TitleShort;
			}
		}

		public Color SkinColor
		{
			get
			{
				return PawnSkinColors.GetSkinColor(this.melanin);
			}
		}

		public IEnumerable<Backstory> AllBackstories
		{
			get
			{
				if (this.childhood != null)
				{
					yield return this.childhood;
				}
				if (this.adulthood != null)
				{
					yield return this.adulthood;
				}
			}
		}

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

		public List<WorkTypeDef> DisabledWorkTypes
		{
			get
			{
				if (this.cachedDisabledWorkTypes == null)
				{
					this.cachedDisabledWorkTypes = new List<WorkTypeDef>();
					foreach (Backstory current in this.AllBackstories)
					{
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
						foreach (WorkTypeDef current3 in this.traits.allTraits[i].GetDisabledWorkTypes())
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
					workTags |= current.workDisables;
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

		public Pawn_StoryTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.traits = new TraitSet(pawn);
		}

		public void ExposeData()
		{
			string text = (this.childhood == null) ? null : this.childhood.identifier;
			Scribe_Values.LookValue<string>(ref text, "childhood", null, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars && !text.NullOrEmpty() && !BackstoryDatabase.TryGetWithIdentifier(text, out this.childhood))
			{
				Log.Error("Couldn't load child backstory with identifier " + text + ". Giving random.");
				this.childhood = BackstoryDatabase.RandomBackstory(BackstorySlot.Childhood);
			}
			string text2 = (this.adulthood == null) ? null : this.adulthood.identifier;
			Scribe_Values.LookValue<string>(ref text2, "adulthood", null, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars && !text2.NullOrEmpty() && !BackstoryDatabase.TryGetWithIdentifier(text2, out this.adulthood))
			{
				Log.Error("Couldn't load adult backstory with identifier " + text2 + ". Giving random.");
				this.adulthood = BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
			}
			Scribe_Values.LookValue<BodyType>(ref this.bodyType, "bodyType", BodyType.Undefined, false);
			Scribe_Values.LookValue<CrownType>(ref this.crownType, "crownType", CrownType.Undefined, false);
			Scribe_Values.LookValue<string>(ref this.headGraphicPath, "headGraphicPath", null, false);
			Scribe_Defs.LookDef<HairDef>(ref this.hairDef, "hairDef");
			Scribe_Values.LookValue<Color>(ref this.hairColor, "hairColor", default(Color), false);
			Scribe_Values.LookValue<float>(ref this.melanin, "melanin", 0f, false);
			Scribe_Deep.LookDeep<TraitSet>(ref this.traits, "traits", new object[]
			{
				this.pawn
			});
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.hairDef == null)
			{
				this.hairDef = DefDatabase<HairDef>.AllDefs.RandomElement<HairDef>();
			}
		}

		public Backstory GetBackstory(BackstorySlot slot)
		{
			if (slot == BackstorySlot.Childhood)
			{
				return this.childhood;
			}
			return this.adulthood;
		}

		public bool WorkTypeIsDisabled(WorkTypeDef w)
		{
			return this.DisabledWorkTypes.Contains(w);
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

		public bool WorkTagIsDisabled(WorkTags w)
		{
			return (this.CombinedDisabledWorkTags & w) != WorkTags.None;
		}

		internal void Notify_TraitChanged()
		{
			this.cachedDisabledWorkTypes = null;
		}
	}
}
