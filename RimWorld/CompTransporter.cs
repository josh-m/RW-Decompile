using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompTransporter : ThingComp, IThingContainerOwner
	{
		public int groupID = -1;

		private ThingContainer innerContainer;

		public List<TransferableOneWay> leftToLoad;

		private CompLaunchable cachedCompLaunchable;

		private static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);

		private static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", true);

		private static readonly Texture2D SelectPreviousInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter", true);

		private static readonly Texture2D SelectAllInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectAllTransporters", true);

		private static readonly Texture2D SelectNextInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter", true);

		private static List<CompTransporter> tmpTransportersInGroup = new List<CompTransporter>();

		public CompProperties_Transporter Props
		{
			get
			{
				return (CompProperties_Transporter)this.props;
			}
		}

		public Map Map
		{
			get
			{
				return this.parent.MapHeld;
			}
		}

		public bool Spawned
		{
			get
			{
				return this.parent.Spawned;
			}
		}

		public bool AnythingLeftToLoad
		{
			get
			{
				return this.FirstThingLeftToLoad != null;
			}
		}

		public bool LoadingInProgressOrReadyToLaunch
		{
			get
			{
				return this.groupID >= 0;
			}
		}

		public bool AnyInGroupHasAnythingLeftToLoad
		{
			get
			{
				return this.FirstThingLeftToLoadInGroup != null;
			}
		}

		public CompLaunchable Launchable
		{
			get
			{
				if (this.cachedCompLaunchable == null)
				{
					this.cachedCompLaunchable = this.parent.GetComp<CompLaunchable>();
				}
				return this.cachedCompLaunchable;
			}
		}

		public Thing FirstThingLeftToLoad
		{
			get
			{
				if (this.leftToLoad == null)
				{
					return null;
				}
				TransferableOneWay transferableOneWay = this.leftToLoad.Find((TransferableOneWay x) => x.countToTransfer != 0 && x.HasAnyThing);
				if (transferableOneWay != null)
				{
					return transferableOneWay.AnyThing;
				}
				return null;
			}
		}

		public Thing FirstThingLeftToLoadInGroup
		{
			get
			{
				List<CompTransporter> list = this.TransportersInGroup(this.parent.Map);
				for (int i = 0; i < list.Count; i++)
				{
					Thing firstThingLeftToLoad = list[i].FirstThingLeftToLoad;
					if (firstThingLeftToLoad != null)
					{
						return firstThingLeftToLoad;
					}
				}
				return null;
			}
		}

		public CompTransporter()
		{
			this.innerContainer = new ThingContainer(this);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<int>(ref this.groupID, "groupID", 0, false);
			Scribe_Deep.LookDeep<ThingContainer>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});
			Scribe_Collections.LookList<TransferableOneWay>(ref this.leftToLoad, "leftToLoad", LookMode.Deep, new object[0]);
		}

		public ThingContainer GetInnerContainer()
		{
			return this.innerContainer;
		}

		public IntVec3 GetPosition()
		{
			return this.parent.PositionHeld;
		}

		public Map GetMap()
		{
			return this.parent.MapHeld;
		}

		public List<CompTransporter> TransportersInGroup(Map map)
		{
			if (!this.LoadingInProgressOrReadyToLaunch)
			{
				return null;
			}
			TransporterUtility.GetTransportersInGroup(this.groupID, map, CompTransporter.tmpTransportersInGroup);
			return CompTransporter.tmpTransportersInGroup;
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo g in base.CompGetGizmosExtra())
			{
				yield return g;
			}
			if (this.LoadingInProgressOrReadyToLaunch)
			{
				yield return new Command_Action
				{
					defaultLabel = "CommandCancelLoad".Translate(),
					defaultDesc = "CommandCancelLoadDesc".Translate(),
					icon = CompTransporter.CancelLoadCommandTex,
					action = delegate
					{
						SoundDefOf.DesignateCancel.PlayOneShotOnCamera();
						this.<>f__this.CancelLoad();
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "CommandSelectPreviousTransporter".Translate(),
					defaultDesc = "CommandSelectPreviousTransporterDesc".Translate(),
					icon = CompTransporter.SelectPreviousInGroupCommandTex,
					action = delegate
					{
						this.<>f__this.SelectPreviousInGroup();
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "CommandSelectAllTransporters".Translate(),
					defaultDesc = "CommandSelectAllTransportersDesc".Translate(),
					icon = CompTransporter.SelectAllInGroupCommandTex,
					action = delegate
					{
						this.<>f__this.SelectAllInGroup();
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "CommandSelectNextTransporter".Translate(),
					defaultDesc = "CommandSelectNextTransporterDesc".Translate(),
					icon = CompTransporter.SelectNextInGroupCommandTex,
					action = delegate
					{
						this.<>f__this.SelectNextInGroup();
					}
				};
			}
			else
			{
				Command_LoadToTransporter loadGroup = new Command_LoadToTransporter();
				int selectedTransportersCount = 0;
				for (int i = 0; i < Find.Selector.NumSelected; i++)
				{
					Thing t = Find.Selector.SelectedObjectsListForReading[i] as Thing;
					if (t != null && t.def == this.parent.def)
					{
						CompLaunchable cl = t.TryGetComp<CompLaunchable>();
						if (cl == null || (cl.FuelingPortSource != null && cl.FuelingPortSourceHasAnyFuel))
						{
							selectedTransportersCount++;
						}
					}
				}
				loadGroup.defaultLabel = "CommandLoadTransporter".Translate(new object[]
				{
					selectedTransportersCount.ToString()
				});
				loadGroup.defaultDesc = "CommandLoadTransporterDesc".Translate();
				loadGroup.icon = CompTransporter.LoadCommandTex;
				loadGroup.transComp = this;
				CompLaunchable launchable = this.Launchable;
				if (launchable != null)
				{
					if (!launchable.ConnectedToFuelingPort)
					{
						loadGroup.Disable("CommandLoadTransporterFailNotConnectedToFuelingPort".Translate());
					}
					else if (!launchable.FuelingPortSourceHasAnyFuel)
					{
						loadGroup.Disable("CommandLoadTransporterFailNoFuel".Translate());
					}
				}
				yield return loadGroup;
			}
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			if (this.CancelLoad(map))
			{
				Messages.Message("MessageTransportersLoadCanceled_TransporterDestroyed".Translate(), MessageSound.Negative);
			}
		}

		public void AddToTheToLoadList(TransferableOneWay t, int count)
		{
			if (!t.HasAnyThing || t.CountToTransfer <= 0)
			{
				return;
			}
			if (this.leftToLoad == null)
			{
				this.leftToLoad = new List<TransferableOneWay>();
			}
			if (TransferableUtility.TransferableMatching<TransferableOneWay>(t.AnyThing, this.leftToLoad) != null)
			{
				Log.Error("Transferable already exists.");
				return;
			}
			TransferableOneWay transferableOneWay = new TransferableOneWay();
			this.leftToLoad.Add(transferableOneWay);
			transferableOneWay.things.AddRange(t.things);
			transferableOneWay.countToTransfer = count;
		}

		public void Notify_DepositedThingInContainer(Thing t)
		{
			this.SubtractFromToLoadList(t);
		}

		public void Notify_PawnEnteredTransporterOnHisOwn(Pawn p)
		{
			this.SubtractFromToLoadList(p);
		}

		public bool CancelLoad()
		{
			return this.CancelLoad(this.Map);
		}

		public bool CancelLoad(Map map)
		{
			if (!this.LoadingInProgressOrReadyToLaunch)
			{
				return false;
			}
			this.TryRemoveLord(map);
			List<CompTransporter> list = this.TransportersInGroup(map);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].CleanUpLoadingVars(map);
			}
			this.CleanUpLoadingVars(map);
			return true;
		}

		public void TryRemoveLord(Map map)
		{
			if (!this.LoadingInProgressOrReadyToLaunch)
			{
				return;
			}
			Lord lord = TransporterUtility.FindLord(this.groupID, map);
			if (lord != null)
			{
				map.lordManager.RemoveLord(lord);
			}
		}

		public void CleanUpLoadingVars(Map map)
		{
			this.groupID = -1;
			this.innerContainer.TryDropAll(this.parent.Position, map, ThingPlaceMode.Near);
			if (this.leftToLoad != null)
			{
				this.leftToLoad.Clear();
			}
		}

		private void SubtractFromToLoadList(Thing t)
		{
			if (this.leftToLoad == null)
			{
				return;
			}
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(t, this.leftToLoad);
			if (transferableOneWay == null)
			{
				return;
			}
			transferableOneWay.countToTransfer -= t.stackCount;
			if (transferableOneWay.countToTransfer <= 0)
			{
				this.leftToLoad.Remove(transferableOneWay);
			}
			if (!this.AnyInGroupHasAnythingLeftToLoad)
			{
				Messages.Message("MessageFinishedLoadingTransporters".Translate(), this.parent, MessageSound.Benefit);
			}
		}

		private void SelectPreviousInGroup()
		{
			List<CompTransporter> list = this.TransportersInGroup(this.Map);
			int num = list.IndexOf(this);
			JumpToTargetUtility.TryJumpAndSelect(list[GenMath.PositiveMod(num - 1, list.Count)].parent);
		}

		private void SelectAllInGroup()
		{
			List<CompTransporter> list = this.TransportersInGroup(this.Map);
			Selector selector = Find.Selector;
			selector.ClearSelection();
			for (int i = 0; i < list.Count; i++)
			{
				selector.Select(list[i].parent, true, true);
			}
		}

		private void SelectNextInGroup()
		{
			List<CompTransporter> list = this.TransportersInGroup(this.Map);
			int num = list.IndexOf(this);
			JumpToTargetUtility.TryJumpAndSelect(list[(num + 1) % list.Count].parent);
		}
	}
}
