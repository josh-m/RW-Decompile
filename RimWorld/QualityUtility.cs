using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[HasDebugOutput]
	public static class QualityUtility
	{
		public static List<QualityCategory> AllQualityCategories;

		static QualityUtility()
		{
			QualityUtility.AllQualityCategories = new List<QualityCategory>();
			foreach (QualityCategory item in Enum.GetValues(typeof(QualityCategory)))
			{
				QualityUtility.AllQualityCategories.Add(item);
			}
		}

		public static bool TryGetQuality(this Thing t, out QualityCategory qc)
		{
			MinifiedThing minifiedThing = t as MinifiedThing;
			CompQuality compQuality = (minifiedThing == null) ? t.TryGetComp<CompQuality>() : minifiedThing.InnerThing.TryGetComp<CompQuality>();
			if (compQuality == null)
			{
				qc = QualityCategory.Normal;
				return false;
			}
			qc = compQuality.Quality;
			return true;
		}

		public static string GetLabel(this QualityCategory cat)
		{
			switch (cat)
			{
			case QualityCategory.Awful:
				return "QualityCategory_Awful".Translate();
			case QualityCategory.Poor:
				return "QualityCategory_Poor".Translate();
			case QualityCategory.Normal:
				return "QualityCategory_Normal".Translate();
			case QualityCategory.Good:
				return "QualityCategory_Good".Translate();
			case QualityCategory.Excellent:
				return "QualityCategory_Excellent".Translate();
			case QualityCategory.Masterwork:
				return "QualityCategory_Masterwork".Translate();
			case QualityCategory.Legendary:
				return "QualityCategory_Legendary".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static string GetLabelShort(this QualityCategory cat)
		{
			switch (cat)
			{
			case QualityCategory.Awful:
				return "QualityCategoryShort_Awful".Translate();
			case QualityCategory.Poor:
				return "QualityCategoryShort_Poor".Translate();
			case QualityCategory.Normal:
				return "QualityCategoryShort_Normal".Translate();
			case QualityCategory.Good:
				return "QualityCategoryShort_Good".Translate();
			case QualityCategory.Excellent:
				return "QualityCategoryShort_Excellent".Translate();
			case QualityCategory.Masterwork:
				return "QualityCategoryShort_Masterwork".Translate();
			case QualityCategory.Legendary:
				return "QualityCategoryShort_Legendary".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static bool FollowQualityThingFilter(this ThingDef def)
		{
			return def.stackLimit == 1 || def.HasComp(typeof(CompQuality));
		}

		public static QualityCategory GenerateQuality(QualityGenerator qualityGenerator)
		{
			switch (qualityGenerator)
			{
			case QualityGenerator.BaseGen:
				return QualityUtility.GenerateQualityBaseGen();
			case QualityGenerator.Reward:
				return QualityUtility.GenerateQualityReward();
			case QualityGenerator.Gift:
				return QualityUtility.GenerateQualityGift();
			default:
				throw new NotImplementedException(qualityGenerator.ToString());
			}
		}

		public static QualityCategory GenerateQualityRandomEqualChance()
		{
			return QualityUtility.AllQualityCategories.RandomElement<QualityCategory>();
		}

		public static QualityCategory GenerateQualityReward()
		{
			return QualityUtility.GenerateFromGaussian(1f, QualityCategory.Legendary, QualityCategory.Excellent, QualityCategory.Good);
		}

		public static QualityCategory GenerateQualityGift()
		{
			return QualityUtility.GenerateFromGaussian(1f, QualityCategory.Legendary, QualityCategory.Normal, QualityCategory.Normal);
		}

		public static QualityCategory GenerateQualityTraderItem()
		{
			if (Rand.Value < 0.25f)
			{
				return QualityCategory.Normal;
			}
			QualityCategory qualityCategory = QualityUtility.GenerateFromGaussian(1.18f, QualityCategory.Masterwork, QualityCategory.Normal, QualityCategory.Poor);
			if (qualityCategory == QualityCategory.Poor && Rand.Value < 0.6f)
			{
				qualityCategory = QualityUtility.GenerateFromGaussian(1.18f, QualityCategory.Masterwork, QualityCategory.Normal, QualityCategory.Poor);
			}
			return qualityCategory;
		}

		public static QualityCategory GenerateQualityBaseGen()
		{
			if (Rand.Value < 0.3f)
			{
				return QualityCategory.Normal;
			}
			return QualityUtility.GenerateFromGaussian(1f, QualityCategory.Excellent, QualityCategory.Normal, QualityCategory.Awful);
		}

		public static QualityCategory GenerateQualityGeneratingPawn(PawnKindDef pawnKind)
		{
			if (pawnKind.forceNormalGearQuality)
			{
				return QualityCategory.Normal;
			}
			int itemQuality = (int)pawnKind.itemQuality;
			float value = Rand.Value;
			int num;
			if (value < 0.1f)
			{
				num = itemQuality - 1;
			}
			else if (value < 0.2f)
			{
				num = itemQuality + 1;
			}
			else
			{
				num = itemQuality;
			}
			num = Mathf.Clamp(num, 0, 4);
			return (QualityCategory)num;
		}

		public static QualityCategory GenerateQualityCreatedByPawn(int relevantSkillLevel, bool inspired)
		{
			float num = 0f;
			switch (relevantSkillLevel)
			{
			case 0:
				num += 0.7f;
				break;
			case 1:
				num += 1.1f;
				break;
			case 2:
				num += 1.5f;
				break;
			case 3:
				num += 1.8f;
				break;
			case 4:
				num += 2f;
				break;
			case 5:
				num += 2.2f;
				break;
			case 6:
				num += 2.4f;
				break;
			case 7:
				num += 2.6f;
				break;
			case 8:
				num += 2.8f;
				break;
			case 9:
				num += 2.95f;
				break;
			case 10:
				num += 3.1f;
				break;
			case 11:
				num += 3.25f;
				break;
			case 12:
				num += 3.4f;
				break;
			case 13:
				num += 3.5f;
				break;
			case 14:
				num += 3.6f;
				break;
			case 15:
				num += 3.7f;
				break;
			case 16:
				num += 3.8f;
				break;
			case 17:
				num += 3.9f;
				break;
			case 18:
				num += 4f;
				break;
			case 19:
				num += 4.1f;
				break;
			case 20:
				num += 4.2f;
				break;
			}
			int num2 = (int)Rand.GaussianAsymmetric(num, 0.6f, 0.8f);
			num2 = Mathf.Clamp(num2, 0, 5);
			if (num2 == 5 && Rand.Value < 0.5f)
			{
				num2 = (int)Rand.GaussianAsymmetric(num, 0.6f, 0.95f);
				num2 = Mathf.Clamp(num2, 0, 5);
			}
			QualityCategory qualityCategory = (QualityCategory)num2;
			if (inspired)
			{
				qualityCategory = QualityUtility.AddLevels(qualityCategory, 2);
			}
			return qualityCategory;
		}

		public static QualityCategory GenerateQualityCreatedByPawn(Pawn pawn, SkillDef relevantSkill)
		{
			int level = pawn.skills.GetSkill(relevantSkill).Level;
			bool flag = pawn.InspirationDef == InspirationDefOf.Inspired_Creativity;
			QualityCategory result = QualityUtility.GenerateQualityCreatedByPawn(level, flag);
			if (flag)
			{
				pawn.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Creativity);
			}
			return result;
		}

		private static QualityCategory GenerateFromGaussian(float widthFactor, QualityCategory max = QualityCategory.Legendary, QualityCategory center = QualityCategory.Normal, QualityCategory min = QualityCategory.Awful)
		{
			float num = Rand.Gaussian((float)center + 0.5f, widthFactor);
			if (num < (float)min)
			{
				num = (float)min;
			}
			if (num > (float)max)
			{
				num = (float)max;
			}
			return (QualityCategory)((int)num);
		}

		private static QualityCategory AddLevels(QualityCategory quality, int levels)
		{
			return (QualityCategory)Mathf.Min((int)(quality + (byte)levels), 6);
		}

		public static void SendCraftNotification(Thing thing, Pawn worker)
		{
			if (worker == null)
			{
				return;
			}
			CompQuality compQuality = thing.TryGetComp<CompQuality>();
			if (compQuality == null)
			{
				return;
			}
			if (compQuality.Quality == QualityCategory.Masterwork)
			{
				Messages.Message("MessageCraftedMasterwork".Translate(worker.LabelShort, thing.LabelShort, worker.Named("WORKER"), thing.Named("CRAFTED")), thing, MessageTypeDefOf.PositiveEvent, true);
			}
			else if (compQuality.Quality == QualityCategory.Legendary)
			{
				Find.LetterStack.ReceiveLetter("LetterCraftedLegendaryLabel".Translate(), "LetterCraftedLegendaryMessage".Translate(worker.LabelShort, thing.LabelShort, worker.Named("WORKER"), thing.Named("CRAFTED")), LetterDefOf.PositiveEvent, thing, null, null);
			}
		}

		[DebugOutput]
		private static void QualityGenerationData()
		{
			List<TableDataGetter<QualityCategory>> list = new List<TableDataGetter<QualityCategory>>();
			list.Add(new TableDataGetter<QualityCategory>("quality", (QualityCategory q) => q.ToString()));
			list.Add(new TableDataGetter<QualityCategory>("Rewards\n(quests,\netc...? )", (QualityCategory q) => QualityUtility.DebugQualitiesStringSingle(q, new Func<QualityCategory>(QualityUtility.GenerateQualityReward))));
			list.Add(new TableDataGetter<QualityCategory>("Trader\nitems", (QualityCategory q) => QualityUtility.DebugQualitiesStringSingle(q, new Func<QualityCategory>(QualityUtility.GenerateQualityTraderItem))));
			list.Add(new TableDataGetter<QualityCategory>("Map generation\nitems and\nbuildings\n(e.g. NPC bases)", (QualityCategory q) => QualityUtility.DebugQualitiesStringSingle(q, new Func<QualityCategory>(QualityUtility.GenerateQualityBaseGen))));
			list.Add(new TableDataGetter<QualityCategory>("Gifts", (QualityCategory q) => QualityUtility.DebugQualitiesStringSingle(q, new Func<QualityCategory>(QualityUtility.GenerateQualityGift))));
			for (int i = 0; i <= 20; i++)
			{
				int localLevel = i;
				list.Add(new TableDataGetter<QualityCategory>("Made\nat skill\n" + i, (QualityCategory q) => QualityUtility.DebugQualitiesStringSingle(q, () => QualityUtility.GenerateQualityCreatedByPawn(localLevel, false))));
			}
			foreach (PawnKindDef current in from k in DefDatabase<PawnKindDef>.AllDefs
			orderby k.combatPower
			select k)
			{
				PawnKindDef localPk = current;
				if (localPk.RaceProps.Humanlike)
				{
					list.Add(new TableDataGetter<QualityCategory>(string.Concat(new object[]
					{
						"Gear for\n",
						localPk.defName,
						"\nPower ",
						localPk.combatPower.ToString("F0"),
						"\nitemQuality:\n",
						localPk.itemQuality
					}), (QualityCategory q) => QualityUtility.DebugQualitiesStringSingle(q, () => QualityUtility.GenerateQualityGeneratingPawn(localPk))));
				}
			}
			DebugTables.MakeTablesDialog<QualityCategory>(QualityUtility.AllQualityCategories, list.ToArray());
		}

		private static string DebugQualitiesStringSingle(QualityCategory quality, Func<QualityCategory> qualityGenerator)
		{
			int num = 10000;
			List<QualityCategory> list = new List<QualityCategory>();
			for (int i = 0; i < num; i++)
			{
				list.Add(qualityGenerator());
			}
			return ((float)(from q in list
			where q == quality
			select q).Count<QualityCategory>() / (float)num).ToStringPercent();
		}

		private static string DebugQualitiesString(Func<QualityCategory> qualityGenerator)
		{
			int num = 10000;
			StringBuilder stringBuilder = new StringBuilder();
			List<QualityCategory> list = new List<QualityCategory>();
			for (int i = 0; i < num; i++)
			{
				list.Add(qualityGenerator());
			}
			foreach (QualityCategory qu in QualityUtility.AllQualityCategories)
			{
				stringBuilder.AppendLine(qu.ToString() + " - " + ((float)(from q in list
				where q == qu
				select q).Count<QualityCategory>() / (float)num).ToStringPercent());
			}
			return stringBuilder.ToString();
		}
	}
}
