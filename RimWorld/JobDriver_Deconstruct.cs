using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class JobDriver_Deconstruct : JobDriver_RemoveBuilding
	{
		private const int MaxDeconstructWork = 3000;

		protected override DesignationDef Designation
		{
			get
			{
				return DesignationDefOf.Deconstruct;
			}
		}

		protected override int TotalNeededWork
		{
			get
			{
				Building building = base.Building;
				int value = Mathf.RoundToInt(building.GetStatValue(StatDefOf.WorkToBuild, true));
				return Mathf.Clamp(value, 20, 3000);
			}
		}

		protected override void FinishedRemoving()
		{
			base.Target.Destroy(DestroyMode.Deconstruct);
			this.pawn.records.Increment(RecordDefOf.ThingsDeconstructed);
		}

		protected override void TickAction()
		{
			this.pawn.skills.Learn(SkillDefOf.Construction, 0.275f, false);
		}
	}
}
