using System;

namespace Verse
{
	public class HediffComp_Effecter : HediffComp
	{
		public EffecterDef CurrentStateEffecter()
		{
			if (this.parent.CurStageIndex >= this.props.severityIndices.min && (this.props.severityIndices.max < 0 || this.parent.CurStageIndex <= this.props.severityIndices.max))
			{
				return this.props.stateEffecter;
			}
			return null;
		}
	}
}
