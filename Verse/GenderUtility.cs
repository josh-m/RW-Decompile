using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class GenderUtility
	{
		private static readonly Texture2D GenderlessIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gender/Genderless", true);

		private static readonly Texture2D MaleIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gender/Male", true);

		private static readonly Texture2D FemaleIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gender/Female", true);

		public static string GetLabel(this Gender gender)
		{
			switch (gender)
			{
			case Gender.None:
				return "NoneLower".Translate();
			case Gender.Male:
				return "Male".Translate();
			case Gender.Female:
				return "Female".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static string GetPronoun(this Gender gender)
		{
			switch (gender)
			{
			case Gender.None:
				return "Proit".Translate();
			case Gender.Male:
				return "Prohe".Translate();
			case Gender.Female:
				return "Proshe".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static string GetPossessive(this Gender gender)
		{
			switch (gender)
			{
			case Gender.None:
				return "Proits".Translate();
			case Gender.Male:
				return "Prohis".Translate();
			case Gender.Female:
				return "Proher".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static string GetObjective(this Gender gender)
		{
			switch (gender)
			{
			case Gender.None:
				return "ProitObj".Translate();
			case Gender.Male:
				return "ProhimObj".Translate();
			case Gender.Female:
				return "ProherObj".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static Texture GetIcon(this Gender gender)
		{
			switch (gender)
			{
			case Gender.None:
				return GenderUtility.GenderlessIcon;
			case Gender.Male:
				return GenderUtility.MaleIcon;
			case Gender.Female:
				return GenderUtility.FemaleIcon;
			default:
				throw new ArgumentException();
			}
		}
	}
}
