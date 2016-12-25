using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Tame : Designator
	{
		private List<Pawn> justDesignated = new List<Pawn>();

		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}

		public Designator_Tame()
		{
			this.defaultLabel = "DesignatorTame".Translate();
			this.defaultDesc = "DesignatorTameDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Tame", true);
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.useMouseIcon = true;
			this.soundSucceeded = SoundDefOf.DesignateClaim;
			this.hotKey = KeyBindingDefOf.Misc4;
			this.tutorTag = "Tame";
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (!this.TameablesInCell(c).Any<Pawn>())
			{
				return "MessageMustDesignateTameable".Translate();
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc)
		{
			foreach (Pawn current in this.TameablesInCell(loc))
			{
				this.DesignateThing(current);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null && pawn.def.race.Animal && pawn.Faction == null && pawn.RaceProps.wildness < 1f && !pawn.HostileTo(t) && base.Map.designationManager.DesignationOn(pawn, DesignationDefOf.Tame) == null)
			{
				return true;
			}
			return false;
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			foreach (PawnKindDef kind in (from p in this.justDesignated
			select p.kindDef).Distinct<PawnKindDef>())
			{
				if (kind.RaceProps.manhunterOnTameFailChance > 0f)
				{
					Messages.Message("MessageAnimalManhuntsOnTameFailed".Translate(new object[]
					{
						kind.label,
						kind.RaceProps.manhunterOnTameFailChance.ToStringPercent("F2")
					}), this.justDesignated.First((Pawn x) => x.kindDef == kind), MessageSound.Standard);
				}
			}
			IEnumerable<Pawn> source = from c in base.Map.mapPawns.FreeColonistsSpawned
			where c.workSettings.WorkIsActive(WorkTypeDefOf.Handling)
			select c;
			if (!source.Any<Pawn>())
			{
				source = base.Map.mapPawns.FreeColonistsSpawned;
			}
			if (source.Any<Pawn>())
			{
				Pawn pawn = source.MaxBy((Pawn c) => c.skills.GetSkill(SkillDefOf.Animals).Level);
				int level = pawn.skills.GetSkill(SkillDefOf.Animals).Level;
				foreach (ThingDef ad in (from t in this.justDesignated
				select t.def).Distinct<ThingDef>())
				{
					int num = Mathf.RoundToInt(ad.GetStatValueAbstract(StatDefOf.MinimumHandlingSkill, null));
					if (num > level)
					{
						Messages.Message("MessageNoHandlerSkilledEnough".Translate(new object[]
						{
							ad.label,
							num.ToStringCached(),
							SkillDefOf.Animals.LabelCap,
							pawn.LabelShort,
							level
						}), this.justDesignated.First((Pawn x) => x.def == ad), MessageSound.Negative);
					}
				}
			}
			this.justDesignated.Clear();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AnimalTaming, KnowledgeAmount.Total);
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.RemoveAllDesignationsOn(t, false);
			base.Map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.Tame));
			this.justDesignated.Add((Pawn)t);
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> TameablesInCell(IntVec3 c)
		{
			if (!c.Fogged(base.Map))
			{
				List<Thing> thingList = c.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (this.CanDesignateThing(thingList[i]).Accepted)
					{
						yield return (Pawn)thingList[i];
					}
				}
			}
		}
	}
}
