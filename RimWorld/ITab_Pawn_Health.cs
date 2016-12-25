using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Pawn_Health : ITab
	{
		private const int HideBloodLossTicksThreshold = 60000;

		public const float Width = 630f;

		private Pawn PawnForHealth
		{
			get
			{
				if (base.SelPawn != null)
				{
					return base.SelPawn;
				}
				Corpse corpse = base.SelThing as Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
				return null;
			}
		}

		public ITab_Pawn_Health()
		{
			this.size = new Vector2(630f, 430f);
			this.labelKey = "TabHealth";
			this.tutorTag = "Health";
		}

		protected override void FillTab()
		{
			Pawn pawnForHealth = this.PawnForHealth;
			if (pawnForHealth == null)
			{
				Log.Error("Health tab found no selected pawn to display.");
				return;
			}
			Corpse corpse = base.SelThing as Corpse;
			bool showBloodLoss = corpse == null || corpse.Age < 60000;
			Rect outRect = new Rect(0f, 20f, this.size.x, this.size.y - 20f);
			HealthCardUtility.DrawPawnHealthCard(outRect, pawnForHealth, this.ShouldAllowOperations(), showBloodLoss, base.SelThing);
		}

		private bool ShouldAllowOperations()
		{
			Pawn pawnForHealth = this.PawnForHealth;
			if (pawnForHealth.Dead)
			{
				return false;
			}
			return base.SelThing.def.AllRecipes.Any((RecipeDef x) => x.AvailableNow) && (pawnForHealth.Faction == Faction.OfPlayer || (pawnForHealth.IsPrisonerOfColony || (pawnForHealth.HostFaction == Faction.OfPlayer && !pawnForHealth.health.capacities.CapableOf(PawnCapacityDefOf.Moving))) || ((!pawnForHealth.RaceProps.IsFlesh || pawnForHealth.Faction == null || !pawnForHealth.Faction.HostileTo(Faction.OfPlayer)) && (!pawnForHealth.RaceProps.Humanlike && pawnForHealth.Downed)));
		}
	}
}
