using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class Graphic_MoteSplash : Graphic_Mote
	{
		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
		{
			MoteSplash moteSplash = (MoteSplash)thing;
			float num = moteSplash.CalculatedAlpha();
			if (num <= 0f)
			{
				return;
			}
			Color color = new Color(1f, 1f, 1f, num);
			Vector3 exactScale = moteSplash.exactScale;
			exactScale.x *= this.data.drawSize.x;
			exactScale.z *= this.data.drawSize.y;
			Matrix4x4 rhs = default(Matrix4x4);
			rhs.SetTRS(moteSplash.DrawPos, Quaternion.AngleAxis(moteSplash.exactRotation, Vector3.up), exactScale);
			Matrix4x4 matrix = Find.Camera.cameraToWorldMatrix * Find.Camera.projectionMatrix * Find.Camera.worldToCameraMatrix * rhs;
			Material matSingle = this.MatSingle;
			matSingle.SetColor(ShaderPropertyIDs.Color, color);
			matSingle.SetFloat(ShaderPropertyIDs.ShockwaveSpan, moteSplash.CalculatedShockwaveSpan());
			matSingle.SetPass(0);
			Graphics.DrawMeshNow(MeshPool.plane10, matrix);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"MoteSplash(path=",
				this.path,
				", shader=",
				base.Shader,
				", color=",
				this.color,
				", colorTwo=unsupported)"
			});
		}
	}
}
