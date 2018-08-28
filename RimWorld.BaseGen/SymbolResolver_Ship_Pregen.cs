using System;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Ship_Pregen : SymbolResolver
	{
		private struct SpawnDescriptor
		{
			public IntVec3 offset;

			public ThingDef def;

			public Rot4 rot;
		}

		public override void Resolve(ResolveParams rp)
		{
			SymbolResolver_Ship_Pregen.SpawnDescriptor[] array = new SymbolResolver_Ship_Pregen.SpawnDescriptor[]
			{
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(0, 0, 0),
					def = ThingDefOf.Ship_Reactor,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(0, 0, 12),
					def = ThingDefOf.Ship_Beam,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(0, 0, 6),
					def = ThingDefOf.Ship_Beam,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-4, 0, -2),
					def = ThingDefOf.Ship_Beam,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(4, 0, -2),
					def = ThingDefOf.Ship_Beam,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(0, 0, -7),
					def = ThingDefOf.Ship_Beam,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(0, 0, 16),
					def = ThingDefOf.Ship_SensorCluster,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(2, 0, -7),
					def = ThingDefOf.Ship_ComputerCore,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-1, 0, 15),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.West
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-1, 0, 13),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.West
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-1, 0, 11),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.West
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-1, 0, 9),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.West
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-1, 0, 7),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.West
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-1, 0, 5),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.West
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-4, 0, 2),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-5, 0, 1),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.West
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(2, 0, 15),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.East
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(2, 0, 13),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.East
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(2, 0, 11),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.East
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(2, 0, 9),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.East
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(2, 0, 7),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.East
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(2, 0, 5),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.East
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(5, 0, 2),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(6, 0, 1),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.East
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-6, 0, -3),
					def = ThingDefOf.Ship_Engine,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(7, 0, -3),
					def = ThingDefOf.Ship_Engine,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(3, 0, -11),
					def = ThingDefOf.Ship_Engine,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-2, 0, -11),
					def = ThingDefOf.Ship_Engine,
					rot = Rot4.North
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-1, 0, -8),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.West
				},
				new SymbolResolver_Ship_Pregen.SpawnDescriptor
				{
					offset = new IntVec3(-1, 0, -6),
					def = ThingDefOf.Ship_CryptosleepCasket,
					rot = Rot4.West
				}
			};
			IntVec3 centerCell = rp.rect.CenterCell;
			IntVec3 b = new IntVec3(-1, 0, -3);
			SymbolResolver_Ship_Pregen.SpawnDescriptor[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				SymbolResolver_Ship_Pregen.SpawnDescriptor spawnDescriptor = array2[i];
				Thing thing = ThingMaker.MakeThing(spawnDescriptor.def, null);
				thing.SetFaction(rp.faction, null);
				if (rp.hpPercentRange.HasValue)
				{
					thing.HitPoints = Mathf.Clamp(Mathf.RoundToInt((float)thing.MaxHitPoints * rp.hpPercentRange.Value.RandomInRange), 1, thing.MaxHitPoints);
					GenLeaving.DropFilthDueToDamage(thing, (float)(thing.MaxHitPoints - thing.HitPoints));
				}
				CompHibernatable compHibernatable = thing.TryGetComp<CompHibernatable>();
				if (compHibernatable != null)
				{
					compHibernatable.State = HibernatableStateDefOf.Hibernating;
				}
				GenSpawn.Spawn(thing, centerCell + b + spawnDescriptor.offset, BaseGen.globalSettings.map, spawnDescriptor.rot, WipeMode.Vanish, false);
			}
		}
	}
}
