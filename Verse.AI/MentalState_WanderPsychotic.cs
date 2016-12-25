using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public class MentalState_WanderPsychotic : MentalState
	{
		private const float DropRandomItemMTBHours = 3f;

		private const int DropRandomItemCheckInterval = 100;

		private static List<Action> actions = new List<Action>();

		public override void MentalStateTick()
		{
			if (this.pawn.IsHashIntervalTick(100) && Rand.MTBEventOccurs(3f, 2500f, 100f))
			{
				this.TryDropRandomItem();
			}
			base.MentalStateTick();
		}

		private void TryDropRandomItem()
		{
			if (!this.pawn.Spawned)
			{
				return;
			}
			MentalState_WanderPsychotic.actions.Clear();
			if (this.pawn.apparel != null && this.pawn.apparel.WornApparelCount > 0)
			{
				MentalState_WanderPsychotic.actions.Add(delegate
				{
					this.pawn.jobs.StopAll(false);
					this.pawn.jobs.StartJob(new Job(JobDefOf.RemoveApparel, this.pawn.apparel.WornApparel.RandomElement<Apparel>()), JobCondition.None, null, false, true, null);
				});
			}
			if (this.pawn.equipment != null && this.pawn.equipment.AllEquipment.Any<ThingWithComps>())
			{
				MentalState_WanderPsychotic.actions.Add(delegate
				{
					ThingWithComps thingWithComps;
					this.pawn.equipment.TryDropEquipment(this.pawn.equipment.AllEquipment.RandomElement<ThingWithComps>(), out thingWithComps, this.pawn.Position, true);
				});
			}
			if (this.pawn.inventory != null && this.pawn.inventory.container.Count > 0)
			{
				IEnumerable<Thing> candidates = from x in this.pawn.inventory.container
				where !x.def.destroyOnDrop
				select x;
				if (candidates.Any<Thing>())
				{
					MentalState_WanderPsychotic.actions.Add(delegate
					{
						Thing thing;
						this.pawn.inventory.container.TryDrop(candidates.RandomElement<Thing>(), this.pawn.Position, ThingPlaceMode.Near, out thing, null);
					});
				}
			}
			if (!MentalState_WanderPsychotic.actions.Any<Action>())
			{
				return;
			}
			MentalState_WanderPsychotic.actions.RandomElement<Action>()();
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
