using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Joy : Need
	{
		private const float BaseFallPerTick = 1.00000007E-05f;

		private const float ThreshLow = 0.15f;

		private const float ThreshSatisfied = 0.3f;

		private const float ThreshHigh = 0.7f;

		private const float ThreshVeryHigh = 0.85f;

		private const float MinDownedJoy = 0.25f;

		public JoyToleranceSet tolerances = new JoyToleranceSet();

		private int lastGainTick = -999;

		public JoyCategory CurCategory
		{
			get
			{
				if (this.CurLevel < 0.01f)
				{
					return JoyCategory.Empty;
				}
				if (this.CurLevel < 0.15f)
				{
					return JoyCategory.VeryLow;
				}
				if (this.CurLevel < 0.3f)
				{
					return JoyCategory.Low;
				}
				if (this.CurLevel < 0.7f)
				{
					return JoyCategory.Satisfied;
				}
				if (this.CurLevel < 0.85f)
				{
					return JoyCategory.High;
				}
				return JoyCategory.Extreme;
			}
		}

		private float FallPerTick
		{
			get
			{
				switch (this.CurCategory)
				{
				case JoyCategory.Empty:
					return 1.00000007E-05f;
				case JoyCategory.VeryLow:
					return 4.00000044E-06f;
				case JoyCategory.Low:
					return 7.00000055E-06f;
				case JoyCategory.Satisfied:
					return 1.00000007E-05f;
				case JoyCategory.High:
					return 1.00000007E-05f;
				case JoyCategory.Extreme:
					return 1.00000007E-05f;
				default:
					throw new InvalidOperationException();
				}
			}
		}

		public override int GUIChangeArrow
		{
			get
			{
				return (!this.GainingJoy) ? -1 : 1;
			}
		}

		private bool GainingJoy
		{
			get
			{
				return Find.TickManager.TicksGame < this.lastGainTick + 10;
			}
		}

		public Need_Joy(Pawn pawn) : base(pawn)
		{
			this.threshPercents = new List<float>();
			this.threshPercents.Add(0.15f);
			this.threshPercents.Add(0.3f);
			this.threshPercents.Add(0.7f);
			this.threshPercents.Add(0.85f);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			this.tolerances.ExposeData();
		}

		public override void SetInitialLevel()
		{
			this.CurLevel = Rand.Range(0.5f, 0.8f);
		}

		public void GainJoy(float amount, JoyKindDef joyKind)
		{
			if (joyKind == null)
			{
				Log.Error("No joyKind!");
			}
			else
			{
				amount *= this.tolerances.JoyFactorFromTolerance(joyKind);
			}
			amount = Mathf.Min(amount, 1f - this.CurLevel);
			this.curLevelInt += amount;
			if (joyKind != null)
			{
				this.tolerances.Notify_JoyGained(amount, joyKind);
			}
			this.lastGainTick = Find.TickManager.TicksGame;
		}

		public override void NeedInterval()
		{
			if (!this.def.freezeWhileSleeping || this.pawn.Awake())
			{
				this.tolerances.NeedInterval();
				if (!this.GainingJoy)
				{
					this.curLevelInt -= this.FallPerTick * 150f;
				}
			}
			if (this.pawn.Downed)
			{
				if (this.curLevelInt < 0.25f)
				{
					this.curLevelInt = 0.25f;
				}
			}
			else if (this.curLevelInt < 0f)
			{
				this.curLevelInt = 0f;
			}
		}

		public override string GetTipString()
		{
			return base.GetTipString() + "\n" + this.tolerances.TolerancesString();
		}
	}
}
