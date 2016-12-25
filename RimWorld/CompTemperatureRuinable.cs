using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompTemperatureRuinable : ThingComp
	{
		public const string RuinedSignal = "RuinedByTemperature";

		protected float ruinedPercent;

		protected bool ruined;

		public CompProperties_TemperatureRuinable Props
		{
			get
			{
				return (CompProperties_TemperatureRuinable)this.props;
			}
		}

		public bool Ruined
		{
			get
			{
				return this.ruinedPercent >= 1f;
			}
		}

		private float Temperature
		{
			get
			{
				return this.parent.PositionHeld.GetTemperature(this.parent.MapHeld);
			}
		}

		private bool OnMap
		{
			get
			{
				return this.parent.MapHeld != null;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.LookValue<float>(ref this.ruinedPercent, "ruinedPercent", 0f, false);
		}

		public void Reset()
		{
			this.ruinedPercent = 0f;
		}

		public override void CompTick()
		{
			this.DoTicks(1);
		}

		public override void CompTickRare()
		{
			this.DoTicks(250);
		}

		private void DoTicks(int ticks)
		{
			if (!this.Ruined && this.OnMap)
			{
				float temperature = this.Temperature;
				if (temperature > this.Props.maxSafeTemperature)
				{
					this.ruinedPercent += (temperature - this.Props.maxSafeTemperature) * this.Props.progressPerDegreePerTick * (float)ticks;
				}
				else if (temperature < this.Props.minSafeTemperature)
				{
					this.ruinedPercent -= (temperature - this.Props.minSafeTemperature) * this.Props.progressPerDegreePerTick * (float)ticks;
				}
				if (this.ruinedPercent >= 1f)
				{
					this.ruinedPercent = 1f;
					this.parent.BroadcastCompSignal("RuinedByTemperature");
				}
				else if (this.ruinedPercent < 0f)
				{
					this.ruinedPercent = 0f;
				}
			}
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			float t = (float)count / (float)(this.parent.stackCount + count);
			CompTemperatureRuinable comp = ((ThingWithComps)otherStack).GetComp<CompTemperatureRuinable>();
			this.ruinedPercent = Mathf.Lerp(this.ruinedPercent, comp.ruinedPercent, t);
		}

		public override bool AllowStackWith(Thing other)
		{
			CompTemperatureRuinable comp = ((ThingWithComps)other).GetComp<CompTemperatureRuinable>();
			return this.ruined == comp.ruined;
		}

		public override void PostSplitOff(Thing piece)
		{
			CompTemperatureRuinable comp = ((ThingWithComps)piece).GetComp<CompTemperatureRuinable>();
			comp.ruinedPercent = this.ruinedPercent;
			comp.ruined = this.ruined;
		}

		public override string CompInspectStringExtra()
		{
			if (this.Ruined)
			{
				return "RuinedByTemperature".Translate();
			}
			if (this.ruinedPercent > 0f && this.OnMap)
			{
				float temperature = this.Temperature;
				string str;
				if (temperature > this.Props.maxSafeTemperature)
				{
					str = "Overheating".Translate();
				}
				else
				{
					if (temperature >= this.Props.minSafeTemperature)
					{
						return null;
					}
					str = "Freezing".Translate();
				}
				return str + ": " + this.ruinedPercent.ToStringPercent();
			}
			return null;
		}
	}
}
