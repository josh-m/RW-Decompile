using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompRefuelable : ThingComp
	{
		public const string RefueledSignal = "Refueled";

		public const string RanOutOfFuelSignal = "RanOutOfFuel";

		private float fuel;

		private CompFlickable flickComp;

		public CompProperties_Refuelable Props
		{
			get
			{
				return (CompProperties_Refuelable)this.props;
			}
		}

		public float FuelPercent
		{
			get
			{
				return this.fuel / this.Props.fuelCapacity;
			}
		}

		public bool IsFull
		{
			get
			{
				return this.Props.fuelCapacity - this.fuel < 1f;
			}
		}

		public bool HasFuel
		{
			get
			{
				return this.fuel > 0f;
			}
		}

		private float ConsumptionRatePerTick
		{
			get
			{
				return this.Props.fuelConsumptionRate / 60000f;
			}
		}

		public bool ShouldAutoRefuelNow
		{
			get
			{
				return this.FuelPercent <= this.Props.autoRefuelPercent && !this.parent.IsBurning() && (this.flickComp == null || this.flickComp.SwitchIsOn) && Find.DesignationManager.DesignationOn(this.parent, DesignationDefOf.Flick) == null && Find.DesignationManager.DesignationOn(this.parent, DesignationDefOf.Deconstruct) == null;
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			if (this.Props.destroyOnNoFuel)
			{
				this.fuel = this.Props.fuelCapacity;
			}
			this.flickComp = this.parent.GetComp<CompFlickable>();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<float>(ref this.fuel, "fuel", 0f, false);
		}

		public override void PostDraw()
		{
			base.PostDraw();
			if (!this.HasFuel)
			{
				OverlayDrawer.DrawOverlay(this.parent, OverlayTypes.OutOfFuel);
			}
		}

		public override void PostDestroy(DestroyMode mode, bool wasSpawned)
		{
			base.PostDestroy(mode, wasSpawned);
			if (wasSpawned && this.Props.fuelFilter.AllowedDefCount == 1)
			{
				ThingDef thingDef = this.Props.fuelFilter.AllowedThingDefs.First<ThingDef>();
				float num = 1f;
				int i = GenMath.RoundRandom(num * this.fuel);
				while (i > 0)
				{
					Thing thing = ThingMaker.MakeThing(thingDef, null);
					thing.stackCount = Mathf.Min(i, thingDef.stackLimit);
					i -= thing.stackCount;
					GenPlace.TryPlaceThing(thing, this.parent.Position, ThingPlaceMode.Near, null);
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = string.Concat(new string[]
			{
				"Fuel".Translate(),
				": ",
				this.fuel.ToStringDecimalIfSmall(),
				" / ",
				this.Props.fuelCapacity.ToStringDecimalIfSmall()
			});
			if (!this.Props.consumeFuelOnlyWhenUsed && this.HasFuel)
			{
				int numTicks = (int)(this.fuel / this.Props.fuelConsumptionRate * 60000f);
				text = text + " (" + numTicks.ToStringTicksToPeriod(true) + ")";
			}
			return text;
		}

		public override void CompTick()
		{
			base.CompTick();
			if (!this.Props.consumeFuelOnlyWhenUsed && (this.flickComp == null || this.flickComp.SwitchIsOn))
			{
				this.ConsumeFuel(this.ConsumptionRatePerTick);
			}
			if (this.Props.fuelConsumptionPerTickInRain > 0f && this.parent.Spawned && Find.WeatherManager.RainRate > 0.4f && !Find.RoofGrid.Roofed(this.parent.Position))
			{
				this.ConsumeFuel(this.Props.fuelConsumptionPerTickInRain);
			}
		}

		public void ConsumeFuel(float amount)
		{
			if (this.fuel <= 0f)
			{
				return;
			}
			this.fuel -= amount;
			if (this.fuel <= 0f)
			{
				this.fuel = 0f;
				if (this.Props.destroyOnNoFuel)
				{
					this.parent.Destroy(DestroyMode.Vanish);
				}
				this.parent.BroadcastCompSignal("RanOutOfFuel");
			}
		}

		public void Refuel(Thing fuelThing)
		{
			this.fuel += (float)fuelThing.stackCount;
			if (this.fuel > this.Props.fuelCapacity)
			{
				this.fuel = this.Props.fuelCapacity;
			}
			fuelThing.Destroy(DestroyMode.Vanish);
			this.parent.BroadcastCompSignal("Refueled");
		}

		public void Notify_UsedThisTick()
		{
			this.ConsumeFuel(this.ConsumptionRatePerTick);
		}

		public int GetFuelCountToFullyRefuel()
		{
			float f = this.Props.fuelCapacity - this.fuel;
			return Mathf.Max(Mathf.CeilToInt(f), 1);
		}

		[DebuggerHidden]
		public override IEnumerable<Command> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "Debug: Set fuel to 0.1",
					action = delegate
					{
						this.<>f__this.fuel = 0.1f;
					}
				};
			}
		}
	}
}
