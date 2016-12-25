using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_WatchTelevision : JobDriver_WatchBuilding
	{
		protected override void WatchTickAction()
		{
			Building thing = (Building)base.TargetA.Thing;
			if (!thing.TryGetComp<CompPowerTrader>().PowerOn)
			{
				base.EndJobWith(JobCondition.Incompletable);
				return;
			}
			base.WatchTickAction();
		}
	}
}
