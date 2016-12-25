using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class ColonistBar
	{
		private const float PawnTextureCameraZoom = 1.28205f;

		private const float PawnTextureHorizontalPadding = 1f;

		private const float MarginTop = 21f;

		private const float BaseSpacingHorizontal = 24f;

		private const float BaseSpacingVertical = 32f;

		private const float BaseSelectedTexJump = 20f;

		private const float BaseIconSize = 20f;

		private const float DoubleClickTime = 0.5f;

		private List<Pawn> cachedColonists = new List<Pawn>();

		private List<Vector2> cachedDrawLocs = new List<Vector2>();

		private bool colonistsDirty = true;

		private Dictionary<string, string> pawnLabelsCache = new Dictionary<string, string>();

		private Pawn clickedColonist;

		private float clickedAt;

		private static readonly Texture2D BGTex = Command.BGTex;

		private static readonly Texture2D SelectedTex = ContentFinder<Texture2D>.Get("UI/Overlays/SelectionBracketGUI", true);

		private static readonly Texture2D DeadColonistTex = ContentFinder<Texture2D>.Get("UI/Misc/DeadColonist", true);

		private static readonly Texture2D Icon_MentalStateNonAggro = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateNonAggro", true);

		private static readonly Texture2D Icon_MentalStateAggro = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateAggro", true);

		private static readonly Texture2D Icon_MedicalRest = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MedicalRest", true);

		private static readonly Texture2D Icon_Sleeping = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Sleeping", true);

		private static readonly Texture2D Icon_Fleeing = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Fleeing", true);

		private static readonly Texture2D Icon_Attacking = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Attacking", true);

		private static readonly Texture2D Icon_Idle = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Idle", true);

		private static readonly Texture2D Icon_Burning = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Burning", true);

		private static readonly Texture2D MoodBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.4f, 0.47f, 0.53f, 0.44f));

		private static readonly Vector2 BaseSize = new Vector2(48f, 48f);

		public static readonly Vector2 PawnTextureSize = new Vector2(ColonistBar.BaseSize.x - 2f, 75f);

		private static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0.3f);

		private static List<Thing> tmpColonists = new List<Thing>();

		public List<Pawn> ColonistsInOrder
		{
			get
			{
				this.CheckRecacheColonistsRaw();
				return this.cachedColonists;
			}
		}

		private float Scale
		{
			get
			{
				float num = 1f;
				while (true)
				{
					int allowedRowsCountForScale = ColonistBar.GetAllowedRowsCountForScale(num);
					int num2 = this.RowsCountAssumingScale(num);
					if (num2 <= allowedRowsCountForScale)
					{
						break;
					}
					num *= 0.95f;
				}
				return num;
			}
		}

		private static float MaxColonistBarWidth
		{
			get
			{
				return (float)Screen.width - 320f;
			}
		}

		private Vector2 Size
		{
			get
			{
				return ColonistBar.SizeAssumingScale(this.Scale);
			}
		}

		private float SpacingHorizontal
		{
			get
			{
				return ColonistBar.SpacingHorizontalAssumingScale(this.Scale);
			}
		}

		private float SpacingVertical
		{
			get
			{
				return ColonistBar.SpacingVerticalAssumingScale(this.Scale);
			}
		}

		private int ColonistsPerRow
		{
			get
			{
				return ColonistBar.ColonistsPerRowAssumingScale(this.Scale);
			}
		}

		private static Vector2 SizeAssumingScale(float scale)
		{
			return ColonistBar.BaseSize * scale;
		}

		private int RowsCountAssumingScale(float scale)
		{
			return Mathf.CeilToInt((float)this.cachedDrawLocs.Count / (float)ColonistBar.ColonistsPerRowAssumingScale(scale));
		}

		private static int ColonistsPerRowAssumingScale(float scale)
		{
			return Mathf.FloorToInt((ColonistBar.MaxColonistBarWidth + ColonistBar.SpacingHorizontalAssumingScale(scale)) / (ColonistBar.SizeAssumingScale(scale).x + ColonistBar.SpacingHorizontalAssumingScale(scale)));
		}

		private static float SpacingHorizontalAssumingScale(float scale)
		{
			return 24f * scale;
		}

		private static float SpacingVerticalAssumingScale(float scale)
		{
			return 32f * scale;
		}

		private static int GetAllowedRowsCountForScale(float scale)
		{
			if (scale > 0.58f)
			{
				return 1;
			}
			if (scale > 0.42f)
			{
				return 2;
			}
			return 3;
		}

		public void MarkColonistsListDirty()
		{
			this.colonistsDirty = true;
		}

		public void ColonistBarOnGUI()
		{
			if (!Find.PlaySettings.showColonistBar)
			{
				return;
			}
			if (Event.current.type == EventType.Layout)
			{
				this.RecacheDrawLocs();
			}
			else
			{
				for (int i = 0; i < this.cachedDrawLocs.Count; i++)
				{
					Rect rect = new Rect(this.cachedDrawLocs[i].x, this.cachedDrawLocs[i].y, this.Size.x, this.Size.y);
					Pawn colonist = this.cachedColonists[i];
					this.HandleColonistClicks(rect, colonist);
					if (Event.current.type == EventType.Repaint)
					{
						this.DrawColonist(rect, colonist);
					}
				}
			}
		}

		public List<Thing> ColonistsInScreenRect(Rect rect)
		{
			ColonistBar.tmpColonists.Clear();
			this.RecacheDrawLocs();
			for (int i = 0; i < this.cachedDrawLocs.Count; i++)
			{
				if (rect.Overlaps(new Rect(this.cachedDrawLocs[i].x, this.cachedDrawLocs[i].y, this.Size.x, this.Size.y)))
				{
					Thing thing;
					if (this.cachedColonists[i].Dead)
					{
						thing = this.cachedColonists[i].corpse;
					}
					else
					{
						thing = this.cachedColonists[i];
					}
					if (thing != null && thing.Spawned)
					{
						ColonistBar.tmpColonists.Add(thing);
					}
				}
			}
			return ColonistBar.tmpColonists;
		}

		public Thing ColonistAt(Vector2 pos)
		{
			Pawn pawn = null;
			this.RecacheDrawLocs();
			for (int i = 0; i < this.cachedDrawLocs.Count; i++)
			{
				Rect rect = new Rect(this.cachedDrawLocs[i].x, this.cachedDrawLocs[i].y, this.Size.x, this.Size.y);
				if (rect.Contains(pos))
				{
					pawn = this.cachedColonists[i];
				}
			}
			Thing thing;
			if (pawn != null && pawn.Dead)
			{
				thing = pawn.corpse;
			}
			else
			{
				thing = pawn;
			}
			if (thing != null && thing.Spawned)
			{
				return thing;
			}
			return null;
		}

		private void RecacheDrawLocs()
		{
			this.CheckRecacheColonistsRaw();
			Vector2 size = this.Size;
			int colonistsPerRow = this.ColonistsPerRow;
			float spacingHorizontal = this.SpacingHorizontal;
			float spacingVertical = this.SpacingVertical;
			float num = 0f;
			float num2 = 21f;
			this.cachedDrawLocs.Clear();
			for (int i = 0; i < this.cachedColonists.Count; i++)
			{
				if (i % colonistsPerRow == 0)
				{
					int num3 = Mathf.Min(colonistsPerRow, this.cachedColonists.Count - i);
					float num4 = (float)num3 * size.x + (float)(num3 - 1) * spacingHorizontal;
					num = ((float)Screen.width - num4) / 2f;
					if (i != 0)
					{
						num2 += size.y + spacingVertical;
					}
				}
				else
				{
					num += size.x + spacingHorizontal;
				}
				this.cachedDrawLocs.Add(new Vector2(num, num2));
			}
		}

		private void CheckRecacheColonistsRaw()
		{
			if (!this.colonistsDirty)
			{
				return;
			}
			this.cachedColonists.Clear();
			this.cachedColonists.AddRange(Find.MapPawns.FreeColonists);
			List<Thing> list = Find.ListerThings.ThingsInGroup(ThingRequestGroup.Corpse);
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].IsDessicated())
				{
					Pawn innerPawn = ((Corpse)list[i]).innerPawn;
					if (innerPawn != null)
					{
						if (innerPawn.IsColonist)
						{
							this.cachedColonists.Add(innerPawn);
						}
					}
				}
			}
			List<Pawn> allPawnsSpawned = Find.MapPawns.AllPawnsSpawned;
			for (int j = 0; j < allPawnsSpawned.Count; j++)
			{
				Corpse corpse = allPawnsSpawned[j].carrier.CarriedThing as Corpse;
				if (corpse != null && !corpse.IsDessicated() && corpse.innerPawn.IsColonist)
				{
					this.cachedColonists.Add(corpse.innerPawn);
				}
			}
			this.cachedColonists.SortBy((Pawn x) => x.thingIDNumber);
			this.pawnLabelsCache.Clear();
			this.colonistsDirty = false;
		}

		private void DrawColonist(Rect rect, Pawn colonist)
		{
			float colonistRectAlpha = this.GetColonistRectAlpha(rect);
			bool flag = (!colonist.Dead) ? Find.Selector.SelectedObjects.Contains(colonist) : Find.Selector.SelectedObjects.Contains(colonist.corpse);
			Color color = new Color(1f, 1f, 1f, colonistRectAlpha);
			GUI.color = color;
			GUI.DrawTexture(rect, ColonistBar.BGTex);
			if (colonist.needs != null && colonist.needs.mood != null)
			{
				Rect position = rect.ContractedBy(2f);
				float num = position.height * colonist.needs.mood.CurLevelPercentage;
				position.yMin = position.yMax - num;
				position.height = num;
				GUI.DrawTexture(position, ColonistBar.MoodBGTex);
			}
			if (flag)
			{
				this.DrawSelectionOverlayOnGUI(colonist, rect.ContractedBy(-2f * this.Scale));
			}
			GUI.DrawTexture(this.GetPawnTextureRect(rect.x, rect.y), PortraitsCache.Get(colonist, ColonistBar.PawnTextureSize, ColonistBar.PawnTextureCameraOffset, 1.28205f));
			GUI.color = new Color(1f, 1f, 1f, colonistRectAlpha * 0.8f);
			this.DrawIcons(rect, colonist);
			GUI.color = color;
			if (colonist.Dead)
			{
				GUI.DrawTexture(rect, ColonistBar.DeadColonistTex);
			}
			float num2 = 4f * this.Scale;
			Vector2 pos = new Vector2(rect.center.x, rect.yMax - num2);
			GenWorldUI.DrawPawnLabel(colonist, pos, colonistRectAlpha, rect.width + this.SpacingHorizontal - 2f, this.pawnLabelsCache);
			GUI.color = Color.white;
		}

		private float GetColonistRectAlpha(Rect rect)
		{
			float t;
			if (Messages.CollidesWithAnyMessage(rect, out t))
			{
				return Mathf.Lerp(1f, 0.2f, t);
			}
			return 1f;
		}

		private Rect GetPawnTextureRect(float x, float y)
		{
			Vector2 vector = ColonistBar.PawnTextureSize * this.Scale;
			Rect rect = new Rect(x + 1f, y - (vector.y - this.Size.y) - 1f, vector.x, vector.y);
			rect = rect.ContractedBy(1f);
			return rect;
		}

		private void DrawIcons(Rect rect, Pawn colonist)
		{
			if (colonist.Dead)
			{
				return;
			}
			float num = 20f * this.Scale;
			Vector2 vector = new Vector2(rect.x + 1f, rect.yMax - num - 1f);
			bool flag = false;
			if (colonist.CurJob != null)
			{
				JobDef def = colonist.CurJob.def;
				if (def == JobDefOf.AttackMelee || def == JobDefOf.AttackStatic)
				{
					flag = true;
				}
				else if (def == JobDefOf.WaitCombat)
				{
					Stance_Busy stance_Busy = colonist.stances.curStance as Stance_Busy;
					if (stance_Busy != null && stance_Busy.focusTarg.IsValid)
					{
						flag = true;
					}
				}
			}
			if (colonist.InAggroMentalState)
			{
				this.DrawIcon(ColonistBar.Icon_MentalStateAggro, ref vector, colonist.MentalStateDef.LabelCap);
			}
			else if (colonist.InMentalState)
			{
				this.DrawIcon(ColonistBar.Icon_MentalStateNonAggro, ref vector, colonist.MentalStateDef.LabelCap);
			}
			else if (colonist.InBed() && colonist.CurrentBed().Medical)
			{
				this.DrawIcon(ColonistBar.Icon_MedicalRest, ref vector, "ActivityIconMedicalRest".Translate());
			}
			else if (colonist.CurJob != null && colonist.jobs.curDriver.asleep)
			{
				this.DrawIcon(ColonistBar.Icon_Sleeping, ref vector, "ActivityIconSleeping".Translate());
			}
			else if (colonist.CurJob != null && colonist.CurJob.def == JobDefOf.FleeAndCower)
			{
				this.DrawIcon(ColonistBar.Icon_Fleeing, ref vector, "ActivityIconFleeing".Translate());
			}
			else if (flag)
			{
				this.DrawIcon(ColonistBar.Icon_Attacking, ref vector, "ActivityIconAttacking".Translate());
			}
			else if (colonist.mindState.IsIdle && GenDate.DaysPassed >= 1)
			{
				this.DrawIcon(ColonistBar.Icon_Idle, ref vector, "ActivityIconIdle".Translate());
			}
			if (colonist.IsBurning())
			{
				this.DrawIcon(ColonistBar.Icon_Burning, ref vector, "ActivityIconBurning".Translate());
			}
		}

		private void DrawIcon(Texture2D icon, ref Vector2 pos, string tooltip)
		{
			float num = 20f * this.Scale;
			Rect rect = new Rect(pos.x, pos.y, num, num);
			GUI.DrawTexture(rect, icon);
			TooltipHandler.TipRegion(rect, tooltip);
			pos.x += num;
		}

		private void HandleColonistClicks(Rect rect, Pawn colonist)
		{
			if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown)
			{
				if (this.clickedColonist == colonist && Time.time - this.clickedAt < 0.5f)
				{
					Event.current.Use();
					JumpToTargetUtility.TryJump(colonist);
					this.clickedColonist = null;
				}
				else
				{
					this.clickedColonist = colonist;
					this.clickedAt = Time.time;
				}
			}
		}

		private void DrawSelectionOverlayOnGUI(Pawn colonist, Rect rect)
		{
			Thing thing = colonist;
			if (colonist.Dead)
			{
				thing = colonist.corpse;
			}
			float num = 0.4f * this.Scale;
			Vector2 textureSize = new Vector2((float)ColonistBar.SelectedTex.width * num, (float)ColonistBar.SelectedTex.height * num);
			Vector3[] array = SelectionDrawer.SelectionBracketPartsPos(thing, rect.center, rect.size, textureSize, 20f * this.Scale);
			int num2 = 90;
			for (int i = 0; i < 4; i++)
			{
				Widgets.DrawTextureRotated(new Vector2(array[i].x, array[i].z), ColonistBar.SelectedTex, (float)num2, num);
				num2 += 90;
			}
		}
	}
}
