using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PowerNet
	{
		private const int MaxRestartTryInterval = 200;

		private const int MinRestartTryInterval = 30;

		private const int ShutdownInterval = 20;

		private const float MinStoredEnergyToTurnOn = 5f;

		public PowerNetManager powerNetManager;

		public bool hasPowerSource;

		public List<CompPower> connectors = new List<CompPower>();

		public List<CompPower> transmitters = new List<CompPower>();

		public List<CompPowerTrader> powerComps = new List<CompPowerTrader>();

		public List<CompPowerBattery> batteryComps = new List<CompPowerBattery>();

		private float debugLastCreatedEnergy;

		private float debugLastRawStoredEnergy;

		private float debugLastApparentStoredEnergy;

		private static List<CompPowerTrader> partsWantingPowerOn = new List<CompPowerTrader>();

		private static List<CompPowerTrader> potentialShutdownParts = new List<CompPowerTrader>();

		private List<CompPowerBattery> givingBats = new List<CompPowerBattery>();

		private List<CompPowerBattery> batteryCompsShuffled = new List<CompPowerBattery>();

		public PowerNet(IEnumerable<CompPower> newTransmitters)
		{
			foreach (CompPower current in newTransmitters)
			{
				this.transmitters.Add(current);
				current.transNet = this;
				this.RegisterAllComponentsOf(current.parent);
				if (current.connectChildren != null)
				{
					List<CompPower> connectChildren = current.connectChildren;
					for (int i = 0; i < connectChildren.Count; i++)
					{
						this.RegisterConnector(connectChildren[i]);
					}
				}
			}
			this.hasPowerSource = false;
			for (int j = 0; j < this.transmitters.Count; j++)
			{
				if (this.IsPowerSource(this.transmitters[j]))
				{
					this.hasPowerSource = true;
					break;
				}
			}
		}

		private bool IsPowerSource(CompPower cp)
		{
			return cp is CompPowerBattery || (cp is CompPowerTrader && cp.Props.basePowerConsumption < 0f);
		}

		public void RegisterConnector(CompPower b)
		{
			if (this.connectors.Contains(b))
			{
				Log.Error("PowerNet registered connector it already had: " + b);
				return;
			}
			this.connectors.Add(b);
			this.RegisterAllComponentsOf(b.parent);
		}

		public void DeregisterConnector(CompPower b)
		{
			this.connectors.Remove(b);
			this.DeregisterAllComponentsOf(b.parent);
		}

		private void RegisterAllComponentsOf(ThingWithComps parentThing)
		{
			CompPowerTrader comp = parentThing.GetComp<CompPowerTrader>();
			if (comp != null)
			{
				if (this.powerComps.Contains(comp))
				{
					Log.Error("PowerNet adding powerComp " + comp + " which it already has.");
				}
				else
				{
					this.powerComps.Add(comp);
				}
			}
			CompPowerBattery comp2 = parentThing.GetComp<CompPowerBattery>();
			if (comp2 != null)
			{
				if (this.batteryComps.Contains(comp2))
				{
					Log.Error("PowerNet adding batteryComp " + comp2 + " which it already has.");
				}
				else
				{
					this.batteryComps.Add(comp2);
				}
			}
		}

		private void DeregisterAllComponentsOf(ThingWithComps parentThing)
		{
			CompPowerTrader comp = parentThing.GetComp<CompPowerTrader>();
			if (comp != null)
			{
				this.powerComps.Remove(comp);
			}
			CompPowerBattery comp2 = parentThing.GetComp<CompPowerBattery>();
			if (comp2 != null)
			{
				this.batteryComps.Remove(comp2);
			}
		}

		public float CurrentEnergyGainRate()
		{
			if (DebugSettings.unlimitedPower)
			{
				return 100000f;
			}
			float num = 0f;
			for (int i = 0; i < this.powerComps.Count; i++)
			{
				if (this.powerComps[i].PowerOn)
				{
					num += this.powerComps[i].EnergyOutputPerTick;
				}
			}
			return num;
		}

		public float CurrentStoredEnergy()
		{
			float num = 0f;
			for (int i = 0; i < this.batteryComps.Count; i++)
			{
				num += this.batteryComps[i].StoredEnergy;
			}
			return num;
		}

		public void PowerNetTick()
		{
			float num = this.CurrentEnergyGainRate();
			float num2 = this.CurrentStoredEnergy();
			if (num2 + num >= -1E-07f && !this.powerNetManager.map.mapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare))
			{
				float num3;
				if (this.batteryComps.Count > 0 && num2 >= 0.1f)
				{
					num3 = num2 - 5f;
				}
				else
				{
					num3 = num2;
				}
				if (UnityData.isDebugBuild)
				{
					this.debugLastApparentStoredEnergy = num3;
					this.debugLastCreatedEnergy = num;
					this.debugLastRawStoredEnergy = num2;
				}
				if (num3 + num >= 0f)
				{
					PowerNet.partsWantingPowerOn.Clear();
					for (int i = 0; i < this.powerComps.Count; i++)
					{
						if (!this.powerComps[i].PowerOn && this.powerComps[i].DesirePowerOn && !this.powerComps[i].parent.IsBrokenDown())
						{
							PowerNet.partsWantingPowerOn.Add(this.powerComps[i]);
						}
					}
					if (PowerNet.partsWantingPowerOn.Count > 0)
					{
						int num4 = 200 / PowerNet.partsWantingPowerOn.Count;
						if (num4 < 30)
						{
							num4 = 30;
						}
						if (Find.TickManager.TicksGame % num4 == 0)
						{
							CompPowerTrader compPowerTrader = PowerNet.partsWantingPowerOn.RandomElement<CompPowerTrader>();
							if (num + num2 >= -(compPowerTrader.EnergyOutputPerTick + 1E-07f))
							{
								compPowerTrader.PowerOn = true;
								num += compPowerTrader.EnergyOutputPerTick;
							}
						}
					}
				}
				this.ChangeStoredEnergy(num);
			}
			else if (Find.TickManager.TicksGame % 20 == 0)
			{
				PowerNet.potentialShutdownParts.Clear();
				for (int j = 0; j < this.powerComps.Count; j++)
				{
					if (this.powerComps[j].PowerOn && this.powerComps[j].EnergyOutputPerTick < 0f)
					{
						PowerNet.potentialShutdownParts.Add(this.powerComps[j]);
					}
				}
				if (PowerNet.potentialShutdownParts.Count > 0)
				{
					PowerNet.potentialShutdownParts.RandomElement<CompPowerTrader>().PowerOn = false;
				}
			}
		}

		private void ChangeStoredEnergy(float extra)
		{
			if (extra > 0f)
			{
				float val = extra / (float)this.batteryComps.Count;
				this.batteryCompsShuffled.Clear();
				for (int i = 0; i < this.batteryComps.Count; i++)
				{
					this.batteryCompsShuffled.Add(this.batteryComps[i]);
				}
				this.batteryCompsShuffled.Shuffle<CompPowerBattery>();
				for (int j = 0; j < this.batteryCompsShuffled.Count; j++)
				{
					CompPowerBattery compPowerBattery = this.batteryCompsShuffled[j];
					float num = Math.Min(val, compPowerBattery.AmountCanAccept);
					compPowerBattery.AddEnergy(num);
					extra -= num;
					if (extra < 0.01f)
					{
						return;
					}
				}
			}
			else
			{
				float num2 = -extra;
				this.givingBats.Clear();
				for (int k = 0; k < this.batteryComps.Count; k++)
				{
					if (this.batteryComps[k].StoredEnergy > 1E-07f)
					{
						this.givingBats.Add(this.batteryComps[k]);
					}
				}
				float a = num2 / (float)this.givingBats.Count;
				int num3 = 0;
				while (num2 > 1E-07f)
				{
					for (int l = 0; l < this.givingBats.Count; l++)
					{
						float num4 = Mathf.Min(a, this.givingBats[l].StoredEnergy);
						this.givingBats[l].DrawPower(num4);
						num2 -= num4;
						if (num2 < 1E-07f)
						{
							return;
						}
					}
					num3++;
					if (num3 > 10)
					{
						break;
					}
				}
				if (num2 > 1E-07f)
				{
					Log.Warning("Drew energy from a PowerNet that didn't have it.");
				}
			}
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("POWERNET:");
			stringBuilder.AppendLine("  Created energy: " + this.debugLastCreatedEnergy);
			stringBuilder.AppendLine("  Raw stored energy: " + this.debugLastRawStoredEnergy);
			stringBuilder.AppendLine("  Apparent stored energy: " + this.debugLastApparentStoredEnergy);
			stringBuilder.AppendLine("  hasPowerSource: " + this.hasPowerSource);
			stringBuilder.AppendLine("  Connectors: ");
			foreach (CompPower current in this.connectors)
			{
				stringBuilder.AppendLine("      " + current.parent);
			}
			stringBuilder.AppendLine("  Transmitters: ");
			foreach (CompPower current2 in this.transmitters)
			{
				stringBuilder.AppendLine("      " + current2.parent);
			}
			stringBuilder.AppendLine("  powerComps: ");
			foreach (CompPowerTrader current3 in this.powerComps)
			{
				stringBuilder.AppendLine("      " + current3.parent);
			}
			stringBuilder.AppendLine("  batteryComps: ");
			foreach (CompPowerBattery current4 in this.batteryComps)
			{
				stringBuilder.AppendLine("      " + current4.parent);
			}
			return stringBuilder.ToString();
		}
	}
}
