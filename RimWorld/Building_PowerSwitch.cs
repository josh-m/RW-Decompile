using System;
using System.Text;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Building_PowerSwitch : Building
	{
		private bool switchOnOld = true;

		private CompFlickable flickableComp;

		public static readonly Graphic GraphicOn = GraphicDatabase.Get<Graphic_Single>("Things/Building/Power/PowerSwitch_On");

		public static readonly Graphic GraphicOff = GraphicDatabase.Get<Graphic_Single>("Things/Building/Power/PowerSwitch_Off");

		public override bool TransmitsPowerNow
		{
			get
			{
				return this.flickableComp.SwitchIsOn;
			}
		}

		public override Graphic Graphic
		{
			get
			{
				if (this.flickableComp.SwitchIsOn)
				{
					return Building_PowerSwitch.GraphicOn;
				}
				return Building_PowerSwitch.GraphicOff;
			}
		}

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			this.flickableComp = base.GetComp<CompFlickable>();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.flickableComp == null)
				{
					this.flickableComp = base.GetComp<CompFlickable>();
				}
				this.switchOnOld = !this.flickableComp.SwitchIsOn;
				this.UpdatePowerGrid();
			}
		}

		protected override void ReceiveCompSignal(string signal)
		{
			if (signal == "FlickedOff" || signal == "FlickedOn")
			{
				this.UpdatePowerGrid();
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.AppendLine();
			stringBuilder.Append("PowerSwitch_Power".Translate() + ": ");
			if (this.flickableComp.SwitchIsOn)
			{
				stringBuilder.Append("On".Translate().ToLower());
			}
			else
			{
				stringBuilder.Append("Off".Translate().ToLower());
			}
			return stringBuilder.ToString();
		}

		private void UpdatePowerGrid()
		{
			if (this.flickableComp.SwitchIsOn != this.switchOnOld)
			{
				if (base.Spawned)
				{
					base.Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(base.PowerComp);
				}
				this.switchOnOld = this.flickableComp.SwitchIsOn;
			}
		}
	}
}
