using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolStack
	{
		private Stack<Pair<string, ResolveParams>> stack = new Stack<Pair<string, ResolveParams>>();

		public bool Empty
		{
			get
			{
				return this.stack.Count == 0;
			}
		}

		public void Push(string symbol, ResolveParams resolveParams)
		{
			this.stack.Push(new Pair<string, ResolveParams>(symbol, resolveParams));
		}

		public void Push(string symbol, CellRect rect)
		{
			this.Push(symbol, new ResolveParams
			{
				rect = rect
			});
		}

		public void PushMany(ResolveParams resolveParams, params string[] symbols)
		{
			for (int i = 0; i < symbols.Length; i++)
			{
				this.Push(symbols[i], resolveParams);
			}
		}

		public void PushMany(CellRect rect, params string[] symbols)
		{
			for (int i = 0; i < symbols.Length; i++)
			{
				this.Push(symbols[i], rect);
			}
		}

		public Pair<string, ResolveParams> Pop()
		{
			return this.stack.Pop();
		}

		public void Clear()
		{
			this.stack.Clear();
		}
	}
}
