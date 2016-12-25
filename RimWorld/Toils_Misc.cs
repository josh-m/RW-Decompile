using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_Misc
	{
		public static Toil Learn(SkillDef skill, float xp)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.skills.Learn(skill, xp);
			};
			return toil;
		}

		public static Toil SetForbidden(TargetIndex ind, bool forbidden)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.CurJob.GetTarget(ind).Thing.SetForbidden(forbidden, true);
			};
			return toil;
		}

		public static Toil TakeItemFromInventoryToCarrier(Pawn pawn, TargetIndex itemInd)
		{
			return new Toil
			{
				initAction = delegate
				{
					Job curJob = pawn.CurJob;
					Thing thing = (Thing)curJob.GetTarget(itemInd);
					int stackCount = Mathf.Min(thing.stackCount, curJob.maxNumToCarry);
					pawn.inventory.container.TransferToContainer(thing, pawn.carrier.container, stackCount);
					curJob.SetTarget(itemInd, pawn.carrier.CarriedThing);
				}
			};
		}

		public static Toil ThrowColonistAttackingMote(TargetIndex target)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.CurJob;
				if (actor.playerSettings != null && actor.playerSettings.UsesConfigurableHostilityResponse && !actor.Drafted && !actor.InMentalState && !curJob.playerForced && actor.HostileTo(curJob.GetTarget(target).Thing))
				{
					MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistAttacking);
				}
			};
			return toil;
		}
	}
}
