using System;

namespace Verse
{
	public class HediffComp_Effecter : HediffComp
	{
		public HediffCompProperties_Effecter Props
		{
			get
			{
				return (HediffCompProperties_Effecter)this.props;
			}
		}

		public EffecterDef CurrentStateEffecter()
		{
			if (this.parent.CurStageIndex >= this.Props.severityIndices.min && (this.Props.severityIndices.max < 0 || this.parent.CurStageIndex <= this.Props.severityIndices.max))
			{
				return this.Props.stateEffecter;
			}
			return null;
		}
	}
}
