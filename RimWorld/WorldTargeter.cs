using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WorldTargeter
	{
		private const float BaseFeedbackTexSize = 0.8f;

		private Func<GlobalTargetInfo, bool> action;

		private bool canTargetTiles;

		private Texture2D mouseAttachment;

		public bool closeWorldTabWhenFinished;

		private Action onUpdate;

		private Func<GlobalTargetInfo, string> extraLabelGetter;

		public bool IsTargeting
		{
			get
			{
				return this.action != null;
			}
		}

		public void BeginTargeting(Func<GlobalTargetInfo, bool> action, bool canTargetTiles, Texture2D mouseAttachment = null, bool closeWorldTabWhenFinished = false, Action onUpdate = null, Func<GlobalTargetInfo, string> extraLabelGetter = null)
		{
			this.action = action;
			this.canTargetTiles = canTargetTiles;
			this.mouseAttachment = mouseAttachment;
			this.closeWorldTabWhenFinished = closeWorldTabWhenFinished;
			this.onUpdate = onUpdate;
			this.extraLabelGetter = extraLabelGetter;
		}

		public void StopTargeting()
		{
			if (this.closeWorldTabWhenFinished)
			{
				JumpToTargetUtility.CloseWorldTab();
			}
			this.action = null;
			this.canTargetTiles = false;
			this.mouseAttachment = null;
			this.closeWorldTabWhenFinished = false;
			this.onUpdate = null;
			this.extraLabelGetter = null;
		}

		public void ProcessInputEvents()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0 && this.IsTargeting)
				{
					GlobalTargetInfo arg = this.CurrentTargetUnderMouse();
					if (this.action(arg))
					{
						SoundDefOf.TickHigh.PlayOneShotOnCamera();
						this.StopTargeting();
					}
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
			if (this.IsTargeting)
			{
				Vector2 mousePosition = Event.current.mousePosition;
				Texture2D image = this.mouseAttachment ?? TexCommand.Attack;
				Rect position = new Rect(mousePosition.x + 8f, mousePosition.y + 8f, 32f, 32f);
				GUI.DrawTexture(position, image);
				if (this.extraLabelGetter != null)
				{
					string text = this.extraLabelGetter(this.CurrentTargetUnderMouse());
					if (!text.NullOrEmpty())
					{
						GUI.color = Color.red;
						Widgets.Label(new Rect(position.xMax, position.y, 250f, 100f), text);
						GUI.color = Color.white;
					}
				}
			}
		}

		public void TargeterUpdate()
		{
			if (this.IsTargeting)
			{
				Vector3 pos = Vector3.zero;
				GlobalTargetInfo globalTargetInfo = this.CurrentTargetUnderMouse();
				if (globalTargetInfo.HasWorldObject)
				{
					pos = globalTargetInfo.WorldObject.DrawPos;
				}
				else if (globalTargetInfo.Tile >= 0)
				{
					pos = Find.WorldGrid.GetTileCenter(globalTargetInfo.Tile);
				}
				if (globalTargetInfo.IsValid)
				{
					WorldRendererUtility.DrawQuadTangentialToPlanet(pos, 0.8f * Find.WorldGrid.averageTileSize, 0.01f, GenDraw.CurTargetingMat, false, false, null);
				}
				if (this.onUpdate != null)
				{
					this.onUpdate();
				}
			}
		}

		public bool IsTargetedNow(WorldObject o, List<WorldObject> worldObjectsUnderMouse = null)
		{
			if (!this.IsTargeting)
			{
				return false;
			}
			if (worldObjectsUnderMouse == null)
			{
				worldObjectsUnderMouse = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
			}
			return worldObjectsUnderMouse.Any<WorldObject>() && o == worldObjectsUnderMouse[0];
		}

		private GlobalTargetInfo CurrentTargetUnderMouse()
		{
			if (!this.IsTargeting)
			{
				return GlobalTargetInfo.Invalid;
			}
			List<WorldObject> list = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
			if (list.Any<WorldObject>())
			{
				return list[0];
			}
			if (!this.canTargetTiles)
			{
				return GlobalTargetInfo.Invalid;
			}
			int num = GenWorld.MouseTile();
			if (num >= 0)
			{
				return new GlobalTargetInfo(num);
			}
			return GlobalTargetInfo.Invalid;
		}
	}
}
