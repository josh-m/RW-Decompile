using System;

namespace Verse
{
	public class DialogChoiceConfig
	{
		public string text;

		public string buttonAText = string.Empty;

		public Action buttonAAction;

		public string buttonBText = string.Empty;

		public Action buttonBAction;
	}
}
