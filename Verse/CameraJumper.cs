using RimWorld;
using RimWorld.Planet;
using System;
using Verse.Sound;

namespace Verse
{
	public static class CameraJumper
	{
		public static void TryJumpAndSelect(GlobalTargetInfo target)
		{
			if (!target.IsValid)
			{
				return;
			}
			CameraJumper.TryJump(target);
			CameraJumper.TrySelect(target);
		}

		public static void TrySelect(GlobalTargetInfo target)
		{
			if (!target.IsValid)
			{
				return;
			}
			target = CameraJumper.GetAdjustedTarget(target);
			if (target.HasThing)
			{
				CameraJumper.TrySelectInternal(target.Thing);
			}
			else if (target.HasWorldObject)
			{
				CameraJumper.TrySelectInternal(target.WorldObject);
			}
		}

		private static void TrySelectInternal(Thing thing)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (thing.Spawned)
			{
				bool flag = CameraJumper.TryHideWorld();
				bool flag2 = false;
				if (thing.Map != Current.Game.VisibleMap)
				{
					Current.Game.VisibleMap = thing.Map;
					flag2 = true;
					if (!flag)
					{
						SoundDefOf.MapSelected.PlayOneShotOnCamera(null);
					}
				}
				if (flag || flag2)
				{
					Find.CameraDriver.JumpToVisibleMapLoc(thing.Position);
				}
				Find.Selector.ClearSelection();
				Find.Selector.Select(thing, true, true);
			}
		}

		private static void TrySelectInternal(WorldObject worldObject)
		{
			if (Find.World == null)
			{
				return;
			}
			if (worldObject.Spawned)
			{
				CameraJumper.TryShowWorld();
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
			target = CameraJumper.GetAdjustedTarget(target);
			if (target.HasThing)
			{
				CameraJumper.TryJumpInternal(target.Thing);
			}
			else if (target.HasWorldObject)
			{
				CameraJumper.TryJumpInternal(target.WorldObject);
			}
			else if (target.Cell.IsValid)
			{
				CameraJumper.TryJumpInternal(target.Cell, target.Map);
			}
			else
			{
				CameraJumper.TryJumpInternal(target.Tile);
			}
		}

		public static void TryJump(IntVec3 cell, Map map)
		{
			CameraJumper.TryJump(new GlobalTargetInfo(cell, map, false));
		}

		public static void TryJump(int tile)
		{
			CameraJumper.TryJump(new GlobalTargetInfo(tile));
		}

		private static void TryJumpInternal(Thing thing)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			Map mapHeld = thing.MapHeld;
			if (mapHeld != null && thing.PositionHeld.IsValid && thing.PositionHeld.InBounds(mapHeld))
			{
				bool flag = CameraJumper.TryHideWorld();
				if (Current.Game.VisibleMap != mapHeld)
				{
					Current.Game.VisibleMap = mapHeld;
					if (!flag)
					{
						SoundDefOf.MapSelected.PlayOneShotOnCamera(null);
					}
				}
				Find.CameraDriver.JumpToVisibleMapLoc(thing.PositionHeld);
			}
		}

		private static void TryJumpInternal(IntVec3 cell, Map map)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (!cell.IsValid)
			{
				return;
			}
			if (map == null || !Find.Maps.Contains(map))
			{
				return;
			}
			bool flag = CameraJumper.TryHideWorld();
			if (Current.Game.VisibleMap != map)
			{
				Current.Game.VisibleMap = map;
				if (!flag)
				{
					SoundDefOf.MapSelected.PlayOneShotOnCamera(null);
				}
			}
			Find.CameraDriver.JumpToVisibleMapLoc(cell);
		}

		private static void TryJumpInternal(WorldObject worldObject)
		{
			if (Find.World == null)
			{
				return;
			}
			if (worldObject.Tile < 0)
			{
				return;
			}
			CameraJumper.TryShowWorld();
			Find.WorldCameraDriver.JumpTo(worldObject.Tile);
		}

		private static void TryJumpInternal(int tile)
		{
			if (Find.World == null)
			{
				return;
			}
			if (tile < 0)
			{
				return;
			}
			CameraJumper.TryShowWorld();
			Find.WorldCameraDriver.JumpTo(tile);
		}

		public static GlobalTargetInfo GetAdjustedTarget(GlobalTargetInfo target)
		{
			if (target.HasThing)
			{
				Thing thing = target.Thing;
				if (thing.Spawned)
				{
					return thing;
				}
				GlobalTargetInfo result = GlobalTargetInfo.Invalid;
				for (IThingHolder parentHolder = thing.ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
				{
					Thing thing2 = parentHolder as Thing;
					if (thing2 != null && thing2.Spawned)
					{
						result = thing2;
					}
					if (!result.HasThing)
					{
						WorldObject worldObject = parentHolder as WorldObject;
						if (worldObject != null && worldObject.Spawned)
						{
							result = worldObject;
						}
					}
				}
				if (result.IsValid)
				{
					return result;
				}
				if (thing.Tile >= 0)
				{
					return new GlobalTargetInfo(thing.Tile);
				}
			}
			else if (target.Cell.IsValid && target.Tile >= 0 && target.Map != null && !Find.Maps.Contains(target.Map))
			{
				MapParent parent = target.Map.info.parent;
				if (parent != null && parent.Spawned)
				{
					return parent;
				}
				if (parent != null && parent.Tile >= 0)
				{
					return new GlobalTargetInfo(target.Map.Tile);
				}
				return GlobalTargetInfo.Invalid;
			}
			else if (target.HasWorldObject && !target.WorldObject.Spawned && target.WorldObject.Tile >= 0)
			{
				return new GlobalTargetInfo(target.WorldObject.Tile);
			}
			return target;
		}

		public static GlobalTargetInfo GetWorldTarget(GlobalTargetInfo target)
		{
			GlobalTargetInfo adjustedTarget = CameraJumper.GetAdjustedTarget(target);
			if (!adjustedTarget.IsValid)
			{
				return GlobalTargetInfo.Invalid;
			}
			if (adjustedTarget.IsWorldTarget)
			{
				return adjustedTarget;
			}
			return CameraJumper.GetWorldTargetOfMap(adjustedTarget.Map);
		}

		public static GlobalTargetInfo GetWorldTargetOfMap(Map map)
		{
			if (map == null)
			{
				return GlobalTargetInfo.Invalid;
			}
			if (map.info.parent != null && map.info.parent.Spawned)
			{
				return map.info.parent;
			}
			if (map.info.parent != null && map.info.parent.Tile >= 0)
			{
				return new GlobalTargetInfo(map.Tile);
			}
			return GlobalTargetInfo.Invalid;
		}

		public static bool TryHideWorld()
		{
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				return true;
			}
			if (Current.ProgramState != ProgramState.Playing)
			{
				return false;
			}
			if (Find.World.renderer.wantedMode != WorldRenderMode.None)
			{
				Find.World.renderer.wantedMode = WorldRenderMode.None;
				SoundDefOf.TabClose.PlayOneShotOnCamera(null);
				return true;
			}
			return false;
		}

		public static bool TryShowWorld()
		{
			if (WorldRendererUtility.WorldRenderedNow)
			{
				return true;
			}
			if (Current.ProgramState != ProgramState.Playing)
			{
				return false;
			}
			if (Find.World.renderer.wantedMode == WorldRenderMode.None)
			{
				Find.World.renderer.wantedMode = WorldRenderMode.Planet;
				SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
				return true;
			}
			return false;
		}
	}
}
