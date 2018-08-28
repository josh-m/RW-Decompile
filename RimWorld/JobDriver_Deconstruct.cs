using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Deconstruct : JobDriver_RemoveBuilding
	{
		private const float MaxDeconstructWork = 3000f;

		private const float MinDeconstructWork = 20f;

		protected override DesignationDef Designation
		{
			get
			{
				return DesignationDefOf.Deconstruct;
			}
		}

		protected override float TotalNeededWork
		{
			get
			{
				Building building = base.Building;
				float statValue = building.GetStatValue(StatDefOf.WorkToBuild, true);
				return Mathf.Clamp(statValue, 20f, 3000f);
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => this.$this.Building == null || !this.$this.Building.DeconstructibleBy(this.$this.pawn.Faction));
			foreach (Toil t in base.MakeNewToils())
			{
				yield return t;
			}
		}

		protected override void FinishedRemoving()
		{
			base.Target.Destroy(DestroyMode.Deconstruct);
			this.pawn.records.Increment(RecordDefOf.ThingsDeconstructed);
		}

		protected override void TickAction()
		{
			if (base.Building.def.CostListAdjusted(base.Building.Stuff, true).Count > 0)
			{
				this.pawn.skills.Learn(SkillDefOf.Construction, 0.25f, false);
			}
		}
	}
}
