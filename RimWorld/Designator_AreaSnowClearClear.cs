using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaSnowClearClear : Designator_AreaSnowClear
	{
		public Designator_AreaSnowClearClear() : base(DesignateMode.Remove)
		{
			this.defaultLabel = "DesignatorAreaSnowClearClear".Translate();
			this.defaultDesc = "DesignatorAreaSnowClearClearDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/SnowClearAreaOff", true);
			this.soundDragSustain = SoundDefOf.DesignateDragAreaDelete;
			this.soundDragChanged = SoundDefOf.DesignateDragAreaDeleteChanged;
			this.soundSucceeded = SoundDefOf.DesignateAreaDelete;
		}
	}
}
