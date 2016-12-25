using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public struct EventPack
	{
		private string tagInt;

		private IntVec3 cellInt;

		private IEnumerable<IntVec3> cellsInt;

		public string Tag
		{
			get
			{
				return this.tagInt;
			}
		}

		public IntVec3 Cell
		{
			get
			{
				return this.cellInt;
			}
		}

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				return this.cellsInt;
			}
		}

		public EventPack(string tag)
		{
			this.tagInt = tag;
			this.cellInt = IntVec3.Invalid;
			this.cellsInt = null;
		}

		public EventPack(string tag, IntVec3 cell)
		{
			this.tagInt = tag;
			this.cellInt = cell;
			this.cellsInt = null;
		}

		public EventPack(string tag, IEnumerable<IntVec3> cells)
		{
			this.tagInt = tag;
			this.cellInt = IntVec3.Invalid;
			this.cellsInt = cells;
		}

		public override string ToString()
		{
			if (this.Cell.IsValid)
			{
				return this.Tag + "-" + this.Cell;
			}
			return this.Tag;
		}

		public static implicit operator EventPack(string s)
		{
			return new EventPack(s);
		}
	}
}
