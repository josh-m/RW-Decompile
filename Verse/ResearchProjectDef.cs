using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class ResearchProjectDef : Def
	{
		public TechLevel techLevel;

		private string descriptionDiscovered;

		public float baseCost = 100f;

		public List<ResearchProjectDef> prerequisites;

		private List<ResearchMod> researchMods;

		public ThingDef requiredResearchBuilding;

		public List<ThingDef> requiredResearchFacilities;

		public List<string> tags;

		public float CostApparent
		{
			get
			{
				return this.baseCost * this.CostFactor(Faction.OfPlayer.def.techLevel);
			}
		}

		public float ProgressReal
		{
			get
			{
				return Find.ResearchManager.GetProgress(this);
			}
		}

		public float ProgressApparent
		{
			get
			{
				return this.ProgressReal * this.CostFactor(Faction.OfPlayer.def.techLevel);
			}
		}

		public float ProgressPercent
		{
			get
			{
				return Find.ResearchManager.GetProgress(this) / this.baseCost;
			}
		}

		public bool IsFinished
		{
			get
			{
				return this.ProgressReal >= this.baseCost;
			}
		}

		public bool CanStartNow
		{
			get
			{
				return !this.IsFinished && this.PrerequisitesCompleted && this.PlayerHasAnyAppropriateResearchBench;
			}
		}

		public bool PrerequisitesCompleted
		{
			get
			{
				if (this.prerequisites != null)
				{
					for (int i = 0; i < this.prerequisites.Count; i++)
					{
						if (!this.prerequisites[i].IsFinished)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public string DescriptionDiscovered
		{
			get
			{
				if (this.descriptionDiscovered != null)
				{
					return this.descriptionDiscovered;
				}
				return this.description;
			}
		}

		private bool PlayerHasAnyAppropriateResearchBench
		{
			get
			{
				List<Building> allBuildingsColonist = Find.ListerBuildings.allBuildingsColonist;
				for (int i = 0; i < allBuildingsColonist.Count; i++)
				{
					Building_ResearchBench building_ResearchBench = allBuildingsColonist[i] as Building_ResearchBench;
					if (building_ResearchBench != null && this.CanBeResearchedAt(building_ResearchBench, true))
					{
						return true;
					}
				}
				return false;
			}
		}

		public float CostFactor(TechLevel researcherTechLevel)
		{
			if (researcherTechLevel >= this.techLevel)
			{
				return 1f;
			}
			int num = (int)(this.techLevel - researcherTechLevel);
			return 1f + (float)num;
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in base.ConfigErrors())
			{
				yield return e;
			}
			if (this.techLevel == TechLevel.Undefined)
			{
				yield return "techLevel is Undefined";
			}
		}

		public bool HasTag(string tag)
		{
			return this.tags != null && this.tags.Contains(tag);
		}

		public bool CanBeResearchedAt(Building_ResearchBench bench, bool ignoreResearchBenchPowerStatus)
		{
			if (this.requiredResearchBuilding != null && bench.def != this.requiredResearchBuilding)
			{
				return false;
			}
			if (!ignoreResearchBenchPowerStatus)
			{
				CompPowerTrader comp = bench.GetComp<CompPowerTrader>();
				if (comp != null && !comp.PowerOn)
				{
					return false;
				}
			}
			if (!this.requiredResearchFacilities.NullOrEmpty<ThingDef>())
			{
				CompAffectedByFacilities affectedByFacilities = bench.TryGetComp<CompAffectedByFacilities>();
				if (affectedByFacilities == null)
				{
					return false;
				}
				List<Thing> linkedFacilitiesListForReading = affectedByFacilities.LinkedFacilitiesListForReading;
				int i;
				for (i = 0; i < this.requiredResearchFacilities.Count; i++)
				{
					if (linkedFacilitiesListForReading.Find((Thing x) => x.def == this.requiredResearchFacilities[i] && affectedByFacilities.IsFacilityActive(x)) == null)
					{
						return false;
					}
				}
			}
			return true;
		}

		public void ReapplyAllMods()
		{
			if (this.researchMods != null)
			{
				for (int i = 0; i < this.researchMods.Count; i++)
				{
					try
					{
						this.researchMods[i].Apply();
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat(new object[]
						{
							"Exception applying research mod for project ",
							this,
							": ",
							ex.ToString()
						}));
					}
				}
			}
		}

		public static ResearchProjectDef Named(string defName)
		{
			return DefDatabase<ResearchProjectDef>.GetNamed(defName, true);
		}
	}
}
