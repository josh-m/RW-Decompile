using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_Construct
	{
		public static Toil MakeSolidThingFromBlueprintIfNecessary(TargetIndex blueTarget)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Blueprint blueprint = curJob.GetTarget(blueTarget).Thing as Blueprint;
				if (blueprint != null)
				{
					Thing thing;
					bool flag;
					if (blueprint.TryReplaceWithSolidThing(actor, out thing, out flag))
					{
						curJob.SetTarget(blueTarget, thing);
						if (thing is Frame)
						{
							actor.Reserve(thing, 1);
						}
					}
					if (flag)
					{
						return;
					}
				}
			};
			return toil;
		}

		public static Toil UninstallIfMinifiable(TargetIndex thingInd)
		{
			Toil uninstallIfMinifiable = new Toil().FailOnDestroyedNullOrForbidden(thingInd);
			uninstallIfMinifiable.initAction = delegate
			{
				Pawn actor = uninstallIfMinifiable.actor;
				JobDriver curDriver = actor.jobs.curDriver;
				Thing thing = actor.CurJob.GetTarget(thingInd).Thing;
				if (thing.def.Minifiable)
				{
					curDriver.uninstallWorkLeft = 90f;
				}
				else
				{
					curDriver.ReadyForNextToil();
				}
			};
			uninstallIfMinifiable.tickAction = delegate
			{
				Pawn actor = uninstallIfMinifiable.actor;
				JobDriver curDriver = actor.jobs.curDriver;
				Job curJob = actor.CurJob;
				curDriver.uninstallWorkLeft -= actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
				if (curDriver.uninstallWorkLeft <= 0f)
				{
					Thing thing = curJob.GetTarget(thingInd).Thing;
					MinifiedThing minifiedThing = thing.MakeMinified();
					GenSpawn.Spawn(minifiedThing, thing.Position, uninstallIfMinifiable.actor.Map);
					curJob.SetTarget(thingInd, minifiedThing);
					actor.jobs.curDriver.ReadyForNextToil();
					return;
				}
			};
			uninstallIfMinifiable.defaultCompleteMode = ToilCompleteMode.Never;
			uninstallIfMinifiable.WithProgressBar(thingInd, () => 1f - uninstallIfMinifiable.actor.jobs.curDriver.uninstallWorkLeft / 90f, false, -0.5f);
			return uninstallIfMinifiable;
		}
	}
}
