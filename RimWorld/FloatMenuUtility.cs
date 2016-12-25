using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class FloatMenuUtility
	{
		public static void MakeMenu<T>(IEnumerable<T> objects, Func<T, string> labelGetter, Func<T, Action> actionGetter)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (T current in objects)
			{
				T arg = current;
				list.Add(new FloatMenuOption(labelGetter(arg), actionGetter(arg), MenuOptionPriority.Default, null, null, 0f, null, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static Action GetRangedAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
		{
			failStr = string.Empty;
			if (pawn.equipment.Primary == null)
			{
				return null;
			}
			Verb primaryVerb = pawn.equipment.PrimaryEq.PrimaryVerb;
			if (primaryVerb.verbProps.MeleeRange)
			{
				return null;
			}
			if (!pawn.Drafted)
			{
				failStr = "IsNotDraftedLower".Translate(new object[]
				{
					pawn.NameStringShort
				});
			}
			else if (!pawn.IsColonistPlayerControlled)
			{
				failStr = "CannotOrderNonControlledLower".Translate();
			}
			else if (target.IsValid && !pawn.equipment.PrimaryEq.PrimaryVerb.CanHitTarget(target))
			{
				if (!pawn.Position.InHorDistOf(target.Cell, primaryVerb.verbProps.range))
				{
					failStr = "OutOfRange".Translate();
				}
				else
				{
					failStr = "CannotHitTarget".Translate();
				}
			}
			else
			{
				if (!pawn.story.WorkTagIsDisabled(WorkTags.Violent))
				{
					return delegate
					{
						Job job = new Job(JobDefOf.AttackStatic, target);
						pawn.jobs.TryTakeOrderedJob(job);
					};
				}
				failStr = "IsIncapableOfViolenceLower".Translate(new object[]
				{
					pawn.NameStringShort
				});
			}
			return null;
		}

		public static Action GetMeleeAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
		{
			failStr = string.Empty;
			if (!pawn.Drafted)
			{
				failStr = "IsNotDraftedLower".Translate(new object[]
				{
					pawn.NameStringShort
				});
			}
			else if (!pawn.IsColonistPlayerControlled)
			{
				failStr = "CannotOrderNonControlledLower".Translate();
			}
			else if (target.IsValid && !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
			{
				failStr = "NoPath".Translate();
			}
			else if (pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				failStr = "IsIncapableOfViolenceLower".Translate(new object[]
				{
					pawn.NameStringShort
				});
			}
			else
			{
				if (pawn.meleeVerbs.TryGetMeleeVerb() != null)
				{
					return delegate
					{
						Job job = new Job(JobDefOf.AttackMelee, target);
						Pawn pawn2 = target.Thing as Pawn;
						if (pawn2 != null)
						{
							job.killIncappedTarget = pawn2.Downed;
						}
						pawn.jobs.TryTakeOrderedJob(job);
					};
				}
				failStr = "Incapable".Translate();
			}
			return null;
		}

		public static Action GetAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
		{
			if (pawn.equipment.Primary != null && !pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.MeleeRange)
			{
				return FloatMenuUtility.GetRangedAttackAction(pawn, target, out failStr);
			}
			return FloatMenuUtility.GetMeleeAttackAction(pawn, target, out failStr);
		}
	}
}
