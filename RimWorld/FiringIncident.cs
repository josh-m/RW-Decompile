using System;
using Verse;

namespace RimWorld
{
	public class FiringIncident : IExposable
	{
		public IncidentDef def;

		public IncidentParms parms = new IncidentParms();

		public StorytellerComp source;

		public FiringIncident()
		{
		}

		public FiringIncident(IncidentDef def, StorytellerComp source, IncidentParms parms = null)
		{
			this.def = def;
			if (parms != null)
			{
				this.parms = parms;
			}
			this.source = source;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look<IncidentDef>(ref this.def, "def");
			Scribe_Deep.Look<IncidentParms>(ref this.parms, "parms", new object[0]);
		}

		public override string ToString()
		{
			string text = this.def.ToString();
			text = text.PadRight(17);
			string text2 = text;
			if (this.parms != null)
			{
				text2 = text2 + " " + this.parms.ToString();
			}
			if (this.source != null)
			{
				text2 = text2 + ", source=" + this.source;
			}
			return text2;
		}
	}
}
