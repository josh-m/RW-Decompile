using System;

namespace Verse
{
	public struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>
	{
		private T1 first;

		private T2 second;

		public T1 First
		{
			get
			{
				return this.first;
			}
		}

		public T2 Second
		{
			get
			{
				return this.second;
			}
		}

		public Pair(T1 first, T2 second)
		{
			this.first = first;
			this.second = second;
		}

		public override string ToString()
		{
			string[] expr_06 = new string[5];
			expr_06[0] = "(";
			int arg_24_1 = 1;
			T1 t = this.First;
			expr_06[arg_24_1] = t.ToString();
			expr_06[2] = ", ";
			int arg_43_1 = 3;
			T2 t2 = this.Second;
			expr_06[arg_43_1] = t2.ToString();
			expr_06[4] = ")";
			return string.Concat(expr_06);
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine<T1>(seed, this.first);
			return Gen.HashCombine<T2>(seed, this.second);
		}

		public override bool Equals(object other)
		{
			return other is Pair<T1, T2> && this.Equals((Pair<T1, T2>)other);
		}

		public bool Equals(Pair<T1, T2> other)
		{
			return this.first.Equals(other.first) && this.second.Equals(other.second);
		}

		public static bool operator ==(Pair<T1, T2> lhs, Pair<T1, T2> rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Pair<T1, T2> lhs, Pair<T1, T2> rhs)
		{
			return !(lhs == rhs);
		}
	}
}
