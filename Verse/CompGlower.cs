using RimWorld;
using System;

namespace Verse
{
	public class CompGlower : ThingComp
	{
		private bool glowOnInt;

		public CompProperties_Glower Props
		{
			get
			{
				return (CompProperties_Glower)this.props;
			}
		}

		private bool ShouldBeLitNow
		{
			get
			{
				if (!this.parent.Spawned)
				{
					return false;
				}
				CompPowerTrader compPowerTrader = this.parent.TryGetComp<CompPowerTrader>();
				if (compPowerTrader != null && !compPowerTrader.PowerOn)
				{
					return false;
				}
				CompRefuelable compRefuelable = this.parent.TryGetComp<CompRefuelable>();
				if (compRefuelable != null && !compRefuelable.HasFuel)
				{
					return false;
				}
				CompFlickable compFlickable = this.parent.TryGetComp<CompFlickable>();
				return compFlickable == null || compFlickable.SwitchIsOn;
			}
		}

		public void UpdateLit(Map map)
		{
			bool shouldBeLitNow = this.ShouldBeLitNow;
			if (this.glowOnInt == shouldBeLitNow)
			{
				return;
			}
			this.glowOnInt = shouldBeLitNow;
			if (!this.glowOnInt)
			{
				map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Things);
				map.glowGrid.DeRegisterGlower(this);
			}
			else
			{
				map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Things);
				map.glowGrid.RegisterGlower(this);
			}
		}

		public override void PostSpawnSetup()
		{
			if (this.ShouldBeLitNow)
			{
				this.UpdateLit(this.parent.Map);
				this.parent.Map.glowGrid.RegisterGlower(this);
			}
			else
			{
				this.UpdateLit(this.parent.Map);
			}
		}

		public override void ReceiveCompSignal(string signal)
		{
			if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff" || signal == "FlickedOn" || signal == "FlickedOff" || signal == "Refueled" || signal == "RanOutOfFuel")
			{
				this.UpdateLit(this.parent.Map);
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.glowOnInt, "glowOn", false, false);
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			this.UpdateLit(map);
		}
	}
}
