using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class OverlayDrawer
	{
		private const int AltitudeIndex_Forbidden = 4;

		private const int AltitudeIndex_BurningWick = 5;

		private const int AltitudeIndex_QuestionMark = 6;

		private const float PulseFrequency = 4f;

		private const float PulseAmplitude = 0.7f;

		private const float StackOffsetMultipiler = 0.25f;

		private static Dictionary<Thing, OverlayTypes> overlaysToDraw;

		private static Vector3 curOffset;

		private static readonly Material ForbiddenMat;

		private static readonly Material NeedsPowerMat;

		private static readonly Material PowerOffMat;

		private static readonly Material QuestionMarkMat;

		private static readonly Material BrokenDownMat;

		private static readonly Material OutOfFuelMat;

		private static readonly Material WickMaterialA;

		private static readonly Material WickMaterialB;

		private static float SingleCellForbiddenOffset;

		private static readonly float BaseAlt;

		static OverlayDrawer()
		{
			OverlayDrawer.overlaysToDraw = new Dictionary<Thing, OverlayTypes>();
			OverlayDrawer.ForbiddenMat = MaterialPool.MatFrom("Things/Special/ForbiddenOverlay", ShaderDatabase.MetaOverlay);
			OverlayDrawer.NeedsPowerMat = MaterialPool.MatFrom("UI/Overlays/NeedsPower", ShaderDatabase.MetaOverlay);
			OverlayDrawer.PowerOffMat = MaterialPool.MatFrom("UI/Overlays/PowerOff", ShaderDatabase.MetaOverlay);
			OverlayDrawer.QuestionMarkMat = MaterialPool.MatFrom("UI/Overlays/QuestionMark", ShaderDatabase.MetaOverlay);
			OverlayDrawer.BrokenDownMat = MaterialPool.MatFrom("UI/Overlays/BrokenDown", ShaderDatabase.MetaOverlay);
			OverlayDrawer.OutOfFuelMat = MaterialPool.MatFrom("UI/Overlays/OutOfFuel", ShaderDatabase.MetaOverlay);
			OverlayDrawer.WickMaterialA = MaterialPool.MatFrom("Things/Special/BurningWickA", ShaderDatabase.MetaOverlay);
			OverlayDrawer.WickMaterialB = MaterialPool.MatFrom("Things/Special/BurningWickB", ShaderDatabase.MetaOverlay);
			OverlayDrawer.SingleCellForbiddenOffset = 0.3f;
			OverlayDrawer.BaseAlt = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays);
		}

		public static void DrawOverlay(Thing t, OverlayTypes overlayType)
		{
			if (OverlayDrawer.overlaysToDraw.ContainsKey(t))
			{
				Dictionary<Thing, OverlayTypes> dictionary;
				Dictionary<Thing, OverlayTypes> expr_15 = dictionary = OverlayDrawer.overlaysToDraw;
				OverlayTypes overlayTypes = dictionary[t];
				expr_15[t] = (overlayTypes | overlayType);
			}
			else
			{
				OverlayDrawer.overlaysToDraw.Add(t, overlayType);
			}
		}

		public static void DrawAllOverlays()
		{
			foreach (KeyValuePair<Thing, OverlayTypes> current in OverlayDrawer.overlaysToDraw)
			{
				OverlayDrawer.curOffset = Vector3.zero;
				Thing key = current.Key;
				OverlayTypes value = current.Value;
				if ((value & OverlayTypes.BurningWick) != (OverlayTypes)0)
				{
					OverlayDrawer.RenderBurningWick(key);
				}
				else
				{
					OverlayTypes overlayTypes = OverlayTypes.NeedsPower | OverlayTypes.PowerOff;
					int bitCountOf = Gen.GetBitCountOf((long)(value & overlayTypes));
					float num = OverlayDrawer.StackOffsetFor(current.Key);
					switch (bitCountOf)
					{
					case 1:
						OverlayDrawer.curOffset = Vector3.zero;
						break;
					case 2:
						OverlayDrawer.curOffset = new Vector3(-0.5f * num, 0f, 0f);
						break;
					case 3:
						OverlayDrawer.curOffset = new Vector3(-1.5f * num, 0f, 0f);
						break;
					}
					if ((value & OverlayTypes.NeedsPower) != (OverlayTypes)0)
					{
						OverlayDrawer.RenderNeedsPowerOverlay(key);
					}
					if ((value & OverlayTypes.PowerOff) != (OverlayTypes)0)
					{
						OverlayDrawer.RenderPowerOffOverlay(key);
					}
					if ((value & OverlayTypes.BrokenDown) != (OverlayTypes)0)
					{
						OverlayDrawer.RenderBrokenDownOverlay(key);
					}
					if ((value & OverlayTypes.OutOfFuel) != (OverlayTypes)0)
					{
						OverlayDrawer.RenderOutOfFuelOverlay(key);
					}
				}
				if ((value & OverlayTypes.ForbiddenBig) != (OverlayTypes)0)
				{
					OverlayDrawer.RenderForbiddenBigOverlay(key);
				}
				if ((value & OverlayTypes.Forbidden) != (OverlayTypes)0)
				{
					OverlayDrawer.RenderForbiddenOverlay(key);
				}
				if ((value & OverlayTypes.QuestionMark) != (OverlayTypes)0)
				{
					OverlayDrawer.RenderQuestionMarkOverlay(key);
				}
			}
			OverlayDrawer.overlaysToDraw.Clear();
		}

		private static float StackOffsetFor(Thing t)
		{
			return (float)t.RotatedSize.x * 0.25f;
		}

		private static void RenderNeedsPowerOverlay(Thing t)
		{
			OverlayDrawer.RenderPulsingOverlay(t, OverlayDrawer.NeedsPowerMat, 2);
		}

		private static void RenderPowerOffOverlay(Thing t)
		{
			OverlayDrawer.RenderPulsingOverlay(t, OverlayDrawer.PowerOffMat, 3);
		}

		private static void RenderBrokenDownOverlay(Thing t)
		{
			OverlayDrawer.RenderPulsingOverlay(t, OverlayDrawer.BrokenDownMat, 4);
		}

		private static void RenderOutOfFuelOverlay(Thing t)
		{
			OverlayDrawer.RenderPulsingOverlay(t, OverlayDrawer.OutOfFuelMat, 5);
		}

		private static void RenderPulsingOverlay(Thing thing, Material mat, int altInd)
		{
			Mesh plane = MeshPool.plane08;
			OverlayDrawer.RenderPulsingOverlay(thing, mat, altInd, plane);
		}

		private static void RenderPulsingOverlay(Thing thing, Material mat, int altInd, Mesh mesh)
		{
			Vector3 vector = thing.TrueCenter();
			vector.y = OverlayDrawer.BaseAlt + 0.05f * (float)altInd;
			vector += OverlayDrawer.curOffset;
			OverlayDrawer.curOffset.x = OverlayDrawer.curOffset.x + OverlayDrawer.StackOffsetFor(thing);
			OverlayDrawer.RenderPulsingOverlay(thing, mat, vector, mesh);
		}

		private static void RenderPulsingOverlay(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
		{
			float num = (Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f;
			float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
			num2 = 0.3f + num2 * 0.7f;
			Material material = FadedMaterialPool.FadedVersionOf(mat, num2);
			Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
		}

		private static void RenderForbiddenOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			if (t.RotatedSize.z == 1)
			{
				drawPos.z -= OverlayDrawer.SingleCellForbiddenOffset;
			}
			else
			{
				drawPos.z -= (float)t.RotatedSize.z * 0.3f;
			}
			drawPos.y = OverlayDrawer.BaseAlt + 0.2f;
			Graphics.DrawMesh(MeshPool.plane05, drawPos, Quaternion.identity, OverlayDrawer.ForbiddenMat, 0);
		}

		private static void RenderForbiddenBigOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			drawPos.y = OverlayDrawer.BaseAlt + 0.2f;
			Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, OverlayDrawer.ForbiddenMat, 0);
		}

		private static void RenderBurningWick(Thing parent)
		{
			Material material;
			if (Rand.Value < 0.5f)
			{
				material = OverlayDrawer.WickMaterialA;
			}
			else
			{
				material = OverlayDrawer.WickMaterialB;
			}
			Vector3 drawPos = parent.DrawPos;
			drawPos.y = OverlayDrawer.BaseAlt + 0.25f;
			Graphics.DrawMesh(MeshPool.plane20, drawPos, Quaternion.identity, material, 0);
		}

		private static void RenderQuestionMarkOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			drawPos.y = OverlayDrawer.BaseAlt + 0.3f;
			if (t is Pawn)
			{
				drawPos.x += (float)t.def.size.x - 0.52f;
				drawPos.z += (float)t.def.size.z - 0.45f;
			}
			OverlayDrawer.RenderPulsingOverlay(t, OverlayDrawer.QuestionMarkMat, drawPos, MeshPool.plane05);
		}
	}
}
