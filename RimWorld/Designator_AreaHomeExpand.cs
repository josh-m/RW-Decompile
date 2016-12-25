using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaHomeExpand : Designator_AreaHome
	{
		public Designator_AreaHomeExpand() : base(DesignateMode.Add)
		{
			this.defaultLabel = "DesignatorAreaHomeExpand".Translate();
			this.defaultDesc = "DesignatorAreaHomeExpandDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOn", true);
			this.soundDragSustain = SoundDefOf.DesignateDragAreaAdd;
			this.soundDragChanged = SoundDefOf.DesignateDragAreaAddChanged;
			this.soundSucceeded = SoundDefOf.DesignateAreaAdd;
			this.tutorTag = "AreaHomeExpand";
		}
	}
}
