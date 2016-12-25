using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RuleDef : Def
	{
		public string symbol;

		public List<SymbolResolver> resolvers;
	}
}
