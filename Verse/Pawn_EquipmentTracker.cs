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

		private List<ThingWithComps> cachedAllEquipment = new List<ThingWithComps>();

		private bool cachedAllEquipmentDirty = true;

		private static List<ThingWithComps> tmpEquipment = new List<ThingWithComps>();

		public ThingWithComps Primary
		{
			get
			{
				return this.primaryInt;
			}
			private set
			{
				if (this.primaryInt == value)
				{
					return;
				}
				this.primaryInt = value;
				this.cachedAllEquipmentDirty = true;
				if (this.pawn.drafter != null)
				{
					this.pawn.drafter.Notify_PrimaryWeaponChanged();
				}
			}
		}

		public CompEquippable PrimaryEq
		{
			get
			{
				return this.primaryInt.GetComp<CompEquippable>();
			}
		}

		public List<ThingWithComps> AllEquipment
		{
			get
			{
				if (this.cachedAllEquipmentDirty)
				{
					this.cachedAllEquipment.Clear();
					if (this.primaryInt != null)
					{
						this.cachedAllEquipment.Add(this.primaryInt);
					}
					this.cachedAllEquipmentDirty = false;
				}
				return this.cachedAllEquipment;
			}
		}

		public IEnumerable<Verb> AllEquipmentVerbs
		{
			get
			{
				for (int i = 0; i < this.AllEquipment.Count; i++)
				{
					ThingWithComps eq = this.AllEquipment[i];
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
			if (Scribe.mode != LoadSaveMode.Saving)
			{
				this.cachedAllEquipmentDirty = true;
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < this.AllEquipment.Count; i++)
				{
					ThingWithComps thingWithComps = this.AllEquipment[i];
					foreach (Verb current in thingWithComps.GetComp<CompEquippable>().AllVerbs)
					{
						current.caster = this.pawn;
						current.ownerEquipment = thingWithComps;
					}
				}
			}
		}

		public void EquipmentTrackerTick()
		{
			for (int i = 0; i < this.AllEquipment.Count; i++)
			{
				ThingWithComps thingWithComps = this.AllEquipment[i];
				thingWithComps.GetComp<CompEquippable>().verbTracker.VerbsTick();
			}
		}

		public bool HasAnything()
		{
			return this.AllEquipment.Any<ThingWithComps>();
		}

		public void MakeRoomFor(ThingWithComps eq)
		{
			if (eq.def.equipmentType == EquipmentType.Primary && this.Primary != null)
			{
				ThingWithComps t;
				if (this.TryDropEquipment(this.Primary, out t, this.pawn.Position, true))
				{
					t.SetForbidden(false, true);
				}
				else
				{
					Log.Error(this.pawn + " couldn't make room for equipment " + eq);
				}
			}
		}

		public void Remove(ThingWithComps eq)
		{
			if (!this.AllEquipment.Contains(eq))
			{
				Log.Warning("Tried to remove equipment " + eq + " but it's not here.");
				return;
			}
			if (this.Primary == eq)
			{
				this.Primary = null;
			}
			else
			{
				Log.Error("Tried to remove equipment " + eq + " but it's not Primary. Where is it?");
			}
			eq.GetComp<CompEquippable>().Notify_EquipmentLost();
			this.pawn.meleeVerbs.Notify_EquipmentLost();
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
			this.Remove(eq);
			Thing thing = null;
			bool result = GenThing.TryDropAndSetForbidden(eq, pos, this.pawn.MapHeld, ThingPlaceMode.Near, out thing, forbid);
			resultingEq = (thing as ThingWithComps);
			return result;
		}

		public void DropAllEquipment(IntVec3 pos, bool forbid = true)
		{
			if (this.Primary != null)
			{
				ThingWithComps thingWithComps;
				this.TryDropEquipment(this.Primary, out thingWithComps, pos, forbid);
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
			if (container.TryAdd(eq, true))
			{
				resultingEq = null;
			}
			else
			{
				resultingEq = eq;
			}
			this.Remove(eq);
			return resultingEq == null;
		}

		public void DestroyEquipment(ThingWithComps eq)
		{
			if (!this.AllEquipment.Contains(eq))
			{
				Log.Warning("Tried to destroy equipment " + eq + " but it's not here.");
				return;
			}
			this.Remove(eq);
			eq.Destroy(DestroyMode.Vanish);
		}

		public void DestroyAllEquipment(DestroyMode mode = DestroyMode.Vanish)
		{
			Pawn_EquipmentTracker.tmpEquipment.Clear();
			Pawn_EquipmentTracker.tmpEquipment.AddRange(this.AllEquipment);
			for (int i = 0; i < Pawn_EquipmentTracker.tmpEquipment.Count; i++)
			{
				this.DestroyEquipment(Pawn_EquipmentTracker.tmpEquipment[i]);
			}
		}

		internal void Notify_PrimaryDestroyed()
		{
			this.Remove(this.Primary);
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
			if (newEq.def.equipmentType == EquipmentType.Primary && this.Primary != null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Pawn ",
					this.pawn.LabelCap,
					" got primaryInt equipment ",
					newEq,
					" while already having primaryInt equipment ",
					this.Primary
				}));
				return;
			}
			if (newEq.def.equipmentType == EquipmentType.Primary)
			{
				this.Primary = newEq;
			}
			else
			{
				Log.Error("Tried to equip " + newEq + " but it's not Primary. Secondary weapons are not supported.");
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
				for (int i = 0; i < this.AllEquipment.Count; i++)
				{
					ThingWithComps eq = this.AllEquipment[i];
					foreach (Command command in eq.GetComp<CompEquippable>().GetVerbsCommands())
					{
						switch (i)
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

		public bool TryStartAttack(LocalTargetInfo targ)
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
			if (FloatMenuUtility.GetAttackAction(this.pawn, LocalTargetInfo.Invalid, out str) == null)
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
