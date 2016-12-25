using System;

namespace Verse
{
	public class InjuryProps
	{
		public float painPerSeverity = 1f;

		public float averagePainPerSeverityOld = 0.5f;

		public float bleeding = 2f;

		public bool canMerge;

		public bool fullyHealableOnlyByTend;

		public string destroyedLabel;

		public string destroyedOutLabel;

		public bool useRemovedLabel;
	}
}
