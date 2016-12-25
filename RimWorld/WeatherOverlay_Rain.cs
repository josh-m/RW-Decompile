using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WeatherOverlay_Rain : SkyOverlay
	{
		private static readonly Material RainOverlayWorld = MatLoader.LoadMat("Weather/RainOverlayWorld");

		public WeatherOverlay_Rain()
		{
			this.worldOverlayMat = WeatherOverlay_Rain.RainOverlayWorld;
			this.worldOverlayPanSpeed1 = 0.015f;
			this.worldPanDir1 = new Vector2(-0.25f, -1f);
			this.worldPanDir1.Normalize();
			this.worldOverlayPanSpeed2 = 0.022f;
			this.worldPanDir2 = new Vector2(-0.24f, -1f);
			this.worldPanDir2.Normalize();
		}
	}
}
