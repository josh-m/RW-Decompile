using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class GlobalSettings
	{
		public Map map;

		public int minBuildings;

		public int minEmptyNodes;

		public int minBarracks;

		public CellRect mainRect;

		public int basePart_buildingsResolved;

		public int basePart_emptyNodesResolved;

		public int basePart_barracksResolved;

		public float basePart_batteriesCoverage;

		public float basePart_farmsCoverage;

		public float basePart_powerPlantsCoverage;

		public float basePart_breweriesCoverage;

		public void Clear()
		{
			this.map = null;
			this.minBuildings = 0;
			this.minBarracks = 0;
			this.minEmptyNodes = 0;
			this.mainRect = CellRect.Empty;
			this.basePart_buildingsResolved = 0;
			this.basePart_emptyNodesResolved = 0;
			this.basePart_barracksResolved = 0;
			this.basePart_batteriesCoverage = 0f;
			this.basePart_farmsCoverage = 0f;
			this.basePart_powerPlantsCoverage = 0f;
			this.basePart_breweriesCoverage = 0f;
		}
	}
}
