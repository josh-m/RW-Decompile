using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	internal class AlertArrow
	{
		private const float ArrowMoveTime = 1.5f;

		private const float ArrowMoveDist = 300f;

		private Alert alert;

		private float lastAlertStartTime = -9999f;

		private float ArrowSize
		{
			get
			{
				return 21f;
			}
		}

		private float TimeSinceAppear
		{
			get
			{
				return Time.time - this.lastAlertStartTime;
			}
		}

		public AlertArrow(Alert alert)
		{
			this.alert = alert;
		}

		public void DoAlertStartEffect()
		{
			if (Find.TickManager.TicksGame < 100)
			{
				this.lastAlertStartTime = -999f;
			}
			else
			{
				this.lastAlertStartTime = Time.time;
			}
		}

		public void AlertArrowOnGUI(float attachX, float y)
		{
			if (this.TimeSinceAppear > 8f)
			{
				return;
			}
			Color arrowColor = this.alert.ArrowColor;
			float x;
			if (this.TimeSinceAppear < 1.5f)
			{
				float num = this.TimeSinceAppear / 1.5f;
				arrowColor.a = num;
				float num2 = (1f - num) * 300f;
				x = attachX - this.ArrowSize - num2;
			}
			else
			{
				x = attachX - this.ArrowSize;
			}
			GUI.color = arrowColor;
			GUI.DrawTexture(new Rect(x, y, this.ArrowSize, this.ArrowSize), TexUI.ArrowTex);
			GUI.color = Color.white;
		}
	}
}
