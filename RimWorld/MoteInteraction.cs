using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MoteInteraction : MoteDualAttached
	{
		private Material iconMat;

		private Pawn target;

		private static readonly Material InteractionArrowTex = MaterialPool.MatFrom("Things/Mote/InteractionArrow");

		public override void Draw()
		{
			base.Draw();
			if (this.iconMat != null)
			{
				Vector3 drawPos = this.DrawPos;
				drawPos.y += 0.01f;
				Graphics.DrawMesh(MeshPool.plane08, drawPos, Quaternion.identity, this.iconMat, 0);
			}
			if (this.target != null)
			{
				Vector3 vector = this.DrawPos;
				vector.y -= 0.01f;
				Vector3 a = this.target.TrueCenter();
				float angle = (a - vector).AngleFlat();
				Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
				vector += 0.6f * (rotation * Vector3.forward);
				Graphics.DrawMesh(MeshPool.plane05, vector, rotation, MoteInteraction.InteractionArrowTex, 0);
			}
		}

		public void SetupInteractionMote(Texture2D icon, Pawn target)
		{
			this.iconMat = MaterialPool.MatFrom(icon, ShaderDatabase.Cutout, Color.white);
			this.target = target;
		}
	}
}
