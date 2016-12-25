using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_Grave : Building_Casket, IAssignableBuilding, IStoreSettingsParent
	{
		private StorageSettings storageSettings;

		private Graphic cachedGraphicFull;

		public Pawn assignedPawn;

		public override Graphic Graphic
		{
			get
			{
				if (!this.HasCorpse)
				{
					return base.Graphic;
				}
				if (this.def.building.fullGraveGraphicData == null)
				{
					return base.Graphic;
				}
				if (this.cachedGraphicFull == null)
				{
					this.cachedGraphicFull = this.def.building.fullGraveGraphicData.GraphicColoredFor(this);
				}
				return this.cachedGraphicFull;
			}
		}

		public bool HasCorpse
		{
			get
			{
				return this.Corpse != null;
			}
		}

		public Corpse Corpse
		{
			get
			{
				for (int i = 0; i < this.container.Count; i++)
				{
					Corpse corpse = this.container[i] as Corpse;
					if (corpse != null)
					{
						return corpse;
					}
				}
				return null;
			}
		}

		public IEnumerable<Pawn> AssigningCandidates
		{
			get
			{
				IEnumerable<Pawn> second = from Corpse x in Find.ListerThings.ThingsInGroup(ThingRequestGroup.Corpse)
				where x.Spawned && x.innerPawn.IsColonist
				select x.innerPawn;
				return Find.MapPawns.FreeColonistsSpawned.Concat(second);
			}
		}

		public IEnumerable<Pawn> AssignedPawns
		{
			get
			{
				if (this.assignedPawn != null)
				{
					yield return this.assignedPawn;
				}
			}
		}

		public int MaxAssignedPawnsCount
		{
			get
			{
				return 1;
			}
		}

		public bool StorageTabVisible
		{
			get
			{
				return this.assignedPawn == null && !this.HasCorpse;
			}
		}

		public void TryAssignPawn(Pawn pawn)
		{
			pawn.ownership.ClaimGrave(this);
		}

		public void TryUnassignPawn(Pawn pawn)
		{
			if (pawn == this.assignedPawn)
			{
				pawn.ownership.UnclaimGrave();
			}
		}

		public StorageSettings GetStoreSettings()
		{
			return this.storageSettings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return this.def.building.fixedStorageSettings;
		}

		public override void PostMake()
		{
			base.PostMake();
			this.storageSettings = new StorageSettings(this);
			if (this.def.building.defaultStorageSettings != null)
			{
				this.storageSettings.CopyFrom(this.def.building.defaultStorageSettings);
			}
		}

		public override void TickRare()
		{
			base.TickRare();
			this.container.ThingContainerTickRare();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.LookDeep<StorageSettings>(ref this.storageSettings, "storageSettings", new object[]
			{
				this
			});
		}

		public override void EjectContents()
		{
			base.EjectContents();
			Find.MapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
		}

		public virtual void Notify_CorpseBuried(Pawn worker)
		{
			CompArt comp = base.GetComp<CompArt>();
			if (comp != null && !comp.Active)
			{
				comp.JustCreatedBy(worker);
				comp.InitializeArt(this.Corpse.innerPawn);
			}
			Find.MapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Buildings);
		}

		public override bool Accepts(Thing thing)
		{
			if (!base.Accepts(thing))
			{
				return false;
			}
			if (this.HasCorpse)
			{
				return false;
			}
			if (this.assignedPawn != null)
			{
				Corpse corpse = thing as Corpse;
				if (corpse == null)
				{
					return false;
				}
				if (corpse.innerPawn != this.assignedPawn)
				{
					return false;
				}
			}
			else if (!this.storageSettings.AllowedToAccept(thing))
			{
				return false;
			}
			return true;
		}

		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				Corpse corpse = thing as Corpse;
				if (corpse != null && corpse.innerPawn.ownership != null && corpse.innerPawn.ownership.AssignedGrave != this)
				{
					corpse.innerPawn.ownership.UnclaimGrave();
				}
				Find.MapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
				return true;
			}
			return false;
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (this.StorageTabVisible)
			{
				foreach (Gizmo g2 in StorageSettingsClipboard.CopyPasteGizmosFor(this.storageSettings))
				{
					yield return g2;
				}
			}
			if (!this.HasCorpse)
			{
				yield return new Command_Action
				{
					defaultLabel = "CommandGraveAssignColonistLabel".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner", true),
					defaultDesc = "CommandGraveAssignColonistDesc".Translate(),
					action = delegate
					{
						Find.WindowStack.Add(new Dialog_AssignBuildingOwner(this.<>f__this));
					},
					hotKey = KeyBindingDefOf.Misc3
				};
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (!this.HasCorpse && this.assignedPawn != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("AssignedColonist".Translate());
				stringBuilder.Append(": ");
				stringBuilder.Append(this.assignedPawn.LabelCap);
			}
			return stringBuilder.ToString();
		}
	}
}
