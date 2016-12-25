using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class CompForbiddable : ThingComp
	{
		private bool forbiddenInt;

		public bool Forbidden
		{
			get
			{
				return this.forbiddenInt;
			}
			set
			{
				if (value == this.forbiddenInt)
				{
					return;
				}
				this.forbiddenInt = value;
				if (this.parent.Spawned)
				{
					if (this.forbiddenInt)
					{
						this.parent.Map.listerHaulables.Notify_Forbidden(this.parent);
					}
					else
					{
						this.parent.Map.listerHaulables.Notify_Unforbidden(this.parent);
					}
					if (this.parent is Building_Door)
					{
						this.parent.Map.reachability.ClearCache();
					}
				}
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.forbiddenInt, "forbidden", false, false);
		}

		public override void PostDraw()
		{
			if (this.forbiddenInt)
			{
				if (this.parent is Blueprint || this.parent is Frame)
				{
					if (this.parent.def.size.x > 1 || this.parent.def.size.z > 1)
					{
						this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.ForbiddenBig);
					}
					else
					{
						this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.Forbidden);
					}
				}
				else if (this.parent.def.category == ThingCategory.Building)
				{
					this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.ForbiddenBig);
				}
				else
				{
					this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.Forbidden);
				}
			}
		}

		public override void PostSplitOff(Thing piece)
		{
			piece.SetForbidden(this.forbiddenInt, true);
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			Command_Toggle com = new Command_Toggle();
			com.hotKey = KeyBindingDefOf.CommandItemForbid;
			com.icon = TexCommand.Forbidden;
			com.isActive = (() => !this.<>f__this.forbiddenInt);
			com.defaultLabel = "CommandForbid".Translate();
			if (this.forbiddenInt)
			{
				com.defaultDesc = "CommandForbiddenDesc".Translate();
			}
			else
			{
				com.defaultDesc = "CommandNotForbiddenDesc".Translate();
			}
			if (this.parent.def.IsDoor)
			{
				com.tutorTag = "ToggleForbidden-Door";
				com.toggleAction = delegate
				{
					this.<>f__this.Forbidden = !this.<>f__this.Forbidden;
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.ForbiddingDoors, KnowledgeAmount.SpecificInteraction);
				};
			}
			else
			{
				com.tutorTag = "ToggleForbidden";
				com.toggleAction = delegate
				{
					this.<>f__this.Forbidden = !this.<>f__this.Forbidden;
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Forbidding, KnowledgeAmount.SpecificInteraction);
				};
			}
			yield return com;
		}
	}
}
