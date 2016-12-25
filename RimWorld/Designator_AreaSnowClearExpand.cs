using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaSnowClearExpand : Designator_AreaSnowClear
	{
		public Designator_AreaSnowClearExpand() : base(DesignateMode.Add)
		{
			this.defaultLabel = "DesignatorAreaSnowClearExpand".Translate();
			this.defaultDesc = "DesignatorAreaSnowClearExpandDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/SnowClearAreaOn", true);
			this.soundDragSustain = SoundDefOf.DesignateDragAreaAdd;
			this.soundDragChanged = SoundDefOf.DesignateDragAreaAddChanged;
			this.soundSucceeded = SoundDefOf.DesignateAreaAdd;
		}
	}
}
