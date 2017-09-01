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
				verb.CasterPawn.jobs.StartJob(job, JobCondition.None, null, false, true, null, null);
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
			if (this.IsTargeting)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					if (this.targetingVerb != null)
					{
						this.OrderVerbForceTarget();
					}
					if (this.action != null)
					{
						LocalTargetInfo obj = this.CurrentTargetUnderMouse(false);
						if (obj.IsValid)
						{
							this.action(obj);
						}
					}
					SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
					this.StopTargeting();
					Event.current.Use();
				}
				if ((Event.current.type == EventType.MouseDown && Event.current.button == 1) || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape))
				{
					SoundDefOf.CancelMode.PlayOneShotOnCamera(null);
					this.StopTargeting();
					Event.current.Use();
				}
			}
		}

		public void TargeterOnGUI()
		{
			if (this.targetingVerb != null)
			{
				Texture2D icon;
				if (this.CurrentTargetUnderMouse(true).IsValid)
				{
					if (this.targetingVerb.UIIcon != BaseContent.BadTex)
					{
						icon = this.targetingVerb.UIIcon;
					}
					else
					{
						icon = TexCommand.Attack;
					}
				}
				else
				{
					icon = TexCommand.CannotShoot;
				}
				GenUI.DrawMouseAttachment(icon);
			}
			if (this.action != null)
			{
				Texture2D icon2 = this.mouseAttachment ?? TexCommand.Attack;
				GenUI.DrawMouseAttachment(icon2);
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
				LocalTargetInfo targ = this.CurrentTargetUnderMouse(true);
				if (targ.IsValid)
				{
					GenDraw.DrawTargetHighlight(targ);
					ShootLine shootLine;
					if (this.targetingVerb.HighlightFieldRadiusAroundTarget() > 0.2f && this.targetingVerb.TryFindShootLineFromTo(this.targetingVerb.caster.Position, targ, out shootLine))
					{
						GenExplosion.RenderPredictedAreaOfEffect(shootLine.Dest, this.targetingVerb.HighlightFieldRadiusAroundTarget());
					}
				}
			}
			if (this.action != null)
			{
				LocalTargetInfo targ2 = this.CurrentTargetUnderMouse(false);
				if (targ2.IsValid)
				{
					GenDraw.DrawTargetHighlight(targ2);
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

		private void OrderVerbForceTarget()
		{
			if (this.targetingVerb.CasterIsPawn)
			{
				this.OrderPawnForceTarget(this.targetingVerb);
				for (int i = 0; i < this.targetingVerbAdditionalPawns.Count; i++)
				{
					Verb verb = this.GetTargetingVerb(this.targetingVerbAdditionalPawns[i]);
					if (verb != null)
					{
						this.OrderPawnForceTarget(verb);
					}
				}
			}
			else
			{
				int numSelected = Find.Selector.NumSelected;
				List<object> selectedObjects = Find.Selector.SelectedObjects;
				for (int j = 0; j < numSelected; j++)
				{
					Building_Turret building_Turret = selectedObjects[j] as Building_Turret;
					if (building_Turret != null && building_Turret.Map == Find.VisibleMap)
					{
						LocalTargetInfo targ = this.CurrentTargetUnderMouse(true);
						building_Turret.OrderAttack(targ);
					}
				}
			}
		}

		private void OrderPawnForceTarget(Verb verb)
		{
			LocalTargetInfo targetA = this.CurrentTargetUnderMouse(true);
			if (!targetA.IsValid)
			{
				return;
			}
			if (verb.verbProps.MeleeRange)
			{
				Job job = new Job(JobDefOf.AttackMelee, targetA);
				job.playerForced = true;
				Pawn pawn = targetA.Thing as Pawn;
				if (pawn != null)
				{
					job.killIncappedTarget = pawn.Downed;
				}
				verb.CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
			else
			{
				JobDef def = (!verb.verbProps.ai_IsWeapon) ? JobDefOf.UseVerbOnThing : JobDefOf.AttackStatic;
				Job job2 = new Job(def);
				job2.verbToUse = verb;
				job2.targetA = targetA;
				verb.CasterPawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
			}
		}

		private LocalTargetInfo CurrentTargetUnderMouse(bool mustBeHittableNowIfNotMelee)
		{
			if (!this.IsTargeting)
			{
				return LocalTargetInfo.Invalid;
			}
			TargetingParameters clickParams = (this.targetingVerb == null) ? this.targetParams : this.targetingVerb.verbProps.targetParams;
			LocalTargetInfo localTargetInfo = LocalTargetInfo.Invalid;
			using (IEnumerator<LocalTargetInfo> enumerator = GenUI.TargetsAtMouse(clickParams, false).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					LocalTargetInfo current = enumerator.Current;
					localTargetInfo = current;
				}
			}
			if (localTargetInfo.IsValid && mustBeHittableNowIfNotMelee && !(localTargetInfo.Thing is Pawn) && this.targetingVerb != null && !this.targetingVerb.verbProps.MeleeRange)
			{
				if (this.targetingVerbAdditionalPawns != null && this.targetingVerbAdditionalPawns.Any<Pawn>())
				{
					bool flag = false;
					for (int i = 0; i < this.targetingVerbAdditionalPawns.Count; i++)
					{
						Verb verb = this.GetTargetingVerb(this.targetingVerbAdditionalPawns[i]);
						if (verb != null && verb.CanHitTarget(localTargetInfo))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						localTargetInfo = LocalTargetInfo.Invalid;
					}
				}
				else if (!this.targetingVerb.CanHitTarget(localTargetInfo))
				{
					localTargetInfo = LocalTargetInfo.Invalid;
				}
			}
			return localTargetInfo;
		}

		private Verb GetTargetingVerb(Pawn pawn)
		{
			return pawn.equipment.AllEquipmentVerbs.FirstOrDefault((Verb x) => x.verbProps == this.targetingVerb.verbProps);
		}
	}
}
