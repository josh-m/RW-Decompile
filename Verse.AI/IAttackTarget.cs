using System;

namespace Verse.AI
{
	public interface IAttackTarget : ILoadReferenceable
	{
		Thing Thing
		{
			get;
		}

		LocalTargetInfo TargetCurrentlyAimingAt
		{
			get;
		}

		bool ThreatDisabled(IAttackTargetSearcher disabledFor);
	}
}
