using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ShipUtility
	{
		private static Dictionary<ThingDef, int> requiredParts;

		private static List<Building> closedSet = new List<Building>();

		private static List<Building> openSet = new List<Building>();

		public static Dictionary<ThingDef, int> RequiredParts()
		{
			if (ShipUtility.requiredParts == null)
			{
				ShipUtility.requiredParts = new Dictionary<ThingDef, int>();
				ShipUtility.requiredParts[ThingDefOf.Ship_CryptosleepCasket] = 1;
				ShipUtility.requiredParts[ThingDefOf.Ship_ComputerCore] = 1;
				ShipUtility.requiredParts[ThingDefOf.Ship_Reactor] = 1;
				ShipUtility.requiredParts[ThingDefOf.Ship_Engine] = 3;
				ShipUtility.requiredParts[ThingDefOf.Ship_Beam] = 1;
				ShipUtility.requiredParts[ThingDefOf.Ship_SensorCluster] = 1;
			}
			return ShipUtility.requiredParts;
		}

		[DebuggerHidden]
		public static IEnumerable<string> LaunchFailReasons(Building rootBuilding)
		{
			List<Building> shipParts = ShipUtility.ShipBuildingsAttachedTo(rootBuilding).ToList<Building>();
			foreach (KeyValuePair<ThingDef, int> partDef in ShipUtility.RequiredParts())
			{
				int shipPartCount = shipParts.Count((Building pa) => pa.def == partDef.Key);
				if (shipPartCount < partDef.Value)
				{
					yield return string.Format("{0}: {1}x {2} ({3} {4})", new object[]
					{
						"ShipReportMissingPart".Translate(),
						partDef.Value - shipPartCount,
						partDef.Key.label,
						"ShipReportMissingPartRequires".Translate(),
						partDef.Value
					});
				}
			}
			bool fullPodFound = false;
			foreach (Building current in shipParts)
			{
				if (current.def == ThingDefOf.Ship_CryptosleepCasket)
				{
					Building_CryptosleepCasket building_CryptosleepCasket = current as Building_CryptosleepCasket;
					if (building_CryptosleepCasket != null && building_CryptosleepCasket.HasAnyContents)
					{
						fullPodFound = true;
						break;
					}
				}
			}
			foreach (Building part in shipParts)
			{
				CompHibernatable hibernatable = part.TryGetComp<CompHibernatable>();
				if (hibernatable != null && hibernatable.State == HibernatableStateDefOf.Hibernating)
				{
					yield return string.Format("{0}: {1}", "ShipReportHibernating".Translate(), part.LabelCap);
				}
				if (hibernatable != null && !hibernatable.Running)
				{
					yield return string.Format("{0}: {1}", "ShipReportNotReady".Translate(), part.LabelCap);
				}
			}
			if (!fullPodFound)
			{
				yield return "ShipReportNoFullPods".Translate();
			}
		}

		public static bool HasHibernatingParts(Building rootBuilding)
		{
			List<Building> list = ShipUtility.ShipBuildingsAttachedTo(rootBuilding).ToList<Building>();
			foreach (Building current in list)
			{
				CompHibernatable compHibernatable = current.TryGetComp<CompHibernatable>();
				if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Hibernating)
				{
					return true;
				}
			}
			return false;
		}

		public static void StartupHibernatingParts(Building rootBuilding)
		{
			List<Building> list = ShipUtility.ShipBuildingsAttachedTo(rootBuilding).ToList<Building>();
			foreach (Building current in list)
			{
				CompHibernatable compHibernatable = current.TryGetComp<CompHibernatable>();
				if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Hibernating)
				{
					compHibernatable.Startup();
				}
			}
		}

		public static List<Building> ShipBuildingsAttachedTo(Building root)
		{
			ShipUtility.closedSet.Clear();
			if (root == null || root.Destroyed)
			{
				return ShipUtility.closedSet;
			}
			ShipUtility.openSet.Clear();
			ShipUtility.openSet.Add(root);
			while (ShipUtility.openSet.Count > 0)
			{
				Building building = ShipUtility.openSet[ShipUtility.openSet.Count - 1];
				ShipUtility.openSet.Remove(building);
				ShipUtility.closedSet.Add(building);
				foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(building))
				{
					Building edifice = current.GetEdifice(building.Map);
					if (edifice != null && edifice.def.building.shipPart && !ShipUtility.closedSet.Contains(edifice) && !ShipUtility.openSet.Contains(edifice))
					{
						ShipUtility.openSet.Add(edifice);
					}
				}
			}
			return ShipUtility.closedSet;
		}

		[DebuggerHidden]
		public static IEnumerable<Gizmo> ShipStartupGizmos(Building building)
		{
			if (ShipUtility.HasHibernatingParts(building))
			{
				yield return new Command_Action
				{
					action = delegate
					{
						string text = "HibernateWarning";
						if (building.Map.info.parent.GetComponent<EscapeShipComp>() == null)
						{
							text += "Standalone";
						}
						if (!Find.Storyteller.difficulty.allowBigThreats)
						{
							text += "Pacifist";
						}
						DiaNode diaNode = new DiaNode(text.Translate());
						DiaOption diaOption = new DiaOption("Confirm".Translate());
						diaOption.action = delegate
						{
							ShipUtility.StartupHibernatingParts(building);
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
		}
	}
}
