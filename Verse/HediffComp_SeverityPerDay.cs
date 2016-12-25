using System;
using System.Text;

namespace Verse
{
	public class HediffComp_SeverityPerDay : HediffComp
	{
		protected const int SeverityUpdateInterval = 200;

		private HediffCompProperties_SeverityPerDay Props
		{
			get
			{
				return (HediffCompProperties_SeverityPerDay)this.props;
			}
		}

		public override void CompPostTick()
		{
			base.CompPostTick();
			if (base.Pawn.IsHashIntervalTick(200))
			{
				float num = this.SeverityChangePerDay();
				num *= 0.00333333341f;
				this.parent.Severity += num;
			}
		}

		protected virtual float SeverityChangePerDay()
		{
			return this.Props.severityPerDay;
		}

		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.CompDebugString());
			if (!base.Pawn.Dead)
			{
				stringBuilder.AppendLine("severity change per day: " + this.SeverityChangePerDay().ToString("F3"));
			}
			return stringBuilder.ToString();
		}
	}
}
