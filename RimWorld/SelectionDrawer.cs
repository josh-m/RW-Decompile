using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class SelectionDrawer
	{
		private const float SelJumpDuration = 0.07f;

		private const float SelJumpDistance = 0.2f;

		private static Dictionary<object, float> selectTimes = new Dictionary<object, float>();

		private static readonly Material SelectionBracketMat = MaterialPool.MatFrom("UI/Overlays/SelectionBracket", ShaderDatabase.MetaOverlay);

		public static void Notify_Selected(object t)
		{
			SelectionDrawer.selectTimes[t] = Time.realtimeSinceStartup;
		}

		public static void Clear()
		{
			SelectionDrawer.selectTimes.Clear();
		}

		public static void DrawSelectionOverlays()
		{
			foreach (object current in Find.Selector.SelectedObjects)
			{
				SelectionDrawer.DrawSelectionBracketFor(current);
				Thing thing = current as Thing;
				if (thing != null)
				{
					thing.DrawExtraSelectionOverlays();
				}
			}
		}

		private static void DrawSelectionBracketFor(object obj)
		{
			Zone zone = obj as Zone;
			if (zone != null)
			{
				GenDraw.DrawFieldEdges(zone.Cells);
			}
			Thing thing = obj as Thing;
			if (thing != null)
			{
				Vector3[] array = SelectionDrawer.SelectionBracketPartsPos(thing, thing.DrawPos, thing.RotatedSize.ToVector2(), Vector2.one, 1f);
				int num = 0;
				for (int i = 0; i < 4; i++)
				{
					Quaternion rotation = Quaternion.AngleAxis((float)num, Vector3.up);
					Graphics.DrawMesh(MeshPool.plane10, array[i], rotation, SelectionDrawer.SelectionBracketMat, 0);
					num -= 90;
				}
			}
		}

		public static Vector3[] SelectionBracketPartsPos(Thing thing, Vector2 pos, Vector2 size, Vector2 textureSize, float jumpDistanceFactor)
		{
			return SelectionDrawer.SelectionBracketPartsPos(thing, new Vector3(pos.x, 0f, pos.y), size, textureSize, jumpDistanceFactor);
		}

		public static Vector3[] SelectionBracketPartsPos(Thing thing, Vector3 pos, Vector2 size, Vector2 textureSize, float jumpDistanceFactor)
		{
			Vector3[] array = new Vector3[]
			{
				default(Vector3),
				default(Vector3),
				default(Vector3),
				default(Vector3)
			};
			Vector2 vector = size * 0.5f;
			vector.x -= 0.5f * textureSize.x;
			vector.y -= 0.5f * textureSize.y;
			float num;
			float num2;
			if (!SelectionDrawer.selectTimes.TryGetValue(thing, out num))
			{
				num2 = 1f;
			}
			else
			{
				num2 = 1f - (Time.realtimeSinceStartup - num) / 0.07f;
				if (num2 < 0f)
				{
					num2 = 0f;
				}
			}
			float num3 = num2 * 0.2f * jumpDistanceFactor;
			vector.x += num3;
			vector.y += num3;
			float y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
			array[0] = new Vector3(pos.x - vector.x, y, pos.z - vector.y);
			array[1] = new Vector3(pos.x + vector.x, y, pos.z - vector.y);
			array[2] = new Vector3(pos.x + vector.x, y, pos.z + vector.y);
			array[3] = new Vector3(pos.x - vector.x, y, pos.z + vector.y);
			return array;
		}
	}
}
