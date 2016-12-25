using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Targeter
	{
		public Verb targetingVerb;

		public List<Pawn> targetingVerbAdditionalPawns;

		private Action<LocalTargetInfo> action;

		private Pawn caster;

		private TargetingParameters targetParams;

		private Action actionWhenFinished;

		private Texture2D mouseAttachment;

		public bool IsTargeting
		{
			get
			{
				return this.targetingVerb != null || this.action != null;
			}
		}

		public void BeginTargeting(Verb verb)
		{
			if (verb.verbProps.targetable)
			{
				this.targetingVerb = verb;
				this.targetingVerbAdditionalPawns = new List<Pawn>();
			}
			else
			{
				Job job = new Job(JobDefOf.UseVerbOnThing);
				job.verbToUse = verb;
				verb.CasterPawn.jobs.StartJob(job, JobCondition.None, null, false, true, null);
			}
			this.action = null;
			this.caster = null;
			this.targetParams = null;
			this.actionWhenFinished = null;
			this.mouseAttachment = null;
		}

		public void BeginTargeting(TargetingParameters targetParams, Action<LocalTargetInfo> action, Pawn caster = null, Action actionWhenFinished = null, Texture2D mouseAttachment = null)
		{
			this.targetingVerb = null;
			this.targetingVerbAdditionalPawns = null;
			this.action = action;
			this.targetParams = targetParams;
			this.caster = caster;
			this.actionWhenFinished = actionWhenFinished;
			this.mouseAttachment = mouseAttachment;
		}

		public void StopTargeting()
		{
			if (this.actionWhenFinished != null)
			{
				this.actionWhenFinished();
				this.actionWhenFinished = null;
			}
			this.targetingVerb = null;
			this.action = null;
		}

		public void ProcessInputEvents()
		{
			this.ConfirmStillValid();
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0 && this.IsTargeting)
				{
					if (this.targetingVerb != null)
					{
						this.CastVerb();
					}
					if (this.action != null)
					{
						IEnumerable<LocalTargetInfo> source = GenUI.TargetsAtMouse(this.targetParams, false);
						if (source.Any<LocalTargetInfo>())
						{
							this.action(source.First<LocalTargetInfo>());
						}
					}
					SoundDefOf.TickHigh.PlayOneShotOnCamera();
					this.StopTargeting();
					Event.current.Use();
				}
				if (Event.current.button == 1 && this.IsTargeting)
				{
					SoundDefOf.CancelMode.PlayOneShotOnCamera();
					this.StopTargeting();
					Event.current.Use();
				}
			}
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && this.IsTargeting)
			{
				SoundDefOf.CancelMode.PlayOneShotOnCamera();
				this.StopTargeting();
				Event.current.Use();
			}
		}

		public void TargeterOnGUI()
		{
			if (this.targetingVerb != null)
			{
				Vector3 vector = Event.current.mousePosition;
				Texture2D image;
				if (this.targetingVerb.verbProps.MeleeRange || this.targetingVerb.CanHitTarget(UI.MouseCell()) || !Find.Targeter.targetingVerbAdditionalPawns.NullOrEmpty<Pawn>())
				{
					if (this.targetingVerb.UIIcon != BaseContent.BadTex)
					{
						image = this.targetingVerb.UIIcon;
					}
					else
					{
						image = TexCommand.Attack;
					}
				}
				else
				{
					image = TexCommand.CannotShoot;
				}
				GUI.DrawTexture(new Rect(vector.x + 8f, vector.y + 8f, 32f, 32f), image);
			}
			if (this.action != null)
			{
				Texture2D image2 = this.mouseAttachment ?? TexCommand.Attack;
				Vector3 vector2 = Event.current.mousePosition;
				GUI.DrawTexture(new Rect(vector2.x + 8f, vector2.y + 8f, 32f, 32f), image2);
			}
		}

		public void TargeterUpdate()
		{
			if (this.targetingVerb != null)
			{
				if (!this.targetingVerb.verbProps.MeleeRange)
				{
					if (this.targetingVerb.verbProps.minRange < GenRadial.MaxRadialPatternRadius)
					{
						GenDraw.DrawRadiusRing(this.targetingVerb.caster.Position, this.targetingVerb.verbProps.minRange);
					}
					if (this.targetingVerb.verbProps.range < GenRadial.MaxRadialPatternRadius)
					{
						GenDraw.DrawRadiusRing(this.targetingVerb.caster.Position, this.targetingVerb.verbProps.range);
					}
				}
				LocalTargetInfo targ = LocalTargetInfo.Invalid;
				foreach (LocalTargetInfo current in GenUI.TargetsAtMouse(this.targetingVerb.verbProps.targetParams, false))
				{
					if (this.targetingVerb.CanHitTarget(current) || this.targetingVerb.verbProps.MeleeRange)
					{
						GenDraw.DrawTargetHighlight(current);
						targ = current;
					}
				}
				if (this.targetingVerb.CanTargetCell)
				{
					targ = UI.MouseCell();
					if (!targ.Cell.InBounds(Find.VisibleMap))
					{
						targ = LocalTargetInfo.Invalid;
					}
				}
				ShootLine shootLine;
				if (targ.IsValid && this.targetingVerb.HighlightFieldRadiusAroundTarget() > 0.2f && this.targetingVerb.TryFindShootLineFromTo(this.targetingVerb.caster.Position, targ, out shootLine))
				{
					GenExplosion.RenderPredictedAreaOfEffect(shootLine.Dest, this.targetingVerb.HighlightFieldRadiusAroundTarget());
				}
			}
			if (this.action != null)
			{
				foreach (LocalTargetInfo current2 in GenUI.TargetsAtMouse(this.targetParams, false))
				{
					GenDraw.DrawTargetHighlight(current2);
				}
			}
		}

		public bool IsPawnTargeting(Pawn p)
		{
			if (this.caster == p)
			{
				return true;
			}
			if (this.targetingVerb != null && this.targetingVerb.CasterIsPawn)
			{
				if (this.targetingVerb.CasterPawn == p)
				{
					return true;
				}
				for (int i = 0; i < this.targetingVerbAdditionalPawns.Count; i++)
				{
					if (this.targetingVerbAdditionalPawns[i] == p)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void ConfirmStillValid()
		{
			if (this.caster != null && (this.caster.Map != Find.VisibleMap || this.caster.Destroyed || !Find.Selector.IsSelected(this.caster)))
			{
				this.StopTargeting();
			}
			if (this.targetingVerb != null)
			{
				Selector selector = Find.Selector;
				if (this.targetingVerb.caster.Map != Find.VisibleMap || this.targetingVerb.caster.Destroyed || !selector.IsSelected(this.targetingVerb.caster))
				{
					this.StopTargeting();
				}
				else
				{
					for (int i = 0; i < this.targetingVerbAdditionalPawns.Count; i++)
					{
						if (this.targetingVerbAdditionalPawns[i].Destroyed || !selector.IsSelected(this.targetingVerbAdditionalPawns[i]))
						{
							this.StopTargeting();
							break;
						}
					}
				}
			}
		}

		private void CastVerb()
		{
			if (this.targetingVerb.CasterIsPawn)
			{
				this.CastPawnVerb(this.targetingVerb);
				for (int i = 0; i < this.targetingVerbAdditionalPawns.Count; i++)
				{
					Verb verb = (from x in this.targetingVerbAdditionalPawns[i].equipment.AllEquipmentVerbs
					where x.verbProps == this.targetingVerb.verbProps
					select x).FirstOrDefault<Verb>();
					if (verb != null)
					{
						this.CastPawnVerb(verb);
					}
				}
			}
			else
			{
				Building_Turret building_Turret = (Building_Turret)this.targetingVerb.caster;
				if (building_Turret.Map == Find.VisibleMap)
				{
					LocalTargetInfo targ = LocalTargetInfo.Invalid;
					if (this.targetingVerb.CanTargetCell)
					{
						targ = UI.MouseCell();
						if (!targ.Cell.InBounds(Find.VisibleMap))
						{
							targ = LocalTargetInfo.Invalid;
						}
					}
					using (IEnumerator<LocalTargetInfo> enumerator = GenUI.TargetsAtMouse(this.targetingVerb.verbProps.targetParams, false).GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							LocalTargetInfo current = enumerator.Current;
							targ = current;
						}
					}
					building_Turret.OrderAttack(targ);
				}
			}
		}

		private void CastPawnVerb(Verb verb)
		{
			foreach (LocalTargetInfo current in GenUI.TargetsAtMouse(verb.verbProps.targetParams, false))
			{
				LocalTargetInfo targetA = current;
				if (verb.verbProps.MeleeRange)
				{
					Job job = new Job(JobDefOf.AttackMelee, targetA);
					job.playerForced = true;
					Pawn pawn = targetA.Thing as Pawn;
					if (pawn != null)
					{
						job.killIncappedTarget = pawn.Downed;
					}
					verb.CasterPawn.jobs.TryTakeOrderedJob(job);
				}
				else
				{
					JobDef def;
					if (verb.verbProps.ai_IsWeapon)
					{
						def = JobDefOf.AttackStatic;
					}
					else
					{
						def = JobDefOf.UseVerbOnThing;
					}
					Job job2 = new Job(def);
					job2.verbToUse = verb;
					job2.targetA = targetA;
					verb.CasterPawn.jobs.TryTakeOrderedJob(job2);
				}
			}
		}
	}
}
