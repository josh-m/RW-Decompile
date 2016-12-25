using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class ExpandableWorldObjectsUtility
	{
		private const float WorldObjectSize = 30f;

		private static float transitionPct;

		private static List<WorldObject> tmpWorldObjects = new List<WorldObject>();

		public static float TransitionPct
		{
			get
			{
				if (!Find.PlaySettings.expandingIcons)
				{
					return 0f;
				}
				return ExpandableWorldObjectsUtility.transitionPct;
			}
		}

		public static void ExpandableWorldObjectsUpdate()
		{
			float num = Time.deltaTime * 3f;
			if (Find.WorldCameraDriver.CurrentZoom == WorldCameraZoomRange.Close)
			{
				ExpandableWorldObjectsUtility.transitionPct -= num;
			}
			else
			{
				ExpandableWorldObjectsUtility.transitionPct += num;
			}
			ExpandableWorldObjectsUtility.transitionPct = Mathf.Clamp01(ExpandableWorldObjectsUtility.transitionPct);
		}

		public static void ExpandableWorldObjectsOnGUI()
		{
			if (ExpandableWorldObjectsUtility.TransitionPct == 0f)
			{
				return;
			}
			ExpandableWorldObjectsUtility.tmpWorldObjects.Clear();
			ExpandableWorldObjectsUtility.tmpWorldObjects.AddRange(Find.WorldObjects.AllWorldObjects);
			ExpandableWorldObjectsUtility.SortByExpandingIconPriority(ExpandableWorldObjectsUtility.tmpWorldObjects);
			WorldTargeter worldTargeter = Find.WorldTargeter;
			List<WorldObject> worldObjectsUnderMouse = null;
			if (worldTargeter.IsTargeting)
			{
				worldObjectsUnderMouse = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
			}
			for (int i = 0; i < ExpandableWorldObjectsUtility.tmpWorldObjects.Count; i++)
			{
				WorldObject worldObject = ExpandableWorldObjectsUtility.tmpWorldObjects[i];
				if (worldObject.def.expandingIcon)
				{
					if (!worldObject.HiddenBehindTerrainNow())
					{
						Color expandingIconColor = worldObject.ExpandingIconColor;
						expandingIconColor.a = ExpandableWorldObjectsUtility.TransitionPct;
						if (worldTargeter.IsTargetedNow(worldObject, worldObjectsUnderMouse))
						{
							float num = GenMath.LerpDouble(-1f, 1f, 0.7f, 1f, Mathf.Sin(Time.time * 8f));
							expandingIconColor.r *= num;
							expandingIconColor.g *= num;
							expandingIconColor.b *= num;
						}
						GUI.color = expandingIconColor;
						GUI.DrawTexture(ExpandableWorldObjectsUtility.ExpandedIconScreenRect(worldObject), worldObject.ExpandingIcon);
					}
				}
			}
			ExpandableWorldObjectsUtility.tmpWorldObjects.Clear();
			GUI.color = Color.white;
		}

		public static Rect ExpandedIconScreenRect(WorldObject o)
		{
			Vector2 vector = o.ScreenPos();
			return new Rect(vector.x - 15f, vector.y - 15f, 30f, 30f);
		}

		public static bool IsExpanded(WorldObject o)
		{
			return ExpandableWorldObjectsUtility.TransitionPct > 0.5f && o.def.expandingIcon;
		}

		public static void GetExpandedWorldObjectUnderMouse(Vector2 mousePos, List<WorldObject> outList)
		{
			outList.Clear();
			Vector2 point = mousePos;
			point.y = (float)UI.screenHeight - point.y;
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				WorldObject worldObject = allWorldObjects[i];
				if (ExpandableWorldObjectsUtility.IsExpanded(worldObject))
				{
					if (ExpandableWorldObjectsUtility.ExpandedIconScreenRect(worldObject).Contains(point))
					{
						if (!worldObject.HiddenBehindTerrainNow())
						{
							outList.Add(worldObject);
						}
					}
				}
			}
			ExpandableWorldObjectsUtility.SortByExpandingIconPriority(outList);
			outList.Reverse();
		}

		private static void SortByExpandingIconPriority(List<WorldObject> worldObjects)
		{
			worldObjects.SortBy(delegate(WorldObject x)
			{
				float num = x.ExpandingIconPriority;
				if (x.Faction != null && x.Faction.IsPlayer)
				{
					num += 0.001f;
				}
				return num;
			}, (WorldObject x) => x.ID);
		}
	}
}
