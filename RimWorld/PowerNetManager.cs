using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PowerNetManager
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

		private static List<PowerNet> allNets = new List<PowerNet>();

		private static List<PowerNetManager.DelayedAction> delayedActions = new List<PowerNetManager.DelayedAction>();

		public static void Reinit()
		{
			PowerNetManager.allNets.Clear();
			PowerNetManager.delayedActions.Clear();
		}

		public static void Notify_TransmitterSpawned(CompPower newTransmitter)
		{
			PowerNetManager.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.RegisterTransmitter, newTransmitter));
			PowerNetManager.NotifyDrawersForWireUpdate(newTransmitter.parent.Position);
		}

		public static void Notify_TransmitterDespawned(CompPower oldTransmitter)
		{
			PowerNetManager.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.DeregisterTransmitter, oldTransmitter));
			PowerNetManager.NotifyDrawersForWireUpdate(oldTransmitter.parent.Position);
		}

		public static void Notfiy_TransmitterTransmitsPowerNowChanged(CompPower transmitter)
		{
			if (!transmitter.parent.Spawned)
			{
				return;
			}
			PowerNetManager.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.DeregisterTransmitter, transmitter));
			PowerNetManager.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.RegisterTransmitter, transmitter));
			PowerNetManager.NotifyDrawersForWireUpdate(transmitter.parent.Position);
		}

		public static void Notify_ConnectorWantsConnect(CompPower wantingCon)
		{
			if (Scribe.mode == LoadSaveMode.Inactive && !PowerNetManager.HasRegisterConnectorDuplicate(wantingCon))
			{
				PowerNetManager.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.RegisterConnector, wantingCon));
			}
			PowerNetManager.NotifyDrawersForWireUpdate(wantingCon.parent.Position);
		}

		public static void Notify_ConnectorDespawned(CompPower oldCon)
		{
			PowerNetManager.delayedActions.Add(new PowerNetManager.DelayedAction(PowerNetManager.DelayedActionType.DeregisterConnector, oldCon));
			PowerNetManager.NotifyDrawersForWireUpdate(oldCon.parent.Position);
		}

		public static void NotifyDrawersForWireUpdate(IntVec3 root)
		{
			Find.MapDrawer.MapMeshDirty(root, MapMeshFlag.Things, true, false);
			Find.MapDrawer.MapMeshDirty(root, MapMeshFlag.PowerGrid, true, false);
		}

		public static void RegisterPowerNet(PowerNet newNet)
		{
			PowerNetManager.allNets.Add(newNet);
			PowerNetGrid.Notify_PowerNetCreated(newNet);
			PowerNetMaker.UpdateVisualLinkagesFor(newNet);
		}

		public static void DeletePowerNet(PowerNet oldNet)
		{
			PowerNetManager.allNets.Remove(oldNet);
			PowerNetGrid.Notify_PowerNetDeleted(oldNet);
		}

		public static void PowerNetsTick()
		{
			for (int i = 0; i < PowerNetManager.allNets.Count; i++)
			{
				PowerNetManager.allNets[i].PowerNetTick();
			}
		}

		public static void UpdatePowerNetsAndConnections_First()
		{
			int count = PowerNetManager.delayedActions.Count;
			for (int i = 0; i < count; i++)
			{
				PowerNetManager.DelayedAction delayedAction = PowerNetManager.delayedActions[i];
				PowerNetManager.DelayedActionType type = PowerNetManager.delayedActions[i].type;
				if (type != PowerNetManager.DelayedActionType.RegisterTransmitter)
				{
					if (type == PowerNetManager.DelayedActionType.DeregisterTransmitter)
					{
						PowerNetManager.TryDestroyNetAt(delayedAction.compPower.parent.Position);
						PowerConnectionMaker.DisconnectAllFromTransmitterAndSetWantConnect(delayedAction.compPower);
						delayedAction.compPower.ResetPowerVars();
					}
				}
				else
				{
					ThingWithComps parent = delayedAction.compPower.parent;
					if (PowerNetGrid.TransmittedPowerNetAt(parent.Position) != null)
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
						PowerNetManager.TryDestroyNetAt(current);
					}
				}
			}
			for (int j = 0; j < count; j++)
			{
				PowerNetManager.DelayedAction delayedAction2 = PowerNetManager.delayedActions[j];
				if (delayedAction2.type == PowerNetManager.DelayedActionType.RegisterTransmitter || delayedAction2.type == PowerNetManager.DelayedActionType.DeregisterTransmitter)
				{
					PowerNetManager.TryCreateNetAt(delayedAction2.compPower.parent.Position);
					foreach (IntVec3 current2 in GenAdj.CellsAdjacentCardinal(delayedAction2.compPower.parent))
					{
						PowerNetManager.TryCreateNetAt(current2);
					}
				}
			}
			for (int k = 0; k < count; k++)
			{
				PowerNetManager.DelayedAction delayedAction3 = PowerNetManager.delayedActions[k];
				PowerNetManager.DelayedActionType type = PowerNetManager.delayedActions[k].type;
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
			PowerNetManager.delayedActions.RemoveRange(0, count);
			if (DebugViewSettings.drawPower)
			{
				PowerNetManager.DrawDebugPowerNets();
			}
		}

		private static bool HasRegisterConnectorDuplicate(CompPower compPower)
		{
			for (int i = PowerNetManager.delayedActions.Count - 1; i >= 0; i--)
			{
				if (PowerNetManager.delayedActions[i].compPower == compPower)
				{
					if (PowerNetManager.delayedActions[i].type == PowerNetManager.DelayedActionType.DeregisterConnector)
					{
						return false;
					}
					if (PowerNetManager.delayedActions[i].type == PowerNetManager.DelayedActionType.RegisterConnector)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static void TryCreateNetAt(IntVec3 cell)
		{
			if (!cell.InBounds())
			{
				return;
			}
			if (PowerNetGrid.TransmittedPowerNetAt(cell) == null)
			{
				Building transmitter = cell.GetTransmitter();
				if (transmitter != null && transmitter.TransmitsPowerNow)
				{
					PowerNet powerNet = PowerNetMaker.NewPowerNetStartingFrom(transmitter);
					PowerNetManager.RegisterPowerNet(powerNet);
					for (int i = 0; i < powerNet.transmitters.Count; i++)
					{
						PowerConnectionMaker.ConnectAllConnectorsToTransmitter(powerNet.transmitters[i]);
					}
				}
			}
		}

		private static void TryDestroyNetAt(IntVec3 cell)
		{
			if (!cell.InBounds())
			{
				return;
			}
			PowerNet powerNet = PowerNetGrid.TransmittedPowerNetAt(cell);
			if (powerNet != null)
			{
				PowerNetManager.DeletePowerNet(powerNet);
			}
		}

		private static void DrawDebugPowerNets()
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			int num = 0;
			foreach (PowerNet current in PowerNetManager.allNets)
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
