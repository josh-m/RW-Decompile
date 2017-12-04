using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class MapParent : WorldObject, IThingHolder
	{
		private bool anyCaravanEverFormed;

		private static readonly Texture2D ShowMapCommand = ContentFinder<Texture2D>.Get("UI/Commands/ShowMap", true);

		public bool HasMap
		{
			get
			{
				return this.Map != null;
			}
		}

		protected virtual bool UseGenericEnterMapFloatMenuOption
		{
			get
			{
				return true;
			}
		}

		public Map Map
		{
			get
			{
				return Current.Game.FindMap(this);
			}
		}

		public virtual MapGeneratorDef MapGeneratorDef
		{
			get
			{
				return this.def.mapGenerator;
			}
		}

		public virtual IEnumerable<GenStepDef> ExtraGenStepDefs
		{
			get
			{
			}
		}

		public virtual bool TransportPodsCanLandAndGenerateMap
		{
			get
			{
				return false;
			}
		}

		public virtual IntVec3 MapSizeGeneratedByTransportPodsArrival
		{
			get
			{
				return Find.World.info.initialMapSize;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.anyCaravanEverFormed, "anyCaravanEverFormed", false, false);
		}

		public virtual void PostMapGenerate()
		{
		}

		public virtual void Notify_MyMapRemoved(Map map)
		{
		}

		public virtual void Notify_CaravanFormed(Caravan caravan)
		{
			if (!this.anyCaravanEverFormed)
			{
				this.anyCaravanEverFormed = true;
				if (this.def.isTempIncidentMapOwner && this.HasMap)
				{
					this.Map.StoryState.CopyTo(caravan.StoryState);
				}
			}
		}

		public virtual bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = false;
			return false;
		}

		public override void PostRemove()
		{
			base.PostRemove();
			if (this.HasMap)
			{
				Current.Game.DeinitAndRemoveMap(this.Map);
			}
		}

		public override void Tick()
		{
			base.Tick();
			this.CheckRemoveMapNow();
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (this.HasMap)
			{
				yield return new Command_Action
				{
					defaultLabel = "CommandShowMap".Translate(),
					defaultDesc = "CommandShowMapDesc".Translate(),
					icon = MapParent.ShowMapCommand,
					hotKey = KeyBindingDefOf.Misc1,
					action = delegate
					{
						Current.Game.VisibleMap = this.$this.Map;
						if (!CameraJumper.TryHideWorld())
						{
							SoundDefOf.TabClose.PlayOneShotOnCamera(null);
						}
					}
				};
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan))
			{
				yield return o;
			}
			if (this.HasMap && this.UseGenericEnterMapFloatMenuOption)
			{
				yield return new FloatMenuOption("EnterMap".Translate(new object[]
				{
					this.Label
				}), delegate
				{
					caravan.pather.StartPath(this.$this.Tile, new CaravanArrivalAction_Enter(this.$this), true);
				}, MenuOptionPriority.Default, null, null, 0f, null, this);
				if (Prefs.DevMode)
				{
					yield return new FloatMenuOption("EnterMap".Translate(new object[]
					{
						this.Label
					}) + " (Dev: instantly)", delegate
					{
						caravan.Tile = this.$this.Tile;
						new CaravanArrivalAction_Enter(this.$this).Arrived(caravan);
					}, MenuOptionPriority.Default, null, null, 0f, null, this);
				}
			}
		}

		public void CheckRemoveMapNow()
		{
			bool flag;
			if (this.HasMap && this.ShouldRemoveMapNow(out flag))
			{
				Map map = this.Map;
				Current.Game.DeinitAndRemoveMap(map);
				if (flag)
				{
					Find.WorldObjects.Remove(this);
				}
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public virtual void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
			if (this.HasMap)
			{
				outChildren.Add(this.Map);
			}
		}
	}
}
