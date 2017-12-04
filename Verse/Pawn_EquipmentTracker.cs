using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class Pawn_EquipmentTracker : IThingHolder, IExposable
	{
		private Pawn pawn;

		private ThingOwner<ThingWithComps> equipment;

		public ThingWithComps Primary
		{
			get
			{
				for (int i = 0; i < this.equipment.Count; i++)
				{
					if (this.equipment[i].def.equipmentType == EquipmentType.Primary)
					{
						return this.equipment[i];
					}
				}
				return null;
			}
			private set
			{
				if (this.Primary == value)
				{
					return;
				}
				if (value != null && value.def.equipmentType != EquipmentType.Primary)
				{
					Log.Error("Tried to set non-primary equipment as primary.");
					return;
				}
				if (this.Primary != null)
				{
					this.equipment.Remove(this.Primary);
				}
				if (value != null)
				{
					this.equipment.TryAdd(value, true);
				}
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
				return (this.Primary == null) ? null : this.Primary.GetComp<CompEquippable>();
			}
		}

		public List<ThingWithComps> AllEquipmentListForReading
		{
			get
			{
				return this.equipment.InnerListForReading;
			}
		}

		public IEnumerable<Verb> AllEquipmentVerbs
		{
			get
			{
				List<ThingWithComps> list = this.AllEquipmentListForReading;
				for (int i = 0; i < list.Count; i++)
				{
					ThingWithComps eq = list[i];
					List<Verb> verbs = eq.GetComp<CompEquippable>().AllVerbs;
					for (int j = 0; j < verbs.Count; j++)
					{
						yield return verbs[j];
					}
				}
			}
		}

		public IThingHolder ParentHolder
		{
			get
			{
				return this.pawn;
			}
		}

		public Pawn_EquipmentTracker(Pawn newPawn)
		{
			this.pawn = newPawn;
			this.equipment = new ThingOwner<ThingWithComps>(this);
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<ThingOwner<ThingWithComps>>(ref this.equipment, "equipment", new object[]
			{
				this
			});
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				List<ThingWithComps> allEquipmentListForReading = this.AllEquipmentListForReading;
				for (int i = 0; i < allEquipmentListForReading.Count; i++)
				{
					ThingWithComps thingWithComps = allEquipmentListForReading[i];
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
			List<ThingWithComps> allEquipmentListForReading = this.AllEquipmentListForReading;
			for (int i = 0; i < allEquipmentListForReading.Count; i++)
			{
				ThingWithComps thingWithComps = allEquipmentListForReading[i];
				thingWithComps.GetComp<CompEquippable>().verbTracker.VerbsTick();
			}
		}

		public bool HasAnything()
		{
			return this.equipment.Any;
		}

		public void MakeRoomFor(ThingWithComps eq)
		{
			if (eq.def.equipmentType == EquipmentType.Primary && this.Primary != null)
			{
				ThingWithComps thingWithComps;
				if (this.TryDropEquipment(this.Primary, out thingWithComps, this.pawn.Position, true))
				{
					if (thingWithComps != null)
					{
						thingWithComps.SetForbidden(false, true);
					}
				}
				else
				{
					Log.Error(this.pawn + " couldn't make room for equipment " + eq);
				}
			}
		}

		public void Remove(ThingWithComps eq)
		{
			this.equipment.Remove(eq);
		}

		public bool TryDropEquipment(ThingWithComps eq, out ThingWithComps resultingEq, IntVec3 pos, bool forbid = true)
		{
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
			if (this.equipment.TryDrop(eq, pos, this.pawn.MapHeld, ThingPlaceMode.Near, out resultingEq, null))
			{
				if (resultingEq != null)
				{
					resultingEq.SetForbidden(forbid, false);
				}
				return true;
			}
			return false;
		}

		public void DropAllEquipment(IntVec3 pos, bool forbid = true)
		{
			for (int i = this.equipment.Count - 1; i >= 0; i--)
			{
				ThingWithComps thingWithComps;
				this.TryDropEquipment(this.equipment[i], out thingWithComps, pos, forbid);
			}
		}

		public bool TryTransferEquipmentToContainer(ThingWithComps eq, ThingOwner container)
		{
			return this.equipment.TryTransferToContainer(eq, container, true);
		}

		public void DestroyEquipment(ThingWithComps eq)
		{
			if (!this.equipment.Contains(eq))
			{
				Log.Warning("Tried to destroy equipment " + eq + " but it's not here.");
				return;
			}
			this.Remove(eq);
			eq.Destroy(DestroyMode.Vanish);
		}

		public void DestroyAllEquipment(DestroyMode mode = DestroyMode.Vanish)
		{
			this.equipment.ClearAndDestroyContents(mode);
		}

		public bool Contains(Thing eq)
		{
			return this.equipment.Contains(eq);
		}

		internal void Notify_PrimaryDestroyed()
		{
			if (this.Primary != null)
			{
				this.Remove(this.Primary);
			}
			if (this.pawn.Spawned)
			{
				this.pawn.stances.CancelBusyStanceSoft();
			}
		}

		public void AddEquipment(ThingWithComps newEq)
		{
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
			this.equipment.TryAdd(newEq, true);
		}

		[DebuggerHidden]
		public IEnumerable<Gizmo> GetGizmos()
		{
			if (PawnAttackGizmoUtility.CanShowEquipmentGizmos())
			{
				List<ThingWithComps> list = this.AllEquipmentListForReading;
				for (int i = 0; i < list.Count; i++)
				{
					ThingWithComps eq = list[i];
					foreach (Command command in eq.GetComp<CompEquippable>().GetVerbsCommands())
					{
						if (i != 0)
						{
							if (i != 1)
							{
								if (i == 2)
								{
									command.hotKey = KeyBindingDefOf.Misc3;
								}
							}
							else
							{
								command.hotKey = KeyBindingDefOf.Misc2;
							}
						}
						else
						{
							command.hotKey = KeyBindingDefOf.Misc1;
						}
						yield return command;
					}
				}
			}
		}

		public void Notify_EquipmentAdded(ThingWithComps eq)
		{
			foreach (Verb current in eq.GetComp<CompEquippable>().AllVerbs)
			{
				current.caster = this.pawn;
				current.Notify_PickedUp();
			}
		}

		public void Notify_EquipmentRemoved(ThingWithComps eq)
		{
			eq.GetComp<CompEquippable>().Notify_EquipmentLost();
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.equipment;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
	}
}
