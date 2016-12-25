using System;
using UnityEngine;

namespace Verse
{
	public class Graphic_Mote : Graphic_Single
	{
		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
		{
			Mote mote = (Mote)thing;
			ThingDef def = mote.def;
			float ageSecs = mote.AgeSecs;
			float num = 1f;
			if (def.mote.fadeInTime != 0f && ageSecs <= def.mote.fadeInTime)
			{
				num = ageSecs / def.mote.fadeInTime;
			}
			else if (ageSecs < def.mote.fadeInTime + def.mote.solidTime)
			{
				num = 1f;
			}
			else if (def.mote.fadeOutTime != 0f)
			{
				num = 1f - (ageSecs - def.mote.fadeInTime - def.mote.solidTime) / def.mote.fadeOutTime;
			}
			if (num <= 0f)
			{
				return;
			}
			Color color = base.Color * mote.instanceColor;
			color.a *= num;
			Material material = this.MatSingle;
			if (color != material.color)
			{
				material = MaterialPool.MatFrom((Texture2D)material.mainTexture, material.shader, color);
			}
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(mote.DrawPos, Quaternion.AngleAxis(mote.exactRotation, Vector3.up), mote.exactScale);
			Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Mote(path=",
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
