using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_ApparelTracker : IExposable, IThingHolder
	{
		private const int RecordWalkedNakedTaleIntervalTicks = 60000;

		public Pawn pawn;

		private ThingOwner<Apparel> wornApparel;

		private int lastApparelWearoutTick = -1;

		private static List<Apparel> tmpApparelList = new List<Apparel>();

		private static List<Apparel> tmpApparel = new List<Apparel>();

		public IThingHolder ParentHolder
		{
			get
			{
				return this.pawn;
			}
		}

		public List<Apparel> WornApparel
		{
			get
			{
				return this.wornApparel.InnerListForReading;
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
			this.wornApparel = new ThingOwner<Apparel>(this);
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<ThingOwner<Apparel>>(ref this.wornApparel, "wornApparel", new object[]
			{
				this
			});
			Scribe_Values.Look<int>(ref this.lastApparelWearoutTick, "lastApparelWearoutTick", 0, false);
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				this.SortWornApparelIntoDrawOrder();
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
			this.wornApparel.ThingOwnerTick(true);
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
				ap.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, num, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
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
					if (dropReplacedApparel)
					{
						bool forbid = this.pawn.Faction.HostileTo(Faction.OfPlayer);
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
			if (newApparel.Wearer != null)
			{
				Log.Warning(string.Concat(new object[]
				{
					this.pawn,
					" is trying to wear ",
					newApparel,
					" but this apparel already has a wearer (",
					newApparel.Wearer,
					"). This may or may not cause bugs."
				}));
			}
			this.wornApparel.TryAdd(newApparel, false);
		}

		public void Remove(Apparel ap)
		{
			this.wornApparel.Remove(ap);
		}

		public bool TryDrop(Apparel ap, out Apparel resultingAp)
		{
			return this.TryDrop(ap, out resultingAp, this.pawn.Position, true);
		}

		public bool TryDrop(Apparel ap, out Apparel resultingAp, IntVec3 pos, bool forbid = true)
		{
			if (this.wornApparel.TryDrop(ap, pos, this.pawn.MapHeld, ThingPlaceMode.Near, out resultingAp, null))
			{
				if (resultingAp != null)
				{
					resultingAp.SetForbidden(forbid, false);
				}
				return true;
			}
			return false;
		}

		public void DropAll(IntVec3 pos, bool forbid = true)
		{
			Pawn_ApparelTracker.tmpApparelList.Clear();
			for (int i = 0; i < this.wornApparel.Count; i++)
			{
				Pawn_ApparelTracker.tmpApparelList.Add(this.wornApparel[i]);
			}
			for (int j = 0; j < Pawn_ApparelTracker.tmpApparelList.Count; j++)
			{
				Apparel apparel;
				this.TryDrop(Pawn_ApparelTracker.tmpApparelList[j], out apparel, pos, forbid);
			}
		}

		public void DestroyAll(DestroyMode mode = DestroyMode.Vanish)
		{
			this.wornApparel.ClearAndDestroyContents(mode);
		}

		public bool Contains(Thing apparel)
		{
			return this.wornApparel.Contains(apparel);
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
						this.wornApparel[i].TakeDamage(new DamageInfo(dinfo.Value.Def, amount, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
					}
				}
			}
			for (int j = 0; j < this.wornApparel.Count; j++)
			{
				this.wornApparel[j].Notify_PawnKilled();
			}
		}

		public void Notify_LostBodyPart()
		{
			Pawn_ApparelTracker.tmpApparel.Clear();
			for (int i = 0; i < this.wornApparel.Count; i++)
			{
				Pawn_ApparelTracker.tmpApparel.Add(this.wornApparel[i]);
			}
			for (int j = 0; j < Pawn_ApparelTracker.tmpApparel.Count; j++)
			{
				Apparel apparel = Pawn_ApparelTracker.tmpApparel[j];
				if (!ApparelUtility.HasPartsToWear(this.pawn, apparel.def))
				{
					this.Remove(apparel);
				}
			}
		}

		private void SortWornApparelIntoDrawOrder()
		{
			this.wornApparel.InnerListForReading.Sort((Apparel a, Apparel b) => a.def.apparel.LastLayer.CompareTo(b.def.apparel.LastLayer));
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

		public Apparel FirstApparelOnBodyPartGroup(BodyPartGroupDef g)
		{
			for (int i = 0; i < this.wornApparel.Count; i++)
			{
				Apparel apparel = this.wornApparel[i];
				for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
				{
					if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Torso)
					{
						return apparel;
					}
				}
			}
			return null;
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

		private void ApparelChanged()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				this.pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
				PortraitsCache.SetDirty(this.pawn);
			});
		}

		public void Notify_ApparelAdded(Apparel apparel)
		{
			this.SortWornApparelIntoDrawOrder();
			this.ApparelChanged();
		}

		public void Notify_ApparelRemoved(Apparel apparel)
		{
			this.ApparelChanged();
			if (this.pawn.outfits != null && this.pawn.outfits.forcedHandler != null)
			{
				this.pawn.outfits.forcedHandler.SetForced(apparel, false);
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.wornApparel;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
	}
}
