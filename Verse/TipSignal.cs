using System;

namespace Verse
{
	public struct TipSignal
	{
		public string text;

		public Func<string> textGetter;

		public int uniqueId;

		public TooltipPriority priority;

		public TipSignal(string text, int uniqueId)
		{
			this.text = text;
			this.textGetter = null;
			this.uniqueId = uniqueId;
			this.priority = TooltipPriority.Default;
		}

		public TipSignal(string text, int uniqueId, TooltipPriority priority)
		{
			this.text = text;
			this.textGetter = null;
			this.uniqueId = uniqueId;
			this.priority = priority;
		}

		public TipSignal(string text)
		{
			if (text == null)
			{
				Log.Error("TipSignal with null text.");
				text = string.Empty;
			}
			this.text = text;
			this.textGetter = null;
			this.uniqueId = text.GetHashCode();
			this.priority = TooltipPriority.Default;
		}

		public TipSignal(Func<string> textGetter, int uniqueId)
		{
			this.text = string.Empty;
			this.textGetter = textGetter;
			this.uniqueId = uniqueId;
			this.priority = TooltipPriority.Default;
		}

		public TipSignal(TipSignal cloneSource)
		{
			this.text = cloneSource.text;
			this.textGetter = null;
			this.priority = cloneSource.priority;
			this.uniqueId = cloneSource.uniqueId;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Tip(",
				this.text,
				", ",
				this.uniqueId,
				")"
			});
		}

		public static implicit operator TipSignal(string str)
		{
			return new TipSignal(str);
		}
	}
}
