using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public sealed class MapPawns
	{
		private Map map;

		private List<Pawn> pawnsSpawned = new List<Pawn>();

		private Dictionary<Faction, List<Pawn>> pawnsInFactionSpawned = new Dictionary<Faction, List<Pawn>>();

		private List<Pawn> prisonersOfColonySpawned = new List<Pawn>();

		private static List<Thing> tmpThings = new List<Thing>();

		private static List<IThingHolder> tmpMapChildHolders = new List<IThingHolder>();

		public IEnumerable<Pawn> AllPawns
		{
			get
			{
				for (int i = 0; i < this.pawnsSpawned.Count; i++)
				{
					yield return this.pawnsSpawned[i];
				}
				foreach (Pawn p in this.AllPawnsUnspawned)
				{
					yield return p;
				}
			}
		}

		public IEnumerable<Pawn> AllPawnsUnspawned
		{
			get
			{
				List<Thing> holders = this.map.listerThings.ThingsInGroup(ThingRequestGroup.ThisOrAnyCompIsThingHolder);
				for (int i = 0; i < holders.Count; i++)
				{
					IThingHolder holder = holders[i] as IThingHolder;
					if (holder != null)
					{
						ThingOwnerUtility.GetAllThingsRecursively(holder, MapPawns.tmpThings, true);
						for (int j = 0; j < MapPawns.tmpThings.Count; j++)
						{
							Pawn p = MapPawns.tmpThings[j] as Pawn;
							if (p != null)
							{
								if (!p.Dead)
								{
									yield return p;
								}
							}
						}
					}
					ThingWithComps twc = holders[i] as ThingWithComps;
					if (twc != null)
					{
						List<ThingComp> comps = twc.AllComps;
						for (int k = 0; k < comps.Count; k++)
						{
							IThingHolder compHolder = comps[k] as IThingHolder;
							if (compHolder != null)
							{
								ThingOwnerUtility.GetAllThingsRecursively(compHolder, MapPawns.tmpThings, true);
								for (int l = 0; l < MapPawns.tmpThings.Count; l++)
								{
									Pawn p2 = MapPawns.tmpThings[l] as Pawn;
									if (p2 != null && !p2.Dead)
									{
										yield return p2;
									}
								}
							}
						}
					}
				}
				MapPawns.tmpMapChildHolders.Clear();
				this.map.GetNonThingChildHolders(MapPawns.tmpMapChildHolders);
				for (int m = 0; m < MapPawns.tmpMapChildHolders.Count; m++)
				{
					ThingOwnerUtility.GetAllThingsRecursively(MapPawns.tmpMapChildHolders[m], MapPawns.tmpThings, true);
					for (int n = 0; n < MapPawns.tmpThings.Count; n++)
					{
						Pawn p3 = MapPawns.tmpThings[n] as Pawn;
						if (p3 != null && !p3.Dead)
						{
							yield return p3;
						}
					}
				}
				MapPawns.tmpMapChildHolders.Clear();
				MapPawns.tmpThings.Clear();
			}
		}

		public IEnumerable<Pawn> FreeColonists
		{
			get
			{
				return this.FreeHumanlikesOfFaction(Faction.OfPlayer);
			}
		}

		public IEnumerable<Pawn> PrisonersOfColony
		{
			get
			{
				return from x in this.AllPawns
				where x.IsPrisonerOfColony
				select x;
			}
		}

		public IEnumerable<Pawn> FreeColonistsAndPrisoners
		{
			get
			{
				return this.FreeColonists.Concat(this.PrisonersOfColony);
			}
		}

		public int ColonistCount
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					Log.Error("ColonistCount while not playing. This should get the starting player pawn count.");
					return 3;
				}
				return (from x in this.AllPawns
				where x.RaceProps.Humanlike && x.Faction == Faction.OfPlayer
				select x).Count<Pawn>();
			}
		}

		public int AllPawnsCount
		{
			get
			{
				return this.AllPawns.Count<Pawn>();
			}
		}

		public int AllPawnsUnspawnedCount
		{
			get
			{
				return this.AllPawnsUnspawned.Count<Pawn>();
			}
		}

		public int FreeColonistsCount
		{
			get
			{
				return this.FreeColonists.Count<Pawn>();
			}
		}

		public int PrisonersOfColonyCount
		{
			get
			{
				return this.PrisonersOfColony.Count<Pawn>();
			}
		}

		public int FreeColonistsAndPrisonersCount
		{
			get
			{
				return this.PrisonersOfColony.Count<Pawn>();
			}
		}

		public bool AnyPawnBlockingMapRemoval
		{
			get
			{
				Faction ofPlayer = Faction.OfPlayer;
				for (int i = 0; i < this.pawnsSpawned.Count; i++)
				{
					if (this.pawnsSpawned[i].Faction == ofPlayer || this.pawnsSpawned[i].HostFaction == ofPlayer)
					{
						return true;
					}
				}
				List<Thing> list = this.map.listerThings.ThingsInGroup(ThingRequestGroup.ThisOrAnyCompIsThingHolder);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j] is IActiveDropPod || list[j].TryGetComp<CompTransporter>() != null)
					{
						IThingHolder holder = list[j].TryGetComp<CompTransporter>() ?? ((IThingHolder)list[j]);
						ThingOwnerUtility.GetAllThingsRecursively(holder, MapPawns.tmpThings, true);
						for (int k = 0; k < MapPawns.tmpThings.Count; k++)
						{
							Pawn pawn = MapPawns.tmpThings[k] as Pawn;
							if (pawn != null && (pawn.Faction == ofPlayer || pawn.HostFaction == ofPlayer))
							{
								MapPawns.tmpThings.Clear();
								return true;
							}
						}
					}
				}
				MapPawns.tmpThings.Clear();
				return false;
			}
		}

		public List<Pawn> AllPawnsSpawned
		{
			get
			{
				return this.pawnsSpawned;
			}
		}

		public IEnumerable<Pawn> FreeColonistsSpawned
		{
			get
			{
				return this.FreeHumanlikesSpawnedOfFaction(Faction.OfPlayer);
			}
		}

		public List<Pawn> PrisonersOfColonySpawned
		{
			get
			{
				return this.prisonersOfColonySpawned;
			}
		}

		public IEnumerable<Pawn> FreeColonistsAndPrisonersSpawned
		{
			get
			{
				return this.FreeColonistsSpawned.Concat(this.PrisonersOfColonySpawned);
			}
		}

		public int AllPawnsSpawnedCount
		{
			get
			{
				return this.pawnsSpawned.Count;
			}
		}

		public int FreeColonistsSpawnedCount
		{
			get
			{
				return this.FreeColonistsSpawned.Count<Pawn>();
			}
		}

		public int PrisonersOfColonySpawnedCount
		{
			get
			{
				return this.PrisonersOfColonySpawned.Count;
			}
		}

		public int FreeColonistsAndPrisonersSpawnedCount
		{
			get
			{
				return this.FreeColonistsAndPrisonersSpawned.Count<Pawn>();
			}
		}

		public int ColonistsSpawnedCount
		{
			get
			{
				int num = 0;
				List<Pawn> list = this.SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].RaceProps.Humanlike)
					{
						num++;
					}
				}
				return num;
			}
		}

		public int FreeColonistsSpawnedOrInPlayerEjectablePodsCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.pawnsSpawned.Count; i++)
				{
					if (this.pawnsSpawned[i].IsColonist && this.pawnsSpawned[i].HostFaction == null)
					{
						num++;
					}
				}
				List<Thing> list = this.map.listerThings.ThingsInGroup(ThingRequestGroup.ThisOrAnyCompIsThingHolder);
				for (int j = 0; j < list.Count; j++)
				{
					Building_CryptosleepCasket building_CryptosleepCasket = list[j] as Building_CryptosleepCasket;
					if ((building_CryptosleepCasket != null && building_CryptosleepCasket.def.building.isPlayerEjectable) || list[j] is IActiveDropPod || list[j].TryGetComp<CompTransporter>() != null)
					{
						IThingHolder holder = list[j].TryGetComp<CompTransporter>() ?? ((IThingHolder)list[j]);
						ThingOwnerUtility.GetAllThingsRecursively(holder, MapPawns.tmpThings, true);
						for (int k = 0; k < MapPawns.tmpThings.Count; k++)
						{
							Pawn pawn = MapPawns.tmpThings[k] as Pawn;
							if (pawn != null && pawn.IsColonist && pawn.HostFaction == null)
							{
								num++;
							}
						}
					}
				}
				MapPawns.tmpThings.Clear();
				return num;
			}
		}

		public MapPawns(Map map)
		{
			this.map = map;
		}

		private void EnsureFactionsListsInit()
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (!this.pawnsInFactionSpawned.ContainsKey(allFactionsListForReading[i]))
				{
					this.pawnsInFactionSpawned.Add(allFactionsListForReading[i], new List<Pawn>());
				}
			}
		}

		public IEnumerable<Pawn> PawnsInFaction(Faction faction)
		{
			if (faction == null)
			{
				Log.Error("Called PawnsInFaction with null faction.");
				return new List<Pawn>();
			}
			return from x in this.AllPawns
			where x.Faction == faction
			select x;
		}

		public List<Pawn> SpawnedPawnsInFaction(Faction faction)
		{
			this.EnsureFactionsListsInit();
			if (faction == null)
			{
				Log.Error("Called SpawnedPawnsInFaction with null faction.");
				return new List<Pawn>();
			}
			return this.pawnsInFactionSpawned[faction];
		}

		public IEnumerable<Pawn> FreeHumanlikesOfFaction(Faction faction)
		{
			return from p in this.PawnsInFaction(faction)
			where p.HostFaction == null && p.RaceProps.Humanlike
			select p;
		}

		public IEnumerable<Pawn> FreeHumanlikesSpawnedOfFaction(Faction faction)
		{
			return from p in this.SpawnedPawnsInFaction(faction)
			where p.HostFaction == null && p.RaceProps.Humanlike
			select p;
		}

		public void RegisterPawn(Pawn p)
		{
			if (p.Dead)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to register dead pawn ",
					p,
					" in ",
					base.GetType(),
					"."
				}));
				return;
			}
			if (!p.Spawned)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to register despawned pawn ",
					p,
					" in ",
					base.GetType(),
					"."
				}));
				return;
			}
			if (p.Map != this.map)
			{
				Log.Warning("Tried to register pawn " + p + " but his Map is not this one.");
				return;
			}
			if (!p.mindState.Active)
			{
				return;
			}
			this.EnsureFactionsListsInit();
			if (!this.pawnsSpawned.Contains(p))
			{
				this.pawnsSpawned.Add(p);
			}
			if (p.Faction != null && !this.pawnsInFactionSpawned[p.Faction].Contains(p))
			{
				this.pawnsInFactionSpawned[p.Faction].Add(p);
				if (p.Faction == Faction.OfPlayer)
				{
					this.pawnsInFactionSpawned[Faction.OfPlayer].InsertionSort(delegate(Pawn a, Pawn b)
					{
						int num = (a.playerSettings == null) ? 0 : a.playerSettings.joinTick;
						int value = (b.playerSettings == null) ? 0 : b.playerSettings.joinTick;
						return num.CompareTo(value);
					});
				}
			}
			if (p.IsPrisonerOfColony && !this.prisonersOfColonySpawned.Contains(p))
			{
				this.prisonersOfColonySpawned.Add(p);
			}
			this.DoListChangedNotifications();
		}

		public void DeRegisterPawn(Pawn p)
		{
			this.EnsureFactionsListsInit();
			this.pawnsSpawned.Remove(p);
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				Faction key = allFactionsListForReading[i];
				this.pawnsInFactionSpawned[key].Remove(p);
			}
			this.prisonersOfColonySpawned.Remove(p);
			this.DoListChangedNotifications();
		}

		public void UpdateRegistryForPawn(Pawn p)
		{
			this.DeRegisterPawn(p);
			if (p.Spawned && p.Map == this.map)
			{
				this.RegisterPawn(p);
			}
			this.DoListChangedNotifications();
		}

		private void DoListChangedNotifications()
		{
			if (Find.WindowStack != null)
			{
				WindowStack windowStack = Find.WindowStack;
				for (int i = 0; i < windowStack.Count; i++)
				{
					MainTabWindow_PawnTable mainTabWindow_PawnTable = windowStack[i] as MainTabWindow_PawnTable;
					if (mainTabWindow_PawnTable != null)
					{
						mainTabWindow_PawnTable.Notify_PawnsChanged();
					}
				}
			}
			if (Find.ColonistBar != null)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
		}

		public void LogListedPawns()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("MapPawns:");
			stringBuilder.AppendLine("pawnsSpawned");
			foreach (Pawn current in this.pawnsSpawned)
			{
				stringBuilder.AppendLine("    " + current.ToString());
			}
			stringBuilder.AppendLine("AllPawnsUnspawned");
			foreach (Pawn current2 in this.AllPawnsUnspawned)
			{
				stringBuilder.AppendLine("    " + current2.ToString());
			}
			foreach (KeyValuePair<Faction, List<Pawn>> current3 in this.pawnsInFactionSpawned)
			{
				stringBuilder.AppendLine("pawnsInFactionSpawned[" + current3.Key.ToString() + "]");
				foreach (Pawn current4 in current3.Value)
				{
					stringBuilder.AppendLine("    " + current4.ToString());
				}
			}
			stringBuilder.AppendLine("prisonersOfColonySpawned");
			foreach (Pawn current5 in this.prisonersOfColonySpawned)
			{
				stringBuilder.AppendLine("    " + current5.ToString());
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
