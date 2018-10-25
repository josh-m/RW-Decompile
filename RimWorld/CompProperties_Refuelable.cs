using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_Refuelable : CompProperties
	{
		public float fuelConsumptionRate = 1f;

		public float fuelCapacity = 2f;

		public float initialFuelPercent;

		public float autoRefuelPercent = 0.3f;

		public float fuelConsumptionPerTickInRain;

		public ThingFilter fuelFilter;

		public bool destroyOnNoFuel;

		public bool consumeFuelOnlyWhenUsed;

		public bool showFuelGizmo;

		public bool targetFuelLevelConfigurable;

		public float initialConfigurableTargetFuelLevel;

		public bool drawOutOfFuelOverlay = true;

		public float minimumFueledThreshold;

		public bool drawFuelGaugeInMap;

		public bool atomicFueling;

		private float fuelMultiplier = 1f;

		public bool factorByDifficulty;

		public string fuelLabel;

		public string fuelGizmoLabel;

		public string outOfFuelMessage;

		public string fuelIconPath;

		private Texture2D fuelIcon;

		public string FuelLabel
		{
			get
			{
				return this.fuelLabel.NullOrEmpty() ? "Fuel".Translate() : this.fuelLabel;
			}
		}

		public string FuelGizmoLabel
		{
			get
			{
				return this.fuelGizmoLabel.NullOrEmpty() ? "Fuel".Translate() : this.fuelGizmoLabel;
			}
		}

		public Texture2D FuelIcon
		{
			get
			{
				if (this.fuelIcon == null)
				{
					if (!this.fuelIconPath.NullOrEmpty())
					{
						this.fuelIcon = ContentFinder<Texture2D>.Get(this.fuelIconPath, true);
					}
					else
					{
						ThingDef thingDef;
						if (this.fuelFilter.AnyAllowedDef != null)
						{
							thingDef = this.fuelFilter.AnyAllowedDef;
						}
						else
						{
							thingDef = ThingDefOf.Chemfuel;
						}
						this.fuelIcon = thingDef.uiIcon;
					}
				}
				return this.fuelIcon;
			}
		}

		public float FuelMultiplierCurrentDifficulty
		{
			get
			{
				if (this.factorByDifficulty)
				{
					return this.fuelMultiplier / Find.Storyteller.difficulty.maintenanceCostFactor;
				}
				return this.fuelMultiplier;
			}
		}

		public CompProperties_Refuelable()
		{
			this.compClass = typeof(CompRefuelable);
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			this.fuelFilter.ResolveReferences();
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string err in base.ConfigErrors(parentDef))
			{
				yield return err;
			}
			if (this.destroyOnNoFuel && this.initialFuelPercent <= 0f)
			{
				yield return "Refuelable component has destroyOnNoFuel, but initialFuelPercent <= 0";
			}
			if ((!this.consumeFuelOnlyWhenUsed || this.fuelConsumptionPerTickInRain > 0f) && parentDef.tickerType != TickerType.Normal)
			{
				yield return string.Format("Refuelable component set to consume fuel per tick, but parent tickertype is {0} instead of {1}", parentDef.tickerType, TickerType.Normal);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			foreach (StatDrawEntry s in base.SpecialDisplayStats(req))
			{
				yield return s;
			}
			if (((ThingDef)req.Def).building.IsTurret)
			{
				StatCategoryDef building = StatCategoryDefOf.Building;
				string text = "RearmCost".Translate();
				string valueString = GenLabel.ThingLabel(this.fuelFilter.AnyAllowedDef, null, (int)(this.fuelCapacity / this.FuelMultiplierCurrentDifficulty)).CapitalizeFirst();
				string text2 = "RearmCostExplanation".Translate();
				yield return new StatDrawEntry(building, text, valueString, 0, text2);
				building = StatCategoryDefOf.Building;
				text2 = "ShotsBeforeRearm".Translate();
				valueString = ((int)this.fuelCapacity).ToString();
				text = "ShotsBeforeRearmExplanation".Translate();
				yield return new StatDrawEntry(building, text2, valueString, 0, text);
			}
		}
	}
}
