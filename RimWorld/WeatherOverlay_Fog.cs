using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WeatherOverlay_Fog : SkyOverlay
	{
		private static readonly Material FogOverlayWorld = MatLoader.LoadMat("Weather/FogOverlayWorld");

		public WeatherOverlay_Fog()
		{
			this.worldOverlayMat = WeatherOverlay_Fog.FogOverlayWorld;
			this.worldOverlayPanSpeed1 = 0.0005f;
			this.worldOverlayPanSpeed2 = 0.0004f;
			this.worldPanDir1 = new Vector2(1f, 1f);
			this.worldPanDir2 = new Vector2(1f, -1f);
		}
	}
}
