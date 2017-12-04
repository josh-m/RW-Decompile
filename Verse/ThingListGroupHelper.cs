using RimWorld;
using System;
using Verse.AI;

namespace Verse
{
	public static class ThingListGroupHelper
	{
		public static readonly ThingRequestGroup[] AllGroups;

		static ThingListGroupHelper()
		{
			int length = Enum.GetValues(typeof(ThingRequestGroup)).Length;
			ThingListGroupHelper.AllGroups = new ThingRequestGroup[length];
			int num = 0;
			foreach (object current in Enum.GetValues(typeof(ThingRequestGroup)))
			{
				ThingListGroupHelper.AllGroups[num] = (ThingRequestGroup)current;
				num++;
			}
		}

		public static bool Includes(this ThingRequestGroup group, ThingDef def)
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
				return def.EverHaulable;
			case ThingRequestGroup.HaulableAlways:
				return def.alwaysHaulable;
			case ThingRequestGroup.FoodSource:
				return def.IsNutritionGivingIngestible || def.thingClass == typeof(Building_NutrientPasteDispenser);
			case ThingRequestGroup.FoodSourceNotPlantOrTree:
				return (def.IsNutritionGivingIngestible && (def.ingestible.foodType & ~FoodTypeFlags.Plant & ~FoodTypeFlags.Tree) != FoodTypeFlags.None) || def.thingClass == typeof(Building_NutrientPasteDispenser);
			case ThingRequestGroup.Corpse:
				return def.thingClass == typeof(Corpse);
			case ThingRequestGroup.Blueprint:
				return def.IsBlueprint;
			case ThingRequestGroup.BuildingArtificial:
				return def.IsBuildingArtificial;
			case ThingRequestGroup.BuildingFrame:
				return def.IsFrame;
			case ThingRequestGroup.Pawn:
				return def.category == ThingCategory.Pawn;
			case ThingRequestGroup.PotentialBillGiver:
				return !def.AllRecipes.NullOrEmpty<RecipeDef>();
			case ThingRequestGroup.Medicine:
				return def.IsMedicine;
			case ThingRequestGroup.Filth:
				return def.filth != null;
			case ThingRequestGroup.AttackTarget:
				return typeof(IAttackTarget).IsAssignableFrom(def.thingClass);
			case ThingRequestGroup.Weapon:
				return def.IsWeapon;
			case ThingRequestGroup.Refuelable:
				return def.HasComp(typeof(CompRefuelable));
			case ThingRequestGroup.HaulableEverOrMinifiable:
				return def.EverHaulable || def.Minifiable;
			case ThingRequestGroup.Drug:
				return def.IsDrug;
			case ThingRequestGroup.Shell:
				return def.IsShell;
			case ThingRequestGroup.HarvestablePlant:
				return def.category == ThingCategory.Plant && def.plant.Harvestable;
			case ThingRequestGroup.Fire:
				return typeof(Fire).IsAssignableFrom(def.thingClass);
			case ThingRequestGroup.Plant:
				return def.category == ThingCategory.Plant;
			case ThingRequestGroup.Construction:
				return def.IsBlueprint || def.IsFrame;
			case ThingRequestGroup.HasGUIOverlay:
				return def.drawGUIOverlay;
			case ThingRequestGroup.Apparel:
				return def.IsApparel;
			case ThingRequestGroup.MinifiedThing:
				return typeof(MinifiedThing).IsAssignableFrom(def.thingClass);
			case ThingRequestGroup.Grave:
				return typeof(Building_Grave).IsAssignableFrom(def.thingClass);
			case ThingRequestGroup.Art:
				return def.HasComp(typeof(CompArt));
			case ThingRequestGroup.ThingHolder:
				return def.ThisOrAnyCompIsThingHolder();
			case ThingRequestGroup.ActiveDropPod:
				return typeof(IActiveDropPod).IsAssignableFrom(def.thingClass);
			case ThingRequestGroup.Transporter:
				return def.HasComp(typeof(CompTransporter));
			case ThingRequestGroup.LongRangeMineralScanner:
				return def.HasComp(typeof(CompLongRangeMineralScanner));
			case ThingRequestGroup.AffectsSky:
				return def.HasComp(typeof(CompAffectsSky));
			case ThingRequestGroup.PsychicDroneEmanator:
				return def.HasComp(typeof(CompPsychicDrone));
			case ThingRequestGroup.WindSource:
				return def.HasComp(typeof(CompWindSource));
			case ThingRequestGroup.AlwaysFlee:
				return def.alwaysFlee;
			default:
				throw new ArgumentException("group");
			}
		}
	}
}
