using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_Cooler : Building_TempControl
	{
		private const float HeatOutputMultiplier = 1.25f;

		private const float EfficiencyLossPerDegreeDifference = 0.0076923077f;

		public override void TickRare()
		{
			if (this.compPowerTrader.PowerOn)
			{
				IntVec3 intVec = base.Position + IntVec3.South.RotatedBy(base.Rotation);
				IntVec3 intVec2 = base.Position + IntVec3.North.RotatedBy(base.Rotation);
				bool flag = false;
				if (!intVec2.Impassable(base.Map) && !intVec.Impassable(base.Map))
				{
					float temperature = intVec2.GetTemperature(base.Map);
					float temperature2 = intVec.GetTemperature(base.Map);
					float num = temperature - temperature2;
					if (temperature - 40f > num)
					{
						num = temperature - 40f;
					}
					float num2 = 1f - num * 0.0076923077f;
					if (num2 < 0f)
					{
						num2 = 0f;
					}
					float num3 = this.compTempControl.Props.energyPerSecond * num2 * 4.16666651f;
					float num4 = GenTemperature.ControlTemperatureTempChange(intVec, base.Map, num3, this.compTempControl.targetTemperature);
					flag = !Mathf.Approximately(num4, 0f);
					if (flag)
					{
						intVec.GetRoom(base.Map).Temperature += num4;
						GenTemperature.PushHeat(intVec2, base.Map, -num3 * 1.25f);
					}
				}
				CompProperties_Power props = this.compPowerTrader.Props;
				if (flag)
				{
					this.compPowerTrader.PowerOutput = -props.basePowerConsumption;
				}
				else
				{
					this.compPowerTrader.PowerOutput = -props.basePowerConsumption * this.compTempControl.Props.lowPowerConsumptionFactor;
				}
				this.compTempControl.operatingAtHighPower = flag;
			}
		}
	}
}
