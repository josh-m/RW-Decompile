using System;
using UnityEngine;

namespace RimWorld.Planet
{
	public class WorldRenderer
	{
		private WorldRenderMode curModeInt;

		public Texture WorldTexture
		{
			get
			{
				return this.CurMode.WorldTex;
			}
		}

		public WorldRenderMode CurMode
		{
			get
			{
				if (this.curModeInt == null)
				{
					this.curModeInt = WorldRenderModeDatabase.ModeOfType<WorldRenderMode_Full>();
				}
				return this.curModeInt;
			}
			set
			{
				this.curModeInt = value;
			}
		}

		public void Notify_WorldChanged()
		{
			foreach (WorldRenderMode current in WorldRenderModeDatabase.AllModes)
			{
				current.Notify_WorldChanged();
			}
		}

		public void Draw(WorldView view)
		{
			GUI.DrawTextureWithTexCoords(view.screenRect, this.WorldTexture, view.NormalizedTexViewCoords);
		}
	}
}
