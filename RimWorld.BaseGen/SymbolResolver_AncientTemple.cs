using System;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_AncientTemple : SymbolResolver
	{
		private const float MechanoidsChance = 0.5f;

		private const float ArtifactsChance = 0.9f;

		private const float LuciferiumChance = 0.9f;

		private const float HivesChance = 0.45f;

		private static readonly IntRange MechanoidCountRange = new IntRange(1, 5);

		private static readonly IntRange ArtifactsCountRange = new IntRange(1, 3);

		private static readonly IntRange HivesCountRange = new IntRange(1, 2);

		private static readonly IntRange LuciferiumCountRange = new IntRange(5, 20);

		public override void Resolve(ResolveParams rp)
		{
			if (Rand.Chance(0.9f))
			{
				ResolveParams resolveParams = rp;
				resolveParams.singleThingDef = ThingDefOf.Luciferium;
				int? singleThingStackCount = rp.singleThingStackCount;
				resolveParams.singleThingStackCount = new int?((!singleThingStackCount.HasValue) ? SymbolResolver_AncientTemple.LuciferiumCountRange.RandomInRange : singleThingStackCount.Value);
				BaseGen.symbolStack.Push("thing", resolveParams);
			}
			if (Rand.Chance(0.9f))
			{
				int randomInRange = SymbolResolver_AncientTemple.ArtifactsCountRange.RandomInRange;
				for (int i = 0; i < randomInRange; i++)
				{
					ResolveParams resolveParams2 = rp;
					resolveParams2.singleThingDef = (from x in DefDatabase<ThingDef>.AllDefs
					where x.HasComp(typeof(CompUseEffect_Artifact))
					select x).RandomElement<ThingDef>();
					BaseGen.symbolStack.Push("thing", resolveParams2);
				}
			}
			if (Rand.Chance(0.5f))
			{
				ResolveParams resolveParams3 = rp;
				int? mechanoidsCount = rp.mechanoidsCount;
				resolveParams3.mechanoidsCount = new int?((!mechanoidsCount.HasValue) ? SymbolResolver_AncientTemple.MechanoidCountRange.RandomInRange : mechanoidsCount.Value);
				BaseGen.symbolStack.Push("randomMechanoidGroup", resolveParams3);
			}
			else if (Rand.Chance(0.45f))
			{
				ResolveParams resolveParams4 = rp;
				int? hivesCount = rp.hivesCount;
				resolveParams4.hivesCount = new int?((!hivesCount.HasValue) ? SymbolResolver_AncientTemple.HivesCountRange.RandomInRange : hivesCount.Value);
				BaseGen.symbolStack.Push("hives", resolveParams4);
			}
			int? ancientTempleEntranceHeight = rp.ancientTempleEntranceHeight;
			int num = (!ancientTempleEntranceHeight.HasValue) ? 0 : ancientTempleEntranceHeight.Value;
			ResolveParams resolveParams5 = rp;
			resolveParams5.rect = rp.rect.ContractedBy(1);
			resolveParams5.rect.minZ = resolveParams5.rect.minZ + num;
			BaseGen.symbolStack.Push("ancientShrinesGroup", resolveParams5);
			ResolveParams resolveParams6 = rp;
			resolveParams6.wallStuff = (rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction, true));
			bool? clearEdificeOnly = rp.clearEdificeOnly;
			resolveParams6.clearEdificeOnly = new bool?(!clearEdificeOnly.HasValue || clearEdificeOnly.Value);
			BaseGen.symbolStack.Push("emptyRoom", resolveParams6);
		}
	}
}
