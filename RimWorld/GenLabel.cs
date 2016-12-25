using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class GenLabel
	{
		private struct LabelRequest : IEquatable<GenLabel.LabelRequest>
		{
			public Thing thing;

			public BuildableDef entDef;

			public ThingDef stuffDef;

			public int stackCount;

			public QualityCategory quality;

			public int health;

			public int maxHealth;

			public bool wornByCorpse;

			public override bool Equals(object obj)
			{
				return obj is GenLabel.LabelRequest && this.Equals((GenLabel.LabelRequest)obj);
			}

			public bool Equals(GenLabel.LabelRequest other)
			{
				return this.thing == other.thing && this.entDef == other.entDef && this.stuffDef == other.stuffDef && this.stackCount == other.stackCount && this.quality == other.quality && this.health == other.health && this.maxHealth == other.maxHealth && this.wornByCorpse == other.wornByCorpse;
			}

			public override int GetHashCode()
			{
				int num = 0;
				num = Gen.HashCombine<Thing>(num, this.thing);
				num = Gen.HashCombine<BuildableDef>(num, this.entDef);
				num = Gen.HashCombine<ThingDef>(num, this.stuffDef);
				ThingDef thingDef = this.entDef as ThingDef;
				if (thingDef != null)
				{
					num = Gen.HashCombineInt(num, this.stackCount);
					QualityCategory qualityCategory;
					if (this.thing != null && this.thing.TryGetQuality(out qualityCategory))
					{
						num = Gen.HashCombineStruct<QualityCategory>(num, this.quality);
					}
					if (thingDef.useHitPoints)
					{
						num = Gen.HashCombineInt(num, this.health);
						num = Gen.HashCombineInt(num, this.maxHealth);
					}
					num = Gen.HashCombineInt(num, (!this.wornByCorpse) ? 0 : 1);
				}
				return num;
			}

			public static bool operator ==(GenLabel.LabelRequest lhs, GenLabel.LabelRequest rhs)
			{
				return lhs.Equals(rhs);
			}

			public static bool operator !=(GenLabel.LabelRequest lhs, GenLabel.LabelRequest rhs)
			{
				return !(lhs == rhs);
			}
		}

		private const int LabelDictionaryMaxCount = 2000;

		private static Dictionary<int, string> labelDictionary = new Dictionary<int, string>();

		public static void ClearCache()
		{
			GenLabel.labelDictionary.Clear();
		}

		public static string ThingLabel(BuildableDef entDef, ThingDef stuffDef, int stackCount = 1)
		{
			GenLabel.LabelRequest labelRequest = default(GenLabel.LabelRequest);
			labelRequest.entDef = entDef;
			labelRequest.stuffDef = stuffDef;
			labelRequest.stackCount = stackCount;
			int hashCode = labelRequest.GetHashCode();
			string text;
			if (!GenLabel.labelDictionary.TryGetValue(hashCode, out text))
			{
				if (GenLabel.labelDictionary.Count > 2000)
				{
					GenLabel.labelDictionary.Clear();
				}
				text = GenLabel.NewThingLabel(entDef, stuffDef, stackCount);
				GenLabel.labelDictionary.Add(hashCode, text);
			}
			return text;
		}

		private static string NewThingLabel(BuildableDef entDef, ThingDef stuffDef, int stackCount)
		{
			string text;
			if (stuffDef == null)
			{
				text = entDef.label;
			}
			else
			{
				text = "ThingMadeOfStuffLabel".Translate(new object[]
				{
					stuffDef.LabelAsStuff,
					entDef.label
				});
			}
			if (stackCount != 1)
			{
				text = text + " x" + stackCount.ToStringCached();
			}
			return text;
		}

		public static string ThingLabel(Thing t)
		{
			GenLabel.LabelRequest labelRequest = default(GenLabel.LabelRequest);
			labelRequest.thing = t;
			labelRequest.entDef = t.def;
			labelRequest.stuffDef = t.Stuff;
			labelRequest.stackCount = t.stackCount;
			t.TryGetQuality(out labelRequest.quality);
			if (t.def.useHitPoints)
			{
				labelRequest.health = t.HitPoints;
				labelRequest.maxHealth = t.MaxHitPoints;
			}
			Apparel apparel = t as Apparel;
			if (apparel != null)
			{
				labelRequest.wornByCorpse = apparel.WornByCorpse;
			}
			int hashCode = labelRequest.GetHashCode();
			string text;
			if (!GenLabel.labelDictionary.TryGetValue(hashCode, out text))
			{
				if (GenLabel.labelDictionary.Count > 2000)
				{
					GenLabel.labelDictionary.Clear();
				}
				text = GenLabel.NewThingLabel(t);
				GenLabel.labelDictionary.Add(hashCode, text);
			}
			return text;
		}

		private static string NewThingLabel(Thing t)
		{
			string text = GenLabel.ThingLabel(t.def, t.Stuff, 1);
			QualityCategory cat;
			bool flag = t.TryGetQuality(out cat);
			int hitPoints = t.HitPoints;
			int maxHitPoints = t.MaxHitPoints;
			bool flag2 = t.def.useHitPoints && hitPoints < maxHitPoints && t.def.stackLimit == 1;
			Apparel apparel = t as Apparel;
			bool flag3 = apparel != null && apparel.WornByCorpse;
			if (flag || flag2 || flag3)
			{
				text += " (";
				if (flag)
				{
					text += cat.GetLabel();
				}
				if (flag2)
				{
					if (flag)
					{
						text += " ";
					}
					text += ((float)hitPoints / (float)maxHitPoints).ToStringPercent();
				}
				if (flag3)
				{
					text = text + " " + "WornByCorpseChar".Translate();
				}
				text += ")";
			}
			return text;
		}

		public static string BestKindLabel(Pawn pawn, bool mustNoteGender = false, bool mustNoteLifeStage = false, bool plural = false)
		{
			bool flag = false;
			bool flag2 = false;
			string text = null;
			switch (pawn.gender)
			{
			case Gender.None:
				if (plural && !pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelPlural != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelPlural;
					flag2 = true;
				}
				else if (!pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.label != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.label;
					flag2 = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text);
					}
				}
				else if (plural && pawn.kindDef.labelPlural != null)
				{
					text = pawn.kindDef.labelPlural;
				}
				else
				{
					text = pawn.kindDef.label;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text);
					}
				}
				break;
			case Gender.Male:
				if (plural && !pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelMalePlural != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelMalePlural;
					flag2 = true;
					flag = true;
				}
				else if (!pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelMale != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelMale;
					flag2 = true;
					flag = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text);
					}
				}
				else if (plural && !pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelPlural != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelPlural;
					flag2 = true;
				}
				else if (!pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.label != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.label;
					flag2 = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text);
					}
				}
				else if (plural && pawn.kindDef.labelMalePlural != null)
				{
					text = pawn.kindDef.labelMalePlural;
					flag = true;
				}
				else if (pawn.kindDef.labelMale != null)
				{
					text = pawn.kindDef.labelMale;
					flag = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text);
					}
				}
				else if (plural && pawn.kindDef.labelPlural != null)
				{
					text = pawn.kindDef.labelPlural;
				}
				else
				{
					text = pawn.kindDef.label;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text);
					}
				}
				break;
			case Gender.Female:
				if (plural && !pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelFemalePlural != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelFemalePlural;
					flag2 = true;
					flag = true;
				}
				else if (!pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelFemale != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelFemale;
					flag2 = true;
					flag = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text);
					}
				}
				else if (plural && !pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelPlural != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelPlural;
					flag2 = true;
				}
				else if (!pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.label != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.label;
					flag2 = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text);
					}
				}
				else if (plural && pawn.kindDef.labelFemalePlural != null)
				{
					text = pawn.kindDef.labelFemalePlural;
					flag = true;
				}
				else if (pawn.kindDef.labelFemale != null)
				{
					text = pawn.kindDef.labelFemale;
					flag = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text);
					}
				}
				else if (plural && pawn.kindDef.labelPlural != null)
				{
					text = pawn.kindDef.labelPlural;
				}
				else
				{
					text = pawn.kindDef.label;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text);
					}
				}
				break;
			}
			if (mustNoteGender && !flag && pawn.gender != Gender.None)
			{
				text = "PawnMainDescGendered".Translate(new object[]
				{
					pawn.gender.GetLabel(),
					text
				});
			}
			if (mustNoteLifeStage && !flag2 && pawn.ageTracker != null && pawn.ageTracker.CurLifeStage.visible)
			{
				text = "PawnMainDescLifestageWrap".Translate(new object[]
				{
					text,
					pawn.ageTracker.CurLifeStage.Adjective
				});
			}
			return text;
		}
	}
}
