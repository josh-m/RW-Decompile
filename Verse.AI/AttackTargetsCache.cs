using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class AttackTargetsCache
	{
		private Map map;

		private HashSet<IAttackTarget> allTargets = new HashSet<IAttackTarget>();

		private Dictionary<Faction, HashSet<IAttackTarget>> targetsHostileToFaction = new Dictionary<Faction, HashSet<IAttackTarget>>();

		private HashSet<Pawn> pawnsInAggroMentalState = new HashSet<Pawn>();

		private static List<IAttackTarget> targets = new List<IAttackTarget>();

		private static HashSet<IAttackTarget> emptySet = new HashSet<IAttackTarget>();

		private static List<IAttackTarget> tmpTargets = new List<IAttackTarget>();

		private static List<IAttackTarget> tmpToUpdate = new List<IAttackTarget>();

		public HashSet<IAttackTarget> TargetsHostileToColony
		{
			get
			{
				return this.TargetsHostileToFaction(Faction.OfPlayer);
			}
		}

		public AttackTargetsCache(Map map)
		{
			this.map = map;
		}

		public void UpdateTarget(IAttackTarget t)
		{
			if (!this.allTargets.Contains(t))
			{
				return;
			}
			this.DeregisterTarget(t);
			Thing thing = (Thing)t;
			if (thing.Spawned && thing.Map == this.map)
			{
				this.RegisterTarget(t);
			}
		}

		public List<IAttackTarget> GetPotentialTargetsFor(Thing th)
		{
			AttackTargetsCache.targets.Clear();
			Faction faction = th.Faction;
			if (faction != null)
			{
				if (UnityData.isDebugBuild)
				{
					this.Debug_AssertHostile(faction, this.TargetsHostileToFaction(faction));
				}
				foreach (IAttackTarget current in this.TargetsHostileToFaction(faction))
				{
					if (th.HostileTo((Thing)current))
					{
						AttackTargetsCache.targets.Add(current);
					}
				}
			}
			foreach (Pawn current2 in this.pawnsInAggroMentalState)
			{
				if (th.HostileTo(current2))
				{
					AttackTargetsCache.targets.Add(current2);
				}
			}
			Pawn pawn = th as Pawn;
			if (pawn != null && PrisonBreakUtility.IsPrisonBreaking(pawn))
			{
				Faction hostFaction = pawn.guest.HostFaction;
				List<Pawn> list = this.map.mapPawns.SpawnedPawnsInFaction(hostFaction);
				for (int i = 0; i < list.Count; i++)
				{
					if (th.HostileTo(list[i]))
					{
						AttackTargetsCache.targets.Add(list[i]);
					}
				}
			}
			return AttackTargetsCache.targets;
		}

		public HashSet<IAttackTarget> TargetsHostileToFaction(Faction f)
		{
			if (f == null)
			{
				Log.Warning("Called TargetsHostileToFaction with null faction.");
				return AttackTargetsCache.emptySet;
			}
			if (this.targetsHostileToFaction.ContainsKey(f))
			{
				return this.targetsHostileToFaction[f];
			}
			return AttackTargetsCache.emptySet;
		}

		public void Notify_ThingSpawned(Thing th)
		{
			IAttackTarget attackTarget = th as IAttackTarget;
			if (attackTarget != null)
			{
				this.RegisterTarget(attackTarget);
			}
		}

		public void Notify_ThingDespawned(Thing th)
		{
			IAttackTarget attackTarget = th as IAttackTarget;
			if (attackTarget != null)
			{
				this.DeregisterTarget(attackTarget);
			}
		}

		public void Notify_FactionHostilityChanged(Faction f1, Faction f2)
		{
			AttackTargetsCache.tmpTargets.Clear();
			foreach (IAttackTarget current in this.allTargets)
			{
				Thing thing = (Thing)current;
				if (thing.Faction == f1 || thing.Faction == f2)
				{
					AttackTargetsCache.tmpTargets.Add(current);
				}
			}
			for (int i = 0; i < AttackTargetsCache.tmpTargets.Count; i++)
			{
				this.UpdateTarget(AttackTargetsCache.tmpTargets[i]);
			}
			AttackTargetsCache.tmpTargets.Clear();
		}

		private void RegisterTarget(IAttackTarget target)
		{
			if (this.allTargets.Contains(target))
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to register the same target twice ",
					(Thing)target,
					" in ",
					base.GetType()
				}));
				return;
			}
			Thing thing = (Thing)target;
			if (!thing.Spawned)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to register unspawned thing ",
					thing,
					" in ",
					base.GetType()
				}));
				return;
			}
			if (thing.Map != this.map)
			{
				Log.Warning("Tried to register attack target " + thing + " but its Map is not this one.");
				return;
			}
			this.allTargets.Add(target);
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (thing.HostileTo(allFactionsListForReading[i]))
				{
					if (!this.targetsHostileToFaction.ContainsKey(allFactionsListForReading[i]))
					{
						this.targetsHostileToFaction.Add(allFactionsListForReading[i], new HashSet<IAttackTarget>());
					}
					this.targetsHostileToFaction[allFactionsListForReading[i]].Add(target);
				}
			}
			Pawn pawn = target as Pawn;
			if (pawn != null && pawn.InAggroMentalState)
			{
				this.pawnsInAggroMentalState.Add(pawn);
			}
		}

		private void DeregisterTarget(IAttackTarget target)
		{
			if (!this.allTargets.Contains(target))
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to deregister ",
					(Thing)target,
					" but it's not in ",
					base.GetType()
				}));
				return;
			}
			this.allTargets.Remove(target);
			foreach (KeyValuePair<Faction, HashSet<IAttackTarget>> current in this.targetsHostileToFaction)
			{
				HashSet<IAttackTarget> value = current.Value;
				value.Remove(target);
			}
			Pawn pawn = target as Pawn;
			if (pawn != null)
			{
				this.pawnsInAggroMentalState.Remove(pawn);
			}
		}

		private void Debug_AssertHostile(Faction f, HashSet<IAttackTarget> targets)
		{
			AttackTargetsCache.tmpToUpdate.Clear();
			foreach (IAttackTarget current in targets)
			{
				if (!((Thing)current).HostileTo(f))
				{
					AttackTargetsCache.tmpToUpdate.Add(current);
					Log.Error(string.Concat(new string[]
					{
						"Target ",
						(current == null) ? "null" : current.ToString(),
						" is not hostile to ",
						(f == null) ? "null" : f.ToString(),
						" (in ",
						base.GetType().Name,
						") but it's in the list (forgot to update the target somewhere?). Trying to update the target..."
					}));
				}
			}
			for (int i = 0; i < AttackTargetsCache.tmpToUpdate.Count; i++)
			{
				this.UpdateTarget(AttackTargetsCache.tmpToUpdate[i]);
			}
			AttackTargetsCache.tmpToUpdate.Clear();
		}
	}
}
