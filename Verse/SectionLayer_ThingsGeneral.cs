using System;

namespace Verse
{
	public class SectionLayer_ThingsGeneral : SectionLayer_Things
	{
		public SectionLayer_ThingsGeneral(Section section) : base(section)
		{
			this.relevantChangeTypes = MapMeshFlag.Things;
			this.requireAddToMapMesh = true;
		}

		protected override void TakePrintFrom(Thing t)
		{
			try
			{
				t.Print(this);
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new object[]
				{
					"Exception printing ",
					t,
					" at ",
					t.Position,
					": ",
					ex.ToString()
				}));
			}
		}
	}
}
