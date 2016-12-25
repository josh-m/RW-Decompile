using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Command_LoadToTransporter : Command
	{
		public CompTransporter transComp;

		private List<CompTransporter> transporters;

		private static HashSet<Building> tmpFuelingPortGivers = new HashSet<Building>();

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			if (this.transporters == null)
			{
				this.transporters = new List<CompTransporter>();
			}
			if (!this.transporters.Contains(this.transComp))
			{
				this.transporters.Add(this.transComp);
			}
			CompLaunchable launchable = this.transComp.Launchable;
			if (launchable != null)
			{
				Building fuelingPortSource = launchable.FuelingPortSource;
				if (fuelingPortSource != null)
				{
					Map map = this.transComp.Map;
					Command_LoadToTransporter.tmpFuelingPortGivers.Clear();
					map.floodFiller.FloodFill(fuelingPortSource.Position, (IntVec3 x) => FuelingPortUtility.AnyFuelingPortGiverAt(x, map), delegate(IntVec3 x)
					{
						Command_LoadToTransporter.tmpFuelingPortGivers.Add(FuelingPortUtility.FuelingPortGiverAt(x, map));
					});
					for (int i = 0; i < this.transporters.Count; i++)
					{
						Building fuelingPortSource2 = this.transporters[i].Launchable.FuelingPortSource;
						if (fuelingPortSource2 != null && !Command_LoadToTransporter.tmpFuelingPortGivers.Contains(fuelingPortSource2))
						{
							Messages.Message("MessageTransportersNotAdjacent".Translate(), fuelingPortSource2, MessageSound.RejectInput);
							return;
						}
					}
				}
			}
			for (int j = 0; j < this.transporters.Count; j++)
			{
				if (this.transporters[j] != this.transComp)
				{
					if (!this.transComp.Map.reachability.CanReach(this.transComp.parent.Position, this.transporters[j].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
					{
						Messages.Message("MessageTransporterUnreachable".Translate(), this.transporters[j].parent, MessageSound.RejectInput);
						return;
					}
				}
			}
			Find.WindowStack.Add(new Dialog_LoadTransporters(this.transComp.Map, this.transporters));
		}

		public override bool InheritInteractionsFrom(Gizmo other)
		{
			Command_LoadToTransporter command_LoadToTransporter = (Command_LoadToTransporter)other;
			if (command_LoadToTransporter.transComp.parent.def != this.transComp.parent.def)
			{
				return false;
			}
			if (this.transporters == null)
			{
				this.transporters = new List<CompTransporter>();
			}
			this.transporters.Add(command_LoadToTransporter.transComp);
			return false;
		}
	}
}
