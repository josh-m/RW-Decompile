using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class TexCommand
	{
		public static readonly Texture2D DesirePower = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true);

		public static readonly Texture2D Draft = ContentFinder<Texture2D>.Get("UI/Commands/Draft", true);

		public static readonly Texture2D ReleaseAnimals = ContentFinder<Texture2D>.Get("UI/Commands/ReleaseAnimals", true);

		public static readonly Texture2D HoldOpen = ContentFinder<Texture2D>.Get("UI/Commands/HoldOpen", true);

		public static readonly Texture2D GatherSpotActive = ContentFinder<Texture2D>.Get("UI/Commands/GatherSpotActive", true);

		public static readonly Texture2D Install = ContentFinder<Texture2D>.Get("UI/Commands/Install", true);

		public static readonly Texture2D SquadAttack = ContentFinder<Texture2D>.Get("UI/Commands/SquadAttack", true);

		public static readonly Texture2D AttackMelee = ContentFinder<Texture2D>.Get("UI/Commands/AttackMelee", true);

		public static readonly Texture2D Attack = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);

		public static readonly Texture2D FireAtWill = ContentFinder<Texture2D>.Get("UI/Commands/FireAtWill", true);

		public static readonly Texture2D ToggleVent = ContentFinder<Texture2D>.Get("UI/Commands/Vent", true);

		public static readonly Texture2D PauseCaravan = ContentFinder<Texture2D>.Get("UI/Commands/PauseCaravan", true);

		public static readonly Texture2D ForbidOff = ContentFinder<Texture2D>.Get("UI/Designators/ForbidOff", true);

		public static readonly Texture2D ForbidOn = ContentFinder<Texture2D>.Get("UI/Designators/ForbidOn", true);

		public static readonly Texture2D RearmTrap = ContentFinder<Texture2D>.Get("UI/Designators/RearmTrap", true);

		public static readonly Texture2D TreeChop = ContentFinder<Texture2D>.Get("UI/Designators/HarvestWood", true);

		public static readonly Texture2D CannotShoot = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);

		public static readonly Texture2D ClearPrioritizedWork = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);

		public static readonly Texture2D RemoveRoutePlannerWaypoint = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
	}
}
