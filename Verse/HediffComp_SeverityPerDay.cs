using System;
using System.Text;

namespace Verse
{
	public class HediffComp_SeverityPerDay : HediffComp
	{
		protected const int SeverityUpdateInterval = 200;

		public override void CompPostTick()
		{
			base.CompPostTick();
			if (base.Pawn.IsHashIntervalTick(200))
			{
				this.parent.Severity += this.SeverityChangePerInterval();
			}
		}

		protected float SeverityChangePerInterval()
		{
			float num = this.SeverityChangePerDay();
			return num * 0.00333333341f;
		}

		protected virtual float SeverityChangePerDay()
		{
			return this.props.severityPerDay;
		}

		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.CompDebugString());
			if (!base.Pawn.Dead)
			{
				stringBuilder.AppendLine("severity change per day: " + (this.SeverityChangePerInterval() / 200f * 60000f).ToString("F3"));
			}
			return stringBuilder.ToString();
		}
	}
}
