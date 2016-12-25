using System;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompRottable : ThingComp
	{
		private float rotProgressInt;

		public float RotProgress
		{
			get
			{
				return this.rotProgressInt;
			}
			set
			{
				RotStage stage = this.Stage;
				this.rotProgressInt = value;
				if (stage != this.Stage)
				{
					this.StageChanged();
				}
			}
		}

		private CompProperties_Rottable PropsRot
		{
			get
			{
				return (CompProperties_Rottable)this.props;
			}
		}

		public RotStage Stage
		{
			get
			{
				if (this.RotProgress < (float)this.PropsRot.TicksToRotStart)
				{
					return RotStage.Fresh;
				}
				if (this.RotProgress < (float)this.PropsRot.TicksToDessicated)
				{
					return RotStage.Rotting;
				}
				return RotStage.Dessicated;
			}
		}

		public int TicksUntilRotAtCurrentTemp
		{
			get
			{
				float num = GenTemperature.GetTemperatureForCell(this.parent.PositionHeld);
				num = (float)Mathf.RoundToInt(num);
				float num2 = GenTemperature.RotRateAtTemperature(num);
				if (num2 <= 0f)
				{
					return 2147483647;
				}
				float num3 = (float)this.PropsRot.TicksToRotStart - this.RotProgress;
				if (num3 <= 0f)
				{
					return 0;
				}
				return Mathf.RoundToInt(num3 / num2);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<float>(ref this.rotProgressInt, "rotProg", 0f, false);
		}

		public override void CompTickRare()
		{
			float rotProgress = this.RotProgress;
			float num = 1f;
			float temperatureForCell = GenTemperature.GetTemperatureForCell(this.parent.PositionHeld);
			num *= GenTemperature.RotRateAtTemperature(temperatureForCell);
			this.RotProgress += Mathf.Round(num * 250f);
			if (this.Stage == RotStage.Rotting && this.PropsRot.rotDestroys)
			{
				if (Find.SlotGroupManager.SlotGroupAt(this.parent.Position) != null)
				{
					Messages.Message("MessageRottedAwayInStorage".Translate(new object[]
					{
						this.parent.Label
					}).CapitalizeFirst(), MessageSound.Silent);
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.SpoilageAndFreezers, OpportunityType.GoodToKnow);
				}
				this.parent.Destroy(DestroyMode.Vanish);
				return;
			}
			bool flag = Mathf.FloorToInt(rotProgress / 60000f) != Mathf.FloorToInt(this.RotProgress / 60000f);
			if (flag)
			{
				if (this.Stage == RotStage.Rotting && this.PropsRot.rotDamagePerDay > 0f)
				{
					this.parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, GenMath.RoundRandom(this.PropsRot.rotDamagePerDay), null, null, null));
				}
				else if (this.Stage == RotStage.Dessicated && this.PropsRot.dessicatedDamagePerDay > 0f && this.ShouldTakeDessicateDamage())
				{
					this.parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, GenMath.RoundRandom(this.PropsRot.dessicatedDamagePerDay), null, null, null));
				}
			}
		}

		private bool ShouldTakeDessicateDamage()
		{
			if (this.parent.holder != null)
			{
				Thing thing = this.parent.holder.owner as Thing;
				if (thing != null && thing.def.category == ThingCategory.Building && thing.def.building.preventDeterioration)
				{
					return false;
				}
			}
			return true;
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			float t = (float)count / (float)(this.parent.stackCount + count);
			float rotProgress = ((ThingWithComps)otherStack).GetComp<CompRottable>().RotProgress;
			this.RotProgress = Mathf.Lerp(this.RotProgress, rotProgress, t);
		}

		public override void PostSplitOff(Thing piece)
		{
			((ThingWithComps)piece).GetComp<CompRottable>().RotProgress = this.RotProgress;
		}

		public override void PostIngested(Pawn ingester)
		{
			if (this.Stage != RotStage.Fresh)
			{
				FoodUtility.AddFoodPoisoningHediff(ingester, this.parent);
			}
		}

		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			switch (this.Stage)
			{
			case RotStage.Fresh:
				stringBuilder.AppendLine("RotStateFresh".Translate());
				break;
			case RotStage.Rotting:
				stringBuilder.AppendLine("RotStateRotting".Translate());
				break;
			case RotStage.Dessicated:
				stringBuilder.AppendLine("RotStateDessicated".Translate());
				break;
			}
			float num = (float)this.PropsRot.TicksToRotStart - this.RotProgress;
			if (num > 0f)
			{
				float num2 = GenTemperature.GetTemperatureForCell(this.parent.PositionHeld);
				num2 = (float)Mathf.RoundToInt(num2);
				float num3 = GenTemperature.RotRateAtTemperature(num2);
				int ticksUntilRotAtCurrentTemp = this.TicksUntilRotAtCurrentTemp;
				if (num3 < 0.001f)
				{
					stringBuilder.AppendLine("CurrentlyFrozen".Translate());
				}
				else if (num3 < 0.999f)
				{
					stringBuilder.AppendLine("CurrentlyRefrigerated".Translate(new object[]
					{
						ticksUntilRotAtCurrentTemp.ToStringTicksToPeriodVagueMax()
					}));
				}
				else
				{
					stringBuilder.AppendLine("NotRefrigerated".Translate(new object[]
					{
						ticksUntilRotAtCurrentTemp.ToStringTicksToPeriodVagueMax()
					}));
				}
			}
			return stringBuilder.ToString();
		}

		private void StageChanged()
		{
			Corpse corpse = this.parent as Corpse;
			if (corpse != null)
			{
				corpse.RotStageChanged();
			}
		}
	}
}
