using System;
using UnityEngine;

namespace Verse
{
	public class MoteText : MoteThrown
	{
		public string text;

		public Color textColor = Color.white;

		public float overrideTimeBeforeStartFadeout = -1f;

		protected float TimeBeforeStartFadeout
		{
			get
			{
				return (this.overrideTimeBeforeStartFadeout < 0f) ? this.def.mote.solidTime : this.overrideTimeBeforeStartFadeout;
			}
		}

		protected override float LifespanSecs
		{
			get
			{
				return this.TimeBeforeStartFadeout + this.def.mote.fadeOutTime;
			}
		}

		public override void Draw()
		{
		}

		public override void DrawGUIOverlay()
		{
			float a = 1f - (base.AgeSecs - this.TimeBeforeStartFadeout) / this.def.mote.fadeOutTime;
			Color color = new Color(this.textColor.r, this.textColor.g, this.textColor.b, a);
			GenMapUI.DrawText(new Vector2(this.exactPosition.x, this.exactPosition.z), this.text, color);
		}
	}
}
