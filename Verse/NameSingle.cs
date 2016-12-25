using System;

namespace Verse
{
	public class NameSingle : Name
	{
		private string nameInt;

		private bool numerical;

		public string Name
		{
			get
			{
				return this.nameInt;
			}
		}

		public override string ToStringFull
		{
			get
			{
				return this.nameInt;
			}
		}

		public override string ToStringShort
		{
			get
			{
				return this.nameInt;
			}
		}

		public override bool IsValid
		{
			get
			{
				return !this.nameInt.NullOrEmpty();
			}
		}

		public override bool Numerical
		{
			get
			{
				return this.numerical;
			}
		}

		public NameSingle()
		{
		}

		public NameSingle(string name, bool numerical = false)
		{
			this.nameInt = name;
			this.numerical = numerical;
		}

		public override void ExposeData()
		{
			Scribe_Values.LookValue<string>(ref this.nameInt, "name", null, false);
			Scribe_Values.LookValue<bool>(ref this.numerical, "numerical", false, false);
		}

		public override bool ConfusinglySimilarTo(Name other)
		{
			NameSingle nameSingle = other as NameSingle;
			if (nameSingle != null && nameSingle.nameInt == this.nameInt)
			{
				return true;
			}
			NameTriple nameTriple = other as NameTriple;
			return nameTriple != null && nameTriple.Nick == this.nameInt;
		}

		public override string ToString()
		{
			return this.nameInt;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (!(obj is NameSingle))
			{
				return false;
			}
			NameSingle nameSingle = (NameSingle)obj;
			return this.nameInt == nameSingle.nameInt;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineInt(this.nameInt.GetHashCode(), 1384661390);
		}
	}
}
