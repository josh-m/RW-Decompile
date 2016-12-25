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

		public float? chanceToSkipSandbag;

		public RoofDef roofDef;

		public bool? noRoof;

		public ThingDef singleThingDef;

		public int? singleThingStackCount;

		public Pawn singlePawnToSpawn;

		public PawnKindDef singlePawnKindDef;

		public bool? disableSinglePawn;

		public Lord singlePawnLord;

		public Predicate<IntVec3> singlePawnSpawnCellExtraPredicate;

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
			object[] expr_07 = new object[50];
			expr_07[0] = "rect=";
			expr_07[1] = this.rect;
			expr_07[2] = ", faction=";
			expr_07[3] = ((this.faction == null) ? "null" : this.faction.ToString());
			expr_07[4] = ", custom=";
			expr_07[5] = ((this.custom == null) ? "null" : this.custom.Count.ToString());
			expr_07[6] = ", pawnGroupMakerParams=";
			expr_07[7] = ((this.pawnGroupMakerParams == null) ? "null" : this.pawnGroupMakerParams.ToString());
			expr_07[8] = ", pawnGroupKindDef=";
			expr_07[9] = ((this.pawnGroupKindDef == null) ? "null" : this.pawnGroupKindDef.ToString());
			expr_07[10] = ", chanceToSkipSandbag=";
			int arg_106_1 = 11;
			float? num = this.chanceToSkipSandbag;
			expr_07[arg_106_1] = ((!num.HasValue) ? "null" : this.chanceToSkipSandbag.ToString());
			expr_07[12] = ", roofDef=";
			expr_07[13] = ((this.roofDef == null) ? "null" : this.roofDef.ToString());
			expr_07[14] = ", noRoof=";
			int arg_168_1 = 15;
			bool? flag = this.noRoof;
			expr_07[arg_168_1] = ((!flag.HasValue) ? "null" : this.noRoof.ToString());
			expr_07[16] = ", singleThingDef=";
			expr_07[17] = ((this.singleThingDef == null) ? "null" : this.singleThingDef.ToString());
			expr_07[18] = ", singleThingStackCount=";
			int arg_1CA_1 = 19;
			int? num2 = this.singleThingStackCount;
			expr_07[arg_1CA_1] = ((!num2.HasValue) ? "null" : this.singleThingStackCount.ToString());
			expr_07[20] = ", singlePawnToSpawn=";
			expr_07[21] = ((this.singlePawnToSpawn == null) ? "null" : this.singlePawnToSpawn.ToString());
			expr_07[22] = ", singlePawnKindDef=";
			expr_07[23] = ((this.singlePawnKindDef == null) ? "null" : this.singlePawnKindDef.ToString());
			expr_07[24] = ", disableSinglePawn=";
			int arg_25A_1 = 25;
			bool? flag2 = this.disableSinglePawn;
			expr_07[arg_25A_1] = ((!flag2.HasValue) ? "null" : this.disableSinglePawn.ToString());
			expr_07[26] = ", singlePawnLord=";
			expr_07[27] = ((this.singlePawnLord == null) ? "null" : this.singlePawnLord.ToString());
			expr_07[28] = ", singlePawnSpawnCellExtraPredicate=";
			expr_07[29] = ((this.singlePawnSpawnCellExtraPredicate == null) ? "null" : this.singlePawnSpawnCellExtraPredicate.ToString());
			expr_07[30] = ", mechanoidsCount=";
			int arg_2EA_1 = 31;
			int? num3 = this.mechanoidsCount;
			expr_07[arg_2EA_1] = ((!num3.HasValue) ? "null" : this.mechanoidsCount.ToString());
			expr_07[32] = ", hivesCount=";
			int arg_320_1 = 33;
			int? num4 = this.hivesCount;
			expr_07[arg_320_1] = ((!num4.HasValue) ? "null" : this.hivesCount.ToString());
			expr_07[34] = ", disableHives=";
			int arg_356_1 = 35;
			bool? flag3 = this.disableHives;
			expr_07[arg_356_1] = ((!flag3.HasValue) ? "null" : this.disableHives.ToString());
			expr_07[36] = ", thingRot=";
			int arg_38C_1 = 37;
			Rot4? rot = this.thingRot;
			expr_07[arg_38C_1] = ((!rot.HasValue) ? "null" : this.thingRot.ToString());
			expr_07[38] = ", wallStuff=";
			expr_07[39] = ((this.wallStuff == null) ? "null" : this.wallStuff.ToString());
			expr_07[40] = ", chanceToSkipWallBlock=";
			int arg_3EF_1 = 41;
			float? num5 = this.chanceToSkipWallBlock;
			expr_07[arg_3EF_1] = ((!num5.HasValue) ? "null" : this.chanceToSkipWallBlock.ToString());
			expr_07[42] = ", floorDef=";
			expr_07[43] = ((this.floorDef == null) ? "null" : this.floorDef.ToString());
			expr_07[44] = ", clearEdificeOnly=";
			int arg_452_1 = 45;
			bool? flag4 = this.clearEdificeOnly;
			expr_07[arg_452_1] = ((!flag4.HasValue) ? "null" : this.clearEdificeOnly.ToString());
			expr_07[46] = ", ancientCryptosleepCasketGroupID=";
			int arg_488_1 = 47;
			int? num6 = this.ancientCryptosleepCasketGroupID;
			expr_07[arg_488_1] = ((!num6.HasValue) ? "null" : this.ancientCryptosleepCasketGroupID.ToString());
			expr_07[48] = ", podContentsType=";
			int arg_4BE_1 = 49;
			PodContentsType? podContentsType = this.podContentsType;
			expr_07[arg_4BE_1] = ((!podContentsType.HasValue) ? "null" : this.podContentsType.ToString());
			return string.Concat(expr_07);
		}
	}
}
