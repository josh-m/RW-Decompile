using RimWorld;
using System;
using System.Runtime.InteropServices;

namespace Verse
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PawnGenerationRequest
	{
		public PawnKindDef KindDef
		{
			get;
			private set;
		}

		public PawnGenerationContext Context
		{
			get;
			private set;
		}

		public Faction Faction
		{
			get;
			private set;
		}

		public int Tile
		{
			get;
			private set;
		}

		public bool ForceGenerateNewPawn
		{
			get;
			private set;
		}

		public bool Newborn
		{
			get;
			private set;
		}

		public bool AllowDead
		{
			get;
			private set;
		}

		public bool AllowDowned
		{
			get;
			private set;
		}

		public bool CanGeneratePawnRelations
		{
			get;
			private set;
		}

		public bool MustBeCapableOfViolence
		{
			get;
			private set;
		}

		public float ColonistRelationChanceFactor
		{
			get;
			private set;
		}

		public bool ForceAddFreeWarmLayerIfNeeded
		{
			get;
			private set;
		}

		public bool AllowGay
		{
			get;
			private set;
		}

		public bool AllowFood
		{
			get;
			private set;
		}

		public bool Inhabitant
		{
			get;
			private set;
		}

		public bool CertainlyBeenInCryptosleep
		{
			get;
			private set;
		}

		public bool ForceRedressWorldPawnIfFormerColonist
		{
			get;
			private set;
		}

		public bool WorldPawnFactionDoesntMatter
		{
			get;
			private set;
		}

		public Predicate<Pawn> ValidatorPreGear
		{
			get;
			private set;
		}

		public Predicate<Pawn> ValidatorPostGear
		{
			get;
			private set;
		}

		public float? MinChanceToRedressWorldPawn
		{
			get;
			private set;
		}

		public float? FixedBiologicalAge
		{
			get;
			private set;
		}

		public float? FixedChronologicalAge
		{
			get;
			private set;
		}

		public Gender? FixedGender
		{
			get;
			private set;
		}

		public float? FixedMelanin
		{
			get;
			private set;
		}

		public string FixedLastName
		{
			get;
			private set;
		}

		public PawnGenerationRequest(PawnKindDef kind, Faction faction = null, PawnGenerationContext context = PawnGenerationContext.NonPlayer, int tile = -1, bool forceGenerateNewPawn = false, bool newborn = false, bool allowDead = false, bool allowDowned = false, bool canGeneratePawnRelations = true, bool mustBeCapableOfViolence = false, float colonistRelationChanceFactor = 1f, bool forceAddFreeWarmLayerIfNeeded = false, bool allowGay = true, bool allowFood = true, bool inhabitant = false, bool certainlyBeenInCryptosleep = false, bool forceRedressWorldPawnIfFormerColonist = false, bool worldPawnFactionDoesntMatter = false, Predicate<Pawn> validatorPreGear = null, Predicate<Pawn> validatorPostGear = null, float? minChanceToRedressWorldPawn = null, float? fixedBiologicalAge = null, float? fixedChronologicalAge = null, Gender? fixedGender = null, float? fixedMelanin = null, string fixedLastName = null)
		{
			this = default(PawnGenerationRequest);
			if (context == PawnGenerationContext.All)
			{
				Log.Error("Should not generate pawns with context 'All'", false);
				context = PawnGenerationContext.NonPlayer;
			}
			if (inhabitant && (tile == -1 || Current.Game.FindMap(tile) == null))
			{
				Log.Error("Trying to generate an inhabitant but map is null.", false);
				inhabitant = false;
			}
			this.KindDef = kind;
			this.Context = context;
			this.Faction = faction;
			this.Tile = tile;
			this.ForceGenerateNewPawn = forceGenerateNewPawn;
			this.Newborn = newborn;
			this.AllowDead = allowDead;
			this.AllowDowned = allowDowned;
			this.CanGeneratePawnRelations = canGeneratePawnRelations;
			this.MustBeCapableOfViolence = mustBeCapableOfViolence;
			this.ColonistRelationChanceFactor = colonistRelationChanceFactor;
			this.ForceAddFreeWarmLayerIfNeeded = forceAddFreeWarmLayerIfNeeded;
			this.AllowGay = allowGay;
			this.AllowFood = allowFood;
			this.Inhabitant = inhabitant;
			this.CertainlyBeenInCryptosleep = certainlyBeenInCryptosleep;
			this.ForceRedressWorldPawnIfFormerColonist = forceRedressWorldPawnIfFormerColonist;
			this.WorldPawnFactionDoesntMatter = worldPawnFactionDoesntMatter;
			this.ValidatorPreGear = validatorPreGear;
			this.ValidatorPostGear = validatorPostGear;
			this.MinChanceToRedressWorldPawn = minChanceToRedressWorldPawn;
			this.FixedBiologicalAge = fixedBiologicalAge;
			this.FixedChronologicalAge = fixedChronologicalAge;
			this.FixedGender = fixedGender;
			this.FixedMelanin = fixedMelanin;
			this.FixedLastName = fixedLastName;
		}

		public void SetFixedLastName(string fixedLastName)
		{
			if (this.FixedLastName != null)
			{
				Log.Error("Last name is already a fixed value: " + this.FixedLastName + ".", false);
				return;
			}
			this.FixedLastName = fixedLastName;
		}

		public void SetFixedMelanin(float fixedMelanin)
		{
			if (this.FixedMelanin.HasValue)
			{
				Log.Error("Melanin is already a fixed value: " + this.FixedMelanin + ".", false);
				return;
			}
			this.FixedMelanin = new float?(fixedMelanin);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"kindDef=",
				this.KindDef,
				", context=",
				this.Context,
				", faction=",
				this.Faction,
				", tile=",
				this.Tile,
				", forceGenerateNewPawn=",
				this.ForceGenerateNewPawn,
				", newborn=",
				this.Newborn,
				", allowDead=",
				this.AllowDead,
				", allowDowned=",
				this.AllowDowned,
				", canGeneratePawnRelations=",
				this.CanGeneratePawnRelations,
				", mustBeCapableOfViolence=",
				this.MustBeCapableOfViolence,
				", colonistRelationChanceFactor=",
				this.ColonistRelationChanceFactor,
				", forceAddFreeWarmLayerIfNeeded=",
				this.ForceAddFreeWarmLayerIfNeeded,
				", allowGay=",
				this.AllowGay,
				", allowFood=",
				this.AllowFood,
				", inhabitant=",
				this.Inhabitant,
				", certainlyBeenInCryptosleep=",
				this.CertainlyBeenInCryptosleep,
				", validatorPreGear=",
				this.ValidatorPreGear,
				", validatorPostGear=",
				this.ValidatorPostGear,
				", fixedBiologicalAge=",
				this.FixedBiologicalAge,
				", fixedChronologicalAge=",
				this.FixedChronologicalAge,
				", fixedGender=",
				this.FixedGender,
				", fixedMelanin=",
				this.FixedMelanin,
				", fixedLastName=",
				this.FixedLastName
			});
		}
	}
}
