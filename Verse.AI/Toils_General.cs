using System;

namespace Verse.AI
{
	public static class Toils_General
	{
		public static Toil Wait(int ticks)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.pather.StopDead();
			};
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = ticks;
			return toil;
		}

		public static Toil RemoveDesignationsOnThing(TargetIndex ind, DesignationDef def)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Find.DesignationManager.RemoveAllDesignationsOn(toil.actor.jobs.curJob.GetTarget(ind).Thing, false);
			};
			return toil;
		}

		public static Toil ClearTarget(TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.GetActor().CurJob.SetTarget(ind, null);
			};
			return toil;
		}

		public static Toil PutCarriedThingInInventory()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.GetActor();
				if (actor.carrier.CarriedThing != null)
				{
					if (actor.inventory.container.TryAdd(actor.carrier.CarriedThing))
					{
						actor.carrier.container.Clear();
					}
					else
					{
						Thing thing;
						actor.carrier.TryDropCarriedThing(actor.Position, actor.carrier.CarriedThing.stackCount, ThingPlaceMode.Near, out thing, null);
					}
				}
			};
			return toil;
		}
	}
}
