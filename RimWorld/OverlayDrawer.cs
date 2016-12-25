using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class OverlayDrawer
	{
		private const int AltitudeIndex_Forbidden = 4;

		private const int AltitudeIndex_BurningWick = 5;

		private const int AltitudeIndex_QuestionMark = 6;

		private const float PulseFrequency = 4f;

		private const float PulseAmplitude = 0.7f;

		private const float StackOffsetMultipiler = 0.25f;

		private Dictionary<Thing, OverlayTypes> overlaysToDraw = new Dictionary<Thing, OverlayTypes>();

		private Vector3 curOffset;

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

		public void DrawOverlay(Thing t, OverlayTypes overlayType)
		{
			if (this.overlaysToDraw.ContainsKey(t))
			{
				Dictionary<Thing, OverlayTypes> dictionary;
				Dictionary<Thing, OverlayTypes> expr_17 = dictionary = this.overlaysToDraw;
				OverlayTypes overlayTypes = dictionary[t];
				expr_17[t] = (overlayTypes | overlayType);
			}
			else
			{
				this.overlaysToDraw.Add(t, overlayType);
			}
		}

		public void DrawAllOverlays()
		{
			foreach (KeyValuePair<Thing, OverlayTypes> current in this.overlaysToDraw)
			{
				this.curOffset = Vector3.zero;
				Thing key = current.Key;
				OverlayTypes value = current.Value;
				if ((value & OverlayTypes.BurningWick) != (OverlayTypes)0)
				{
					this.RenderBurningWick(key);
				}
				else
				{
					OverlayTypes overlayTypes = OverlayTypes.NeedsPower | OverlayTypes.PowerOff;
					int bitCountOf = Gen.GetBitCountOf((long)(value & overlayTypes));
					float num = this.StackOffsetFor(current.Key);
					switch (bitCountOf)
					{
					case 1:
						this.curOffset = Vector3.zero;
						break;
					case 2:
						this.curOffset = new Vector3(-0.5f * num, 0f, 0f);
						break;
					case 3:
						this.curOffset = new Vector3(-1.5f * num, 0f, 0f);
						break;
					}
					if ((value & OverlayTypes.NeedsPower) != (OverlayTypes)0)
					{
						this.RenderNeedsPowerOverlay(key);
					}
					if ((value & OverlayTypes.PowerOff) != (OverlayTypes)0)
					{
						this.RenderPowerOffOverlay(key);
					}
					if ((value & OverlayTypes.BrokenDown) != (OverlayTypes)0)
					{
						this.RenderBrokenDownOverlay(key);
					}
					if ((value & OverlayTypes.OutOfFuel) != (OverlayTypes)0)
					{
						this.RenderOutOfFuelOverlay(key);
					}
				}
				if ((value & OverlayTypes.ForbiddenBig) != (OverlayTypes)0)
				{
					this.RenderForbiddenBigOverlay(key);
				}
				if ((value & OverlayTypes.Forbidden) != (OverlayTypes)0)
				{
					this.RenderForbiddenOverlay(key);
				}
				if ((value & OverlayTypes.QuestionMark) != (OverlayTypes)0)
				{
					this.RenderQuestionMarkOverlay(key);
				}
			}
			this.overlaysToDraw.Clear();
		}

		private float StackOffsetFor(Thing t)
		{
			return (float)t.RotatedSize.x * 0.25f;
		}

		private void RenderNeedsPowerOverlay(Thing t)
		{
			this.RenderPulsingOverlay(t, OverlayDrawer.NeedsPowerMat, 2);
		}

		private void RenderPowerOffOverlay(Thing t)
		{
			this.RenderPulsingOverlay(t, OverlayDrawer.PowerOffMat, 3);
		}

		private void RenderBrokenDownOverlay(Thing t)
		{
			this.RenderPulsingOverlay(t, OverlayDrawer.BrokenDownMat, 4);
		}

		private void RenderOutOfFuelOverlay(Thing t)
		{
			this.RenderPulsingOverlay(t, OverlayDrawer.OutOfFuelMat, 5);
		}

		private void RenderPulsingOverlay(Thing thing, Material mat, int altInd)
		{
			Mesh plane = MeshPool.plane08;
			this.RenderPulsingOverlay(thing, mat, altInd, plane);
		}

		private void RenderPulsingOverlay(Thing thing, Material mat, int altInd, Mesh mesh)
		{
			Vector3 vector = thing.TrueCenter();
			vector.y = OverlayDrawer.BaseAlt + 0.05f * (float)altInd;
			vector += this.curOffset;
			this.curOffset.x = this.curOffset.x + this.StackOffsetFor(thing);
			this.RenderPulsingOverlay(thing, mat, vector, mesh);
		}

		private void RenderPulsingOverlay(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
		{
			float num = (Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f;
			float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
			num2 = 0.3f + num2 * 0.7f;
			Material material = FadedMaterialPool.FadedVersionOf(mat, num2);
			Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
		}

		private void RenderForbiddenOverlay(Thing t)
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

		private void RenderForbiddenBigOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			drawPos.y = OverlayDrawer.BaseAlt + 0.2f;
			Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, OverlayDrawer.ForbiddenMat, 0);
		}

		private void RenderBurningWick(Thing parent)
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

		private void RenderQuestionMarkOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			drawPos.y = OverlayDrawer.BaseAlt + 0.3f;
			if (t is Pawn)
			{
				drawPos.x += (float)t.def.size.x - 0.52f;
				drawPos.z += (float)t.def.size.z - 0.45f;
			}
			this.RenderPulsingOverlay(t, OverlayDrawer.QuestionMarkMat, drawPos, MeshPool.plane05);
		}
	}
}
