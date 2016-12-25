using System;

namespace Verse
{
	public enum ThingRequestGroup : byte
	{
		Undefined,
		Nothing,
		Everything,
		HaulableEver,
		HaulableAlways,
		Plant,
		FoodSource,
		FoodSourceNotPlantOrTree,
		Corpse,
		Blueprint,
		BuildingArtificial,
		BuildingFrame,
		Pawn,
		PotentialBillGiver,
		Medicine,
		Filth,
		AttackTarget,
		Weapon,
		Refuelable,
		HaulableEverOrMinifiable,
		Drug,
		Construction,
		HasGUIOverlay,
		Apparel,
		MinifiedThing,
		Grave,
		Art,
		ContainerEnclosure,
		ActiveDropPod,
		Transporter
	}
}
