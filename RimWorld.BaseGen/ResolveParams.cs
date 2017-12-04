using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen
{
	public struct ResolveParams
	{
		public CellRect rect;

		public Faction faction;

		private Dictionary<string, object> custom;

		public int? ancientTempleEntranceHeight;

		public PawnGroupMakerParms pawnGroupMakerParams;

		public PawnGroupKindDef pawnGroupKindDef;

		public RoofDef roofDef;

		public bool? noRoof;

		public bool? addRoomCenterToRootsToUnfog;

		public Thing singleThingToSpawn;

		public ThingDef singleThingDef;

		public ThingDef singleThingStuff;

		public int? singleThingStackCount;

		public bool? skipSingleThingIfHasToWipeBuildingOrDoesntFit;

		public Pawn singlePawnToSpawn;

		public PawnKindDef singlePawnKindDef;

		public bool? disableSinglePawn;

		public Lord singlePawnLord;

		public Predicate<IntVec3> singlePawnSpawnCellExtraPredicate;

		public PawnGenerationRequest? singlePawnGenerationRequest;

		public Action<Thing> postThingSpawn;

		public Action<Thing> postThingGenerate;

		public int? mechanoidsCount;

		public int? hivesCount;

		public bool? disableHives;

		public Rot4? thingRot;

		public ThingDef wallStuff;

		public float? chanceToSkipWallBlock;

		public TerrainDef floorDef;

		public float? chanceToSkipFloor;

		public ThingDef filthDef;

		public FloatRange? filthDensity;

		public bool? clearEdificeOnly;

		public bool? clearFillageOnly;

		public bool? clearRoof;

		public int? ancientCryptosleepCasketGroupID;

		public PodContentsType? podContentsType;

		public ItemCollectionGeneratorDef itemCollectionGeneratorDef;

		public ItemCollectionGeneratorParams? itemCollectionGeneratorParams;

		public IList<Thing> stockpileConcreteContents;

		public float? stockpileMarketValue;

		public int? innerStockpileSize;

		public int? edgeDefenseWidth;

		public int? edgeDefenseTurretsCount;

		public int? edgeDefenseMortarsCount;

		public int? edgeDefenseGuardsCount;

		public ThingDef mortarDef;

		public TerrainDef pathwayFloorDef;

		public ThingDef cultivatedPlantDef;

		public int? fillWithThingsPadding;

		public float? factionBasePawnGroupPointsFactor;

		public bool? streetHorizontal;

		public bool? edgeThingAvoidOtherEdgeThings;

		public bool? allowPlacementOffEdge;

		public Rot4? thrustAxis;

		public FloatRange? hpPercentRange;

		public void SetCustom<T>(string name, T obj, bool inherit = false)
		{
			if (this.custom == null)
			{
				this.custom = new Dictionary<string, object>();
			}
			else
			{
				this.custom = new Dictionary<string, object>(this.custom);
			}
			if (!this.custom.ContainsKey(name))
			{
				this.custom.Add(name, obj);
			}
			else if (!inherit)
			{
				this.custom[name] = obj;
			}
		}

		public void RemoveCustom(string name)
		{
			if (this.custom == null)
			{
				return;
			}
			this.custom = new Dictionary<string, object>(this.custom);
			this.custom.Remove(name);
		}

		public bool TryGetCustom<T>(string name, out T obj)
		{
			object obj2;
			if (this.custom == null || !this.custom.TryGetValue(name, out obj2))
			{
				obj = default(T);
				return false;
			}
			obj = (T)((object)obj2);
			return true;
		}

		public T GetCustom<T>(string name)
		{
			object obj;
			if (this.custom == null || !this.custom.TryGetValue(name, out obj))
			{
				return default(T);
			}
			return (T)((object)obj);
		}

		public override string ToString()
		{
			object[] expr_07 = new object[110];
			expr_07[0] = "rect=";
			expr_07[1] = this.rect;
			expr_07[2] = ", faction=";
			expr_07[3] = ((this.faction == null) ? "null" : this.faction.ToString());
			expr_07[4] = ", custom=";
			expr_07[5] = ((this.custom == null) ? "null" : this.custom.Count.ToString());
			expr_07[6] = ", ancientTempleEntranceHeight=";
			int arg_B9_1 = 7;
			int? num = this.ancientTempleEntranceHeight;
			expr_07[arg_B9_1] = ((!num.HasValue) ? "null" : this.ancientTempleEntranceHeight.ToString());
			expr_07[8] = ", pawnGroupMakerParams=";
			expr_07[9] = ((this.pawnGroupMakerParams == null) ? "null" : this.pawnGroupMakerParams.ToString());
			expr_07[10] = ", pawnGroupKindDef=";
			expr_07[11] = ((this.pawnGroupKindDef == null) ? "null" : this.pawnGroupKindDef.ToString());
			expr_07[12] = ", roofDef=";
			expr_07[13] = ((this.roofDef == null) ? "null" : this.roofDef.ToString());
			expr_07[14] = ", noRoof=";
			int arg_17A_1 = 15;
			bool? flag = this.noRoof;
			expr_07[arg_17A_1] = ((!flag.HasValue) ? "null" : this.noRoof.ToString());
			expr_07[16] = ", addRoomCenterToRootsToUnfog=";
			int arg_1B5_1 = 17;
			bool? flag2 = this.addRoomCenterToRootsToUnfog;
			expr_07[arg_1B5_1] = ((!flag2.HasValue) ? "null" : this.addRoomCenterToRootsToUnfog.ToString());
			expr_07[18] = ", singleThingToSpawn=";
			expr_07[19] = ((this.singleThingToSpawn == null) ? "null" : this.singleThingToSpawn.ToString());
			expr_07[20] = ", singleThingDef=";
			expr_07[21] = ((this.singleThingDef == null) ? "null" : this.singleThingDef.ToString());
			expr_07[22] = ", singleThingStuff=";
			expr_07[23] = ((this.singleThingStuff == null) ? "null" : this.singleThingStuff.ToString());
			expr_07[24] = ", singleThingStackCount=";
			int arg_278_1 = 25;
			int? num2 = this.singleThingStackCount;
			expr_07[arg_278_1] = ((!num2.HasValue) ? "null" : this.singleThingStackCount.ToString());
			expr_07[26] = ", skipSingleThingIfHasToWipeBuildingOrDoesntFit=";
			int arg_2B4_1 = 27;
			bool? flag3 = this.skipSingleThingIfHasToWipeBuildingOrDoesntFit;
			expr_07[arg_2B4_1] = ((!flag3.HasValue) ? "null" : this.skipSingleThingIfHasToWipeBuildingOrDoesntFit.ToString());
			expr_07[28] = ", singlePawnToSpawn=";
			expr_07[29] = ((this.singlePawnToSpawn == null) ? "null" : this.singlePawnToSpawn.ToString());
			expr_07[30] = ", singlePawnKindDef=";
			expr_07[31] = ((this.singlePawnKindDef == null) ? "null" : this.singlePawnKindDef.ToString());
			expr_07[32] = ", disableSinglePawn=";
			int arg_34A_1 = 33;
			bool? flag4 = this.disableSinglePawn;
			expr_07[arg_34A_1] = ((!flag4.HasValue) ? "null" : this.disableSinglePawn.ToString());
			expr_07[34] = ", singlePawnLord=";
			expr_07[35] = ((this.singlePawnLord == null) ? "null" : this.singlePawnLord.ToString());
			expr_07[36] = ", singlePawnSpawnCellExtraPredicate=";
			expr_07[37] = ((this.singlePawnSpawnCellExtraPredicate == null) ? "null" : this.singlePawnSpawnCellExtraPredicate.ToString());
			expr_07[38] = ", singlePawnGenerationRequest=";
			int arg_3E0_1 = 39;
			PawnGenerationRequest? pawnGenerationRequest = this.singlePawnGenerationRequest;
			expr_07[arg_3E0_1] = ((!pawnGenerationRequest.HasValue) ? "null" : this.singlePawnGenerationRequest.ToString());
			expr_07[40] = ", postThingSpawn=";
			expr_07[41] = ((this.postThingSpawn == null) ? "null" : this.postThingSpawn.ToString());
			expr_07[42] = ", postThingGenerate=";
			expr_07[43] = ((this.postThingGenerate == null) ? "null" : this.postThingGenerate.ToString());
			expr_07[44] = ", mechanoidsCount=";
			int arg_476_1 = 45;
			int? num3 = this.mechanoidsCount;
			expr_07[arg_476_1] = ((!num3.HasValue) ? "null" : this.mechanoidsCount.ToString());
			expr_07[46] = ", hivesCount=";
			int arg_4B2_1 = 47;
			int? num4 = this.hivesCount;
			expr_07[arg_4B2_1] = ((!num4.HasValue) ? "null" : this.hivesCount.ToString());
			expr_07[48] = ", disableHives=";
			int arg_4EE_1 = 49;
			bool? flag5 = this.disableHives;
			expr_07[arg_4EE_1] = ((!flag5.HasValue) ? "null" : this.disableHives.ToString());
			expr_07[50] = ", thingRot=";
			int arg_52A_1 = 51;
			Rot4? rot = this.thingRot;
			expr_07[arg_52A_1] = ((!rot.HasValue) ? "null" : this.thingRot.ToString());
			expr_07[52] = ", wallStuff=";
			expr_07[53] = ((this.wallStuff == null) ? "null" : this.wallStuff.ToString());
			expr_07[54] = ", chanceToSkipWallBlock=";
			int arg_593_1 = 55;
			float? num5 = this.chanceToSkipWallBlock;
			expr_07[arg_593_1] = ((!num5.HasValue) ? "null" : this.chanceToSkipWallBlock.ToString());
			expr_07[56] = ", floorDef=";
			expr_07[57] = ((this.floorDef == null) ? "null" : this.floorDef.ToString());
			expr_07[58] = ", chanceToSkipFloor=";
			int arg_5FC_1 = 59;
			float? num6 = this.chanceToSkipFloor;
			expr_07[arg_5FC_1] = ((!num6.HasValue) ? "null" : this.chanceToSkipFloor.ToString());
			expr_07[60] = ", filthDef=";
			expr_07[61] = ((this.filthDef == null) ? "null" : this.filthDef.ToString());
			expr_07[62] = ", filthDensity=";
			int arg_665_1 = 63;
			FloatRange? floatRange = this.filthDensity;
			expr_07[arg_665_1] = ((!floatRange.HasValue) ? "null" : this.filthDensity.ToString());
			expr_07[64] = ", clearEdificeOnly=";
			int arg_6A1_1 = 65;
			bool? flag6 = this.clearEdificeOnly;
			expr_07[arg_6A1_1] = ((!flag6.HasValue) ? "null" : this.clearEdificeOnly.ToString());
			expr_07[66] = ", clearFillageOnly=";
			int arg_6DD_1 = 67;
			bool? flag7 = this.clearFillageOnly;
			expr_07[arg_6DD_1] = ((!flag7.HasValue) ? "null" : this.clearFillageOnly.ToString());
			expr_07[68] = ", clearRoof=";
			int arg_719_1 = 69;
			bool? flag8 = this.clearRoof;
			expr_07[arg_719_1] = ((!flag8.HasValue) ? "null" : this.clearRoof.ToString());
			expr_07[70] = ", ancientCryptosleepCasketGroupID=";
			int arg_755_1 = 71;
			int? num7 = this.ancientCryptosleepCasketGroupID;
			expr_07[arg_755_1] = ((!num7.HasValue) ? "null" : this.ancientCryptosleepCasketGroupID.ToString());
			expr_07[72] = ", podContentsType=";
			int arg_791_1 = 73;
			PodContentsType? podContentsType = this.podContentsType;
			expr_07[arg_791_1] = ((!podContentsType.HasValue) ? "null" : this.podContentsType.ToString());
			expr_07[74] = ", itemCollectionGeneratorDef=";
			expr_07[75] = ((this.itemCollectionGeneratorDef == null) ? "null" : this.itemCollectionGeneratorDef.ToString());
			expr_07[76] = ", itemCollectionGeneratorParams=";
			int arg_7FA_1 = 77;
			ItemCollectionGeneratorParams? itemCollectionGeneratorParams = this.itemCollectionGeneratorParams;
			expr_07[arg_7FA_1] = ((!itemCollectionGeneratorParams.HasValue) ? "null" : this.itemCollectionGeneratorParams.ToString());
			expr_07[78] = ", stockpileConcreteContents=";
			expr_07[79] = ((this.stockpileConcreteContents == null) ? "null" : this.stockpileConcreteContents.Count.ToString());
			expr_07[80] = ", stockpileMarketValue=";
			int arg_872_1 = 81;
			float? num8 = this.stockpileMarketValue;
			expr_07[arg_872_1] = ((!num8.HasValue) ? "null" : this.stockpileMarketValue.ToString());
			expr_07[82] = ", innerStockpileSize=";
			int arg_8AE_1 = 83;
			int? num9 = this.innerStockpileSize;
			expr_07[arg_8AE_1] = ((!num9.HasValue) ? "null" : this.innerStockpileSize.ToString());
			expr_07[84] = ", edgeDefenseWidth=";
			int arg_8EA_1 = 85;
			int? num10 = this.edgeDefenseWidth;
			expr_07[arg_8EA_1] = ((!num10.HasValue) ? "null" : this.edgeDefenseWidth.ToString());
			expr_07[86] = ", edgeDefenseTurretsCount=";
			int arg_926_1 = 87;
			int? num11 = this.edgeDefenseTurretsCount;
			expr_07[arg_926_1] = ((!num11.HasValue) ? "null" : this.edgeDefenseTurretsCount.ToString());
			expr_07[88] = ", edgeDefenseMortarsCount=";
			int arg_962_1 = 89;
			int? num12 = this.edgeDefenseMortarsCount;
			expr_07[arg_962_1] = ((!num12.HasValue) ? "null" : this.edgeDefenseMortarsCount.ToString());
			expr_07[90] = ", edgeDefenseGuardsCount=";
			int arg_99E_1 = 91;
			int? num13 = this.edgeDefenseGuardsCount;
			expr_07[arg_99E_1] = ((!num13.HasValue) ? "null" : this.edgeDefenseGuardsCount.ToString());
			expr_07[92] = ", mortarDef=";
			expr_07[93] = ((this.mortarDef == null) ? "null" : this.mortarDef.ToString());
			expr_07[94] = ", pathwayFloorDef=";
			expr_07[95] = ((this.pathwayFloorDef == null) ? "null" : this.pathwayFloorDef.ToString());
			expr_07[96] = ", cultivatedPlantDef=";
			expr_07[97] = ((this.cultivatedPlantDef == null) ? "null" : this.cultivatedPlantDef.ToString());
			expr_07[98] = ", fillWithThingsPadding=";
			int arg_A61_1 = 99;
			int? num14 = this.fillWithThingsPadding;
			expr_07[arg_A61_1] = ((!num14.HasValue) ? "null" : this.fillWithThingsPadding.ToString());
			expr_07[100] = ", factionBasePawnGroupPointsFactor=";
			int arg_A9D_1 = 101;
			float? num15 = this.factionBasePawnGroupPointsFactor;
			expr_07[arg_A9D_1] = ((!num15.HasValue) ? "null" : this.factionBasePawnGroupPointsFactor.ToString());
			expr_07[102] = ", streetHorizontal=";
			int arg_AD9_1 = 103;
			bool? flag9 = this.streetHorizontal;
			expr_07[arg_AD9_1] = ((!flag9.HasValue) ? "null" : this.streetHorizontal.ToString());
			expr_07[104] = ", edgeThingAvoidOtherEdgeThings=";
			int arg_B15_1 = 105;
			bool? flag10 = this.edgeThingAvoidOtherEdgeThings;
			expr_07[arg_B15_1] = ((!flag10.HasValue) ? "null" : this.edgeThingAvoidOtherEdgeThings.ToString());
			expr_07[106] = ", allowPlacementOffEdge=";
			int arg_B51_1 = 107;
			bool? flag11 = this.allowPlacementOffEdge;
			expr_07[arg_B51_1] = ((!flag11.HasValue) ? "null" : this.allowPlacementOffEdge.ToString());
			expr_07[108] = ", thrustAxis=";
			int arg_B8D_1 = 109;
			Rot4? rot2 = this.thrustAxis;
			expr_07[arg_B8D_1] = ((!rot2.HasValue) ? "null" : this.thrustAxis.ToString());
			return string.Concat(expr_07);
		}
	}
}
