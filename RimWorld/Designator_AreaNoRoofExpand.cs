using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaNoRoofExpand : Designator_AreaNoRoof
	{
		public Designator_AreaNoRoofExpand() : base(DesignateMode.Add)
		{
			this.defaultLabel = "DesignatorAreaNoRoofExpand".Translate();
			this.defaultDesc = "DesignatorAreaNoRoofExpandDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/NoRoofAreaOn", true);
			this.hotKey = KeyBindingDefOf.Misc5;
			this.soundDragSustain = SoundDefOf.DesignateDragAreaAdd;
			this.soundDragChanged = SoundDefOf.DesignateDragAreaAddChanged;
			this.soundSucceeded = SoundDefOf.DesignateAreaAdd;
		}
	}
}
