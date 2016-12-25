using System;
using System.Linq;

namespace Verse
{
	public class NameTriple : Name
	{
		[LoadAlias("first")]
		private string firstInt;

		[LoadAlias("nick")]
		private string nickInt;

		[LoadAlias("last")]
		private string lastInt;

		private static NameTriple invalidInt = new NameTriple("Invalid", "Invalid", "Invalid");

		public string First
		{
			get
			{
				return this.firstInt;
			}
		}

		public string Nick
		{
			get
			{
				return this.nickInt;
			}
		}

		public string Last
		{
			get
			{
				return this.lastInt;
			}
		}

		public override string ToStringFull
		{
			get
			{
				if (this.First == this.Nick || this.Last == this.Nick)
				{
					return this.First + " " + this.Last;
				}
				return string.Concat(new string[]
				{
					this.First,
					" '",
					this.Nick,
					"' ",
					this.Last
				});
			}
		}

		public override string ToStringShort
		{
			get
			{
				return this.nickInt;
			}
		}

		public override bool IsValid
		{
			get
			{
				return !this.First.NullOrEmpty() && !this.Last.NullOrEmpty();
			}
		}

		public override bool Numerical
		{
			get
			{
				return false;
			}
		}

		public static NameTriple Invalid
		{
			get
			{
				return NameTriple.invalidInt;
			}
		}

		public NameTriple()
		{
		}

		public NameTriple(string first, string nick, string last)
		{
			this.firstInt = first;
			this.nickInt = nick;
			this.lastInt = last;
		}

		public override void ExposeData()
		{
			Scribe_Values.LookValue<string>(ref this.firstInt, "first", null, false);
			Scribe_Values.LookValue<string>(ref this.nickInt, "nick", null, false);
			Scribe_Values.LookValue<string>(ref this.lastInt, "last", null, false);
		}

		public void ResolveMissingPieces(string overrideLastName = null)
		{
			if (this.First.NullOrEmpty() && this.Nick.NullOrEmpty() && this.Last.NullOrEmpty())
			{
				Log.Error("Cannot resolve misssing pieces in PawnName: No name data.");
				this.firstInt = (this.nickInt = (this.lastInt = "Empty"));
				return;
			}
			if (this.First == null)
			{
				this.firstInt = string.Empty;
			}
			if (this.Last == null)
			{
				this.lastInt = string.Empty;
			}
			if (overrideLastName != null)
			{
				this.lastInt = overrideLastName;
			}
			if (this.Nick.NullOrEmpty())
			{
				if (this.Last == string.Empty)
				{
					this.nickInt = this.First;
				}
				else
				{
					Rand.PushSeed();
					Rand.Seed = Gen.HashCombine<string>(this.First.GetHashCode(), this.Last);
					if (Rand.Value < 0.5f)
					{
						this.nickInt = this.First;
					}
					else
					{
						this.nickInt = this.Last;
					}
					Rand.PopSeed();
					this.CapitalizeNick();
				}
			}
		}

		public override bool ConfusinglySimilarTo(Name other)
		{
			NameTriple nameTriple = other as NameTriple;
			if (nameTriple != null)
			{
				if (this.Nick != null && this.Nick == nameTriple.Nick)
				{
					return true;
				}
				if (this.First == nameTriple.First && this.Last == nameTriple.Last)
				{
					return true;
				}
			}
			NameSingle nameSingle = other as NameSingle;
			return nameSingle != null && nameSingle.Name == this.Nick;
		}

		public static NameTriple FromString(string rawName)
		{
			if (rawName.Trim().Length == 0)
			{
				Log.Error("Tried to parse PawnName from empty or whitespace string.");
				return NameTriple.Invalid;
			}
			NameTriple nameTriple = new NameTriple();
			int num = -1;
			int num2 = -1;
			for (int i = 0; i < rawName.Length - 1; i++)
			{
				if (rawName[i] == ' ' && rawName[i + 1] == '\'' && num == -1)
				{
					num = i;
				}
				if (rawName[i] == '\'' && rawName[i + 1] == ' ')
				{
					num2 = i;
				}
			}
			if (num == -1 || num2 == -1)
			{
				if (!rawName.Contains(' '))
				{
					nameTriple.nickInt = rawName.Trim();
				}
				else
				{
					string[] array = rawName.Split(new char[]
					{
						' '
					});
					if (array.Length == 1)
					{
						nameTriple.nickInt = array[0].Trim();
					}
					else if (array.Length == 2)
					{
						nameTriple.firstInt = array[0].Trim();
						nameTriple.lastInt = array[1].Trim();
					}
					else
					{
						nameTriple.firstInt = array[0].Trim();
						nameTriple.lastInt = string.Empty;
						for (int j = 1; j < array.Length; j++)
						{
							NameTriple expr_137 = nameTriple;
							expr_137.lastInt += array[j];
							if (j < array.Length - 1)
							{
								NameTriple expr_15A = nameTriple;
								expr_15A.lastInt += " ";
							}
						}
					}
				}
			}
			else
			{
				nameTriple.firstInt = rawName.Substring(0, num).Trim();
				nameTriple.nickInt = rawName.Substring(num + 2, num2 - num - 2).Trim();
				nameTriple.lastInt = ((num2 >= rawName.Length - 2) ? string.Empty : rawName.Substring(num2 + 2).Trim());
			}
			return nameTriple;
		}

		public void CapitalizeNick()
		{
			if (!this.nickInt.NullOrEmpty())
			{
				this.nickInt = char.ToUpper(this.Nick[0]) + this.Nick.Substring(1);
			}
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.First,
				" '",
				this.Nick,
				"' ",
				this.Last
			});
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (!(obj is NameTriple))
			{
				return false;
			}
			NameTriple nameTriple = (NameTriple)obj;
			return this.First == nameTriple.First && this.Last == nameTriple.Last && this.Nick == nameTriple.Nick;
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine<string>(seed, this.First);
			seed = Gen.HashCombine<string>(seed, this.Last);
			return Gen.HashCombine<string>(seed, this.Nick);
		}
	}
}
