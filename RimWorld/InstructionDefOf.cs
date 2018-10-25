using System;

namespace RimWorld
{
	[DefOf]
	public static class InstructionDefOf
	{
		public static InstructionDef RandomizeCharacter;

		static InstructionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(InstructionDefOf));
		}
	}
}
