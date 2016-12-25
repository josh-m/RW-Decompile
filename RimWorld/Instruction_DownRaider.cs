using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Instruction_DownRaider : Lesson_Instruction
	{
		private List<IntVec3> coverCells;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.LookList<IntVec3>(ref this.coverCells, "coverCells", LookMode.Undefined, new object[0]);
		}

		public override void OnActivated()
		{
			base.OnActivated();
			CellRect cellRect = Find.TutorialState.sandbagsRect.ContractedBy(1);
			this.coverCells = new List<IntVec3>();
			foreach (IntVec3 current in cellRect.EdgeCells)
			{
				if (current.x != cellRect.CenterCell.x && current.z != cellRect.CenterCell.z)
				{
					this.coverCells.Add(current);
				}
			}
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = base.Map;
			incidentParms.points = 30f;
			incidentParms.raidArrivalMode = PawnsArriveMode.EdgeWalkIn;
			incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			incidentParms.raidForceOneIncap = true;
			incidentParms.raidNeverFleeIndividual = true;
			IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
		}

		private bool AllColonistsInCover()
		{
			foreach (Pawn current in base.Map.mapPawns.FreeColonistsSpawned)
			{
				if (!this.coverCells.Contains(current.Position))
				{
					return false;
				}
			}
			return true;
		}

		public override void LessonOnGUI()
		{
			if (!this.AllColonistsInCover())
			{
				TutorUtility.DrawCellRectOnGUI(Find.TutorialState.sandbagsRect, this.def.onMapInstruction);
			}
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			if (!this.AllColonistsInCover())
			{
				for (int i = 0; i < this.coverCells.Count; i++)
				{
					Vector3 position = this.coverCells[i].ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
					Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, GenDraw.InteractionCellMaterial, 0);
				}
			}
			IEnumerable<Pawn> source = base.Map.mapPawns.PawnsInFaction(Faction.OfPlayer);
			if (source.Any((Pawn p) => p.Downed))
			{
				foreach (Pawn current in base.Map.mapPawns.AllPawns)
				{
					if (current.HostileTo(Faction.OfPlayer))
					{
						HealthUtility.GiveInjuriesToForceDowned(current);
					}
				}
			}
			if ((from p in base.Map.mapPawns.AllPawnsSpawned
			where p.HostileTo(Faction.OfPlayer) && !p.Downed
			select p).Count<Pawn>() == 0)
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
