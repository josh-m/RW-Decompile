using System;
using UnityEngine;

namespace Verse
{
	public class LogMessage
	{
		public string text;

		public LogMessageType type;

		public int repeats = 1;

		private string stackTrace;

		public Color Color
		{
			get
			{
				switch (this.type)
				{
				case LogMessageType.Message:
					return Color.white;
				case LogMessageType.Warning:
					return Color.yellow;
				case LogMessageType.Error:
					return Color.red;
				default:
					return Color.white;
				}
			}
		}

		public string StackTrace
		{
			get
			{
				if (this.stackTrace != null)
				{
					return this.stackTrace;
				}
				return "No stack trace.";
			}
		}

		public LogMessage(string text)
		{
			this.text = text;
			this.type = LogMessageType.Message;
			this.stackTrace = null;
		}

		public LogMessage(LogMessageType type, string text, string stackTrace)
		{
			this.text = text;
			this.type = type;
			this.stackTrace = stackTrace;
		}

		public override string ToString()
		{
			if (this.repeats > 1)
			{
				return "(" + this.repeats.ToString() + ") " + this.text;
			}
			return this.text;
		}

		public bool CanCombineWith(LogMessage other)
		{
			return this.text == other.text && this.type == other.type;
		}
	}
}
