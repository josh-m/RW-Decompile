using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class SiteCoreOrPartDefBase : Def
	{
		public Type workerClass = typeof(SiteCoreOrPartWorkerBase);

		[NoTranslate]
		public string siteTexture;

		[NoTranslate]
		public string expandingIconTexture;

		public bool applyFactionColorToSiteTexture;

		public bool showFactionInInspectString;

		public bool requiresFaction;

		public TechLevel minFactionTechLevel;

		[MustTranslate]
		public string approachOrderString;

		[MustTranslate]
		public string approachingReportString;

		[NoTranslate]
		public List<string> tags = new List<string>();

		[MustTranslate]
		public string arrivedLetter;

		[MustTranslate]
		public string arrivedLetterLabel;

		public LetterDef arrivedLetterDef;

		[MustTranslate]
		public string descriptionDialogue;

		public bool wantsThreatPoints;

		[Unsaved]
		private SiteCoreOrPartWorkerBase workerInt;

		[Unsaved]
		private Texture2D expandingIconTextureInt;

		[Unsaved]
		private List<GenStepDef> extraGenSteps;

		public SiteCoreOrPartWorkerBase Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = this.CreateWorker();
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}

		public Texture2D ExpandingIconTexture
		{
			get
			{
				if (this.expandingIconTextureInt == null)
				{
					if (!this.expandingIconTexture.NullOrEmpty())
					{
						this.expandingIconTextureInt = ContentFinder<Texture2D>.Get(this.expandingIconTexture, true);
					}
					else
					{
						this.expandingIconTextureInt = BaseContent.BadTex;
					}
				}
				return this.expandingIconTextureInt;
			}
		}

		public List<GenStepDef> ExtraGenSteps
		{
			get
			{
				if (this.extraGenSteps == null)
				{
					this.extraGenSteps = new List<GenStepDef>();
					List<GenStepDef> allDefsListForReading = DefDatabase<GenStepDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].linkWithSite == this)
						{
							this.extraGenSteps.Add(allDefsListForReading[i]);
						}
					}
				}
				return this.extraGenSteps;
			}
		}

		public virtual bool FactionCanOwn(Faction faction)
		{
			return (!this.requiresFaction || faction != null) && (this.minFactionTechLevel == TechLevel.Undefined || (faction != null && faction.def.techLevel >= this.minFactionTechLevel)) && (faction == null || (!faction.IsPlayer && !faction.defeated && !faction.def.hidden));
		}

		protected abstract SiteCoreOrPartWorkerBase CreateWorker();
	}
}
