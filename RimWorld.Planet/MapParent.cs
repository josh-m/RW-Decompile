using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class MapParent : WorldObject
	{
		public const int DefaultForceExitAndRemoveMapCountdownHours = 24;

		private int ticksLeftToForceExitAndRemoveMap = -1;

		private static readonly Texture2D ShowMapCommand = ContentFinder<Texture2D>.Get("UI/Commands/ShowMap", true);

		public static readonly Texture2D FormCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan", true);

		private static List<Pawn> tmpPawns = new List<Pawn>();

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

		public bool ForceExitAndRemoveMapCountdownActive
		{
			get
			{
				return this.ticksLeftToForceExitAndRemoveMap >= 0;
			}
		}

		public string ForceExitAndRemoveMapCountdownHoursLeftString
		{
			get
			{
				if (!this.ForceExitAndRemoveMapCountdownActive)
				{
					return string.Empty;
				}
				float num = (float)this.ticksLeftToForceExitAndRemoveMap / 2500f;
				if (num < 1f)
				{
					return num.ToString("0.#");
				}
				return Mathf.RoundToInt(num).ToString();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.ticksLeftToForceExitAndRemoveMap, "ticksLeftToForceExitAndRemoveMap", -1, false);
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

		public void StartForceExitAndRemoveMapCountdown()
		{
			this.StartForceExitAndRemoveMapCountdown(60000);
		}

		public void StartForceExitAndRemoveMapCountdown(int duration)
		{
			this.ticksLeftToForceExitAndRemoveMap = duration;
		}

		public override void Tick()
		{
			base.Tick();
			this.TickForceExitAndRemoveMapCountdown();
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
						Current.Game.VisibleMap = this.<>f__this.Map;
						if (!JumpToTargetUtility.CloseWorldTab())
						{
							SoundDefOf.TabClose.PlayOneShotOnCamera();
						}
					}
				};
				if (this is FactionBase && base.Faction == Faction.OfPlayer)
				{
					yield return new Command_Action
					{
						defaultLabel = "CommandFormCaravan".Translate(),
						defaultDesc = "CommandFormCaravanDesc".Translate(),
						icon = MapParent.FormCaravanCommand,
						hotKey = KeyBindingDefOf.Misc2,
						tutorTag = "FormCaravan",
						action = delegate
						{
							Find.WindowStack.Add(new Dialog_FormCaravan(this.<>f__this.Map, false));
						}
					};
				}
				else if (this.Map.mapPawns.FreeColonistsSpawnedCount != 0)
				{
					Command_Action reformCaravan = new Command_Action();
					reformCaravan.defaultLabel = "CommandReformCaravan".Translate();
					reformCaravan.defaultDesc = "CommandReformCaravanDesc".Translate();
					reformCaravan.icon = MapParent.FormCaravanCommand;
					reformCaravan.hotKey = KeyBindingDefOf.Misc2;
					reformCaravan.tutorTag = "ReformCaravan";
					reformCaravan.action = delegate
					{
						Find.WindowStack.Add(new Dialog_FormCaravan(this.<>f__this.Map, true));
					};
					if (GenHostility.AnyHostileThreat(this.Map))
					{
						reformCaravan.Disable("CommandReformCaravanFailHostilePawns".Translate());
					}
					yield return reformCaravan;
				}
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
					this.caravan.pather.StartPath(this.<>f__this.Tile, new CaravanArrivalAction_Enter(this.<>f__this), true);
				}, MenuOptionPriority.Default, null, null, 0f, null, this);
				if (Prefs.DevMode)
				{
					yield return new FloatMenuOption("EnterMap".Translate(new object[]
					{
						this.Label
					}) + " (Dev: instantly)", delegate
					{
						this.caravan.Tile = this.<>f__this.Tile;
						new CaravanArrivalAction_Enter(this.<>f__this).Arrived(this.caravan);
					}, MenuOptionPriority.Default, null, null, 0f, null, this);
				}
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (this.ForceExitAndRemoveMapCountdownActive)
			{
				if (text.Length > 0)
				{
					text += "\n";
				}
				text = text + "ForceExitAndRemoveMapCountdown".Translate(new object[]
				{
					this.ForceExitAndRemoveMapCountdownHoursLeftString
				}) + ".";
			}
			return text;
		}

		private void TickForceExitAndRemoveMapCountdown()
		{
			if (this.ForceExitAndRemoveMapCountdownActive)
			{
				if (this.HasMap)
				{
					this.ticksLeftToForceExitAndRemoveMap--;
					if (this.ticksLeftToForceExitAndRemoveMap == 0)
					{
						MapParent.tmpPawns.Clear();
						MapParent.tmpPawns.AddRange(from x in this.Map.mapPawns.AllPawns
						where x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer
						select x);
						if (MapParent.tmpPawns.Any<Pawn>())
						{
							if (MapParent.tmpPawns.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer)))
							{
								Caravan o = CaravanExitMapUtility.ExitMapAndCreateCaravan(MapParent.tmpPawns, Faction.OfPlayer, base.Tile);
								Messages.Message("MessageAutomaticallyReformedCaravan".Translate(), o, MessageSound.Benefit);
							}
							else
							{
								StringBuilder stringBuilder = new StringBuilder();
								for (int i = 0; i < MapParent.tmpPawns.Count; i++)
								{
									stringBuilder.AppendLine("    " + MapParent.tmpPawns[i].LabelCap);
								}
								Find.LetterStack.ReceiveLetter("LetterLabelPawnsLostDueToMapCountdown".Translate(), "LetterPawnsLostDueToMapCountdown".Translate(new object[]
								{
									stringBuilder.ToString().TrimEndNewlines()
								}), LetterType.BadNonUrgent, new GlobalTargetInfo(base.Tile), null);
							}
							MapParent.tmpPawns.Clear();
						}
						this.OpenWorldTabIfVisibleMapAboutToBeRemoved(this.Map);
						Find.WorldObjects.Remove(this);
					}
				}
				else
				{
					this.ticksLeftToForceExitAndRemoveMap = -1;
				}
			}
		}

		public void CheckRemoveMapNow()
		{
			bool flag;
			if (this.HasMap && this.ShouldRemoveMapNow(out flag))
			{
				Map map = this.Map;
				this.OpenWorldTabIfVisibleMapAboutToBeRemoved(map);
				Current.Game.DeinitAndRemoveMap(map);
				if (flag)
				{
					Find.WorldObjects.Remove(this);
				}
			}
		}

		private void OpenWorldTabIfVisibleMapAboutToBeRemoved(Map map)
		{
			if (map == Find.VisibleMap)
			{
				Find.MainTabsRoot.SetCurrentTab(MainTabDefOf.World, true);
			}
		}
	}
}
