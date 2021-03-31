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

		public static class GKIApplicationStatus
		{
			public static readonly Guid New = new Guid(("ede5b81b-eca2-4b36-82b8-4f8cafa2da47").ToUpper());
			public static readonly Guid RequestSent = new Guid(("a17b17c2-6bd1-4355-83c6-1d1c5d12d70e").ToUpper());
			public static readonly Guid UpdateRequired = new Guid(("0890be6c-1c3a-4f52-8601-2a9814727e73").ToUpper());
			public static readonly Guid TlsLoaded = new Guid(("05fe1f8a-90b9-4dbe-b9db-17aad724f5a8").ToUpper());
			public static readonly Guid TlsInstalled = new Guid(("3d2a8bf5-cb58-4ad8-b431-554592e44cb0").ToUpper());
			public static readonly Guid TlsNotInstalled = new Guid(("a87289ed-53d9-4a1b-b1e2-c11c9fa243b5").ToUpper());
		}

		public static class GKILicStatus
		{
			public static readonly Guid Active = new Guid(("672f84f8-45de-4383-8220-a805b30b745e").ToUpper());
			public static readonly Guid Inactive = new Guid(("a39496cf-cb5f-44a8-a376-16ebd2ad1ea4").ToUpper());
		}
		public static class GKILicType
		{
			public static readonly Guid Personal = new Guid(("7cb7fff2-f050-41e0-b277-e2e1674c86a4").ToUpper());
			public static readonly Guid Server = new Guid(("e704b9df-122c-407d-a80d-851f8df82a41").ToUpper());
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
			public static readonly Guid LDAP = new Guid(("d8756a62-f37a-458b-a134-515fb8495615").ToUpper());
		}
		public static class SysAdminUnitRole
		{
			public static readonly Guid LicAdmin = new Guid(("b1aa7564-7e2b-4edf-8d4b-02ee011a5def").ToUpper());
			public static readonly Guid SecurityAuditor = new Guid(("a4b65f4b-e504-419c-a811-9e399f347af2").ToUpper());
		}
		public static class Misc
		{
			public static readonly Guid TlrRequestEmailTemplate = new Guid(("1f16b3ad-97de-4ae1-969b-443b8df06a1c").ToUpper());
			public static readonly Guid SlaveAndADNotInSyncEmailTemplate = new Guid(("245e82f3-f3f8-43b6-9071-dbcf86497484").ToUpper());
			public static readonly int lastMasterCheckInWaitMinutes = 5;
		}
		public static class LicensingServices
		{
			public static readonly string authName = ".ASPXAUTH";
			public static readonly string crsfName = "BPMCSRF";
			public static readonly string authServicePath = "/ServiceModel/AuthService.svc/Login";
			public static readonly string authTemplate = "{{\"UserName\": \"{0}\",\"UserPassword\": \"{1}\"}}";
			public static readonly string GKIUsersSyncServiceUrl = "/0/rest/GKILicensingRegularService/GKIUsersSync";
			public static readonly string GKIAddLicenseServiceUrl = "/0/rest/GKILicensingRegularService/GKIAddLicense";
			public static readonly string GKIRemoveLicenseServiceUrl = "/0/rest/GKILicensingRegularService/GKIRemoveLicense";
			public static readonly string GKITlsInstallRegularServiceUrl = "/0/rest/GKILicensingRegularService/GKIInstallLicenses";
			public static readonly string GKIAuthCheckServiceUrl = "/0/rest/GKILicensingRegularService/GKIAuthCheck";
			public static readonly string GKIGetInstalledLicensesInfoServiceUrl = "/0/rest/GKILicensingRegularService/GKIGetInstalledLicensesInfo";
			public static readonly string GKITlrCustomerIdUrl = "/0/ServiceModel/LicenseService.svc/GetCustomerId";
			public static readonly string GKITlrRequestUrl = "/0/ServiceModel/LicenseService.svc/CreateLicenseRequest";
			public static readonly string GKIPulseUrl = "/0/rest/GKILicensingRegularService/GKIPulse";
			public static readonly string GKIVIPUsersUrl = "/0/rest/GKILicensingRegularService/GKIGetVIPUsers";
			public static readonly string GKIImportTlsFile = "/rest/GKILicensingAdminService/GKIImportTlsFile";
		}

		public static class LicensingLDAP
        {
			public static readonly string gkiLdapGroupMacroName = "[#LDAPGroupDN#]";
			public static readonly string GKIGetSyncLDAPResponseUrl = "/0/rest/GKILicensingLDAPService/GKIGetSyncLDAPResponse";
			public static readonly string GKIAuthCheckServiceUrl = "/0/rest/GKILicensingLDAPService/GKIAuthCheck";
		}
	}
}