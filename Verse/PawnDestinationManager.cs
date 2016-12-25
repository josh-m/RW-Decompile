using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public sealed class PawnDestinationManager
	{
		private Dictionary<Faction, Dictionary<Pawn, IntVec3>> reservedDestinations = new Dictionary<Faction, Dictionary<Pawn, IntVec3>>();

		private static readonly Material DestinationMat = MaterialPool.MatFrom("UI/Overlays/ReservedDestination");

		private static readonly Material DestinationSelectionMat = MaterialPool.MatFrom("UI/Overlays/ReservedDestinationSelection");

		public PawnDestinationManager()
		{
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				this.RegisterFaction(current);
			}
		}

		public void RegisterFaction(Faction faction)
		{
			this.reservedDestinations.Add(faction, new Dictionary<Pawn, IntVec3>());
		}

		public void ReserveDestinationFor(Pawn p, IntVec3 loc)
		{
			if (p.Faction == null)
			{
				return;
			}
			Pawn pawn = this.ReserverOfDestinationForFaction(loc, p.Faction);
			if (pawn != null && pawn != p)
			{
				return;
			}
			this.reservedDestinations[p.Faction][p] = loc;
		}

		public IntVec3 DestinationReservedFor(Pawn p)
		{
			if (p.Faction == null)
			{
				return IntVec3.Invalid;
			}
			if (this.reservedDestinations[p.Faction].ContainsKey(p))
			{
				return this.reservedDestinations[p.Faction][p];
			}
			return IntVec3.Invalid;
		}

		public bool DestinationIsReserved(IntVec3 loc)
		{
			foreach (KeyValuePair<Faction, Dictionary<Pawn, IntVec3>> current in this.reservedDestinations)
			{
				foreach (KeyValuePair<Pawn, IntVec3> current2 in current.Value)
				{
					if (current2.Value == loc)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool DestinationIsReserved(IntVec3 c, Pawn searcher)
		{
			if (searcher.Faction == null)
			{
				return false;
			}
			foreach (KeyValuePair<Pawn, IntVec3> current in this.reservedDestinations[searcher.Faction])
			{
				if (current.Value == c && current.Key != searcher)
				{
					return true;
				}
			}
			return false;
		}

		private Pawn ReserverOfDestinationForFaction(IntVec3 c, Faction faction)
		{
			if (faction == null)
			{
				return null;
			}
			foreach (KeyValuePair<Pawn, IntVec3> current in this.reservedDestinations[faction])
			{
				if (current.Value == c)
				{
					return current.Key;
				}
			}
			return null;
		}

		public void UnreserveAllFor(Pawn p)
		{
			if (p.Faction == null)
			{
				return;
			}
			this.reservedDestinations[p.Faction].Remove(p);
		}

		public void RemovePawnFromSystem(Pawn p)
		{
			if (p.Faction == null)
			{
				return;
			}
			if (this.reservedDestinations[p.Faction].ContainsKey(p))
			{
				this.reservedDestinations[p.Faction].Remove(p);
			}
		}

		public void DebugDrawDestinations()
		{
			foreach (KeyValuePair<Pawn, IntVec3> current in this.reservedDestinations[Faction.OfPlayer])
			{
				if (!(current.Key.Position == current.Value))
				{
					IntVec3 value = current.Value;
					Vector3 s = new Vector3(1f, 1f, 1f);
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(value.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), Quaternion.identity, s);
					Graphics.DrawMesh(MeshPool.plane10, matrix, PawnDestinationManager.DestinationMat, 0);
					if (Find.Selector.IsSelected(current.Key))
					{
						Graphics.DrawMesh(MeshPool.plane10, matrix, PawnDestinationManager.DestinationSelectionMat, 0);
					}
				}
			}
		}
	}
}
