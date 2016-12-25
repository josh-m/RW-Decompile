using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	internal class Building_ShipComputerCore : Building
	{
		private bool CanLaunchNow
		{
			get
			{
				return !ShipUtility.LaunchFailReasons(this).Any<string>();
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo c in base.GetGizmos())
			{
				yield return c;
			}
			Command_Action launch = new Command_Action();
			launch.action = new Action(this.TryLaunch);
			launch.defaultLabel = "CommandShipLaunch".Translate();
			launch.defaultDesc = "CommandShipLaunchDesc".Translate();
			if (!this.CanLaunchNow)
			{
				launch.Disable(ShipUtility.LaunchFailReasons(this).First<string>());
			}
			if (ShipCountdown.CountingDown)
			{
				launch.Disable(null);
			}
			launch.hotKey = KeyBindingDefOf.Misc1;
			launch.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true);
			yield return launch;
		}

		private void TryLaunch()
		{
			if (this.CanLaunchNow)
			{
				ShipCountdown.InitiateCountdown(this, -1);
			}
		}
	}
}
