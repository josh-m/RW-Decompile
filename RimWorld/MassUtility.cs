using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class MassUtility
	{
		public const float MassCapacityPerBodySize = 35f;

		public static float EncumbrancePercent(Pawn pawn)
		{
			return Mathf.Clamp01(MassUtility.UnboundedEncumbrancePercent(pawn));
		}

		public static float UnboundedEncumbrancePercent(Pawn pawn)
		{
			return MassUtility.GearAndInventoryMass(pawn) / MassUtility.Capacity(pawn);
		}

		public static bool IsOverEncumbered(Pawn pawn)
		{
			return MassUtility.UnboundedEncumbrancePercent(pawn) > 1f;
		}

		public static bool WillBeOverEncumberedAfterPickingUp(Pawn pawn, Thing thing, int count)
		{
			return MassUtility.FreeSpace(pawn) < (float)count * thing.GetInnerIfMinified().GetStatValue(StatDefOf.Mass, true);
		}

		public static int CountToPickUpUntilOverEncumbered(Pawn pawn, Thing thing)
		{
			return Mathf.FloorToInt(MassUtility.FreeSpace(pawn) / thing.GetInnerIfMinified().GetStatValue(StatDefOf.Mass, true));
		}

		public static float FreeSpace(Pawn pawn)
		{
			return Mathf.Max(MassUtility.Capacity(pawn) - MassUtility.GearAndInventoryMass(pawn), 0f);
		}

		public static float GearAndInventoryMass(Pawn pawn)
		{
			return MassUtility.GearMass(pawn) + MassUtility.InventoryMass(pawn);
		}

		public static float GearMass(Pawn p)
		{
			float num = 0f;
			if (p.apparel != null)
			{
				List<Apparel> wornApparel = p.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					num += wornApparel[i].GetStatValue(StatDefOf.Mass, true);
				}
			}
			if (p.equipment != null)
			{
				foreach (ThingWithComps current in p.equipment.AllEquipment)
				{
					num += current.GetStatValue(StatDefOf.Mass, true);
				}
			}
			return num;
		}

		public static float InventoryMass(Pawn p)
		{
			float num = 0f;
			for (int i = 0; i < p.inventory.innerContainer.Count; i++)
			{
				Thing thing = p.inventory.innerContainer[i];
				num += (float)thing.stackCount * thing.GetInnerIfMinified().GetStatValue(StatDefOf.Mass, true);
			}
			return num;
		}

		public static float Capacity(Pawn p)
		{
			if (!MassUtility.CanEverCarryAnything(p))
			{
				return 0f;
			}
			return p.BodySize * 35f;
		}

		public static bool CanEverCarryAnything(Pawn p)
		{
			return p.RaceProps.ToolUser || p.RaceProps.packAnimal;
		}
	}
}
