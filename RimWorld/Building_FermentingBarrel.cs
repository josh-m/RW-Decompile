using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Building_FermentingBarrel : Building
	{
		private enum SpoilReason
		{
			None,
			Freezing,
			HighTemperature
		}

		private const int MaxCapacity = 25;

		private const int BaseFermentationDuration = 600000;

		public const float MinTemperature = -1f;

		private const float MinIdealTemperature = 7f;

		public const float MaxTemperature = 30f;

		private int wortCount;

		private float progressInt;

		private Building_FermentingBarrel.SpoilReason spoilReason;

		private Material barFilledCachedMat;

		private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

		private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);

		private static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);

		private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

		public float Progress
		{
			get
			{
				return this.progressInt;
			}
			set
			{
				if (value == this.progressInt)
				{
					return;
				}
				this.progressInt = value;
				this.barFilledCachedMat = null;
			}
		}

		private Material BarFilledMat
		{
			get
			{
				if (this.barFilledCachedMat == null)
				{
					this.barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(Building_FermentingBarrel.BarZeroProgressColor, Building_FermentingBarrel.BarFermentedColor, this.Progress));
				}
				return this.barFilledCachedMat;
			}
		}

		private float Temperature
		{
			get
			{
				return this.GetInnerIfMinified().PositionHeld.GetTemperature();
			}
		}

		public int SpaceLeftForWort
		{
			get
			{
				if (this.Fermented)
				{
					return 0;
				}
				return 25 - this.wortCount;
			}
		}

		private bool Empty
		{
			get
			{
				return this.wortCount <= 0;
			}
		}

		public bool Fermented
		{
			get
			{
				return !this.Empty && this.Progress >= 1f;
			}
		}

		private float CurrentTempProgressSpeedFactor
		{
			get
			{
				float temperature = this.Temperature;
				if (temperature < -1f)
				{
					return 0.1f;
				}
				if (temperature < 7f)
				{
					return GenMath.LerpDouble(-1f, 7f, 0.1f, 1f, temperature);
				}
				return 1f;
			}
		}

		private float ProgressPerTickAtCurrentTemp
		{
			get
			{
				return 1.66666666E-06f * this.CurrentTempProgressSpeedFactor;
			}
		}

		private int EstimatedTicksLeft
		{
			get
			{
				return Mathf.Max(Mathf.RoundToInt((1f - this.Progress) / this.ProgressPerTickAtCurrentTemp), 0);
			}
		}

		private string SpoilReasonLabel
		{
			get
			{
				Building_FermentingBarrel.SpoilReason spoilReason = this.spoilReason;
				if (spoilReason == Building_FermentingBarrel.SpoilReason.Freezing)
				{
					return "SpoilReason_Freezing".Translate();
				}
				if (spoilReason != Building_FermentingBarrel.SpoilReason.HighTemperature)
				{
					throw new NotImplementedException();
				}
				return "SpoilReason_HighTemperature".Translate();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.wortCount, "wortCount", 0, false);
			Scribe_Values.LookValue<float>(ref this.progressInt, "progress", 0f, false);
			Scribe_Values.LookValue<Building_FermentingBarrel.SpoilReason>(ref this.spoilReason, "spoilReason", Building_FermentingBarrel.SpoilReason.None, false);
		}

		public override void TickRare()
		{
			base.TickRare();
			if (!this.Empty)
			{
				this.Progress = Mathf.Min(this.Progress + 250f * this.ProgressPerTickAtCurrentTemp, 1f);
				this.CheckSpoiled();
			}
		}

		public void AddWort(int count)
		{
			if (this.Fermented)
			{
				Log.Warning("Tried to add wort to a barrel full of beer. Colonists should take the beer first.");
				return;
			}
			int num = Mathf.Min(count, 25 - this.wortCount);
			if (num <= 0)
			{
				return;
			}
			this.Progress = GenMath.WeightedAverage(0f, (float)num, this.Progress, (float)this.wortCount);
			this.wortCount += num;
			this.spoilReason = Building_FermentingBarrel.SpoilReason.None;
		}

		public void AddWort(Thing wort)
		{
			this.AddWort(wort.stackCount);
			wort.Destroy(DestroyMode.Vanish);
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (this.Empty && this.spoilReason != Building_FermentingBarrel.SpoilReason.None)
			{
				stringBuilder.AppendLine(this.SpoilReasonLabel);
			}
			else if (this.Fermented)
			{
				stringBuilder.AppendLine("ContainsBeer".Translate(new object[]
				{
					this.wortCount,
					25
				}));
			}
			else
			{
				stringBuilder.AppendLine("ContainsWort".Translate(new object[]
				{
					this.wortCount,
					25
				}));
			}
			if (!this.Empty)
			{
				if (this.Fermented)
				{
					stringBuilder.AppendLine("Fermented".Translate());
				}
				else
				{
					stringBuilder.AppendLine("FermentationProgress".Translate(new object[]
					{
						this.Progress.ToStringPercent(),
						this.EstimatedTicksLeft.ToStringTicksToPeriod(true)
					}));
					if (this.CurrentTempProgressSpeedFactor != 1f)
					{
						stringBuilder.AppendLine("FermentationBarrelOutOfIdealTemperature".Translate(new object[]
						{
							this.CurrentTempProgressSpeedFactor.ToStringPercent()
						}));
					}
				}
			}
			stringBuilder.AppendLine("Temperature".Translate() + ": " + GenTemperature.GetTemperatureForCell(base.Position).ToStringTemperature("F0"));
			stringBuilder.AppendLine(string.Concat(new string[]
			{
				"IdealFermentingTemperature".Translate(),
				": ",
				7f.ToStringTemperature("F0"),
				" ~ ",
				30f.ToStringTemperature("F0")
			}));
			return stringBuilder.ToString();
		}

		public Thing TakeOutBeer()
		{
			if (!this.Fermented)
			{
				Log.Warning("Tried to get beer but it's not yet fermented.");
				return null;
			}
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Beer, null);
			thing.stackCount = this.wortCount;
			this.Reset();
			return thing;
		}

		public override void Draw()
		{
			base.Draw();
			if (!this.Empty)
			{
				Vector3 drawPos = this.DrawPos;
				drawPos.y += 0.05f;
				drawPos.z += 0.25f;
				GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
				{
					center = drawPos,
					size = Building_FermentingBarrel.BarSize,
					fillPercent = (float)this.wortCount / 25f,
					filledMat = this.BarFilledMat,
					unfilledMat = Building_FermentingBarrel.BarUnfilledMat,
					margin = 0.1f,
					rotation = Rot4.North
				});
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (Prefs.DevMode && !this.Empty)
			{
				yield return new Command_Action
				{
					defaultLabel = "Debug: Set progress to 1",
					action = delegate
					{
						this.<>f__this.Progress = 1f;
					}
				};
			}
		}

		private void CheckSpoiled()
		{
			if (this.Empty || this.Fermented)
			{
				return;
			}
			float temperature = this.Temperature;
			if (temperature < -1f)
			{
				this.Spoiled(Building_FermentingBarrel.SpoilReason.Freezing);
			}
			else if (temperature > 30f)
			{
				this.Spoiled(Building_FermentingBarrel.SpoilReason.HighTemperature);
			}
		}

		private void Spoiled(Building_FermentingBarrel.SpoilReason reason)
		{
			this.Reset();
			this.spoilReason = reason;
		}

		private void Reset()
		{
			this.wortCount = 0;
			this.Progress = 0f;
			this.spoilReason = Building_FermentingBarrel.SpoilReason.None;
		}
	}
}
