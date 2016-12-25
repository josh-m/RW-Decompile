using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;

namespace Verse.Steam
{
	public class WorkshopItemHook
	{
		private WorkshopUploadable owner;

		private CSteamID steamAuthor = CSteamID.Nil;

		private CallResult<SteamUGCRequestUGCDetailsResult_t> queryResult;

		public PublishedFileId_t PublishedFileId
		{
			get
			{
				return this.owner.GetPublishedFileId();
			}
			set
			{
				this.owner.SetPublishedFileId(value);
			}
		}

		public string Name
		{
			get
			{
				return this.owner.GetWorkshopName();
			}
		}

		public string Description
		{
			get
			{
				return this.owner.GetWorkshopDescription();
			}
		}

		public string PreviewImagePath
		{
			get
			{
				return this.owner.GetWorkshopPreviewImagePath();
			}
		}

		public IList<string> Tags
		{
			get
			{
				return this.owner.GetWorkshopTags();
			}
		}

		public DirectoryInfo Directory
		{
			get
			{
				return this.owner.GetWorkshopUploadDirectory();
			}
		}

		public bool MayHaveAuthorNotCurrentUser
		{
			get
			{
				return !(this.PublishedFileId == PublishedFileId_t.Invalid) && (this.steamAuthor == CSteamID.Nil || this.steamAuthor != SteamUser.GetSteamID());
			}
		}

		public WorkshopItemHook(WorkshopUploadable owner)
		{
			this.owner = owner;
			if (owner.GetPublishedFileId() != PublishedFileId_t.Invalid)
			{
				this.SendSteamDetailsQuery();
			}
		}

		public void PrepareForWorkshopUpload()
		{
			this.owner.PrepareForWorkshopUpload();
		}

		private void SendSteamDetailsQuery()
		{
			SteamAPICall_t hAPICall = SteamUGC.RequestUGCDetails(this.PublishedFileId, 999999u);
			this.queryResult = CallResult<SteamUGCRequestUGCDetailsResult_t>.Create(new CallResult<SteamUGCRequestUGCDetailsResult_t>.APIDispatchDelegate(this.OnDetailsQueryReturned));
			this.queryResult.Set(hAPICall, null);
		}

		private void OnDetailsQueryReturned(SteamUGCRequestUGCDetailsResult_t result, bool IOFailure)
		{
			this.steamAuthor = (CSteamID)result.m_details.m_ulSteamIDOwner;
		}
	}
}
