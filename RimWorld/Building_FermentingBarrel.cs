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
		private const int MaxCapacity = 25;

		private const int BaseFermentationDuration = 600000;

		private const float MinIdealTemperature = 7f;

		private int wortCount;

		private float progressInt;

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
				if (base.MapHeld == null)
				{
					Log.ErrorOnce("Tried to get a fermenting barrel temperature but MapHeld is null.", 847163513);
					return 7f;
				}
				return base.PositionHeld.GetTemperature(base.MapHeld);
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
				CompProperties_TemperatureRuinable compProperties = this.def.GetCompProperties<CompProperties_TemperatureRuinable>();
				float temperature = this.Temperature;
				if (temperature < compProperties.minSafeTemperature)
				{
					return 0.1f;
				}
				if (temperature < 7f)
				{
					return GenMath.LerpDouble(compProperties.minSafeTemperature, 7f, 0.1f, 1f, temperature);
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

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.wortCount, "wortCount", 0, false);
			Scribe_Values.LookValue<float>(ref this.progressInt, "progress", 0f, false);
		}

		public override void TickRare()
		{
			base.TickRare();
			if (!this.Empty)
			{
				this.Progress = Mathf.Min(this.Progress + 250f * this.ProgressPerTickAtCurrentTemp, 1f);
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
			base.GetComp<CompTemperatureRuinable>().Reset();
		}

		protected override void ReceiveCompSignal(string signal)
		{
			if (signal == "RuinedByTemperature")
			{
				this.Reset();
			}
		}

		private void Reset()
		{
			this.wortCount = 0;
			this.Progress = 0f;
		}

		public void AddWort(Thing wort)
		{
			CompTemperatureRuinable comp = base.GetComp<CompTemperatureRuinable>();
			if (comp.Ruined)
			{
				comp.Reset();
			}
			this.AddWort(wort.stackCount);
			wort.Destroy(DestroyMode.Vanish);
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			CompTemperatureRuinable comp = base.GetComp<CompTemperatureRuinable>();
			if (!this.Empty && !comp.Ruined)
			{
				if (this.Fermented)
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
			if (base.MapHeld != null)
			{
				stringBuilder.AppendLine("Temperature".Translate() + ": " + this.Temperature.ToStringTemperature("F0"));
			}
			stringBuilder.AppendLine(string.Concat(new string[]
			{
				"IdealFermentingTemperature".Translate(),
				": ",
				7f.ToStringTemperature("F0"),
				" ~ ",
				comp.Props.maxSafeTemperature.ToStringTemperature("F0")
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
	}
}
