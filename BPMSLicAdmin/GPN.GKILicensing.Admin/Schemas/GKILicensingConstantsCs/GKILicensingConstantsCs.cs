namespace Terrasoft.Configuration
{
	using System;
	using Terrasoft.Core;
	public static class GKILicensingConstantsCs
	{
		public static class GKILicSyncSource
		{
			public static readonly Guid Regular = new Guid("B0BA614D-9580-4022-B803-B542A35C4F2F");
			public static readonly Guid MSAD = new Guid("5CB91734-66FB-491A-9A17-1535D3D218AD");
		}

		public static class GKILicStatus
		{
			public static readonly Guid Active = new Guid(("672f84f8-45de-4383-8220-a805b30b745e").ToUpper());
			public static readonly Guid Inactive = new Guid(("a39496cf-cb5f-44a8-a376-16ebd2ad1ea4").ToUpper());
		}
		public static class GKILicUserStatus
		{
			public static readonly Guid Active = new Guid(("976115be-b5cb-4e07-9e69-2f0d675bd025").ToUpper());
			public static readonly Guid Inactive = new Guid(("cc6f7010-a1a1-4f45-bd1a-3281f6cc81cd").ToUpper());
			public static readonly Guid NotInUse = new Guid(("c78e0835-38d0-4ca4-b768-8d05507f49a3").ToUpper());
		}
		public static class GKIDeactivationReasonLookup
        {
			public static readonly Guid Inactive = new Guid(("34034c4e-f1b5-4fec-8962-6167726e3819").ToUpper());
			public static readonly Guid DidntEnter = new Guid(("221b20f6-ed14-4bb8-8875-3bb962c64ab2").ToUpper());
			public static readonly Guid HaventEnteredInTheTimespan = new Guid(("0781c2de-db17-4718-8c5d-ccf0a4790b34").ToUpper());
		}
	}
}