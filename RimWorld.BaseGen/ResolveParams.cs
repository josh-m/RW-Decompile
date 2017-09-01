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

		public Pawn singlePawnToSpawn;

		public PawnKindDef singlePawnKindDef;

		public bool? disableSinglePawn;

		public Lord singlePawnLord;

		public Predicate<IntVec3> singlePawnSpawnCellExtraPredicate;

		public PawnGenerationRequest? singlePawnGenerationRequest;

		public int? mechanoidsCount;

		public int? hivesCount;

		public bool? disableHives;

		public Rot4? thingRot;

		public ThingDef wallStuff;

		public float? chanceToSkipWallBlock;

		public TerrainDef floorDef;

		public bool? clearEdificeOnly;

		public int? ancientCryptosleepCasketGroupID;

		public PodContentsType? podContentsType;

		public ItemCollectionGeneratorDef itemCollectionGeneratorDef;

		public ItemCollectionGeneratorParams? itemCollectionGeneratorParams;

		public IList<Thing> stockpileConcreteContents;

		public float? stockpileMarketValue;

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
			object[] expr_07 = new object[88];
			expr_07[0] = "rect=";
			expr_07[1] = this.rect;
			expr_07[2] = ", faction=";
			expr_07[3] = ((this.faction == null) ? "null" : this.faction.ToString());
			expr_07[4] = ", custom=";
			expr_07[5] = ((this.custom == null) ? "null" : this.custom.Count.ToString());
			expr_07[6] = ", ancientTempleEntranceHeight=";
			int arg_AD_1 = 7;
			int? num = this.ancientTempleEntranceHeight;
			expr_07[arg_AD_1] = ((!num.HasValue) ? "null" : this.ancientTempleEntranceHeight.ToString());
			expr_07[8] = ", pawnGroupMakerParams=";
			expr_07[9] = ((this.pawnGroupMakerParams == null) ? "null" : this.pawnGroupMakerParams.ToString());
			expr_07[10] = ", pawnGroupKindDef=";
			expr_07[11] = ((this.pawnGroupKindDef == null) ? "null" : this.pawnGroupKindDef.ToString());
			expr_07[12] = ", roofDef=";
			expr_07[13] = ((this.roofDef == null) ? "null" : this.roofDef.ToString());
			expr_07[14] = ", noRoof=";
			int arg_168_1 = 15;
			bool? flag = this.noRoof;
			expr_07[arg_168_1] = ((!flag.HasValue) ? "null" : this.noRoof.ToString());
			expr_07[16] = ", addRoomCenterToRootsToUnfog=";
			int arg_19D_1 = 17;
			bool? flag2 = this.addRoomCenterToRootsToUnfog;
			expr_07[arg_19D_1] = ((!flag2.HasValue) ? "null" : this.addRoomCenterToRootsToUnfog.ToString());
			expr_07[18] = ", singleThingToSpawn=";
			expr_07[19] = ((this.singleThingToSpawn == null) ? "null" : this.singleThingToSpawn.ToString());
			expr_07[20] = ", singleThingDef=";
			expr_07[21] = ((this.singleThingDef == null) ? "null" : this.singleThingDef.ToString());
			expr_07[22] = ", singleThingStuff=";
			expr_07[23] = ((this.singleThingStuff == null) ? "null" : this.singleThingStuff.ToString());
			expr_07[24] = ", singleThingStackCount=";
			int arg_25A_1 = 25;
			int? num2 = this.singleThingStackCount;
			expr_07[arg_25A_1] = ((!num2.HasValue) ? "null" : this.singleThingStackCount.ToString());
			expr_07[26] = ", singlePawnToSpawn=";
			expr_07[27] = ((this.singlePawnToSpawn == null) ? "null" : this.singlePawnToSpawn.ToString());
			expr_07[28] = ", singlePawnKindDef=";
			expr_07[29] = ((this.singlePawnKindDef == null) ? "null" : this.singlePawnKindDef.ToString());
			expr_07[30] = ", disableSinglePawn=";
			int arg_2EA_1 = 31;
			bool? flag3 = this.disableSinglePawn;
			expr_07[arg_2EA_1] = ((!flag3.HasValue) ? "null" : this.disableSinglePawn.ToString());
			expr_07[32] = ", singlePawnLord=";
			expr_07[33] = ((this.singlePawnLord == null) ? "null" : this.singlePawnLord.ToString());
			expr_07[34] = ", singlePawnSpawnCellExtraPredicate=";
			expr_07[35] = ((this.singlePawnSpawnCellExtraPredicate == null) ? "null" : this.singlePawnSpawnCellExtraPredicate.ToString());
			expr_07[36] = ", singlePawnGenerationRequest=";
			int arg_37A_1 = 37;
			PawnGenerationRequest? pawnGenerationRequest = this.singlePawnGenerationRequest;
			expr_07[arg_37A_1] = ((!pawnGenerationRequest.HasValue) ? "null" : this.singlePawnGenerationRequest.ToString());
			expr_07[38] = ", mechanoidsCount=";
			int arg_3B0_1 = 39;
			int? num3 = this.mechanoidsCount;
			expr_07[arg_3B0_1] = ((!num3.HasValue) ? "null" : this.mechanoidsCount.ToString());
			expr_07[40] = ", hivesCount=";
			int arg_3E6_1 = 41;
			int? num4 = this.hivesCount;
			expr_07[arg_3E6_1] = ((!num4.HasValue) ? "null" : this.hivesCount.ToString());
			expr_07[42] = ", disableHives=";
			int arg_41C_1 = 43;
			bool? flag4 = this.disableHives;
			expr_07[arg_41C_1] = ((!flag4.HasValue) ? "null" : this.disableHives.ToString());
			expr_07[44] = ", thingRot=";
			int arg_452_1 = 45;
			Rot4? rot = this.thingRot;
			expr_07[arg_452_1] = ((!rot.HasValue) ? "null" : this.thingRot.ToString());
			expr_07[46] = ", wallStuff=";
			expr_07[47] = ((this.wallStuff == null) ? "null" : this.wallStuff.ToString());
			expr_07[48] = ", chanceToSkipWallBlock=";
			int arg_4B5_1 = 49;
			float? num5 = this.chanceToSkipWallBlock;
			expr_07[arg_4B5_1] = ((!num5.HasValue) ? "null" : this.chanceToSkipWallBlock.ToString());
			expr_07[50] = ", floorDef=";
			expr_07[51] = ((this.floorDef == null) ? "null" : this.floorDef.ToString());
			expr_07[52] = ", clearEdificeOnly=";
			int arg_518_1 = 53;
			bool? flag5 = this.clearEdificeOnly;
			expr_07[arg_518_1] = ((!flag5.HasValue) ? "null" : this.clearEdificeOnly.ToString());
			expr_07[54] = ", ancientCryptosleepCasketGroupID=";
			int arg_54E_1 = 55;
			int? num6 = this.ancientCryptosleepCasketGroupID;
			expr_07[arg_54E_1] = ((!num6.HasValue) ? "null" : this.ancientCryptosleepCasketGroupID.ToString());
			expr_07[56] = ", podContentsType=";
			int arg_584_1 = 57;
			PodContentsType? podContentsType = this.podContentsType;
			expr_07[arg_584_1] = ((!podContentsType.HasValue) ? "null" : this.podContentsType.ToString());
			expr_07[58] = ", itemCollectionGeneratorDef=";
			expr_07[59] = ((this.itemCollectionGeneratorDef == null) ? "null" : this.itemCollectionGeneratorDef.ToString());
			expr_07[60] = ", itemCollectionGeneratorParams=";
			int arg_5E7_1 = 61;
			ItemCollectionGeneratorParams? itemCollectionGeneratorParams = this.itemCollectionGeneratorParams;
			expr_07[arg_5E7_1] = ((!itemCollectionGeneratorParams.HasValue) ? "null" : this.itemCollectionGeneratorParams.ToString());
			expr_07[62] = ", stockpileConcreteContents=";
			expr_07[63] = ((this.stockpileConcreteContents == null) ? "null" : this.stockpileConcreteContents.Count.ToString());
			expr_07[64] = ", stockpileMarketValue=";
			int arg_653_1 = 65;
			float? num7 = this.stockpileMarketValue;
			expr_07[arg_653_1] = ((!num7.HasValue) ? "null" : this.stockpileMarketValue.ToString());
			expr_07[66] = ", edgeDefenseWidth=";
			int arg_689_1 = 67;
			int? num8 = this.edgeDefenseWidth;
			expr_07[arg_689_1] = ((!num8.HasValue) ? "null" : this.edgeDefenseWidth.ToString());
			expr_07[68] = ", edgeDefenseTurretsCount=";
			int arg_6BF_1 = 69;
			int? num9 = this.edgeDefenseTurretsCount;
			expr_07[arg_6BF_1] = ((!num9.HasValue) ? "null" : this.edgeDefenseTurretsCount.ToString());
			expr_07[70] = ", edgeDefenseMortarsCount=";
			int arg_6F5_1 = 71;
			int? num10 = this.edgeDefenseMortarsCount;
			expr_07[arg_6F5_1] = ((!num10.HasValue) ? "null" : this.edgeDefenseMortarsCount.ToString());
			expr_07[72] = ", edgeDefenseGuardsCount=";
			int arg_72B_1 = 73;
			int? num11 = this.edgeDefenseGuardsCount;
			expr_07[arg_72B_1] = ((!num11.HasValue) ? "null" : this.edgeDefenseGuardsCount.ToString());
			expr_07[74] = ", mortarDef=";
			expr_07[75] = ((this.mortarDef == null) ? "null" : this.mortarDef.ToString());
			expr_07[76] = ", pathwayFloorDef=";
			expr_07[77] = ((this.pathwayFloorDef == null) ? "null" : this.pathwayFloorDef.ToString());
			expr_07[78] = ", cultivatedPlantDef=";
			expr_07[79] = ((this.cultivatedPlantDef == null) ? "null" : this.cultivatedPlantDef.ToString());
			expr_07[80] = ", fillWithThingsPadding=";
			int arg_7E8_1 = 81;
			int? num12 = this.fillWithThingsPadding;
			expr_07[arg_7E8_1] = ((!num12.HasValue) ? "null" : this.fillWithThingsPadding.ToString());
			expr_07[82] = ", factionBasePawnGroupPointsFactor=";
			int arg_81E_1 = 83;
			float? num13 = this.factionBasePawnGroupPointsFactor;
			expr_07[arg_81E_1] = ((!num13.HasValue) ? "null" : this.factionBasePawnGroupPointsFactor.ToString());
			expr_07[84] = ", streetHorizontal=";
			int arg_854_1 = 85;
			bool? flag6 = this.streetHorizontal;
			expr_07[arg_854_1] = ((!flag6.HasValue) ? "null" : this.streetHorizontal.ToString());
			expr_07[86] = ", edgeThingAvoidOtherEdgeThings=";
			int arg_88A_1 = 87;
			bool? flag7 = this.edgeThingAvoidOtherEdgeThings;
			expr_07[arg_88A_1] = ((!flag7.HasValue) ? "null" : this.edgeThingAvoidOtherEdgeThings.ToString());
			return string.Concat(expr_07);
		}
	}
}
