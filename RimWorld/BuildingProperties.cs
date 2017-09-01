using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class BuildingProperties
	{
		public bool isEdifice = true;

		public List<string> buildingTags = new List<string>();

		public bool isInert;

		private bool deconstructible = true;

		public bool alwaysDeconstructible;

		public bool claimable = true;

		public bool isSittable;

		public SoundDef soundAmbient;

		public ConceptDef spawnedConceptLearnOpportunity;

		public ConceptDef boughtConceptLearnOpportunity;

		public bool expandHomeArea = true;

		public bool wantsHopperAdjacent;

		public bool ignoreNeedsPower;

		public bool allowWireConnection = true;

		public bool shipPart;

		public bool canPlaceOverImpassablePlant = true;

		public float heatPerTickWhileWorking;

		public bool canBuildNonEdificesUnder = true;

		public bool canPlaceOverWall;

		public bool allowAutoroof = true;

		public bool preventDeterioration;

		public bool isMealSource;

		public bool isJoySource;

		public bool isNaturalRock;

		public bool isResourceRock;

		public bool repairable = true;

		public float roofCollapseDamageMultiplier = 1f;

		public bool hasFuelingPort;

		public bool isPlayerEjectable;

		public GraphicData fullGraveGraphicData;

		public float bed_healPerDay;

		public bool bed_defaultMedical;

		public bool bed_showSleeperBody;

		public bool bed_humanlike = true;

		public float bed_maxBodySize = 9999f;

		public float nutritionCostPerDispense;

		public SoundDef soundDispense;

		public ThingDef turretGunDef;

		public ThingDef turretShellDef;

		public float turretBurstWarmupTime;

		public float turretBurstCooldownTime = -1f;

		public string turretTopGraphicPath;

		[Unsaved]
		public Material turretTopMat;

		public bool ai_combatDangerous;

		public bool ai_chillDestination = true;

		public SoundDef soundDoorOpenPowered = SoundDefOf.DoorOpen;

		public SoundDef soundDoorClosePowered = SoundDefOf.DoorClose;

		public SoundDef soundDoorOpenManual = SoundDefOf.DoorOpenManual;

		public SoundDef soundDoorCloseManual = SoundDefOf.DoorCloseManual;

		public string sowTag;

		public ThingDef defaultPlantToGrow;

		public ThingDef mineableThing;

		public int mineableYield = 1;

		public float mineableNonMinedEfficiency = 0.7f;

		public float mineableDropChance = 1f;

		public bool mineableYieldWasteable = true;

		public float mineableScatterCommonality;

		public IntRange mineableScatterLumpSizeRange = new IntRange(20, 40);

		public StorageSettings fixedStorageSettings;

		public StorageSettings defaultStorageSettings;

		public bool ignoreStoredThingsBeauty;

		public bool isTrap;

		public DamageArmorCategoryDef trapDamageCategory;

		public GraphicData trapUnarmedGraphicData;

		[Unsaved]
		public Graphic trapUnarmedGraphic;

		public float unpoweredWorkTableWorkSpeedFactor;

		public bool workSpeedPenaltyOutdoors;

		public bool workSpeedPenaltyTemperature;

		public IntRange watchBuildingStandDistanceRange = IntRange.one;

		public int watchBuildingStandRectWidth = 3;

		public bool SupportsPlants
		{
			get
			{
				return this.sowTag != null;
			}
		}

		public bool IsTurret
		{
			get
			{
				return this.turretGunDef != null;
			}
		}

		public bool IsDeconstructible
		{
			get
			{
				return this.alwaysDeconstructible || (!this.isNaturalRock && this.deconstructible);
			}
		}

		public bool IsMortar
		{
			get
			{
				if (!this.IsTurret)
				{
					return false;
				}
				List<VerbProperties> verbs = this.turretGunDef.Verbs;
				for (int i = 0; i < verbs.Count; i++)
				{
					if (verbs[i].projectileDef != null && verbs[i].projectileDef.projectile.flyOverhead)
					{
						return true;
					}
				}
				return false;
			}
		}

		[DebuggerHidden]
		public IEnumerable<string> ConfigErrors(ThingDef parent)
		{
			if (this.isTrap && !this.isEdifice)
			{
				yield return "isTrap but is not edifice. Code will break.";
			}
			if (this.alwaysDeconstructible && !this.deconstructible)
			{
				yield return "alwaysDeconstructible=true but deconstructible=false";
			}
			if (parent.holdsRoof && !this.isEdifice)
			{
				yield return "holds roof but is not an edifice.";
			}
		}

		public void PostLoadSpecial(ThingDef parent)
		{
		}

		public void ResolveReferencesSpecial()
		{
			if (!this.turretTopGraphicPath.NullOrEmpty())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					this.turretTopMat = MaterialPool.MatFrom(this.turretTopGraphicPath);
				});
			}
			if (this.fixedStorageSettings != null)
			{
				this.fixedStorageSettings.filter.ResolveReferences();
			}
			if (this.defaultStorageSettings == null && this.fixedStorageSettings != null)
			{
				this.defaultStorageSettings = new StorageSettings();
				this.defaultStorageSettings.CopyFrom(this.fixedStorageSettings);
			}
			if (this.defaultStorageSettings != null)
			{
				this.defaultStorageSettings.filter.ResolveReferences();
			}
		}
	}
}
