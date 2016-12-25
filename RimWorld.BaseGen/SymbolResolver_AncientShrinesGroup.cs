using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_AncientShrinesGroup : SymbolResolver
	{
		private const int MaxNumCaskets = 6;

		private const float SkipShrineChance = 0.25f;

		public const int MarginCells = 1;

		public static readonly IntVec2 StandardAncientShrineSize = new IntVec2(4, 3);

		public override void Resolve(ResolveParams rp)
		{
			int num = (rp.rect.Width + 1) / (SymbolResolver_AncientShrinesGroup.StandardAncientShrineSize.x + 1);
			int num2 = (rp.rect.Height + 1) / (SymbolResolver_AncientShrinesGroup.StandardAncientShrineSize.z + 1);
			IntVec3 bottomLeft = rp.rect.BottomLeft;
			PodContentsType? podContentsType = rp.podContentsType;
			if (!podContentsType.HasValue)
			{
				float value = Rand.Value;
				if (value < 0.5f)
				{
					podContentsType = null;
				}
				else if (value < 0.7f)
				{
					podContentsType = new PodContentsType?(PodContentsType.Slave);
				}
				else
				{
					podContentsType = new PodContentsType?(PodContentsType.SpacerHostile);
				}
			}
			int? ancientCryptosleepCasketGroupID = rp.ancientCryptosleepCasketGroupID;
			int value2 = (!ancientCryptosleepCasketGroupID.HasValue) ? Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID() : ancientCryptosleepCasketGroupID.Value;
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					if (!Rand.Chance(0.25f))
					{
						if (num3 >= 6)
						{
							break;
						}
						CellRect rect = new CellRect(bottomLeft.x + j * (SymbolResolver_AncientShrinesGroup.StandardAncientShrineSize.x + 1), bottomLeft.z + i * (SymbolResolver_AncientShrinesGroup.StandardAncientShrineSize.z + 1), SymbolResolver_AncientShrinesGroup.StandardAncientShrineSize.x, SymbolResolver_AncientShrinesGroup.StandardAncientShrineSize.z);
						if (rect.FullyContainedWithin(rp.rect))
						{
							ResolveParams resolveParams = rp;
							resolveParams.rect = rect;
							resolveParams.ancientCryptosleepCasketGroupID = new int?(value2);
							resolveParams.podContentsType = podContentsType;
							BaseGen.symbolStack.Push("ancientShrine", resolveParams);
							num3++;
						}
					}
				}
			}
		}
	}
}
