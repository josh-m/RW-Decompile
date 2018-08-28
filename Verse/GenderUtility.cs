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

		public static string GetGenderLabel(this Pawn pawn)
		{
			return pawn.gender.GetLabel(pawn.RaceProps.Animal);
		}

		public static string GetLabel(this Gender gender, bool animal = false)
		{
			if (gender == Gender.None)
			{
				return "NoneLower".Translate();
			}
			if (gender == Gender.Male)
			{
				return (!animal) ? "Male".Translate() : "MaleAnimal".Translate();
			}
			if (gender != Gender.Female)
			{
				throw new ArgumentException();
			}
			return (!animal) ? "Female".Translate() : "FemaleAnimal".Translate();
		}

		public static string GetPronoun(this Gender gender)
		{
			if (gender == Gender.None)
			{
				return "Proit".Translate();
			}
			if (gender == Gender.Male)
			{
				return "Prohe".Translate();
			}
			if (gender != Gender.Female)
			{
				throw new ArgumentException();
			}
			return "Proshe".Translate();
		}

		public static string GetPossessive(this Gender gender)
		{
			if (gender == Gender.None)
			{
				return "Proits".Translate();
			}
			if (gender == Gender.Male)
			{
				return "Prohis".Translate();
			}
			if (gender != Gender.Female)
			{
				throw new ArgumentException();
			}
			return "Proher".Translate();
		}

		public static string GetObjective(this Gender gender)
		{
			if (gender == Gender.None)
			{
				return "ProitObj".Translate();
			}
			if (gender == Gender.Male)
			{
				return "ProhimObj".Translate();
			}
			if (gender != Gender.Female)
			{
				throw new ArgumentException();
			}
			return "ProherObj".Translate();
		}

		public static Texture2D GetIcon(this Gender gender)
		{
			if (gender == Gender.None)
			{
				return GenderUtility.GenderlessIcon;
			}
			if (gender == Gender.Male)
			{
				return GenderUtility.MaleIcon;
			}
			if (gender != Gender.Female)
			{
				throw new ArgumentException();
			}
			return GenderUtility.FemaleIcon;
		}
	}
}
