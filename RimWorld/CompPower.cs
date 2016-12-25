using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class CompPower : ThingComp
	{
		public PowerNet transNet;

		public CompPower connectParent;

		public List<CompPower> connectChildren;

		private static List<PowerNet> recentlyConnectedNets = new List<PowerNet>();

		private static CompPower lastManualReconnector = null;

		public static readonly float WattsToWattDaysPerTick = 1.66666669E-05f;

		public bool TransmitsPowerNow
		{
			get
			{
				return ((Building)this.parent).TransmitsPowerNow;
			}
		}

		public PowerNet PowerNet
		{
			get
			{
				if (this.transNet != null)
				{
					return this.transNet;
				}
				if (this.connectParent != null)
				{
					return this.connectParent.transNet;
				}
				return null;
			}
		}

		public CompProperties_Power Props
		{
			get
			{
				return (CompProperties_Power)this.props;
			}
		}

		public virtual void ResetPowerVars()
		{
			this.transNet = null;
			this.connectParent = null;
			this.connectChildren = null;
			CompPower.recentlyConnectedNets.Clear();
			CompPower.lastManualReconnector = null;
		}

		public virtual void SetUpPowerVars()
		{
		}

		public override void PostExposeData()
		{
			Thing thing = null;
			if (Scribe.mode == LoadSaveMode.Saving && this.connectParent != null)
			{
				thing = this.connectParent.parent;
			}
			Scribe_References.LookReference<Thing>(ref thing, "parentThing", false);
			if (thing != null)
			{
				this.connectParent = ((ThingWithComps)thing).GetComp<CompPower>();
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.connectParent != null)
			{
				this.ConnectToTransmitter(this.connectParent);
			}
		}

		public override void PostSpawnSetup()
		{
			base.PostSpawnSetup();
			if (this.Props.transmitsPower || this.parent.def.ConnectToPower)
			{
				Find.MapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.PowerGrid, true, false);
				if (this.Props.transmitsPower)
				{
					PowerNetManager.Notify_TransmitterSpawned(this);
				}
				if (this.parent.def.ConnectToPower)
				{
					PowerNetManager.Notify_ConnectorWantsConnect(this);
				}
				this.SetUpPowerVars();
			}
		}

		public override void PostDeSpawn()
		{
			base.PostDeSpawn();
			if (this.Props.transmitsPower || this.parent.def.ConnectToPower)
			{
				if (this.Props.transmitsPower)
				{
					if (this.connectChildren != null)
					{
						for (int i = 0; i < this.connectChildren.Count; i++)
						{
							this.connectChildren[i].LostConnectParent();
						}
					}
					PowerNetManager.Notify_TransmitterDespawned(this);
				}
				if (this.parent.def.ConnectToPower)
				{
					PowerNetManager.Notify_ConnectorDespawned(this);
				}
				Find.MapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.PowerGrid, true, false);
			}
		}

		public virtual void LostConnectParent()
		{
			this.connectParent = null;
			PowerNetManager.Notify_ConnectorWantsConnect(this);
		}

		public override void PostPrintOnto(SectionLayer layer)
		{
			base.PostPrintOnto(layer);
			if (this.connectParent != null)
			{
				PowerNetGraphics.PrintWirePieceConnecting(layer, this.parent, this.connectParent.parent, false);
			}
		}

		public override void CompPrintForPowerGrid(SectionLayer layer)
		{
			if (this.TransmitsPowerNow)
			{
				PowerOverlayMats.LinkedOverlayGraphic.Print(layer, this.parent);
			}
			if (this.parent.def.ConnectToPower)
			{
				PowerNetGraphics.PrintOverlayConnectorBaseFor(layer, this.parent);
			}
			if (this.connectParent != null)
			{
				PowerNetGraphics.PrintWirePieceConnecting(layer, this.parent, this.connectParent.parent, true);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Command> CompGetGizmosExtra()
		{
			foreach (Command c in base.CompGetGizmosExtra())
			{
				yield return c;
			}
			if (this.connectParent != null && this.parent.Faction == Faction.OfPlayer)
			{
				yield return new Command_Action
				{
					action = delegate
					{
						SoundDefOf.TickTiny.PlayOneShotOnCamera();
						this.<>f__this.TryManualReconnect();
					},
					hotKey = KeyBindingDefOf.Misc1,
					defaultDesc = "CommandTryReconnectDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/TryReconnect", true),
					defaultLabel = "CommandTryReconnectLabel".Translate()
				};
			}
		}

		private void TryManualReconnect()
		{
			if (CompPower.lastManualReconnector != this)
			{
				CompPower.recentlyConnectedNets.Clear();
				CompPower.lastManualReconnector = this;
			}
			if (this.PowerNet != null)
			{
				CompPower.recentlyConnectedNets.Add(this.PowerNet);
			}
			CompPower compPower = PowerConnectionMaker.BestTransmitterForConnector(this.parent.Position, CompPower.recentlyConnectedNets);
			if (compPower == null)
			{
				CompPower.recentlyConnectedNets.Clear();
				compPower = PowerConnectionMaker.BestTransmitterForConnector(this.parent.Position, null);
			}
			if (compPower != null)
			{
				PowerConnectionMaker.DisconnectFromPowerNet(this);
				this.ConnectToTransmitter(compPower);
				for (int i = 0; i < 5; i++)
				{
					MoteMaker.ThrowMetaPuff(compPower.parent.Position.ToVector3Shifted());
				}
				Find.MapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.PowerGrid);
				Find.MapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Things);
			}
		}

		public void ConnectToTransmitter(CompPower transmitter)
		{
			this.connectParent = transmitter;
			if (this.connectParent.connectChildren == null)
			{
				this.connectParent.connectChildren = new List<CompPower>();
			}
			transmitter.connectChildren.Add(this);
			PowerNet powerNet = this.PowerNet;
			if (powerNet != null)
			{
				powerNet.RegisterConnector(this);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (this.PowerNet == null)
			{
				return "PowerNotConnected".Translate();
			}
			string text = (this.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick).ToString("F0");
			string text2 = this.PowerNet.CurrentStoredEnergy().ToString("F0");
			return "PowerConnectedRateStored".Translate(new object[]
			{
				text,
				text2
			});
		}
	}
}
