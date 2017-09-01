using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_Animals : MainTabWindow_PawnTable
	{
		protected override PawnTableDef PawnTableDef
		{
			get
			{
				return PawnTableDefOf.Animals;
			}
		}

		protected override IEnumerable<Pawn> Pawns
		{
			get
			{
				return from p in Find.VisibleMap.mapPawns.PawnsInFaction(Faction.OfPlayer)
				where p.RaceProps.Animal
				orderby p.RaceProps.petness descending, p.RaceProps.baseBodySize, p.def.label
				select p;
			}
		}

		public override void PostOpen()
		{
			base.PostOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}
	}
}
