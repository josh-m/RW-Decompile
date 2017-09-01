using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class DefeatAllEnemiesQuestComp : WorldObjectComp, IThingHolder
	{
		private bool active;

		public Faction requestingFaction;

		public float relationsImprovement;

		public ThingOwner rewards;

		private static List<Thing> tmpRewards = new List<Thing>();

		public bool Active
		{
			get
			{
				return this.active;
			}
		}

		public DefeatAllEnemiesQuestComp()
		{
			this.rewards = new ThingOwner<Thing>(this);
		}

		public void StartQuest(Faction requestingFaction, float relationsImprovement, List<Thing> rewards)
		{
			this.StopQuest();
			this.active = true;
			this.requestingFaction = requestingFaction;
			this.relationsImprovement = relationsImprovement;
			this.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
			this.rewards.TryAddRange(rewards, true);
		}

		public void StopQuest()
		{
			this.active = false;
			this.requestingFaction = null;
			this.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
		}

		public override void CompTick()
		{
			base.CompTick();
			if (this.active)
			{
				MapParent mapParent = this.parent as MapParent;
				if (mapParent != null)
				{
					this.CheckAllEnemiesDefeated(mapParent);
				}
			}
		}

		private void CheckAllEnemiesDefeated(MapParent mapParent)
		{
			if (!mapParent.HasMap)
			{
				return;
			}
			if (GenHostility.AnyHostileActiveThreat(mapParent.Map))
			{
				return;
			}
			this.GiveRewardsAndSendLetter();
			this.StopQuest();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<bool>(ref this.active, "active", false, false);
			Scribe_Values.Look<float>(ref this.relationsImprovement, "relationsImprovement", 0f, false);
			Scribe_References.Look<Faction>(ref this.requestingFaction, "requestingFaction", false);
			Scribe_Deep.Look<ThingOwner>(ref this.rewards, "rewards", new object[]
			{
				this
			});
		}

		private void GiveRewardsAndSendLetter()
		{
			Map map = Find.AnyPlayerHomeMap ?? ((MapParent)this.parent).Map;
			DefeatAllEnemiesQuestComp.tmpRewards.AddRange(this.rewards);
			this.rewards.Clear();
			IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
			DropPodUtility.DropThingsNear(intVec, map, DefeatAllEnemiesQuestComp.tmpRewards, 110, false, false, true);
			DefeatAllEnemiesQuestComp.tmpRewards.Clear();
			Find.LetterStack.ReceiveLetter("LetterLabelDefeatAllEnemiesQuestCompleted".Translate(), "LetterDefeatAllEnemiesQuestCompleted".Translate(new object[]
			{
				this.requestingFaction.Name,
				this.relationsImprovement.ToString("F0")
			}), LetterDefOf.Good, new GlobalTargetInfo(intVec, map, false), null);
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.rewards;
		}

		public override void PostPostRemove()
		{
			base.PostPostRemove();
			this.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
		}

		public override string CompInspectStringExtra()
		{
			if (this.active)
			{
				return "QuestTargetDestroyInspectString".Translate(new object[]
				{
					this.requestingFaction.Name,
					this.rewards[0].LabelCap
				}).CapitalizeFirst();
			}
			return null;
		}

		virtual IThingHolder get_ParentHolder()
		{
			return base.ParentHolder;
		}
	}
}
