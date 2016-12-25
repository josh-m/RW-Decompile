using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PowerNetManager
	{
		private enum DelayedActionType
		{
			RegisterTransmitter,
			DeregisterTransmitter,
			RegisterConnector,
			DeregisterConnector
		}

		private struct DelayedAction
		{
			public PowerNetManager.DelayedActionType type;

			public CompPower compPower;

			public DelayedAction(PowerNetManager.DelayedActionType type, CompPower compPower)
			{
				this.type = type;
				this.compPower = compPower;
			}
		}

		public Map map;

		private List<PowerNet> allNets = new List<PowerNet>();

		private List<PowerNetManager.DelayedAction> delayedActions = new List<PowerNetManager.DelayedAction>();

		public PowerNetManager(Map map)
		{
			this.map = map;
		}

		public void Notify_TransmitterSpawned(CompPower newTransmitter)
		{
			this.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.RegisterTransmitter, newTransmitter));
			this.NotifyDrawersForWireUpdate(newTransmitter.parent.Position);
		}

		public void Notify_TransmitterDespawned(CompPower oldTransmitter)
		{
			this.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.DeregisterTransmitter, oldTransmitter));
			this.NotifyDrawersForWireUpdate(oldTransmitter.parent.Position);
		}

		public void Notfiy_TransmitterTransmitsPowerNowChanged(CompPower transmitter)
		{
			if (!transmitter.parent.Spawned)
			{
				return;
			}
			this.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.DeregisterTransmitter, transmitter));
			this.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.RegisterTransmitter, transmitter));
			this.NotifyDrawersForWireUpdate(transmitter.parent.Position);
		}

		public void Notify_ConnectorWantsConnect(CompPower wantingCon)
		{
			if (Scribe.mode == LoadSaveMode.Inactive && !this.HasRegisterConnectorDuplicate(wantingCon))
			{
				this.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.RegisterConnector, wantingCon));
			}
			this.NotifyDrawersForWireUpdate(wantingCon.parent.Position);
		}

		public void Notify_ConnectorDespawned(CompPower oldCon)
		{
			this.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.DeregisterConnector, oldCon));
			this.NotifyDrawersForWireUpdate(oldCon.parent.Position);
		}

		public void NotifyDrawersForWireUpdate(IntVec3 root)
		{
			this.map.mapDrawer.MapMeshDirty(root, MapMeshFlag.Things, true, false);
			this.map.mapDrawer.MapMeshDirty(root, MapMeshFlag.PowerGrid, true, false);
		}

		public void RegisterPowerNet(PowerNet newNet)
		{
			this.allNets.Add(newNet);
			newNet.powerNetManager = this;
			this.map.powerNetGrid.Notify_PowerNetCreated(newNet);
			PowerNetMaker.UpdateVisualLinkagesFor(newNet);
		}

		public void DeletePowerNet(PowerNet oldNet)
		{
			this.allNets.Remove(oldNet);
			this.map.powerNetGrid.Notify_PowerNetDeleted(oldNet);
		}

		public void PowerNetsTick()
		{
			for (int i = 0; i < this.allNets.Count; i++)
			{
				this.allNets[i].PowerNetTick();
			}
		}

		public void UpdatePowerNetsAndConnections_First()
		{
			int count = this.delayedActions.Count;
			for (int i = 0; i < count; i++)
			{
				PowerNetManager.DelayedAction delayedAction = this.delayedActions[i];
				PowerNetManager.DelayedActionType type = this.delayedActions[i].type;
				if (type != PowerNetManager.DelayedActionType.RegisterTransmitter)
				{
					if (type == PowerNetManager.DelayedActionType.DeregisterTransmitter)
					{
						this.TryDestroyNetAt(delayedAction.compPower.parent.Position);
						PowerConnectionMaker.DisconnectAllFromTransmitterAndSetWantConnect(delayedAction.compPower, this.map);
						delayedAction.compPower.ResetPowerVars();
					}
				}
				else
				{
					ThingWithComps parent = delayedAction.compPower.parent;
					if (this.map.powerNetGrid.TransmittedPowerNetAt(parent.Position) != null)
					{
						Log.Warning(string.Concat(new object[]
						{
							"Tried to register trasmitter ",
							parent,
							" at ",
							parent.Position,
							", but there is already a power net here. There can't be two transmitters on the same cell."
						}));
					}
					delayedAction.compPower.SetUpPowerVars();
					foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(parent))
					{
						this.TryDestroyNetAt(current);
					}
				}
			}
			for (int j = 0; j < count; j++)
			{
				PowerNetManager.DelayedAction delayedAction2 = this.delayedActions[j];
				if (delayedAction2.type == PowerNetManager.DelayedActionType.RegisterTransmitter || delayedAction2.type == PowerNetManager.DelayedActionType.DeregisterTransmitter)
				{
					this.TryCreateNetAt(delayedAction2.compPower.parent.Position);
					foreach (IntVec3 current2 in GenAdj.CellsAdjacentCardinal(delayedAction2.compPower.parent))
					{
						this.TryCreateNetAt(current2);
					}
				}
			}
			for (int k = 0; k < count; k++)
			{
				PowerNetManager.DelayedAction delayedAction3 = this.delayedActions[k];
				PowerNetManager.DelayedActionType type = this.delayedActions[k].type;
				if (type != PowerNetManager.DelayedActionType.RegisterConnector)
				{
					if (type == PowerNetManager.DelayedActionType.DeregisterConnector)
					{
						PowerConnectionMaker.DisconnectFromPowerNet(delayedAction3.compPower);
						delayedAction3.compPower.ResetPowerVars();
					}
				}
				else
				{
					delayedAction3.compPower.SetUpPowerVars();
					PowerConnectionMaker.TryConnectToAnyPowerNet(delayedAction3.compPower, null);
				}
			}
			this.delayedActions.RemoveRange(0, count);
			if (DebugViewSettings.drawPower)
			{
				this.DrawDebugPowerNets();
			}
		}

		private bool HasRegisterConnectorDuplicate(CompPower compPower)
		{
			for (int i = this.delayedActions.Count - 1; i >= 0; i--)
			{
				if (this.delayedActions[i].compPower == compPower)
				{
					if (this.delayedActions[i].type == PowerNetManager.DelayedActionType.DeregisterConnector)
					{
						return false;
					}
					if (this.delayedActions[i].type == PowerNetManager.DelayedActionType.RegisterConnector)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void TryCreateNetAt(IntVec3 cell)
		{
			if (!cell.InBounds(this.map))
			{
				return;
			}
			if (this.map.powerNetGrid.TransmittedPowerNetAt(cell) == null)
			{
				Building transmitter = cell.GetTransmitter(this.map);
				if (transmitter != null && transmitter.TransmitsPowerNow)
				{
					PowerNet powerNet = PowerNetMaker.NewPowerNetStartingFrom(transmitter);
					this.RegisterPowerNet(powerNet);
					for (int i = 0; i < powerNet.transmitters.Count; i++)
					{
						PowerConnectionMaker.ConnectAllConnectorsToTransmitter(powerNet.transmitters[i]);
					}
				}
			}
		}

		private void TryDestroyNetAt(IntVec3 cell)
		{
			if (!cell.InBounds(this.map))
			{
				return;
			}
			PowerNet powerNet = this.map.powerNetGrid.TransmittedPowerNetAt(cell);
			if (powerNet != null)
			{
				this.DeletePowerNet(powerNet);
			}
		}

		private void DrawDebugPowerNets()
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			int num = 0;
			foreach (PowerNet current in this.allNets)
			{
				foreach (CompPower current2 in current.transmitters.Concat(current.connectors))
				{
					foreach (IntVec3 current3 in GenAdj.CellsOccupiedBy(current2.parent))
					{
						CellRenderer.RenderCell(current3, (float)num * 0.44f);
					}
				}
				num++;
			}
		}
	}
}
