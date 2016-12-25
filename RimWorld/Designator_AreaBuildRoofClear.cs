using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaBuildRoofClear : Designator_AreaBuildRoof
	{
		public Designator_AreaBuildRoofClear() : base(DesignateMode.Remove)
		{
			this.defaultLabel = "DesignatorAreaBuildRoofClear".Translate();
			this.defaultDesc = "DesignatorAreaBuildRoofClearDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/BuildRoofAreaClear", true);
			this.hotKey = KeyBindingDefOf.Misc11;
			this.soundDragSustain = SoundDefOf.DesignateDragAreaDelete;
			this.soundDragChanged = SoundDefOf.DesignateDragAreaDeleteChanged;
			this.soundSucceeded = SoundDefOf.DesignateAreaDelete;
		}
	}
}
