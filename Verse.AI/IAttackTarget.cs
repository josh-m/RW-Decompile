using System;

namespace Verse.AI
{
	public interface IAttackTarget : ILoadReferenceable
	{
		bool ThreatDisabled();
	}
}
