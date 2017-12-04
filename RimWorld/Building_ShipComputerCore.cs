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
			if (ShipUtility.HasHibernatingParts(this))
			{
				yield return new Command_Action
				{
					action = delegate
					{
						DiaNode diaNode = new DiaNode("HibernateWarning".Translate());
						DiaOption diaOption = new DiaOption("Confirm".Translate());
						diaOption.action = delegate
						{
							ShipUtility.StartupHibernatingParts(this.$this);
						};
						diaOption.resolveTree = true;
						diaNode.options.Add(diaOption);
						DiaOption diaOption2 = new DiaOption("GoBack".Translate());
						diaOption2.resolveTree = true;
						diaNode.options.Add(diaOption2);
						Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, null));
					},
					defaultLabel = "CommandShipStartup".Translate(),
					defaultDesc = "CommandShipStartupDesc".Translate(),
					hotKey = KeyBindingDefOf.Misc1,
					icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true)
				};
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
				ShipCountdown.InitiateCountdown(this);
			}
		}
	}
}
