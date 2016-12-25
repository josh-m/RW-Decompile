using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class UI_BackgroundMain : UIMenuBackground
	{
		private static readonly Vector2 BGPlanetSize = new Vector2(2048f, 1280f);

		private static readonly Texture2D BGPlanet = ContentFinder<Texture2D>.Get("UI/HeroArt/BGPlanet", true);

		public override void BackgroundOnGUI()
		{
			bool flag = true;
			if ((float)UI.screenWidth > (float)UI.screenHeight * (UI_BackgroundMain.BGPlanetSize.x / UI_BackgroundMain.BGPlanetSize.y))
			{
				flag = false;
			}
			Rect position;
			if (flag)
			{
				float height = (float)UI.screenHeight;
				float num = (float)UI.screenHeight * (UI_BackgroundMain.BGPlanetSize.x / UI_BackgroundMain.BGPlanetSize.y);
				position = new Rect((float)(UI.screenWidth / 2) - num / 2f, 0f, num, height);
			}
			else
			{
				float width = (float)UI.screenWidth;
				float num2 = (float)UI.screenWidth * (UI_BackgroundMain.BGPlanetSize.y / UI_BackgroundMain.BGPlanetSize.x);
				position = new Rect(0f, (float)(UI.screenHeight / 2) - num2 / 2f, width, num2);
			}
			GUI.DrawTexture(position, UI_BackgroundMain.BGPlanet, ScaleMode.ScaleToFit);
		}
	}
}
