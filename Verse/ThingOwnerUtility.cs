using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class ThingOwnerUtility
	{
		private static Stack<IThingHolder> tmpStack = new Stack<IThingHolder>();

		private static List<IThingHolder> tmpHolders = new List<IThingHolder>();

		private static List<Thing> tmpThings = new List<Thing>();

		private static List<IThingHolder> tmpMapChildHolders = new List<IThingHolder>();

		public static bool ThisOrAnyCompIsThingHolder(this ThingDef thingDef)
		{
			if (typeof(IThingHolder).IsAssignableFrom(thingDef.thingClass))
			{
				return true;
			}
			for (int i = 0; i < thingDef.comps.Count; i++)
			{
				if (typeof(IThingHolder).IsAssignableFrom(thingDef.comps[i].compClass))
				{
					return true;
				}
			}
			return false;
		}

		public static ThingOwner TryGetInnerInteractableThingOwner(this Thing thing)
		{
			IThingHolder thingHolder = thing as IThingHolder;
			ThingWithComps thingWithComps = thing as ThingWithComps;
			if (thingHolder != null)
			{
				ThingOwner directlyHeldThings = thingHolder.GetDirectlyHeldThings();
				if (directlyHeldThings != null)
				{
					return directlyHeldThings;
				}
			}
			if (thingWithComps != null)
			{
				List<ThingComp> allComps = thingWithComps.AllComps;
				for (int i = 0; i < allComps.Count; i++)
				{
					IThingHolder thingHolder2 = allComps[i] as IThingHolder;
					if (thingHolder2 != null)
					{
						ThingOwner directlyHeldThings2 = thingHolder2.GetDirectlyHeldThings();
						if (directlyHeldThings2 != null)
						{
							return directlyHeldThings2;
						}
					}
				}
			}
			ThingOwnerUtility.tmpHolders.Clear();
			if (thingHolder != null)
			{
				thingHolder.GetChildHolders(ThingOwnerUtility.tmpHolders);
				if (ThingOwnerUtility.tmpHolders.Any<IThingHolder>())
				{
					ThingOwner directlyHeldThings3 = ThingOwnerUtility.tmpHolders[0].GetDirectlyHeldThings();
					if (directlyHeldThings3 != null)
					{
						ThingOwnerUtility.tmpHolders.Clear();
						return directlyHeldThings3;
					}
				}
			}
			if (thingWithComps != null)
			{
				List<ThingComp> allComps2 = thingWithComps.AllComps;
				for (int j = 0; j < allComps2.Count; j++)
				{
					IThingHolder thingHolder3 = allComps2[j] as IThingHolder;
					if (thingHolder3 != null)
					{
						thingHolder3.GetChildHolders(ThingOwnerUtility.tmpHolders);
						if (ThingOwnerUtility.tmpHolders.Any<IThingHolder>())
						{
							ThingOwner directlyHeldThings4 = ThingOwnerUtility.tmpHolders[0].GetDirectlyHeldThings();
							if (directlyHeldThings4 != null)
							{
								ThingOwnerUtility.tmpHolders.Clear();
								return directlyHeldThings4;
							}
						}
					}
				}
			}
			ThingOwnerUtility.tmpHolders.Clear();
			return null;
		}

		public static bool SpawnedOrAnyParentSpawned(IThingHolder holder)
		{
			return ThingOwnerUtility.SpawnedParentOrMe(holder) != null;
		}

		public static Thing SpawnedParentOrMe(IThingHolder holder)
		{
			while (holder != null)
			{
				Thing thing = holder as Thing;
				if (thing != null && thing.Spawned)
				{
					return thing;
				}
				ThingComp thingComp = holder as ThingComp;
				if (thingComp != null && thingComp.parent.Spawned)
				{
					return thingComp.parent;
				}
				holder = holder.ParentHolder;
			}
			return null;
		}

		public static IntVec3 GetRootPosition(IThingHolder holder)
		{
			IntVec3 result = IntVec3.Invalid;
			while (holder != null)
			{
				Thing thing = holder as Thing;
				if (thing != null && thing.Position.IsValid)
				{
					result = thing.Position;
				}
				else
				{
					ThingComp thingComp = holder as ThingComp;
					if (thingComp != null && thingComp.parent.Position.IsValid)
					{
						result = thingComp.parent.Position;
					}
				}
				holder = holder.ParentHolder;
			}
			return result;
		}

		public static Map GetRootMap(IThingHolder holder)
		{
			while (holder != null)
			{
				Map map = holder as Map;
				if (map != null)
				{
					return map;
				}
				holder = holder.ParentHolder;
			}
			return null;
		}

		public static int GetRootTile(IThingHolder holder)
		{
			while (holder != null)
			{
				WorldObject worldObject = holder as WorldObject;
				if (worldObject != null && worldObject.Tile >= 0)
				{
					return worldObject.Tile;
				}
				holder = holder.ParentHolder;
			}
			return -1;
		}

		public static bool ContentsSuspended(IThingHolder holder)
		{
			while (holder != null)
			{
				if (holder is Building_CryptosleepCasket || holder is ImportantPawnComp)
				{
					return true;
				}
				holder = holder.ParentHolder;
			}
			return false;
		}

		public static bool IsEnclosingContainer(this IThingHolder holder)
		{
			return holder != null && !(holder is Pawn_CarryTracker) && !(holder is Corpse) && !(holder is Map) && !(holder is Caravan) && !(holder is SettlementBase_TraderTracker) && !(holder is TradeShip);
		}

		public static bool ShouldAutoRemoveDestroyedThings(IThingHolder holder)
		{
			return !(holder is Corpse) && !(holder is Caravan);
		}

		public static bool ShouldAutoExtinguishInnerThings(IThingHolder holder)
		{
			return !(holder is Map);
		}

		public static bool ShouldRemoveDesignationsOnAddedThings(IThingHolder holder)
		{
			return holder.IsEnclosingContainer();
		}

		public static void AppendThingHoldersFromThings(List<IThingHolder> outThingsHolders, IList<Thing> container)
		{
			if (container == null)
			{
				return;
			}
			int i = 0;
			int count = container.Count;
			while (i < count)
			{
				IThingHolder thingHolder = container[i] as IThingHolder;
				if (thingHolder != null)
				{
					outThingsHolders.Add(thingHolder);
				}
				ThingWithComps thingWithComps = container[i] as ThingWithComps;
				if (thingWithComps != null)
				{
					List<ThingComp> allComps = thingWithComps.AllComps;
					for (int j = 0; j < allComps.Count; j++)
					{
						IThingHolder thingHolder2 = allComps[j] as IThingHolder;
						if (thingHolder2 != null)
						{
							outThingsHolders.Add(thingHolder2);
						}
					}
				}
				i++;
			}
		}

		public static bool AnyParentIs<T>(Thing thing) where T : class, IThingHolder
		{
			return ThingOwnerUtility.GetAnyParent<T>(thing) != null;
		}

		public static T GetAnyParent<T>(Thing thing) where T : class, IThingHolder
		{
			T t = thing as T;
			if (t != null)
			{
				return t;
			}
			for (IThingHolder parentHolder = thing.ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
			{
				T t2 = parentHolder as T;
				if (t2 != null)
				{
					return t2;
				}
			}
			return (T)((object)null);
		}

		public static Thing GetFirstSpawnedParentThing(Thing thing)
		{
			if (thing.Spawned)
			{
				return thing;
			}
			for (IThingHolder parentHolder = thing.ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
			{
				Thing thing2 = parentHolder as Thing;
				if (thing2 != null && thing2.Spawned)
				{
					return thing2;
				}
				ThingComp thingComp = parentHolder as ThingComp;
				if (thingComp != null && thingComp.parent.Spawned)
				{
					return thingComp.parent;
				}
			}
			return null;
		}

		public static void GetAllThingsRecursively(IThingHolder holder, List<Thing> outThings, bool allowUnreal = true, Predicate<IThingHolder> passCheck = null)
		{
			outThings.Clear();
			if (passCheck != null && !passCheck(holder))
			{
				return;
			}
			ThingOwnerUtility.tmpStack.Clear();
			ThingOwnerUtility.tmpStack.Push(holder);
			while (ThingOwnerUtility.tmpStack.Count != 0)
			{
				IThingHolder thingHolder = ThingOwnerUtility.tmpStack.Pop();
				if (allowUnreal || ThingOwnerUtility.AreImmediateContentsReal(thingHolder))
				{
					ThingOwner directlyHeldThings = thingHolder.GetDirectlyHeldThings();
					if (directlyHeldThings != null)
					{
						outThings.AddRange(directlyHeldThings);
					}
				}
				ThingOwnerUtility.tmpHolders.Clear();
				thingHolder.GetChildHolders(ThingOwnerUtility.tmpHolders);
				for (int i = 0; i < ThingOwnerUtility.tmpHolders.Count; i++)
				{
					if (passCheck == null || passCheck(ThingOwnerUtility.tmpHolders[i]))
					{
						ThingOwnerUtility.tmpStack.Push(ThingOwnerUtility.tmpHolders[i]);
					}
				}
			}
			ThingOwnerUtility.tmpStack.Clear();
			ThingOwnerUtility.tmpHolders.Clear();
		}

		public static void GetAllThingsRecursively<T>(Map map, ThingRequest request, List<T> outThings, bool allowUnreal = true, Predicate<IThingHolder> passCheck = null, bool alsoGetSpawnedThings = true) where T : Thing
		{
			outThings.Clear();
			if (alsoGetSpawnedThings)
			{
				List<Thing> list = map.listerThings.ThingsMatching(request);
				for (int i = 0; i < list.Count; i++)
				{
					T t = list[i] as T;
					if (t != null)
					{
						outThings.Add(t);
					}
				}
			}
			ThingOwnerUtility.tmpMapChildHolders.Clear();
			map.GetChildHolders(ThingOwnerUtility.tmpMapChildHolders);
			for (int j = 0; j < ThingOwnerUtility.tmpMapChildHolders.Count; j++)
			{
				ThingOwnerUtility.tmpThings.Clear();
				ThingOwnerUtility.GetAllThingsRecursively(ThingOwnerUtility.tmpMapChildHolders[j], ThingOwnerUtility.tmpThings, allowUnreal, passCheck);
				for (int k = 0; k < ThingOwnerUtility.tmpThings.Count; k++)
				{
					T t2 = ThingOwnerUtility.tmpThings[k] as T;
					if (t2 != null)
					{
						if (request.Accepts(t2))
						{
							outThings.Add(t2);
						}
					}
				}
			}
			ThingOwnerUtility.tmpThings.Clear();
			ThingOwnerUtility.tmpMapChildHolders.Clear();
		}

		public static List<Thing> GetAllThingsRecursively(IThingHolder holder, bool allowUnreal = true)
		{
			List<Thing> list = new List<Thing>();
			ThingOwnerUtility.GetAllThingsRecursively(holder, list, allowUnreal, null);
			return list;
		}

		public static bool AreImmediateContentsReal(IThingHolder holder)
		{
			return !(holder is Corpse) && !(holder is MinifiedThing);
		}

		public static bool TryGetFixedTemperature(IThingHolder holder, Thing forThing, out float temperature)
		{
			if (holder is Pawn_InventoryTracker && forThing.TryGetComp<CompHatcher>() != null)
			{
				temperature = 14f;
				return true;
			}
			if (holder is CompLaunchable || holder is ActiveDropPodInfo || holder is TravelingTransportPods)
			{
				temperature = 14f;
				return true;
			}
			if (holder is SettlementBase_TraderTracker || holder is TradeShip)
			{
				temperature = 14f;
				return true;
			}
			temperature = 21f;
			return false;
		}
	}
}
