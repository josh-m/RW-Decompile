using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class EscapeShipComp : WorldObjectComp
	{
		public bool raidBeaconEnabled;

		public override void CompTick()
		{
			MapParent mapParent = this.parent as MapParent;
			if (mapParent.HasMap)
			{
				List<Pawn> allPawnsSpawned = mapParent.Map.mapPawns.AllPawnsSpawned;
				bool flag = mapParent.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount != 0;
				bool flag2 = false;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					Pawn pawn = allPawnsSpawned[i];
					if (pawn.RaceProps.Humanlike)
					{
						if (pawn.HostFaction == null)
						{
							if (!pawn.Downed)
							{
								if (pawn.Faction.HostileTo(Faction.OfPlayer))
								{
									flag2 = true;
								}
							}
						}
					}
				}
				if (flag2 && !flag)
				{
					Find.LetterStack.ReceiveLetter("EscapeShipLostLabel".Translate(), "EscapeShipLost".Translate(), LetterDefOf.NegativeEvent, null);
					Find.WorldObjects.Remove(this.parent);
				}
			}
		}

		[DebuggerHidden]
		public override IEnumerable<IncidentTargetTypeDef> AcceptedTypes()
		{
			foreach (IncidentTargetTypeDef type in base.AcceptedTypes())
			{
				yield return type;
			}
			if (this.raidBeaconEnabled)
			{
				yield return IncidentTargetTypeDefOf.MapRaidBeacon;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<bool>(ref this.raidBeaconEnabled, "raidBeaconEnabled", false, false);
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			string label = "VisitEscapeShip".Translate(new object[]
			{
				this.parent.Label
			});
			Action action = delegate
			{
				caravan.pather.StartPath(this.$this.parent.Tile, new CaravanArrivalAction_VisitEscapeShip(this.$this.parent as MapParent), true);
			};
			WorldObject parent = this.parent;
			yield return new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, parent);
			if (Prefs.DevMode)
			{
				label = "VisitEscapeShip".Translate(new object[]
				{
					this.parent.Label
				}) + " (Dev: instantly)";
				action = delegate
				{
					caravan.Tile = this.$this.parent.Tile;
					caravan.pather.StopDead();
					new CaravanArrivalAction_VisitEscapeShip(this.$this.parent as MapParent).Arrived(caravan);
				};
				parent = this.parent;
				yield return new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, parent);
			}
		}
	}
}
