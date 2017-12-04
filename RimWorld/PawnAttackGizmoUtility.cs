using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnAttackGizmoUtility
	{
		[DebuggerHidden]
		public static IEnumerable<Gizmo> GetAttackGizmos(Pawn pawn)
		{
			if (PawnAttackGizmoUtility.ShouldUseMeleeAttackGizmo(pawn))
			{
				yield return PawnAttackGizmoUtility.GetMeleeAttackGizmo(pawn);
			}
			if (PawnAttackGizmoUtility.ShouldUseSquadAttackGizmo())
			{
				yield return PawnAttackGizmoUtility.GetSquadAttackGizmo(pawn);
			}
		}

		public static bool CanShowEquipmentGizmos()
		{
			return !PawnAttackGizmoUtility.AtLeastTwoSelectedColonistsHaveDifferentWeapons();
		}

		private static bool ShouldUseSquadAttackGizmo()
		{
			return PawnAttackGizmoUtility.AtLeastOneSelectedColonistHasRangedWeapon() && PawnAttackGizmoUtility.AtLeastTwoSelectedColonistsHaveDifferentWeapons();
		}

		private static Gizmo GetSquadAttackGizmo(Pawn pawn)
		{
			Command_Target command_Target = new Command_Target();
			command_Target.defaultLabel = "CommandSquadAttack".Translate();
			command_Target.defaultDesc = "CommandSquadAttackDesc".Translate();
			command_Target.targetingParams = TargetingParameters.ForAttackAny();
			command_Target.hotKey = KeyBindingDefOf.Misc1;
			command_Target.icon = TexCommand.SquadAttack;
			string str;
			if (FloatMenuUtility.GetAttackAction(pawn, LocalTargetInfo.Invalid, out str) == null)
			{
				command_Target.Disable(str.CapitalizeFirst() + ".");
			}
			command_Target.action = delegate(Thing target)
			{
				IEnumerable<Pawn> enumerable = Find.Selector.SelectedObjects.Where(delegate(object x)
				{
					Pawn pawn2 = x as Pawn;
					return pawn2 != null && pawn2.IsColonistPlayerControlled && pawn2.Drafted;
				}).Cast<Pawn>();
				foreach (Pawn current in enumerable)
				{
					string text;
					Action attackAction = FloatMenuUtility.GetAttackAction(current, target, out text);
					if (attackAction != null)
					{
						attackAction();
					}
				}
			};
			return command_Target;
		}

		private static bool ShouldUseMeleeAttackGizmo(Pawn pawn)
		{
			return pawn.Drafted && (PawnAttackGizmoUtility.AtLeastOneSelectedColonistHasRangedWeapon() || PawnAttackGizmoUtility.AtLeastOneSelectedColonistHasNoWeapon() || PawnAttackGizmoUtility.AtLeastTwoSelectedColonistsHaveDifferentWeapons());
		}

		private static Gizmo GetMeleeAttackGizmo(Pawn pawn)
		{
			Command_Target command_Target = new Command_Target();
			command_Target.defaultLabel = "CommandMeleeAttack".Translate();
			command_Target.defaultDesc = "CommandMeleeAttackDesc".Translate();
			command_Target.targetingParams = TargetingParameters.ForAttackAny();
			command_Target.hotKey = KeyBindingDefOf.Misc2;
			command_Target.icon = TexCommand.AttackMelee;
			string str;
			if (FloatMenuUtility.GetMeleeAttackAction(pawn, LocalTargetInfo.Invalid, out str) == null)
			{
				command_Target.Disable(str.CapitalizeFirst() + ".");
			}
			command_Target.action = delegate(Thing target)
			{
				IEnumerable<Pawn> enumerable = Find.Selector.SelectedObjects.Where(delegate(object x)
				{
					Pawn pawn2 = x as Pawn;
					return pawn2 != null && pawn2.IsColonistPlayerControlled && pawn2.Drafted;
				}).Cast<Pawn>();
				foreach (Pawn current in enumerable)
				{
					string text;
					Action meleeAttackAction = FloatMenuUtility.GetMeleeAttackAction(current, target, out text);
					if (meleeAttackAction != null)
					{
						meleeAttackAction();
					}
				}
			};
			return command_Target;
		}

		private static bool AtLeastOneSelectedColonistHasRangedWeapon()
		{
			List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
			for (int i = 0; i < selectedObjectsListForReading.Count; i++)
			{
				Pawn pawn = selectedObjectsListForReading[i] as Pawn;
				if (pawn != null && pawn.IsColonistPlayerControlled)
				{
					if (pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool AtLeastOneSelectedColonistHasNoWeapon()
		{
			List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
			for (int i = 0; i < selectedObjectsListForReading.Count; i++)
			{
				Pawn pawn = selectedObjectsListForReading[i] as Pawn;
				if (pawn != null && pawn.IsColonistPlayerControlled)
				{
					if (pawn.equipment == null || pawn.equipment.Primary == null)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool AtLeastTwoSelectedColonistsHaveDifferentWeapons()
		{
			if (Find.Selector.NumSelected <= 1)
			{
				return false;
			}
			ThingDef thingDef = null;
			bool flag = false;
			List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
			for (int i = 0; i < selectedObjectsListForReading.Count; i++)
			{
				Pawn pawn = selectedObjectsListForReading[i] as Pawn;
				if (pawn != null && pawn.IsColonistPlayerControlled)
				{
					ThingDef thingDef2;
					if (pawn.equipment == null || pawn.equipment.Primary == null)
					{
						thingDef2 = null;
					}
					else
					{
						thingDef2 = pawn.equipment.Primary.def;
					}
					if (!flag)
					{
						thingDef = thingDef2;
						flag = true;
					}
					else if (thingDef2 != thingDef)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
