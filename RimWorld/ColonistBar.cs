using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class ColonistBar
	{
		public struct Entry
		{
			public Pawn pawn;

			public Map map;

			public int group;

			public Entry(Pawn pawn, Map map, int group)
			{
				this.pawn = pawn;
				this.map = map;
				this.group = group;
			}
		}

		public const float BaseSelectedTexJump = 20f;

		public const float BaseSelectedTexScale = 0.4f;

		public const float EntryInAnotherMapAlpha = 0.4f;

		public const float BaseSpaceBetweenGroups = 25f;

		public const float BaseSpaceBetweenColonistsHorizontal = 24f;

		public const float BaseSpaceBetweenColonistsVertical = 32f;

		public ColonistBarColonistDrawer drawer = new ColonistBarColonistDrawer();

		private ColonistBarDrawLocsFinder drawLocsFinder = new ColonistBarDrawLocsFinder();

		private List<ColonistBar.Entry> cachedEntries = new List<ColonistBar.Entry>();

		private List<Vector2> cachedDrawLocs = new List<Vector2>();

		private float cachedScale = 1f;

		private bool entriesDirty = true;

		public static readonly Texture2D BGTex = Command.BGTex;

		public static readonly Vector2 BaseSize = new Vector2(48f, 48f);

		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static List<Map> tmpMaps = new List<Map>();

		private static List<Caravan> tmpCaravans = new List<Caravan>();

		private static List<Pawn> tmpColonistsInOrder = new List<Pawn>();

		private static List<Pair<Thing, Map>> tmpColonistsWithMap = new List<Pair<Thing, Map>>();

		private static List<Thing> tmpColonists = new List<Thing>();

		private static List<Thing> tmpMapColonistsOrCorpsesInScreenRect = new List<Thing>();

		private static List<Pawn> tmpCaravanPawns = new List<Pawn>();

		public List<ColonistBar.Entry> Entries
		{
			get
			{
				this.CheckRecacheEntries();
				return this.cachedEntries;
			}
		}

		private bool ShowGroupFrames
		{
			get
			{
				List<ColonistBar.Entry> entries = this.Entries;
				int num = -1;
				for (int i = 0; i < entries.Count; i++)
				{
					num = Mathf.Max(num, entries[i].group);
				}
				return num >= 1;
			}
		}

		public float Scale
		{
			get
			{
				return this.cachedScale;
			}
		}

		public List<Vector2> DrawLocs
		{
			get
			{
				return this.cachedDrawLocs;
			}
		}

		public Vector2 Size
		{
			get
			{
				return ColonistBar.BaseSize * this.Scale;
			}
		}

		public float SpaceBetweenColonistsHorizontal
		{
			get
			{
				return 24f * this.Scale;
			}
		}

		private bool Visible
		{
			get
			{
				return UI.screenWidth >= 1000 && UI.screenHeight >= 760;
			}
		}

		public void MarkColonistsDirty()
		{
			this.entriesDirty = true;
		}

		public void ColonistBarOnGUI()
		{
			if (!this.Visible)
			{
				return;
			}
			if (Event.current.type != EventType.Layout)
			{
				List<ColonistBar.Entry> entries = this.Entries;
				int num = -1;
				bool showGroupFrames = this.ShowGroupFrames;
				for (int i = 0; i < this.cachedDrawLocs.Count; i++)
				{
					Rect rect = new Rect(this.cachedDrawLocs[i].x, this.cachedDrawLocs[i].y, this.Size.x, this.Size.y);
					ColonistBar.Entry entry = entries[i];
					bool flag = num != entry.group;
					num = entry.group;
					if (entry.pawn != null)
					{
						this.drawer.HandleClicks(rect, entry.pawn);
					}
					if (Event.current.type == EventType.Repaint)
					{
						if (flag && showGroupFrames)
						{
							this.drawer.DrawGroupFrame(entry.group);
						}
						if (entry.pawn != null)
						{
							this.drawer.DrawColonist(rect, entry.pawn, entry.map);
						}
					}
				}
				num = -1;
				if (showGroupFrames)
				{
					for (int j = 0; j < this.cachedDrawLocs.Count; j++)
					{
						ColonistBar.Entry entry2 = entries[j];
						bool flag2 = num != entry2.group;
						num = entry2.group;
						if (flag2)
						{
							this.drawer.HandleGroupFrameClicks(entry2.group);
						}
					}
				}
			}
		}

		private void CheckRecacheEntries()
		{
			if (!this.entriesDirty)
			{
				return;
			}
			this.entriesDirty = false;
			this.cachedEntries.Clear();
			if (Find.PlaySettings.showColonistBar)
			{
				ColonistBar.tmpMaps.Clear();
				ColonistBar.tmpMaps.AddRange(Find.Maps);
				ColonistBar.tmpMaps.SortBy((Map x) => !x.IsPlayerHome, (Map x) => x.uniqueID);
				int num = 0;
				for (int i = 0; i < ColonistBar.tmpMaps.Count; i++)
				{
					ColonistBar.tmpPawns.Clear();
					ColonistBar.tmpPawns.AddRange(ColonistBar.tmpMaps[i].mapPawns.FreeColonists);
					List<Thing> list = ColonistBar.tmpMaps[i].listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
					for (int j = 0; j < list.Count; j++)
					{
						if (!list[j].IsDessicated())
						{
							Pawn innerPawn = ((Corpse)list[j]).InnerPawn;
							if (innerPawn != null)
							{
								if (innerPawn.IsColonist)
								{
									ColonistBar.tmpPawns.Add(innerPawn);
								}
							}
						}
					}
					List<Pawn> allPawnsSpawned = ColonistBar.tmpMaps[i].mapPawns.AllPawnsSpawned;
					for (int k = 0; k < allPawnsSpawned.Count; k++)
					{
						Corpse corpse = allPawnsSpawned[k].carryTracker.CarriedThing as Corpse;
						if (corpse != null && !corpse.IsDessicated() && corpse.InnerPawn.IsColonist)
						{
							ColonistBar.tmpPawns.Add(corpse.InnerPawn);
						}
					}
					ColonistBar.tmpPawns.SortBy((Pawn x) => x.thingIDNumber);
					for (int l = 0; l < ColonistBar.tmpPawns.Count; l++)
					{
						this.cachedEntries.Add(new ColonistBar.Entry(ColonistBar.tmpPawns[l], ColonistBar.tmpMaps[i], num));
					}
					if (!ColonistBar.tmpPawns.Any<Pawn>())
					{
						this.cachedEntries.Add(new ColonistBar.Entry(null, ColonistBar.tmpMaps[i], num));
					}
					num++;
				}
				ColonistBar.tmpCaravans.Clear();
				ColonistBar.tmpCaravans.AddRange(Find.WorldObjects.Caravans);
				ColonistBar.tmpCaravans.SortBy((Caravan x) => x.ID);
				for (int m = 0; m < ColonistBar.tmpCaravans.Count; m++)
				{
					if (ColonistBar.tmpCaravans[m].IsPlayerControlled)
					{
						ColonistBar.tmpPawns.Clear();
						ColonistBar.tmpPawns.AddRange(ColonistBar.tmpCaravans[m].PawnsListForReading);
						ColonistBar.tmpPawns.SortBy((Pawn x) => x.thingIDNumber);
						for (int n = 0; n < ColonistBar.tmpPawns.Count; n++)
						{
							if (ColonistBar.tmpPawns[n].IsColonist)
							{
								this.cachedEntries.Add(new ColonistBar.Entry(ColonistBar.tmpPawns[n], null, num));
							}
						}
						num++;
					}
				}
			}
			this.drawer.Notify_RecachedEntries();
			ColonistBar.tmpPawns.Clear();
			ColonistBar.tmpMaps.Clear();
			ColonistBar.tmpCaravans.Clear();
			this.drawLocsFinder.CalculateDrawLocs(this.cachedDrawLocs, out this.cachedScale);
		}

		public float GetEntryRectAlpha(Rect rect)
		{
			float t;
			if (Messages.CollidesWithAnyMessage(rect, out t))
			{
				return Mathf.Lerp(1f, 0.2f, t);
			}
			return 1f;
		}

		public bool AnyColonistOrCorpseAt(Vector2 pos)
		{
			ColonistBar.Entry entry;
			return this.TryGetEntryAt(pos, out entry) && entry.pawn != null;
		}

		public bool TryGetEntryAt(Vector2 pos, out ColonistBar.Entry entry)
		{
			List<Vector2> drawLocs = this.DrawLocs;
			List<ColonistBar.Entry> entries = this.Entries;
			Vector2 size = this.Size;
			for (int i = 0; i < drawLocs.Count; i++)
			{
				Rect rect = new Rect(drawLocs[i].x, drawLocs[i].y, size.x, size.y);
				if (rect.Contains(pos))
				{
					entry = entries[i];
					return true;
				}
			}
			entry = default(ColonistBar.Entry);
			return false;
		}

		public List<Pawn> GetColonistsInOrder()
		{
			List<ColonistBar.Entry> entries = this.Entries;
			ColonistBar.tmpColonistsInOrder.Clear();
			for (int i = 0; i < entries.Count; i++)
			{
				if (entries[i].pawn != null)
				{
					ColonistBar.tmpColonistsInOrder.Add(entries[i].pawn);
				}
			}
			return ColonistBar.tmpColonistsInOrder;
		}

		public List<Thing> ColonistsOrCorpsesInScreenRect(Rect rect)
		{
			List<Vector2> drawLocs = this.DrawLocs;
			List<ColonistBar.Entry> entries = this.Entries;
			Vector2 size = this.Size;
			ColonistBar.tmpColonistsWithMap.Clear();
			for (int i = 0; i < drawLocs.Count; i++)
			{
				if (rect.Overlaps(new Rect(drawLocs[i].x, drawLocs[i].y, size.x, size.y)))
				{
					Pawn pawn = entries[i].pawn;
					if (pawn != null)
					{
						Thing first;
						if (pawn.Dead && pawn.Corpse != null && pawn.Corpse.MapHeld != null)
						{
							first = pawn.Corpse;
						}
						else
						{
							first = pawn;
						}
						ColonistBar.tmpColonistsWithMap.Add(new Pair<Thing, Map>(first, entries[i].map));
					}
				}
			}
			if (WorldRendererUtility.WorldRenderedNow)
			{
				if (ColonistBar.tmpColonistsWithMap.Any((Pair<Thing, Map> x) => x.Second == null))
				{
					ColonistBar.tmpColonistsWithMap.RemoveAll((Pair<Thing, Map> x) => x.Second != null);
					goto IL_1A1;
				}
			}
			if (ColonistBar.tmpColonistsWithMap.Any((Pair<Thing, Map> x) => x.Second == Find.VisibleMap))
			{
				ColonistBar.tmpColonistsWithMap.RemoveAll((Pair<Thing, Map> x) => x.Second != Find.VisibleMap);
			}
			IL_1A1:
			ColonistBar.tmpColonists.Clear();
			for (int j = 0; j < ColonistBar.tmpColonistsWithMap.Count; j++)
			{
				ColonistBar.tmpColonists.Add(ColonistBar.tmpColonistsWithMap[j].First);
			}
			ColonistBar.tmpColonistsWithMap.Clear();
			return ColonistBar.tmpColonists;
		}

		public List<Thing> MapColonistsOrCorpsesInScreenRect(Rect rect)
		{
			ColonistBar.tmpMapColonistsOrCorpsesInScreenRect.Clear();
			if (!this.Visible)
			{
				return ColonistBar.tmpMapColonistsOrCorpsesInScreenRect;
			}
			List<Thing> list = this.ColonistsOrCorpsesInScreenRect(rect);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Spawned)
				{
					ColonistBar.tmpMapColonistsOrCorpsesInScreenRect.Add(list[i]);
				}
			}
			return ColonistBar.tmpMapColonistsOrCorpsesInScreenRect;
		}

		public List<Pawn> CaravanMembersInScreenRect(Rect rect)
		{
			ColonistBar.tmpCaravanPawns.Clear();
			if (!this.Visible)
			{
				return ColonistBar.tmpCaravanPawns;
			}
			List<Thing> list = this.ColonistsOrCorpsesInScreenRect(rect);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i] as Pawn;
				if (pawn != null && pawn.IsCaravanMember())
				{
					ColonistBar.tmpCaravanPawns.Add(pawn);
				}
			}
			return ColonistBar.tmpCaravanPawns;
		}

		public List<Caravan> CaravanMembersCaravansInScreenRect(Rect rect)
		{
			ColonistBar.tmpCaravans.Clear();
			if (!this.Visible)
			{
				return ColonistBar.tmpCaravans;
			}
			List<Pawn> list = this.CaravanMembersInScreenRect(rect);
			for (int i = 0; i < list.Count; i++)
			{
				ColonistBar.tmpCaravans.Add(list[i].GetCaravan());
			}
			return ColonistBar.tmpCaravans;
		}

		public Caravan CaravanMemberCaravanAt(Vector2 at)
		{
			if (!this.Visible)
			{
				return null;
			}
			Pawn pawn = this.ColonistOrCorpseAt(at) as Pawn;
			if (pawn != null && pawn.IsCaravanMember())
			{
				return pawn.GetCaravan();
			}
			return null;
		}

		public Thing ColonistOrCorpseAt(Vector2 pos)
		{
			if (!this.Visible)
			{
				return null;
			}
			ColonistBar.Entry entry;
			if (!this.TryGetEntryAt(pos, out entry))
			{
				return null;
			}
			Pawn pawn = entry.pawn;
			Thing result;
			if (pawn != null && pawn.Dead && pawn.Corpse != null && pawn.Corpse.MapHeld != null)
			{
				result = pawn.Corpse;
			}
			else
			{
				result = pawn;
			}
			return result;
		}
	}
}
