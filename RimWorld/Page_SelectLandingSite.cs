using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Page_SelectLandingSite : Page
	{
		private WorldInterface worldInterface;

		public override string PageTitle
		{
			get
			{
				return "SelectLandingSite".Translate();
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			this.worldInterface = new WorldInterface();
		}

		public override void PostOpen()
		{
			base.PostOpen();
			Find.GameInitData.ChooseRandomStartingWorldSquare();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.WorldCameraMovement, OpportunityType.Important);
			TutorSystem.Notify_Event("PageStart-SelectLandingSite");
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DrawPageTitle(rect);
			this.worldInterface.Draw(base.GetMainRect(rect, 0f, false), true);
			if (TutorSystem.TutorialMode)
			{
				base.DoBottomButtons(rect, "SelectSite".Translate(), null, null, true);
			}
			else
			{
				base.DoBottomButtons(rect, "SelectSite".Translate(), "Advanced".Translate(), delegate
				{
					Find.WindowStack.Add(new Dialog_AdvancedGameConfig(this.worldInterface.selectedCoords));
				}, true);
			}
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			if (!this.worldInterface.selectedCoords.IsValid)
			{
				Messages.Message("MustSelectLandingSite".Translate(), MessageSound.RejectInput);
				return false;
			}
			WorldSquare worldSquare = Find.World.grid.Get(this.worldInterface.selectedCoords);
			if (!worldSquare.biome.canBuildBase)
			{
				Messages.Message("CannotLandBiome".Translate(new object[]
				{
					worldSquare.biome.label
				}), MessageSound.RejectInput);
				return false;
			}
			if (!worldSquare.biome.implemented)
			{
				Messages.Message("BiomeNotImplemented".Translate() + ": " + worldSquare.biome.label, MessageSound.RejectInput);
				return false;
			}
			Faction faction = Find.World.factionManager.FactionInWorldSquare(this.worldInterface.selectedCoords);
			if (faction != null)
			{
				Messages.Message("BaseAlreadyThere".Translate(new object[]
				{
					faction.Name
				}), MessageSound.RejectInput);
				return false;
			}
			if (!TutorSystem.AllowAction("ChooseBiome-" + worldSquare.biome.defName + "-" + worldSquare.hilliness.ToString()))
			{
				return false;
			}
			Find.GameInitData.startingCoords = this.worldInterface.selectedCoords;
			return true;
		}
	}
}
