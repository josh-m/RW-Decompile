using System;
using Verse;

namespace RimWorld
{
	public class CompPowerBattery : CompPower
	{
		private float storedEnergy;

		public float AmountCanAccept
		{
			get
			{
				if (this.parent.IsBrokenDown())
				{
					return 0f;
				}
				CompProperties_Battery props = this.Props;
				return (props.storedEnergyMax - this.storedEnergy) / props.efficiency;
			}
		}

		public float StoredEnergy
		{
			get
			{
				return this.storedEnergy;
			}
		}

		public new CompProperties_Battery Props
		{
			get
			{
				return (CompProperties_Battery)this.props;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<float>(ref this.storedEnergy, "storedPower", 0f, false);
			CompProperties_Battery props = this.Props;
			if (this.storedEnergy > props.storedEnergyMax)
			{
				this.storedEnergy = props.storedEnergyMax;
			}
		}

		public void AddEnergy(float amount)
		{
			if (amount < 0f)
			{
				Log.Error("Cannot add negative energy " + amount);
				return;
			}
			if (amount > this.AmountCanAccept)
			{
				amount = this.AmountCanAccept;
			}
			amount *= this.Props.efficiency;
			this.storedEnergy += amount;
		}

		public void DrawPower(float amount)
		{
			this.storedEnergy -= amount;
			if (this.storedEnergy < 0f)
			{
				Log.Error("Drawing power we don't have from " + this.parent);
				this.storedEnergy = 0f;
			}
		}

		public override void ReceiveCompSignal(string signal)
		{
			if (signal == "Breakdown")
			{
				this.DrawPower(this.StoredEnergy);
			}
		}

		public override string CompInspectStringExtra()
		{
			CompProperties_Battery props = this.Props;
			string text = string.Concat(new string[]
			{
				"PowerBatteryStored".Translate(),
				": ",
				this.storedEnergy.ToString("F0"),
				" / ",
				props.storedEnergyMax.ToString("F0"),
				" Wd"
			});
			string text2 = text;
			text = string.Concat(new string[]
			{
				text2,
				"\n",
				"PowerBatteryEfficiency".Translate(),
				": ",
				(props.efficiency * 100f).ToString("F0"),
				"%"
			});
			return text + "\n" + base.CompInspectStringExtra();
		}
	}
}
