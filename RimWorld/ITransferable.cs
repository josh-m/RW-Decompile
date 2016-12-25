using System;
using Verse;

namespace RimWorld
{
	public interface ITransferable
	{
		Thing AnyThing
		{
			get;
		}

		ThingDef ThingDef
		{
			get;
		}

		bool Interactive
		{
			get;
		}

		bool HasAnyThing
		{
			get;
		}

		string Label
		{
			get;
		}

		string TipDescription
		{
			get;
		}

		int CountToTransfer
		{
			get;
			set;
		}

		string EditBuffer
		{
			get;
			set;
		}

		TransferablePositiveCountDirection PositiveCountDirection
		{
			get;
		}

		AcceptanceReport CanSetToTransferOneMoreToSource();

		AcceptanceReport TrySetToTransferOneMoreToSource();

		void SetToTransferMaxToSource();

		AcceptanceReport CanSetToTransferOneMoreToDest();

		AcceptanceReport TrySetToTransferOneMoreToDest();

		void SetToTransferMaxToDest();
	}
}
