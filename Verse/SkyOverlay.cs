using System;
using UnityEngine;

namespace Verse
{
	public abstract class SkyOverlay
	{
		public Material worldOverlayMat;

		public Material screenOverlayMat;

		protected float worldOverlayPanSpeed1;

		protected float worldOverlayPanSpeed2;

		protected Vector2 worldPanDir1;

		protected Vector2 worldPanDir2;

		public Color OverlayColor
		{
			set
			{
				if (this.worldOverlayMat != null)
				{
					this.worldOverlayMat.color = value;
				}
				if (this.screenOverlayMat != null)
				{
					this.screenOverlayMat.color = value;
				}
			}
		}

		public SkyOverlay()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				this.OverlayColor = Color.clear;
			});
		}

		public virtual void TickOverlay(Map map)
		{
			if (this.worldOverlayMat != null)
			{
				this.worldOverlayMat.SetTextureOffset("_MainTex", (float)Find.TickManager.TicksGame * this.worldPanDir1 * -1f * this.worldOverlayPanSpeed1 * this.worldOverlayMat.GetTextureScale("_MainTex").x);
				if (this.worldOverlayMat.HasProperty("_MainTex2"))
				{
					this.worldOverlayMat.SetTextureOffset("_MainTex2", (float)Find.TickManager.TicksGame * this.worldPanDir2 * -1f * this.worldOverlayPanSpeed2 * this.worldOverlayMat.GetTextureScale("_MainTex2").x);
				}
			}
		}

		public void DrawOverlay(Map map)
		{
			if (this.worldOverlayMat != null)
			{
				Vector3 position = map.Center.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather);
				Graphics.DrawMesh(MeshPool.wholeMapPlane, position, Quaternion.identity, this.worldOverlayMat, 0);
			}
			if (this.screenOverlayMat != null)
			{
				float num = Find.Camera.orthographicSize * 2f;
				Vector3 s = new Vector3(num * Find.Camera.aspect, 1f, num);
				Vector3 position2 = Find.Camera.transform.position;
				position2.y = Altitudes.AltitudeFor(AltitudeLayer.Weather) + 0.05f;
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(position2, Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, this.screenOverlayMat, 0);
			}
		}

		public override string ToString()
		{
			if (this.worldOverlayMat != null)
			{
				return this.worldOverlayMat.name;
			}
			if (this.screenOverlayMat != null)
			{
				return this.screenOverlayMat.name;
			}
			return "NoOverlayOverlay";
		}
	}
}
