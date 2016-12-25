using System;

namespace Verse
{
	public static class ThingRequestGroupUtility
	{
		public static bool StoreInRegion(this ThingRequestGroup group)
		{
			switch (group)
			{
			case ThingRequestGroup.Undefined:
				return false;
			case ThingRequestGroup.Nothing:
				return false;
			case ThingRequestGroup.Everything:
				return true;
			case ThingRequestGroup.HaulableEver:
				return true;
			case ThingRequestGroup.HaulableAlways:
				return true;
			case ThingRequestGroup.Plant:
				return false;
			case ThingRequestGroup.FoodSource:
				return true;
			case ThingRequestGroup.FoodSourceNotPlantOrTree:
				return true;
			case ThingRequestGroup.Corpse:
				return true;
			case ThingRequestGroup.Blueprint:
				return true;
			case ThingRequestGroup.BuildingArtificial:
				return true;
			case ThingRequestGroup.BuildingFrame:
				return true;
			case ThingRequestGroup.Pawn:
				return true;
			case ThingRequestGroup.PotentialBillGiver:
				return true;
			case ThingRequestGroup.Medicine:
				return true;
			case ThingRequestGroup.Filth:
				return true;
			case ThingRequestGroup.AttackTarget:
				return true;
			case ThingRequestGroup.Weapon:
				return true;
			case ThingRequestGroup.Refuelable:
				return true;
			case ThingRequestGroup.HaulableEverOrMinifiable:
				return true;
			case ThingRequestGroup.Drug:
				return true;
			case ThingRequestGroup.Construction:
				return false;
			case ThingRequestGroup.HasGUIOverlay:
				return false;
			case ThingRequestGroup.Apparel:
				return false;
			case ThingRequestGroup.MinifiedThing:
				return false;
			case ThingRequestGroup.Grave:
				return false;
			case ThingRequestGroup.Art:
				return false;
			case ThingRequestGroup.ContainerEnclosure:
				return false;
			case ThingRequestGroup.ActiveDropPod:
				return false;
			case ThingRequestGroup.Transporter:
				return false;
			default:
				throw new ArgumentException("group");
			}
		}
	}
}
