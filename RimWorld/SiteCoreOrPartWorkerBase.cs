using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public abstract class SiteCoreOrPartWorkerBase
	{
		public SiteCoreOrPartDefBase def;

		public virtual void PostMapGenerate(Map map)
		{
		}

		public virtual bool FactionCanOwn(Faction faction)
		{
			return true;
		}

		public virtual string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			preferredLetterDef = this.def.arrivedLetterDef;
			lookTargets = null;
			return this.def.arrivedLetter;
		}

		public virtual string GetPostProcessedDescriptionDialogue(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			return this.def.descriptionDialogue;
		}

		public virtual string GetPostProcessedThreatLabel(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			return this.def.label;
		}

		public virtual SiteCoreOrPartParams GenerateDefaultParams(Site site, float myThreatPoints)
		{
			return new SiteCoreOrPartParams
			{
				randomValue = Rand.Int,
				threatPoints = ((!this.def.wantsThreatPoints) ? 0f : myThreatPoints)
			};
		}
	}
}
