using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using Verse.AI.Group;

namespace Verse
{
	public class CompressibilityDecider
	{
		private Map map;

		private HashSet<Thing> referencedThings = new HashSet<Thing>();

		public CompressibilityDecider(Map map)
		{
			this.map = map;
		}

		public void DetermineReferences()
		{
			this.referencedThings.Clear();
			foreach (Thing current in from des in this.map.designationManager.allDesignations
			select des.target.Thing)
			{
				this.referencedThings.Add(current);
			}
			foreach (Thing current2 in this.map.reservationManager.AllReservedThings())
			{
				this.referencedThings.Add(current2);
			}
			List<Pawn> allPawnsSpawned = this.map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Pawn pawn = allPawnsSpawned[i];
				Job curJob = pawn.jobs.curJob;
				if (curJob != null)
				{
					if (curJob.targetA.HasThing)
					{
						this.referencedThings.Add(curJob.targetA.Thing);
					}
					if (curJob.targetB.HasThing)
					{
						this.referencedThings.Add(curJob.targetB.Thing);
					}
					if (curJob.targetC.HasThing)
					{
						this.referencedThings.Add(curJob.targetC.Thing);
					}
				}
			}
			List<Lord> lords = this.map.lordManager.lords;
			for (int j = 0; j < lords.Count; j++)
			{
				LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = lords[j].LordJob as LordJob_FormAndSendCaravan;
				if (lordJob_FormAndSendCaravan != null)
				{
					for (int k = 0; k < lordJob_FormAndSendCaravan.transferables.Count; k++)
					{
						TransferableOneWay transferableOneWay = lordJob_FormAndSendCaravan.transferables[k];
						for (int l = 0; l < transferableOneWay.things.Count; l++)
						{
							this.referencedThings.Add(transferableOneWay.things[l]);
						}
					}
				}
			}
			List<Thing> list = this.map.listerThings.ThingsInGroup(ThingRequestGroup.Transporter);
			for (int m = 0; m < list.Count; m++)
			{
				CompTransporter compTransporter = list[m].TryGetComp<CompTransporter>();
				if (compTransporter.leftToLoad != null)
				{
					for (int n = 0; n < compTransporter.leftToLoad.Count; n++)
					{
						TransferableOneWay transferableOneWay2 = compTransporter.leftToLoad[n];
						for (int num = 0; num < transferableOneWay2.things.Count; num++)
						{
							this.referencedThings.Add(transferableOneWay2.things[num]);
						}
					}
				}
			}
		}

		public bool IsReferenced(Thing th)
		{
			return this.referencedThings.Contains(th);
		}
	}
}
