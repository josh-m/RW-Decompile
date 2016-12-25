using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public static class BaseGen
	{
		private const int MaxResolvedSymbols = 100000;

		public static GlobalSettings globalSettings = new GlobalSettings();

		public static SymbolStack symbolStack = new SymbolStack();

		private static bool working;

		private static List<RuleDef> tmpRuleDefs = new List<RuleDef>();

		private static List<SymbolResolver> tmpResolvers = new List<SymbolResolver>();

		public static void Generate()
		{
			if (BaseGen.working)
			{
				Log.Error("Cannot call Generate() while already generating. Nested calls are not allowed.");
				return;
			}
			BaseGen.working = true;
			try
			{
				if (BaseGen.symbolStack.Empty)
				{
					Log.Warning("Symbol stack is empty.");
				}
				else if (BaseGen.globalSettings.map == null)
				{
					Log.Error("Called BaseGen.Resolve() with null map.");
				}
				else
				{
					int num = 0;
					while (!BaseGen.symbolStack.Empty)
					{
						num++;
						if (num > 100000)
						{
							Log.Error("Error in BaseGen: Too many iterations. Infinite loop?");
							break;
						}
						Pair<string, ResolveParams> toResolve = BaseGen.symbolStack.Pop();
						try
						{
							BaseGen.Resolve(toResolve);
						}
						catch (Exception ex)
						{
							Log.Error(string.Concat(new object[]
							{
								"Error while resolving symbol \"",
								toResolve.First,
								"\" with params=",
								toResolve.Second,
								"\n\nException: ",
								ex
							}));
						}
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error("Error in BaseGen: " + arg);
			}
			finally
			{
				BaseGen.working = false;
				BaseGen.symbolStack.Clear();
				BaseGen.globalSettings.Clear();
			}
		}

		private static void Resolve(Pair<string, ResolveParams> toResolve)
		{
			string first = toResolve.First;
			ResolveParams second = toResolve.Second;
			BaseGen.tmpRuleDefs.Clear();
			List<RuleDef> allDefsListForReading = DefDatabase<RuleDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				RuleDef ruleDef = allDefsListForReading[i];
				if (!(ruleDef.symbol != first))
				{
					bool flag = false;
					for (int j = 0; j < ruleDef.resolvers.Count; j++)
					{
						if (ruleDef.resolvers[j].CanResolve(second))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						BaseGen.tmpRuleDefs.Add(ruleDef);
					}
				}
			}
			if (!BaseGen.tmpRuleDefs.Any<RuleDef>())
			{
				Log.Warning(string.Concat(new object[]
				{
					"Could not find any RuleDef for symbol \"",
					first,
					"\" with any resolver that could resolve ",
					second
				}));
				return;
			}
			RuleDef ruleDef2 = BaseGen.tmpRuleDefs.RandomElement<RuleDef>();
			BaseGen.tmpResolvers.Clear();
			for (int k = 0; k < ruleDef2.resolvers.Count; k++)
			{
				SymbolResolver symbolResolver = ruleDef2.resolvers[k];
				if (symbolResolver.CanResolve(second))
				{
					BaseGen.tmpResolvers.Add(symbolResolver);
				}
			}
			SymbolResolver symbolResolver2 = BaseGen.tmpResolvers.RandomElement<SymbolResolver>();
			symbolResolver2.Resolve(second);
		}
	}
}
