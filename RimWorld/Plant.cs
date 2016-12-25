using System;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Plant : Thing
	{
		public const float BaseGrowthPercent = 0.05f;

		private const float DyingDamagePerTick = 0.005f;

		private const float GridPosRandomnessFactor = 0.3f;

		private const int TicksWithoutLightBeforeStartDying = 450000;

		private const int LeaflessMinRecoveryTicks = 60000;

		public const float MinGrowthTemperature = 0f;

		public const float MinOptimalGrowthTemperature = 10f;

		public const float MaxOptimalGrowthTemperature = 42f;

		public const float MaxGrowthTemperature = 58f;

		public const float MaxLeaflessTemperature = -2f;

		private const float MinLeaflessTemperature = -10f;

		private const float MinAnimalEatPlantsTemperature = 0f;

		public const float SeedShootMinGrowthPercent = 0.6f;

		protected float growthInt = 0.05f;

		protected int ageInt;

		protected int unlitTicks;

		protected int madeLeaflessTick = -99999;

		public bool sown;

		private string cachedLabelMouseover;

		private static Color32[] workingColors = new Color32[4];

		private static Graphic GraphicSowing = GraphicDatabase.Get<Graphic_Single>("Things/Plant/Plant_Sowing", ShaderDatabase.Cutout, Vector2.one, Color.white);

		public virtual float Growth
		{
			get
			{
				return this.growthInt;
			}
			set
			{
				this.growthInt = value;
				this.cachedLabelMouseover = null;
			}
		}

		public virtual int Age
		{
			get
			{
				return this.ageInt;
			}
			set
			{
				this.ageInt = value;
				this.cachedLabelMouseover = null;
			}
		}

		public virtual bool HarvestableNow
		{
			get
			{
				return this.def.plant.Harvestable && this.growthInt > this.def.plant.harvestMinGrowth;
			}
		}

		public override bool IngestibleNow
		{
			get
			{
				return this.def.plant.IsTree || (this.growthInt >= this.def.plant.harvestMinGrowth && !this.LeaflessNow && (!base.Spawned || base.Position.GetSnowDepth(base.Map) <= this.def.hideAtSnowDepth));
			}
		}

		public virtual bool Dying
		{
			get
			{
				return (this.def.plant.LimitedLifespan && this.ageInt > this.def.plant.LifespanTicks) || this.unlitTicks > 450000;
			}
		}

		protected virtual bool Resting
		{
			get
			{
				return GenLocalDate.DayPercent(this) < 0.25f || GenLocalDate.DayPercent(this) > 0.8f;
			}
		}

		public virtual float GrowthRate
		{
			get
			{
				return this.GrowthRateFactor_Fertility * this.GrowthRateFactor_Temperature * this.GrowthRateFactor_Light;
			}
		}

		protected float GrowthPerTick
		{
			get
			{
				if (this.LifeStage != PlantLifeStage.Growing || this.Resting)
				{
					return 0f;
				}
				float num = 1f / (60000f * this.def.plant.growDays);
				return num * this.GrowthRate;
			}
		}

		public float GrowthRateFactor_Fertility
		{
			get
			{
				return base.Map.fertilityGrid.FertilityAt(base.Position) * this.def.plant.fertilitySensitivity + (1f - this.def.plant.fertilitySensitivity);
			}
		}

		public float GrowthRateFactor_Light
		{
			get
			{
				float value = Mathf.InverseLerp(this.def.plant.growMinGlow, this.def.plant.growOptimalGlow, base.Map.glowGrid.GameGlowAt(base.Position));
				return Mathf.Clamp01(value);
			}
		}

		public float GrowthRateFactor_Temperature
		{
			get
			{
				float num;
				if (!GenTemperature.TryGetTemperatureForCell(base.Position, base.Map, out num))
				{
					return 1f;
				}
				if (num < 10f)
				{
					return Mathf.InverseLerp(0f, 10f, num);
				}
				if (num > 42f)
				{
					return Mathf.InverseLerp(58f, 42f, num);
				}
				return 1f;
			}
		}

		protected int TicksUntilFullyGrown
		{
			get
			{
				if (this.growthInt > 0.9999f)
				{
					return 0;
				}
				return (int)((1f - this.growthInt) / this.GrowthPerTick);
			}
		}

		protected string GrowthPercentString
		{
			get
			{
				return (this.growthInt + 0.0001f).ToStringPercent();
			}
		}

		public override string LabelMouseover
		{
			get
			{
				if (this.cachedLabelMouseover == null)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(this.def.LabelCap);
					stringBuilder.Append(" (" + "PercentGrowth".Translate(new object[]
					{
						this.GrowthPercentString
					}));
					if (this.Dying)
					{
						stringBuilder.Append(", " + "DyingLower".Translate());
					}
					stringBuilder.Append(")");
					this.cachedLabelMouseover = stringBuilder.ToString();
				}
				return this.cachedLabelMouseover;
			}
		}

		protected virtual bool HasEnoughLightToGrow
		{
			get
			{
				return this.GrowthRateFactor_Light > 0.001f;
			}
		}

		public PlantLifeStage LifeStage
		{
			get
			{
				if (this.growthInt < 0.001f)
				{
					return PlantLifeStage.Sowing;
				}
				if (this.growthInt > 0.999f)
				{
					return PlantLifeStage.Mature;
				}
				return PlantLifeStage.Growing;
			}
		}

		public override Graphic Graphic
		{
			get
			{
				if (this.LifeStage == PlantLifeStage.Sowing)
				{
					return Plant.GraphicSowing;
				}
				if (this.def.plant.leaflessGraphic != null && this.LeaflessNow)
				{
					return this.def.plant.leaflessGraphic;
				}
				return base.Graphic;
			}
		}

		public bool LeaflessNow
		{
			get
			{
				return Find.TickManager.TicksGame - this.madeLeaflessTick < 60000;
			}
		}

		protected virtual float LeaflessTemperatureThresh
		{
			get
			{
				float num = 8f;
				return (float)this.HashOffset() * 0.01f % num - num + -2f;
			}
		}

		public bool IsCrop
		{
			get
			{
				if (!this.def.plant.Sowable)
				{
					return false;
				}
				if (!base.Spawned)
				{
					Log.Warning("Can't determine if crop when unspawned.");
					return false;
				}
				return this.def == WorkGiver_Grower.CalculateWantedPlantDef(base.Position, base.Map);
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy(mode);
		}

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.CheckTemperatureMakeLeafless();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<float>(ref this.growthInt, "growth", 0f, false);
			Scribe_Values.LookValue<int>(ref this.ageInt, "age", 0, false);
			Scribe_Values.LookValue<int>(ref this.unlitTicks, "unlitTicks", 0, false);
			Scribe_Values.LookValue<bool>(ref this.sown, "sown", false, false);
		}

		public override void PostMapInit()
		{
			this.CheckTemperatureMakeLeafless();
		}

		protected override void IngestedCalculateAmounts(Pawn ingester, float nutritionWanted, out int numTaken, out float nutritionIngested)
		{
			float num = this.def.ingestible.nutrition;
			if (this.def.plant.Sowable)
			{
				num *= this.growthInt;
			}
			else
			{
				num *= Mathf.Lerp(0.5f, 1f, this.growthInt);
			}
			if (this.def.plant.harvestDestroys)
			{
				numTaken = 1;
			}
			else
			{
				this.growthInt -= 0.3f;
				if (this.growthInt < 0.08f)
				{
					this.growthInt = 0.08f;
				}
				if (base.Spawned)
				{
					base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
				}
				numTaken = 0;
			}
			nutritionIngested = num;
		}

		public virtual void PlantCollected()
		{
			if (this.def.plant.harvestDestroys)
			{
				this.Destroy(DestroyMode.Vanish);
			}
			else
			{
				this.growthInt = 0.08f;
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
			}
		}

		protected virtual void CheckTemperatureMakeLeafless()
		{
			if (base.Position.GetTemperature(base.Map) < this.LeaflessTemperatureThresh)
			{
				this.MakeLeafless(true);
			}
		}

		public virtual void MakeLeafless(bool causedByCold = false)
		{
			bool flag = !this.LeaflessNow;
			Map map = base.Map;
			this.madeLeaflessTick = Find.TickManager.TicksGame;
			if (this.def.plant.dieIfLeafless)
			{
				if (causedByCold && this.IsCrop && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfCold-" + this.def.defName, 240f))
				{
					Messages.Message("MessagePlantDiedOfCold".Translate(new object[]
					{
						this.Label
					}).CapitalizeFirst(), new TargetInfo(base.Position, map, false), MessageSound.Negative);
				}
				base.TakeDamage(new DamageInfo(DamageDefOf.Rotting, 99999, -1f, null, null, null));
			}
			if (flag)
			{
				map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
			}
		}

		public override void TickLong()
		{
			this.CheckTemperatureMakeLeafless();
			if (base.Destroyed)
			{
				return;
			}
			if (GenPlant.GrowthSeasonNow(base.Position, base.Map))
			{
				if (!this.HasEnoughLightToGrow)
				{
					this.unlitTicks += 2000;
				}
				else
				{
					this.unlitTicks = 0;
				}
				float num = this.growthInt;
				bool flag = this.LifeStage == PlantLifeStage.Mature;
				this.growthInt += this.GrowthPerTick * 2000f;
				if (this.growthInt > 1f)
				{
					this.growthInt = 1f;
				}
				if (((!flag && this.LifeStage == PlantLifeStage.Mature) || (int)(num * 10f) != (int)(this.growthInt * 10f)) && this.CurrentlyCultivated())
				{
					base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
				}
				if (this.def.plant.LimitedLifespan)
				{
					this.ageInt += 2000;
					if (this.Dying)
					{
						Map map = base.Map;
						bool isCrop = this.IsCrop;
						int amount = Mathf.CeilToInt(10f);
						base.TakeDamage(new DamageInfo(DamageDefOf.Rotting, amount, -1f, null, null, null));
						if (base.Destroyed)
						{
							if (isCrop && this.def.plant.Harvestable && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfRot-" + this.def.defName, 240f))
							{
								Messages.Message("MessagePlantDiedOfRot".Translate(new object[]
								{
									this.Label
								}).CapitalizeFirst(), new TargetInfo(base.Position, map, false), MessageSound.Negative);
							}
							return;
						}
					}
				}
				if (this.def.plant.reproduces && this.growthInt >= 0.6f && Rand.MTBEventOccurs(this.def.plant.reproduceMtbDays, 60000f, 2000f))
				{
					if (!GenPlant.SnowAllowsPlanting(base.Position, base.Map))
					{
						return;
					}
					GenPlantReproduction.TryReproduceFrom(base.Position, this.def, SeedTargFindMode.Reproduce, base.Map);
				}
			}
			this.cachedLabelMouseover = null;
		}

		protected virtual bool CurrentlyCultivated()
		{
			if (!this.def.plant.Sowable)
			{
				return false;
			}
			if (!base.Spawned)
			{
				return false;
			}
			Zone zone = base.Map.zoneManager.ZoneAt(base.Position);
			if (zone != null && zone is Zone_Growing)
			{
				return true;
			}
			Building edifice = base.Position.GetEdifice(base.Map);
			return edifice != null && edifice.def.building.SupportsPlants;
		}

		public virtual int YieldNow()
		{
			if (!this.HarvestableNow)
			{
				return 0;
			}
			if (this.def.plant.harvestYield <= 0f)
			{
				return 0;
			}
			float num = this.def.plant.harvestYield;
			float num2 = Mathf.InverseLerp(this.def.plant.harvestMinGrowth, 1f, this.growthInt);
			num2 = 0.5f + num2 * 0.5f;
			num *= num2;
			num *= Mathf.Lerp(0.5f, 1f, (float)this.HitPoints / (float)base.MaxHitPoints);
			num *= Find.Storyteller.difficulty.cropYieldFactor;
			return GenMath.RoundRandom(num);
		}

		public override void Print(SectionLayer layer)
		{
			Vector3 a = this.TrueCenter();
			Rand.PushSeed();
			Rand.Seed = base.Position.GetHashCode();
			float num;
			if (this.def.plant.maxMeshCount == 1)
			{
				num = 0.05f;
			}
			else
			{
				num = 0.5f;
			}
			int num2 = Mathf.CeilToInt(this.growthInt * (float)this.def.plant.maxMeshCount);
			if (num2 < 1)
			{
				num2 = 1;
			}
			int num3 = 1;
			int maxMeshCount = this.def.plant.maxMeshCount;
			switch (maxMeshCount)
			{
			case 1:
				num3 = 1;
				goto IL_F8;
			case 2:
			case 3:
				IL_9B:
				if (maxMeshCount == 9)
				{
					num3 = 3;
					goto IL_F8;
				}
				if (maxMeshCount == 16)
				{
					num3 = 4;
					goto IL_F8;
				}
				if (maxMeshCount != 25)
				{
					Log.Error(this.def + " must have plant.MaxMeshCount that is a perfect square.");
					goto IL_F8;
				}
				num3 = 5;
				goto IL_F8;
			case 4:
				num3 = 2;
				goto IL_F8;
			}
			goto IL_9B;
			IL_F8:
			float num4 = 1f / (float)num3;
			Vector3 vector = Vector3.zero;
			Vector2 zero = Vector2.zero;
			int num5 = 0;
			int[] positionIndices = PlantPosIndices.GetPositionIndices(this);
			for (int i = 0; i < positionIndices.Length; i++)
			{
				int num6 = positionIndices[i];
				float num7 = this.def.plant.visualSizeRange.LerpThroughRange(this.growthInt);
				if (this.def.plant.maxMeshCount == 1)
				{
					vector = a + new Vector3(Rand.Range(-num, num), 0f, Rand.Range(-num, num));
					float num8 = Mathf.Floor(a.z);
					if (vector.z - num7 / 2f < num8)
					{
						vector.z = num8 + num7 / 2f;
					}
				}
				else
				{
					vector = base.Position.ToVector3();
					vector.y = this.def.Altitude;
					vector.x += 0.5f * num4;
					vector.z += 0.5f * num4;
					int num9 = num6 / num3;
					int num10 = num6 % num3;
					vector.x += (float)num9 * num4;
					vector.z += (float)num10 * num4;
					float num11 = num4 * 0.3f;
					vector += new Vector3(Rand.Range(-num11, num11), 0f, Rand.Range(-num11, num11));
				}
				bool flag = Rand.Value < 0.5f;
				Material matSingle = this.Graphic.MatSingle;
				Plant.workingColors[1].a = (Plant.workingColors[2].a = (byte)(255f * this.def.plant.topWindExposure));
				Plant.workingColors[0].a = (Plant.workingColors[3].a = 0);
				num7 *= this.def.graphicData.drawSize.x;
				zero = new Vector2(num7, num7);
				bool flipUv = flag;
				Printer_Plane.PrintPlane(layer, vector, zero, matSingle, 0f, flipUv, null, Plant.workingColors, 0.1f);
				num5++;
				if (num5 >= num2)
				{
					break;
				}
			}
			if (this.def.graphicData.shadowData != null)
			{
				float num12;
				if (zero.y < 1f)
				{
					num12 = 0.6f;
				}
				else
				{
					num12 = 0.81f;
				}
				Vector3 center = vector;
				center.z -= zero.y / 2f * num12;
				center.y -= 0.05f;
				Printer_Shadow.PrintShadow(layer, center, this.def.graphicData.shadowData);
			}
			Rand.PopSeed();
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (this.LifeStage == PlantLifeStage.Growing)
			{
				stringBuilder.AppendLine("PercentGrowth".Translate(new object[]
				{
					this.GrowthPercentString
				}));
				stringBuilder.AppendLine("GrowthRate".Translate() + ": " + this.GrowthRate.ToStringPercent());
				if (this.Resting)
				{
					stringBuilder.AppendLine("PlantResting".Translate());
				}
				if (!this.HasEnoughLightToGrow)
				{
					stringBuilder.AppendLine("PlantNeedsLightLevel".Translate() + ": " + this.def.plant.growMinGlow.ToStringPercent());
				}
				float growthRateFactor_Temperature = this.GrowthRateFactor_Temperature;
				if (growthRateFactor_Temperature < 0.99f)
				{
					if (growthRateFactor_Temperature < 0.01f)
					{
						stringBuilder.AppendLine("OutOfIdealTemperatureRangeNotGrowing".Translate());
					}
					else
					{
						stringBuilder.AppendLine("OutOfIdealTemperatureRange".Translate(new object[]
						{
							Mathf.RoundToInt(growthRateFactor_Temperature * 100f).ToString()
						}));
					}
				}
			}
			else if (this.LifeStage == PlantLifeStage.Mature)
			{
				if (this.def.plant.Harvestable)
				{
					stringBuilder.AppendLine("ReadyToHarvest".Translate());
				}
				else
				{
					stringBuilder.AppendLine("Mature".Translate());
				}
			}
			return stringBuilder.ToString();
		}

		public virtual void CropBlighted()
		{
			if (Rand.Value < 0.85f)
			{
				this.Destroy(DestroyMode.Vanish);
			}
		}
	}
}
