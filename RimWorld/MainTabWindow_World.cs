using System;
using UnityEngine;

namespace RimWorld
{
	public class MainTabWindow_World : MainTabWindow
	{
		private WorldInterface worldInterface;

		public MainTabWindow_World()
		{
			this.preventCameraMotion = true;
		}

		public override void DoWindowContents(Rect fillRect)
		{
			base.DoWindowContents(fillRect);
			if (this.worldInterface == null)
			{
				this.worldInterface = new WorldInterface();
			}
			this.worldInterface.Draw(fillRect, false);
		}
	}
}
