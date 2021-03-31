delete from "SysSettingsValue" where "SysSettingsId" in (select "Id" from "SysSettings" where "Code" = 'GKILicensingCredentials');
delete from "SysSettings" where "Code" = 'GKILicensingCredentials';
