using System;
using Verse;

namespace RimWorld
{
	public static class PawnGenerationContextUtility
	{
		public static string ToStringHuman(this PawnGenerationContext context)
		{
			switch (context)
			{
			case PawnGenerationContext.All:
				return "PawnGenerationContext_All".Translate();
			case PawnGenerationContext.PlayerStarter:
				return "PawnGenerationContext_PlayerStarter".Translate();
			case PawnGenerationContext.NonPlayer:
				return "PawnGenerationContext_NonPlayer".Translate();
			default:
				throw new NotImplementedException();
			}
		}

		public static bool Includes(this PawnGenerationContext includer, PawnGenerationContext other)
		{
			return includer == PawnGenerationContext.All || includer == other;
		}

		public static PawnGenerationContext GetRandom()
		{
			Array values = Enum.GetValues(typeof(PawnGenerationContext));
			return (PawnGenerationContext)((int)values.GetValue(Rand.Range(0, values.Length)));
		}
	}
}
