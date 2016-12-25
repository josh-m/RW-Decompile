using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Verse
{
	public class Pawn_EquipmentTracker : IExposable
	{
		private Pawn pawn;

		private ThingWithComps primaryInt;

		public CompEquippable PrimaryEq
		{
			get
			{
				return this.primaryInt.GetComp<CompEquippable>();
			}
		}

		public ThingWithComps Primary
		{
			get
			{
				return this.primaryInt;
			}
		}

		public IEnumerable<ThingWithComps> AllEquipment
		{
			get
			{
				if (this.primaryInt != null)
				{
					yield return this.primaryInt;
				}
			}
		}

		public IEnumerable<Verb> AllEquipmentVerbs
		{
			get
			{
				foreach (ThingWithComps eq in this.AllEquipment)
				{
					foreach (Verb verb in eq.GetComp<CompEquippable>().AllVerbs)
					{
						yield return verb;
					}
				}
			}
		}

		public Pawn_EquipmentTracker(Pawn newPawn)
		{
			this.pawn = newPawn;
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<ThingWithComps>(ref this.primaryInt, "primary", new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				foreach (ThingWithComps current in this.AllEquipment)
				{
					foreach (Verb current2 in current.GetComp<CompEquippable>().AllVerbs)
					{
						current2.caster = this.pawn;
						current2.ownerEquipment = current;
					}
				}
			}
		}

		public void EquipmentTrackerTick()
		{
			foreach (ThingWithComps current in this.AllEquipment)
			{
				current.GetComp<CompEquippable>().verbTracker.VerbsTick();
			}
		}

		public bool HasAnything()
		{
			return this.AllEquipment.Any<ThingWithComps>();
		}

		public void MakeRoomFor(ThingWithComps eq)
		{
			if (eq.def.equipmentType == EquipmentType.Primary && this.primaryInt != null)
			{
				ThingWithComps t;
				if (this.TryDropEquipment(this.primaryInt, out t, this.pawn.Position, true))
				{
					t.SetForbidden(false, true);
				}
				else
				{
					Log.Error(this.pawn + " couldn't make room for equipment " + eq);
				}
			}
		}

		public bool TryDropEquipment(ThingWithComps eq, out ThingWithComps resultingEq, IntVec3 pos, bool forbid = true)
		{
			if (!this.AllEquipment.Contains(eq))
			{
				Log.Warning(this.pawn.LabelCap + " tried to drop equipment they didn't have: " + eq);
				resultingEq = null;
				return false;
			}
			if (!pos.IsValid)
			{
				Log.Error(string.Concat(new object[]
				{
					this.pawn,
					" tried to drop ",
					eq,
					" at invalid cell."
				}));
				resultingEq = null;
				return false;
			}
			if (this.primaryInt == eq)
			{
				this.primaryInt = null;
			}
			Thing thing = null;
			bool flag = GenThing.TryDropAndSetForbidden(eq, pos, ThingPlaceMode.Near, out thing, forbid);
			resultingEq = (thing as ThingWithComps);
			if (flag && resultingEq != null)
			{
				resultingEq.GetComp<CompEquippable>().Notify_Dropped();
			}
			this.pawn.meleeVerbs.Notify_EquipmentLost();
			return flag;
		}

		public void DropAllEquipment(IntVec3 pos, bool forbid = true)
		{
			if (this.primaryInt != null)
			{
				ThingWithComps thingWithComps;
				this.TryDropEquipment(this.primaryInt, out thingWithComps, pos, forbid);
			}
		}

		public bool TryTransferEquipmentToContainer(ThingWithComps eq, ThingContainer container, out ThingWithComps resultingEq)
		{
			if (!this.AllEquipment.Contains(eq))
			{
				Log.Warning(this.pawn.LabelCap + " tried to transfer equipment he didn't have: " + eq);
				resultingEq = null;
				return false;
			}
			if (container.TryAdd(eq))
			{
				resultingEq = null;
			}
			else
			{
				resultingEq = eq;
			}
			if (this.primaryInt == eq)
			{
				this.primaryInt = null;
			}
			this.pawn.meleeVerbs.Notify_EquipmentLost();
			return resultingEq == null;
		}

		public void DestroyEquipment(ThingWithComps eq)
		{
			if (eq != null && eq == this.primaryInt)
			{
				this.primaryInt.Destroy(DestroyMode.Vanish);
				this.primaryInt = null;
			}
			this.pawn.meleeVerbs.Notify_EquipmentLost();
		}

		public void DestroyAllEquipment(DestroyMode mode = DestroyMode.Vanish)
		{
			if (this.primaryInt != null)
			{
				this.primaryInt.Destroy(mode);
				this.primaryInt = null;
			}
			this.pawn.meleeVerbs.Notify_EquipmentLost();
		}

		internal void Notify_PrimaryDestroyed()
		{
			this.primaryInt = null;
			this.pawn.meleeVerbs.Notify_EquipmentLost();
			if (this.pawn.Spawned)
			{
				this.pawn.stances.CancelBusyStanceSoft();
			}
		}

		public void AddEquipment(ThingWithComps newEq)
		{
			SlotGroupUtility.Notify_TakingThing(newEq);
			if ((from eq in this.AllEquipment
			where eq.def == newEq.def
			select eq).Any<ThingWithComps>())
			{
				Log.Error(string.Concat(new object[]
				{
					"Pawn ",
					this.pawn.LabelCap,
					" got equipment ",
					newEq,
					" while already having it."
				}));
				return;
			}
			if (newEq.def.equipmentType == EquipmentType.Primary && this.primaryInt != null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Pawn ",
					this.pawn.LabelCap,
					" got primaryInt equipment ",
					newEq,
					" while already having primaryInt equipment ",
					this.primaryInt
				}));
				return;
			}
			if (newEq.def.equipmentType == EquipmentType.Primary)
			{
				this.primaryInt = newEq;
			}
			foreach (Verb current in newEq.GetComp<CompEquippable>().AllVerbs)
			{
				current.caster = this.pawn;
				current.Notify_PickedUp();
			}
		}

		[DebuggerHidden]
		public IEnumerable<Gizmo> GetGizmos()
		{
			if (this.ShouldUseSquadAttackGizmo())
			{
				yield return this.GetSquadAttackGizmo();
			}
			else
			{
				int index = 0;
				foreach (ThingWithComps equipment in this.AllEquipment)
				{
					foreach (Command command in equipment.GetComp<CompEquippable>().GetVerbsCommands())
					{
						switch (index)
						{
						case 0:
							command.hotKey = KeyBindingDefOf.Misc1;
							break;
						case 1:
							command.hotKey = KeyBindingDefOf.Misc2;
							break;
						case 2:
							command.hotKey = KeyBindingDefOf.Misc3;
							break;
						}
						yield return command;
					}
				}
			}
		}

		public bool TryStartAttack(TargetInfo targ)
		{
			if (this.pawn.stances.FullBodyBusy)
			{
				return false;
			}
			if (this.pawn.story != null && this.pawn.story.DisabledWorkTags.Contains(WorkTags.Violent))
			{
				return false;
			}
			bool allowManualCastWeapons = !this.pawn.IsColonist;
			Verb verb = this.pawn.TryGetAttackVerb(allowManualCastWeapons);
			return verb != null && verb.TryStartCastOn(targ, false, true);
		}

		private bool ShouldUseSquadAttackGizmo()
		{
			if (Find.Selector.NumSelected <= 1)
			{
				return false;
			}
			ThingDef thingDef = null;
			bool flag = false;
			List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
			for (int i = 0; i < selectedObjectsListForReading.Count; i++)
			{
				Pawn pawn = selectedObjectsListForReading[i] as Pawn;
				if (pawn != null && pawn.IsColonist)
				{
					ThingDef thingDef2;
					if (pawn.equipment.Primary == null)
					{
						thingDef2 = null;
					}
					else
					{
						thingDef2 = pawn.equipment.Primary.def;
					}
					if (!flag)
					{
						thingDef = thingDef2;
						flag = true;
					}
					else if (thingDef2 != thingDef)
					{
						return true;
					}
				}
			}
			return false;
		}

		private Gizmo GetSquadAttackGizmo()
		{
			Command_Target command_Target = new Command_Target();
			command_Target.defaultLabel = "CommandSquadAttack".Translate();
			command_Target.defaultDesc = "CommandSquadAttackDesc".Translate();
			command_Target.targetingParams = TargetingParameters.ForAttackAny();
			command_Target.hotKey = KeyBindingDefOf.Misc1;
			command_Target.icon = TexCommand.SquadAttack;
			string str;
			if (FloatMenuUtility.GetAttackAction(this.pawn, TargetInfo.Invalid, out str) == null)
			{
				command_Target.Disable(str.CapitalizeFirst() + ".");
			}
			command_Target.action = delegate(Thing target)
			{
				IEnumerable<Pawn> enumerable = Find.Selector.SelectedObjects.Where(delegate(object x)
				{
					Pawn pawn = x as Pawn;
					return pawn != null && pawn.IsColonistPlayerControlled && pawn.Drafted;
				}).Cast<Pawn>();
				foreach (Pawn current in enumerable)
				{
					string text;
					Action attackAction = FloatMenuUtility.GetAttackAction(current, target, out text);
					if (attackAction != null)
					{
						attackAction();
					}
				}
			};
			return command_Target;
		}
	}
}
