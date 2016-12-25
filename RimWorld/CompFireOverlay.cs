using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompFireOverlay : ThingComp
	{
		private static readonly Graphic FireGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Fire", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);

		public CompProperties_FireOverlay Props
		{
			get
			{
				return (CompProperties_FireOverlay)this.props;
			}
		}

		public override void PostDraw()
		{
			base.PostDraw();
			Vector3 drawPos = this.parent.DrawPos;
			drawPos.y += 0.05f;
			CompFireOverlay.FireGraphic.Draw(drawPos, Rot4.North, this.parent);
		}
	}
}
