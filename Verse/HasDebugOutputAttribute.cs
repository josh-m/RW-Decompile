using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class HasDebugOutputAttribute : Attribute
	{
	}
}
