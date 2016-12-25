using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_ApparelTracker : IExposable
	{
		private const int RecordWalkedNakedTaleIntervalTicks = 60000;

		public Pawn pawn;

		private List<Apparel> wornApparel = new List<Apparel>();

		private int lastApparelWearoutTick = -1;

		private static List<Apparel> tmpApparelList = new List<Apparel>();

		private static List<Apparel> tmpApparel = new List<Apparel>();

		public List<Apparel> WornApparel
		{
			get
			{
				return this.wornApparel;
			}
		}

		public IEnumerable<Apparel> WornApparelInDrawOrder
		{
			get
			{
				for (int i = 0; i < this.wornApparel.Count; i++)
				{
					yield return this.wornApparel[i];
				}
			}
		}

		public int WornApparelCount
		{
			get
			{
				return this.wornApparel.Count;
			}
		}

		public bool PsychologicallyNude
		{
			get
			{
				if (this.pawn.gender == Gender.None)
				{
					return false;
				}
				bool flag;
				bool flag2;
				this.HasBasicApparel(out flag, out flag2);
				if (this.pawn.gender == Gender.Male)
				{
					return !flag;
				}
				return this.pawn.gender == Gender.Female && (!flag || !flag2);
			}
		}

		public Pawn_ApparelTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Apparel>(ref this.wornApparel, "wornApparel", LookMode.Deep, new object[0]);
			Scribe_Values.LookValue<int>(ref this.lastApparelWearoutTick, "lastApparelWearoutTick", 0, false);
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				this.SortWornApparelIntoDrawOrder();
				for (int i = 0; i < this.wornApparel.Count; i++)
				{
					this.wornApparel[i].wearer = this.pawn;
				}
			}
		}

		public void ApparelTrackerTickRare()
		{
			int ticksGame = Find.TickManager.TicksGame;
			if (this.lastApparelWearoutTick < 0)
			{
				this.lastApparelWearoutTick = ticksGame;
			}
			if (ticksGame - this.lastApparelWearoutTick >= 60000)
			{
				for (int i = 0; i < this.wornApparel.Count; i++)
				{
					this.TakeWearoutDamageForDay(this.wornApparel[i]);
				}
				this.lastApparelWearoutTick = ticksGame;
			}
		}

		public void ApparelTrackerTick()
		{
			for (int i = 0; i < this.wornApparel.Count; i++)
			{
				if (this.wornApparel[i].def.tickerType == TickerType.Normal)
				{
					this.wornApparel[i].Tick();
				}
			}
			if (this.pawn.IsColonist && this.pawn.Spawned && !this.pawn.Dead && this.pawn.IsHashIntervalTick(60000) && this.PsychologicallyNude)
			{
				TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, new object[]
				{
					this.pawn
				});
			}
		}

		private void TakeWearoutDamageForDay(Thing ap)
		{
			int num = GenMath.RoundRandom(ap.def.apparel.wearPerDay);
			if (num > 0)
			{
				ap.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, num, -1f, null, null, null));
			}
			if (ap.Destroyed && PawnUtility.ShouldSendNotificationAbout(this.pawn) && !this.pawn.Dead)
			{
				string text = "MessageWornApparelDeterioratedAway".Translate(new object[]
				{
					GenLabel.ThingLabel(ap.def, ap.Stuff, 1),
					this.pawn
				});
				text = text.CapitalizeFirst();
				Messages.Message(text, this.pawn, MessageSound.Negative);
			}
		}

		public bool CanWearWithoutDroppingAnything(ThingDef apDef)
		{
			for (int i = 0; i < this.wornApparel.Count; i++)
			{
				if (!ApparelUtility.CanWearTogether(apDef, this.wornApparel[i].def))
				{
					return false;
				}
			}
			return true;
		}

		public void Wear(Apparel newApparel, bool dropReplacedApparel = true)
		{
			SlotGroupUtility.Notify_TakingThing(newApparel);
			if (newApparel.Spawned)
			{
				newApparel.DeSpawn();
			}
			if (!ApparelUtility.HasPartsToWear(this.pawn, newApparel.def))
			{
				Log.Warning(string.Concat(new object[]
				{
					this.pawn,
					" tried to wear ",
					newApparel,
					" but he has no body parts required to wear it."
				}));
				return;
			}
			for (int i = this.wornApparel.Count - 1; i >= 0; i--)
			{
				Apparel apparel = this.wornApparel[i];
				if (!ApparelUtility.CanWearTogether(newApparel.def, apparel.def))
				{
					bool forbid = this.pawn.Faction.HostileTo(Faction.OfPlayer);
					if (dropReplacedApparel)
					{
						Apparel apparel2;
						if (!this.TryDrop(apparel, out apparel2, this.pawn.Position, forbid))
						{
							Log.Error(this.pawn + " could not drop " + apparel);
							return;
						}
					}
					else
					{
						this.Remove(apparel);
					}
				}
			}
			if (newApparel.wearer != null)
			{
				Log.Warning(string.Concat(new object[]
				{
					this.pawn,
					" is trying to wear ",
					newApparel,
					" but this apparel already has a wearer (",
					newApparel.wearer,
					"). This may or may not cause bugs."
				}));
			}
			this.wornApparel.Add(newApparel);
			newApparel.wearer = this.pawn;
			this.SortWornApparelIntoDrawOrder();
			this.ApparelChanged();
		}

		public void Remove(Apparel ap)
		{
			if (!this.wornApparel.Contains(ap))
			{
				Log.Warning("Tried to remove apparel " + ap + " but it's not here.");
				return;
			}
			this.wornApparel.Remove(ap);
			ap.wearer = null;
			this.ApparelChanged();
			if (this.pawn.outfits != null && this.pawn.outfits.forcedHandler != null)
			{
				this.pawn.outfits.forcedHandler.SetForced(ap, false);
			}
		}

		public bool TryDrop(Apparel ap, out Apparel resultingAp)
		{
			return this.TryDrop(ap, out resultingAp, this.pawn.Position, true);
		}

		public bool TryDrop(Apparel ap, out Apparel resultingAp, IntVec3 pos, bool forbid = true)
		{
			if (!this.wornApparel.Contains(ap))
			{
				Log.Warning(this.pawn.LabelCap + " tried to drop apparel he didn't have: " + ap.LabelCap);
				resultingAp = null;
				return false;
			}
			if (this.pawn.MapHeld == null)
			{
				Log.Warning(this.pawn.LabelCap + " tried to drop apparel but his MapHeld is null.");
				resultingAp = null;
				return false;
			}
			ap.Notify_Stripped(this.pawn);
			this.Remove(ap);
			Thing thing = null;
			bool result = GenThing.TryDropAndSetForbidden(ap, pos, this.pawn.MapHeld, ThingPlaceMode.Near, out thing, forbid);
			resultingAp = (thing as Apparel);
			return result;
		}

		public void DropAll(IntVec3 pos, bool forbid = true)
		{
			Pawn_ApparelTracker.tmpApparelList.Clear();
			Pawn_ApparelTracker.tmpApparelList.AddRange(this.wornApparel);
			for (int i = 0; i < Pawn_ApparelTracker.tmpApparelList.Count; i++)
			{
				Apparel apparel;
				this.TryDrop(Pawn_ApparelTracker.tmpApparelList[i], out apparel, pos, forbid);
			}
		}

		public void DestroyAll(DestroyMode mode = DestroyMode.Vanish)
		{
			for (int i = this.wornApparel.Count - 1; i >= 0; i--)
			{
				this.wornApparel[i].Destroy(mode);
			}
			this.wornApparel.Clear();
			this.ApparelChanged();
		}

		public void Notify_PawnKilled(DamageInfo? dinfo)
		{
			if (dinfo.HasValue && dinfo.Value.Def.externalViolence)
			{
				for (int i = 0; i < this.wornApparel.Count; i++)
				{
					if (this.wornApparel[i].def.useHitPoints)
					{
						int amount = Mathf.RoundToInt((float)this.wornApparel[i].HitPoints * Rand.Range(0.15f, 0.4f));
						this.wornApparel[i].TakeDamage(new DamageInfo(dinfo.Value.Def, amount, -1f, null, null, null));
					}
				}
			}
		}

		public void Notify_LostBodyPart()
		{
			Pawn_ApparelTracker.tmpApparel.Clear();
			Pawn_ApparelTracker.tmpApparel.AddRange(this.wornApparel);
			for (int i = 0; i < Pawn_ApparelTracker.tmpApparel.Count; i++)
			{
				Apparel apparel = Pawn_ApparelTracker.tmpApparel[i];
				if (!ApparelUtility.HasPartsToWear(this.pawn, apparel.def))
				{
					this.Remove(apparel);
				}
			}
		}

		private void SortWornApparelIntoDrawOrder()
		{
			this.wornApparel.Sort((Apparel a, Apparel b) => a.def.apparel.LastLayer.CompareTo(b.def.apparel.LastLayer));
		}

		public void HasBasicApparel(out bool hasPants, out bool hasShirt)
		{
			hasShirt = false;
			hasPants = false;
			for (int i = 0; i < this.wornApparel.Count; i++)
			{
				Apparel apparel = this.wornApparel[i];
				for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
				{
					if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Torso)
					{
						hasShirt = true;
					}
					if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Legs)
					{
						hasPants = true;
					}
					if (hasShirt && hasPants)
					{
						return;
					}
				}
			}
		}

		public bool BodyPartGroupIsCovered(BodyPartGroupDef bp)
		{
			for (int i = 0; i < this.wornApparel.Count; i++)
			{
				Apparel apparel = this.wornApparel[i];
				for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
				{
					if (apparel.def.apparel.bodyPartGroups[j] == bp)
					{
						return true;
					}
				}
			}
			return false;
		}

		[DebuggerHidden]
		public IEnumerable<Gizmo> GetGizmos()
		{
			for (int i = 0; i < this.wornApparel.Count; i++)
			{
				foreach (Gizmo g in this.wornApparel[i].GetWornGizmos())
				{
					yield return g;
				}
			}
		}

		internal void Notify_WornApparelDestroyed(Apparel apparel)
		{
			this.Remove(apparel);
		}

		private void ApparelChanged()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				this.pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
				PortraitsCache.SetDirty(this.pawn);
			});
		}
	}
}
