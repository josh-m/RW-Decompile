using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public static class PowerConnectionMaker
	{
		private const int ConnectMaxDist = 6;

		public static void ConnectAllConnectorsToTransmitter(CompPower newTransmitter)
		{
			foreach (CompPower current in PowerConnectionMaker.PotentialConnectorsForTransmitter(newTransmitter))
			{
				if (current.connectParent == null)
				{
					current.ConnectToTransmitter(newTransmitter);
				}
			}
		}

		public static void DisconnectAllFromTransmitterAndSetWantConnect(CompPower deadPc)
		{
			if (deadPc.connectChildren == null)
			{
				return;
			}
			for (int i = 0; i < deadPc.connectChildren.Count; i++)
			{
				CompPower compPower = deadPc.connectChildren[i];
				compPower.connectParent = null;
				CompPowerTrader compPowerTrader = compPower as CompPowerTrader;
				if (compPowerTrader != null)
				{
					compPowerTrader.PowerOn = false;
				}
				PowerNetManager.Notify_ConnectorWantsConnect(compPower);
			}
		}

		public static void TryConnectToAnyPowerNet(CompPower pc, List<PowerNet> disallowedNets = null)
		{
			if (pc.connectParent != null)
			{
				return;
			}
			CompPower compPower = PowerConnectionMaker.BestTransmitterForConnector(pc.parent.Position, disallowedNets);
			if (compPower != null)
			{
				pc.ConnectToTransmitter(compPower);
			}
			else
			{
				pc.connectParent = null;
			}
		}

		public static void DisconnectFromPowerNet(CompPower pc)
		{
			if (pc.connectParent == null)
			{
				return;
			}
			if (pc.PowerNet != null)
			{
				pc.PowerNet.DeregisterConnector(pc);
			}
			if (pc.connectParent.connectChildren != null)
			{
				pc.connectParent.connectChildren.Remove(pc);
				if (pc.connectParent.connectChildren.Count == 0)
				{
					pc.connectParent.connectChildren = null;
				}
			}
		}

		[DebuggerHidden]
		private static IEnumerable<CompPower> PotentialConnectorsForTransmitter(CompPower b)
		{
			CellRect rect = b.parent.OccupiedRect().ExpandedBy(6);
			for (int z = rect.minZ; z <= rect.maxZ; z++)
			{
				for (int x = rect.minX; x <= rect.maxX; x++)
				{
					IntVec3 c = new IntVec3(x, 0, z);
					List<Thing> thingList = Find.ThingGrid.ThingsListAt(c);
					for (int i = 0; i < thingList.Count; i++)
					{
						if (thingList[i].def.ConnectToPower)
						{
							yield return ((Building)thingList[i]).PowerComp;
						}
					}
				}
			}
		}

		public static CompPower BestTransmitterForConnector(IntVec3 connectorPos, List<PowerNet> disallowedNets = null)
		{
			CellRect cellRect = CellRect.SingleCell(connectorPos).ExpandedBy(6);
			cellRect.ClipInsideMap();
			float num = 999999f;
			CompPower result = null;
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					Building transmitter = c.GetTransmitter();
					if (transmitter != null && !transmitter.Destroyed)
					{
						CompPower powerComp = transmitter.PowerComp;
						if (powerComp != null && powerComp.TransmitsPowerNow && (transmitter.def.building == null || transmitter.def.building.allowWireConnection))
						{
							if (disallowedNets == null || !disallowedNets.Contains(powerComp.transNet))
							{
								float lengthHorizontalSquared = (transmitter.Position - connectorPos).LengthHorizontalSquared;
								if (lengthHorizontalSquared < num)
								{
									num = lengthHorizontalSquared;
									result = powerComp;
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
}
