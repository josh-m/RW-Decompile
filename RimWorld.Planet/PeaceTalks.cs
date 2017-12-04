using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.Planet
{
	public class PeaceTalks : WorldObject
	{
		private Material cachedMat;

		private static readonly SimpleCurve BadOutcomeFactorAtDiplomacyPower = new SimpleCurve
		{
			{
				new CurvePoint(0f, 4f),
				true
			},
			{
				new CurvePoint(1f, 1f),
				true
			},
			{
				new CurvePoint(1.5f, 0.4f),
				true
			}
		};

		private const float BaseWeight_Disaster = 0.05f;

		private const float BaseWeight_Backfire = 0.1f;

		private const float BaseWeight_TalksFlounder = 0.2f;

		private const float BaseWeight_Success = 0.55f;

		private const float BaseWeight_Triumph = 0.1f;

		private static readonly FloatRange DisasterFactionRelationOffset = new FloatRange(-75f, -25f);

		private static readonly FloatRange BackfireFactionRelationOffset = new FloatRange(-50f, -10f);

		private static readonly FloatRange SuccessFactionRelationOffset = new FloatRange(25f, 75f);

		private static readonly FloatRange TriumphFactionRelationOffset = new FloatRange(50f, 75f);

		private const float SocialXPGainAmount = 6000f;

		private static List<Pair<Action, float>> tmpPossibleOutcomes = new List<Pair<Action, float>>();

		public override Material Material
		{
			get
			{
				if (this.cachedMat == null)
				{
					Color color;
					if (base.Faction != null)
					{
						color = base.Faction.Color;
					}
					else
					{
						color = Color.white;
					}
					this.cachedMat = MaterialPool.MatFrom(this.def.texture, ShaderDatabase.WorldOverlayTransparentLit, color, WorldMaterials.WorldObjectRenderQueue);
				}
				return this.cachedMat;
			}
		}

		public void Notify_CaravanArrived(Caravan caravan)
		{
			Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
			if (pawn == null)
			{
				Messages.Message("MessagePeaceTalksNoDiplomat".Translate(), caravan, MessageTypeDefOf.NegativeEvent);
				return;
			}
			float badOutcomeWeightFactor = PeaceTalks.GetBadOutcomeWeightFactor(pawn);
			float num = 1f / badOutcomeWeightFactor;
			PeaceTalks.tmpPossibleOutcomes.Clear();
			PeaceTalks.tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
			{
				this.Outcome_Disaster(caravan);
			}, 0.05f * badOutcomeWeightFactor));
			PeaceTalks.tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
			{
				this.Outcome_Backfire(caravan);
			}, 0.1f * badOutcomeWeightFactor));
			PeaceTalks.tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
			{
				this.Outcome_TalksFlounder(caravan);
			}, 0.2f));
			PeaceTalks.tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
			{
				this.Outcome_Success(caravan);
			}, 0.55f * num));
			PeaceTalks.tmpPossibleOutcomes.Add(new Pair<Action, float>(delegate
			{
				this.Outcome_Triumph(caravan);
			}, 0.1f * num));
			Action first = PeaceTalks.tmpPossibleOutcomes.RandomElementByWeight((Pair<Action, float> x) => x.Second).First;
			first();
			pawn.skills.Learn(SkillDefOf.Social, 6000f, true);
			Find.WorldObjects.Remove(this);
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan))
			{
				yield return o;
			}
			yield return new FloatMenuOption("VisitPeaceTalks".Translate(new object[]
			{
				this.Label
			}), delegate
			{
				caravan.pather.StartPath(this.$this.Tile, new CaravanArrivalAction_VisitPeaceTalks(this.$this), true);
			}, MenuOptionPriority.Default, null, null, 0f, null, this);
			if (Prefs.DevMode)
			{
				yield return new FloatMenuOption("VisitPeaceTalks".Translate(new object[]
				{
					this.Label
				}) + " (Dev: instantly)", delegate
				{
					caravan.Tile = this.$this.Tile;
					caravan.pather.StopDead();
					new CaravanArrivalAction_VisitPeaceTalks(this.$this).Arrived(caravan);
				}, MenuOptionPriority.Default, null, null, 0f, null, this);
			}
		}

		private void Outcome_Disaster(Caravan caravan)
		{
			LongEventHandler.QueueLongEvent(delegate
			{
				float randomInRange = PeaceTalks.DisasterFactionRelationOffset.RandomInRange;
				this.Faction.AffectGoodwillWith(Faction.OfPlayer, randomInRange);
				if (!this.Faction.HostileTo(Faction.OfPlayer))
				{
					this.Faction.SetHostileTo(Faction.OfPlayer, true);
				}
				IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, IncidentCategory.ThreatBig, caravan);
				incidentParms.faction = this.Faction;
				PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(incidentParms, true);
				defaultPawnGroupMakerParms.generateFightersOnly = true;
				List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, defaultPawnGroupMakerParms, true).ToList<Pawn>();
				Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, list);
				if (list.Any<Pawn>())
				{
					LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(this.Faction, true, true, false, false, true), map, list);
				}
				Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
				GlobalTargetInfo lookTarget = (!list.Any<Pawn>()) ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(list[0].Position, map, false);
				Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_Disaster".Translate(), this.GetLetterText("LetterPeaceTalks_Disaster".Translate(new object[]
				{
					this.Faction.def.pawnsPlural.CapitalizeFirst(),
					this.Faction.Name,
					Mathf.RoundToInt(randomInRange)
				}), caravan), LetterDefOf.ThreatBig, lookTarget, null);
			}, "GeneratingMapForNewEncounter", false, null);
		}

		private void Outcome_Backfire(Caravan caravan)
		{
			float randomInRange = PeaceTalks.BackfireFactionRelationOffset.RandomInRange;
			base.Faction.AffectGoodwillWith(Faction.OfPlayer, randomInRange);
			Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_Backfire".Translate(), this.GetLetterText("LetterPeaceTalks_Backfire".Translate(new object[]
			{
				base.Faction.Name,
				Mathf.RoundToInt(randomInRange)
			}), caravan), LetterDefOf.NegativeEvent, caravan, null);
		}

		private void Outcome_TalksFlounder(Caravan caravan)
		{
			Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_TalksFlounder".Translate(), this.GetLetterText("LetterPeaceTalks_TalksFlounder".Translate(new object[]
			{
				base.Faction.Name
			}), caravan), LetterDefOf.NeutralEvent, caravan, null);
		}

		private void Outcome_Success(Caravan caravan)
		{
			float randomInRange = PeaceTalks.SuccessFactionRelationOffset.RandomInRange;
			base.Faction.AffectGoodwillWith(Faction.OfPlayer, randomInRange);
			Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_Success".Translate(), this.GetLetterText("LetterPeaceTalks_Success".Translate(new object[]
			{
				base.Faction.Name,
				Mathf.RoundToInt(randomInRange)
			}), caravan), LetterDefOf.PositiveEvent, caravan, null);
		}

		private void Outcome_Triumph(Caravan caravan)
		{
			float randomInRange = PeaceTalks.TriumphFactionRelationOffset.RandomInRange;
			base.Faction.AffectGoodwillWith(Faction.OfPlayer, randomInRange);
			List<Thing> list = ItemCollectionGeneratorDefOf.PeaceTalksGift.Worker.Generate(default(ItemCollectionGeneratorParams));
			for (int i = 0; i < list.Count; i++)
			{
				caravan.AddPawnOrItem(list[0], true);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_Triumph".Translate(), this.GetLetterText("LetterPeaceTalks_Triumph".Translate(new object[]
			{
				base.Faction.Name,
				Mathf.RoundToInt(randomInRange),
				list[0].Label
			}), caravan), LetterDefOf.PositiveEvent, caravan, null);
		}

		private string GetLetterText(string baseText, Caravan caravan)
		{
			string text = baseText;
			Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
			if (pawn != null)
			{
				text = text + "\n\n" + "PeaceTalksSocialXPGain".Translate(new object[]
				{
					pawn.LabelShort,
					6000f
				});
			}
			return text;
		}

		private static float GetBadOutcomeWeightFactor(Pawn diplomat)
		{
			float statValue = diplomat.GetStatValue(StatDefOf.DiplomacyPower, true);
			return PeaceTalks.GetBadOutcomeWeightFactor(statValue);
		}

		private static float GetBadOutcomeWeightFactor(float diplomacyPower)
		{
			return PeaceTalks.BadOutcomeFactorAtDiplomacyPower.Evaluate(diplomacyPower);
		}

		public static void LogChances()
		{
			StringBuilder stringBuilder = new StringBuilder();
			PeaceTalks.AppendDebugChances(stringBuilder, 0f);
			PeaceTalks.AppendDebugChances(stringBuilder, 1f);
			PeaceTalks.AppendDebugChances(stringBuilder, 1.5f);
			Log.Message(stringBuilder.ToString());
		}

		private static void AppendDebugChances(StringBuilder sb, float diplomacyPower)
		{
			if (sb.Length > 0)
			{
				sb.AppendLine();
			}
			sb.AppendLine("--- DiplomacyPower = " + diplomacyPower.ToStringPercent() + " ---");
			float badOutcomeWeightFactor = PeaceTalks.GetBadOutcomeWeightFactor(diplomacyPower);
			float num = 1f / badOutcomeWeightFactor;
			sb.AppendLine("Bad outcome weight factor: " + badOutcomeWeightFactor.ToString("0.##"));
			float num2 = 0.05f * badOutcomeWeightFactor;
			float num3 = 0.1f * badOutcomeWeightFactor;
			float num4 = 0.2f;
			float num5 = 0.55f * num;
			float num6 = 0.1f * num;
			float num7 = num2 + num3 + num4 + num5 + num6;
			sb.AppendLine("Disaster: " + (num2 / num7).ToStringPercent());
			sb.AppendLine("Backfire: " + (num3 / num7).ToStringPercent());
			sb.AppendLine("Talks flounder: " + (num4 / num7).ToStringPercent());
			sb.AppendLine("Success: " + (num5 / num7).ToStringPercent());
			sb.AppendLine("Triumph: " + (num6 / num7).ToStringPercent());
		}
	}
}
