using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class OverlayDrawer
	{
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

		private const int AltitudeIndex_Forbidden = 4;

		private const int AltitudeIndex_BurningWick = 5;

		private const int AltitudeIndex_QuestionMark = 6;

		private static float SingleCellForbiddenOffset;

		private const float PulseFrequency = 4f;

		private const float PulseAmplitude = 0.7f;

		private static readonly float BaseAlt;

		private const float StackOffsetMultipiler = 0.25f;

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
				(dictionary = this.overlaysToDraw)[t] = (dictionary[t] | overlayType);
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
					if (bitCountOf != 1)
					{
						if (bitCountOf != 2)
						{
							if (bitCountOf == 3)
							{
								this.curOffset = new Vector3(-1.5f * num, 0f, 0f);
							}
						}
						else
						{
							this.curOffset = new Vector3(-0.5f * num, 0f, 0f);
						}
					}
					else
					{
						this.curOffset = Vector3.zero;
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
			this.RenderPulsingOverlay(t, OverlayDrawer.NeedsPowerMat, 2, true);
		}

		private void RenderPowerOffOverlay(Thing t)
		{
			this.RenderPulsingOverlay(t, OverlayDrawer.PowerOffMat, 3, true);
		}

		private void RenderBrokenDownOverlay(Thing t)
		{
			this.RenderPulsingOverlay(t, OverlayDrawer.BrokenDownMat, 4, true);
		}

		private void RenderOutOfFuelOverlay(Thing t)
		{
			CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
			ThingDef thingDef;
			if (compRefuelable != null && compRefuelable.Props.fuelFilter.AnyAllowedDef != null)
			{
				thingDef = compRefuelable.Props.fuelFilter.AnyAllowedDef;
			}
			else
			{
				thingDef = ThingDefOf.Chemfuel;
			}
			Material mat = MaterialPool.MatFrom(thingDef.uiIcon, ShaderDatabase.MetaOverlay, Color.white);
			this.RenderPulsingOverlay(t, mat, 5, false);
			this.RenderPulsingOverlay(t, OverlayDrawer.OutOfFuelMat, 6, true);
		}

		private void RenderPulsingOverlay(Thing thing, Material mat, int altInd, bool incrementOffset = true)
		{
			Mesh plane = MeshPool.plane08;
			this.RenderPulsingOverlay(thing, mat, altInd, plane, incrementOffset);
		}

		private void RenderPulsingOverlay(Thing thing, Material mat, int altInd, Mesh mesh, bool incrementOffset = true)
		{
			Vector3 vector = thing.TrueCenter();
			vector.y = OverlayDrawer.BaseAlt + 0.046875f * (float)altInd;
			vector += this.curOffset;
			if (incrementOffset)
			{
				this.curOffset.x = this.curOffset.x + this.StackOffsetFor(thing);
			}
			this.RenderPulsingOverlayInternal(thing, mat, vector, mesh);
		}

		private void RenderPulsingOverlayInternal(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
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
			drawPos.y = OverlayDrawer.BaseAlt + 0.1875f;
			Graphics.DrawMesh(MeshPool.plane05, drawPos, Quaternion.identity, OverlayDrawer.ForbiddenMat, 0);
		}

		private void RenderForbiddenBigOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			drawPos.y = OverlayDrawer.BaseAlt + 0.1875f;
			Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, OverlayDrawer.ForbiddenMat, 0);
		}

		private void RenderBurningWick(Thing parent)
		{
			Material material;
			if ((parent.thingIDNumber + Find.TickManager.TicksGame) % 6 < 3)
			{
				material = OverlayDrawer.WickMaterialA;
			}
			else
			{
				material = OverlayDrawer.WickMaterialB;
			}
			Vector3 drawPos = parent.DrawPos;
			drawPos.y = OverlayDrawer.BaseAlt + 0.234375f;
			Graphics.DrawMesh(MeshPool.plane20, drawPos, Quaternion.identity, material, 0);
		}

		private void RenderQuestionMarkOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			drawPos.y = OverlayDrawer.BaseAlt + 0.28125f;
			if (t is Pawn)
			{
				drawPos.x += (float)t.def.size.x - 0.52f;
				drawPos.z += (float)t.def.size.z - 0.45f;
			}
			this.RenderPulsingOverlayInternal(t, OverlayDrawer.QuestionMarkMat, drawPos, MeshPool.plane05);
		}
	}
}
