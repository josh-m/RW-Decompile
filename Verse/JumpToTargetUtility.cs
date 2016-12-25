using RimWorld;
using RimWorld.Planet;
using System;
using Verse.Sound;

namespace Verse
{
	public static class JumpToTargetUtility
	{
		public static void TryJumpAndSelect(GlobalTargetInfo target)
		{
			if (!target.IsValid)
			{
				return;
			}
			JumpToTargetUtility.TryJump(target);
			JumpToTargetUtility.TrySelect(target);
		}

		public static void TrySelect(GlobalTargetInfo target)
		{
			if (!target.IsValid)
			{
				return;
			}
			target = JumpToTargetUtility.GetAdjustedLookTarget(target);
			if (target.HasThing)
			{
				JumpToTargetUtility.TrySelect(target.Thing);
			}
			else if (target.HasWorldObject)
			{
				JumpToTargetUtility.TrySelect(target.WorldObject);
			}
		}

		private static void TrySelect(Thing thing)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (thing.Spawned)
			{
				bool flag = JumpToTargetUtility.CloseWorldTab();
				bool flag2 = false;
				if (thing.Map != Current.Game.VisibleMap)
				{
					Current.Game.VisibleMap = thing.Map;
					flag2 = true;
					if (!flag)
					{
						SoundDefOf.MapSelected.PlayOneShotOnCamera();
					}
				}
				if (flag || flag2)
				{
					Find.CameraDriver.JumpTo(thing.Position);
				}
				Find.Selector.ClearSelection();
				Find.Selector.Select(thing, true, true);
			}
		}

		private static void TrySelect(WorldObject worldObject)
		{
			if (Find.World == null)
			{
				return;
			}
			if (worldObject.Spawned)
			{
				JumpToTargetUtility.OpenWorldTab();
				Find.WorldSelector.ClearSelection();
				Find.WorldSelector.Select(worldObject, true);
			}
		}

		public static void TryJump(GlobalTargetInfo target)
		{
			if (!target.IsValid)
			{
				return;
			}
			target = JumpToTargetUtility.GetAdjustedLookTarget(target);
			if (target.HasThing)
			{
				JumpToTargetUtility.TryJump(target.Thing);
			}
			else if (target.HasWorldObject)
			{
				JumpToTargetUtility.TryJump(target.WorldObject);
			}
			else if (target.Cell.IsValid)
			{
				JumpToTargetUtility.TryJump(target.Cell, target.Map);
			}
			else
			{
				JumpToTargetUtility.TryJump(target.Tile);
			}
		}

		private static void TryJump(Thing thing)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (thing.MapHeld != null && thing.PositionHeld.IsValid)
			{
				bool flag = JumpToTargetUtility.CloseWorldTab();
				if (Current.Game.VisibleMap != thing.MapHeld)
				{
					Current.Game.VisibleMap = thing.MapHeld;
					if (!flag)
					{
						SoundDefOf.MapSelected.PlayOneShotOnCamera();
					}
				}
				Find.CameraDriver.JumpTo(thing.PositionHeld);
			}
		}

		private static void TryJump(IntVec3 cell, Map map)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (map == null || !Find.Maps.Contains(map))
			{
				return;
			}
			bool flag = JumpToTargetUtility.CloseWorldTab();
			if (Current.Game.VisibleMap != map)
			{
				Current.Game.VisibleMap = map;
				if (!flag)
				{
					SoundDefOf.MapSelected.PlayOneShotOnCamera();
				}
			}
			Find.CameraDriver.JumpTo(cell);
		}

		private static void TryJump(WorldObject worldObject)
		{
			if (Find.World == null)
			{
				return;
			}
			if (worldObject.Tile >= 0)
			{
				JumpToTargetUtility.OpenWorldTab();
				Find.WorldCameraDriver.JumpTo(worldObject.Tile);
			}
		}

		private static void TryJump(int tile)
		{
			if (Find.World == null)
			{
				return;
			}
			JumpToTargetUtility.OpenWorldTab();
			Find.WorldCameraDriver.JumpTo(tile);
		}

		private static GlobalTargetInfo GetAdjustedLookTarget(GlobalTargetInfo target)
		{
			if (target.HasThing)
			{
				Thing thing = target.Thing;
				Pawn pawn = thing as Pawn;
				if (pawn != null)
				{
					if (pawn.IsCaravanMember())
					{
						return pawn.GetCaravan();
					}
					if (pawn.Dead && pawn.Corpse != null)
					{
						return JumpToTargetUtility.GetAdjustedLookTarget(pawn.Corpse);
					}
				}
				if (thing.holdingContainer != null)
				{
					IThingContainerOwner owner = thing.holdingContainer.owner;
					Thing thing2 = owner as Thing;
					if (thing2 != null)
					{
						return thing2;
					}
					if (owner.Spawned)
					{
						return new GlobalTargetInfo(owner.GetPosition(), owner.GetMap(), false);
					}
				}
			}
			return target;
		}

		public static GlobalTargetInfo GetWorldTarget(GlobalTargetInfo target)
		{
			GlobalTargetInfo adjustedLookTarget = JumpToTargetUtility.GetAdjustedLookTarget(target);
			if (!adjustedLookTarget.IsValid)
			{
				return GlobalTargetInfo.Invalid;
			}
			if (adjustedLookTarget.IsWorldTarget)
			{
				return adjustedLookTarget;
			}
			return JumpToTargetUtility.GetGlobalTargetInfoForMap(adjustedLookTarget.Map);
		}

		public static bool CloseWorldTab()
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return false;
			}
			if (Find.MainTabsRoot.OpenTab == MainTabDefOf.World && Find.VisibleMap != null)
			{
				Find.MainTabsRoot.EscapeCurrentTab(true);
				return true;
			}
			return false;
		}

		public static GlobalTargetInfo GetGlobalTargetInfoForMap(Map map)
		{
			if (map == null)
			{
				return GlobalTargetInfo.Invalid;
			}
			if (map.info.parent != null)
			{
				return map.info.parent;
			}
			return new GlobalTargetInfo(map.info.tile);
		}

		private static void OpenWorldTab()
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (Find.MainTabsRoot.OpenTab != MainTabDefOf.World)
			{
				Find.MainTabsRoot.SetCurrentTab(MainTabDefOf.World, true);
			}
		}
	}
}
