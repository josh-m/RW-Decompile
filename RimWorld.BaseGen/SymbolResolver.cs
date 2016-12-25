using System;
using Verse;

namespace RimWorld.BaseGen
{
	public abstract class SymbolResolver
	{
		public IntVec2 minRectSize = IntVec2.One;

		public virtual bool CanResolve(ResolveParams rp)
		{
			return rp.rect.Width >= this.minRectSize.x && rp.rect.Height >= this.minRectSize.z;
		}

		public abstract void Resolve(ResolveParams rp);
	}
}
