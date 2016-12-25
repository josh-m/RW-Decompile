using RimWorld;
using System;

namespace Verse
{
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

		public Predicate<Pawn> Validator
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

		public float? FixedSkinWhiteness
		{
			get;
			private set;
		}

		public string FixedLastName
		{
			get;
			private set;
		}

		public PawnGenerationRequest(PawnKindDef kind, Faction faction = null, PawnGenerationContext context = PawnGenerationContext.NonPlayer, bool forceGenerateNewPawn = false, bool newborn = false, bool allowDead = false, bool allowDowned = false, bool canGeneratePawnRelations = true, bool mustBeCapableOfViolence = false, float colonistRelationChanceFactor = 1f, bool forceAddFreeWarmLayerIfNeeded = false, bool allowGay = true, bool allowFood = true, Predicate<Pawn> validator = null, float? fixedBiologicalAge = null, float? fixedChronologicalAge = null, Gender? fixedGender = null, float? fixedSkinWhiteness = null, string fixedLastName = null)
		{
			if (context == PawnGenerationContext.All)
			{
				Log.Error("Should not generate pawns with context 'All'");
			}
			this.KindDef = kind;
			this.Context = context;
			this.Faction = faction;
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
			this.Validator = validator;
			this.FixedBiologicalAge = fixedBiologicalAge;
			this.FixedChronologicalAge = fixedChronologicalAge;
			this.FixedGender = fixedGender;
			this.FixedSkinWhiteness = fixedSkinWhiteness;
			this.FixedLastName = fixedLastName;
		}

		public void EnsureNonNullFaction()
		{
			if (this.KindDef.RaceProps.Humanlike && this.Faction == null)
			{
				this.Faction = FactionUtility.DefaultFactionFrom(this.KindDef.defaultFactionType);
				Log.Error(string.Concat(new object[]
				{
					"Tried to generate pawn of Humanlike race ",
					this.KindDef,
					" with null faction. Setting to ",
					this.Faction
				}));
			}
		}

		public void SetFixedLastName(string fixedLastName)
		{
			if (this.FixedLastName != null)
			{
				Log.Error("Last name is already a fixed value: " + this.FixedLastName + ".");
				return;
			}
			this.FixedLastName = fixedLastName;
		}

		public void SetFixedSkinWhiteness(float fixedSkinWhiteness)
		{
			if (this.FixedSkinWhiteness.HasValue)
			{
				Log.Error("Skin whiteness is already a fixed value: " + this.FixedSkinWhiteness + ".");
				return;
			}
			this.FixedSkinWhiteness = new float?(fixedSkinWhiteness);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"kindDef=",
				this.KindDef,
				", faction=",
				this.Faction,
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
				", validator=",
				this.Validator,
				", fixedBiologicalAge=",
				this.FixedBiologicalAge,
				", fixedChronologicalAge=",
				this.FixedChronologicalAge,
				", fixedGender=",
				this.FixedGender,
				", fixedSkinWhiteness=",
				this.FixedSkinWhiteness,
				", fixedLastName=",
				this.FixedLastName
			});
		}
	}
}
