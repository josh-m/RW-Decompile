using RimWorld;
using System;
using System.Text;

namespace Verse.AI.Group
{
	public struct TriggerSignal
	{
		public TriggerSignalType type;

		public string memo;

		public Pawn pawn;

		public DamageInfo dinfo;

		public PawnLostCondition condition;

		public Faction faction;

		public static TriggerSignal ForTick
		{
			get
			{
				return new TriggerSignal(TriggerSignalType.Tick);
			}
		}

		public TriggerSignal(TriggerSignalType type)
		{
			this.type = type;
			this.memo = null;
			this.pawn = null;
			this.dinfo = default(DamageInfo);
			this.condition = PawnLostCondition.Undefined;
			this.faction = null;
		}

		public static TriggerSignal ForMemo(string memo)
		{
			return new TriggerSignal(TriggerSignalType.Memo)
			{
				memo = memo
			};
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			stringBuilder.Append(this.type.ToString());
			if (this.memo != null)
			{
				stringBuilder.Append(", memo=" + this.memo);
			}
			if (this.pawn != null)
			{
				stringBuilder.Append(", pawn=" + this.pawn);
			}
			if (this.dinfo.Def != null)
			{
				stringBuilder.Append(", dinfo=" + this.dinfo);
			}
			if (this.condition != PawnLostCondition.Undefined)
			{
				stringBuilder.Append(", condition=" + this.condition);
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
	}
}
