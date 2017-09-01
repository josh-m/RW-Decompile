using System;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Interior_DiningRoom : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push("indoorLighting", rp);
			BaseGen.symbolStack.Push("randomlyPlaceMealsOnTables", rp);
			BaseGen.symbolStack.Push("placeChairsNearTables", rp);
			int num = Mathf.Max(GenMath.RoundRandom((float)rp.rect.Area / 20f), 1);
			for (int i = 0; i < num; i++)
			{
				ResolveParams resolveParams = rp;
				resolveParams.singleThingDef = ThingDefOf.TableShort;
				BaseGen.symbolStack.Push("thing", resolveParams);
			}
		}
	}
}
