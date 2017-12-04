using System;
using Verse;

namespace RimWorld
{
	public static class PawnGenerationContextUtility
	{
		public static string ToStringHuman(this PawnGenerationContext context)
		{
			if (context == PawnGenerationContext.All)
			{
				return "PawnGenerationContext_All".Translate();
			}
			if (context == PawnGenerationContext.PlayerStarter)
			{
				return "PawnGenerationContext_PlayerStarter".Translate();
			}
			if (context != PawnGenerationContext.NonPlayer)
			{
				throw new NotImplementedException();
			}
			return "PawnGenerationContext_NonPlayer".Translate();
		}

		public static bool Includes(this PawnGenerationContext includer, PawnGenerationContext other)
		{
			return includer == PawnGenerationContext.All || includer == other;
		}

		public static PawnGenerationContext GetRandom()
		{
			Array values = Enum.GetValues(typeof(PawnGenerationContext));
			return (PawnGenerationContext)values.GetValue(Rand.Range(0, values.Length));
		}

		public static bool OverlapsWith(this PawnGenerationContext a, PawnGenerationContext b)
		{
			return a == PawnGenerationContext.All || b == PawnGenerationContext.All || a == b;
		}
	}
}
