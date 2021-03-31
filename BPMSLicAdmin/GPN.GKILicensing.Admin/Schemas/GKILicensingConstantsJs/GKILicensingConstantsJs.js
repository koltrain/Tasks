define("GKILicensingConstantsJs", ["GKILicensingConstantsJsResources"], function(resources) {
	const localizableStrings = resources.localizableStrings;
	const licStatus = {
		Active: "672f84f8-45de-4383-8220-a805b30b745e", // Активна
		Inactive: "a39496cf-cb5f-44a8-a376-16ebd2ad1ea4", // Неактивна
		NotInUse: "b1da1257-7c82-46d5-a9c0-cfc85597d26a" // Не используется
	};
	const sysSettings = {
		LDAPAllowLicenseDistributionDuringLDAPSync: "d84677da-8c01-48a5-987a-5e156b27764b"
	};
	const licType =	{
		Personal: "7cb7fff2-f050-41e0-b277-e2e1674c86a4", // Именная
		Server: "e704b9df-122c-407d-a80d-851f8df82a41" // Серверная
	};
	const appStatus =	{
		New: "ede5b81b-eca2-4b36-82b8-4f8cafa2da47",
		RequestSent: "a17b17c2-6bd1-4355-83c6-1d1c5d12d70e",
		UpdateRequired: "0890be6c-1c3a-4f52-8601-2a9814727e73",
		TlsLoaded: "05fe1f8a-90b9-4dbe-b9db-17aad724f5a8",
		TlsInstalled: "3d2a8bf5-cb58-4ad8-b431-554592e44cb0",
		TlsNotInstalled: "a87289ed-53d9-4a1b-b1e2-c11c9fa243b5"
	};
	return {
		GKILicStatus: licStatus,
		SysSettings: sysSettings,
		GKILicType: licType,
		GKIApplicationStatus: appStatus
	};
});
