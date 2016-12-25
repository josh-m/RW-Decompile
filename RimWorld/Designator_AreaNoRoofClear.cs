using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaNoRoofClear : Designator_AreaNoRoof
	{
		public Designator_AreaNoRoofClear() : base(DesignateMode.Remove)
		{
			this.defaultLabel = "DesignatorAreaNoRoofClear".Translate();
			this.defaultDesc = "DesignatorAreaNoRoofClearDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/NoRoofAreaOff", true);
			this.hotKey = KeyBindingDefOf.Misc6;
			this.soundDragSustain = SoundDefOf.DesignateDragAreaDelete;
			this.soundDragChanged = SoundDefOf.DesignateDragAreaDeleteChanged;
			this.soundSucceeded = SoundDefOf.DesignateAreaDelete;
		}
	}
}
