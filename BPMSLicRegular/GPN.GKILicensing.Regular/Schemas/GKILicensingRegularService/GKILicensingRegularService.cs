namespace Terrasoft.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Linq;
	using System.IO;
	using System.Text;
	using System.ServiceModel;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Web;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.DB;
	using Terrasoft.Web.Common;
	using Terrasoft.Core.Entities;


	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class GKILicensingRegularService : BaseService
	{

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		ResponseFormat = WebMessageFormat.Json)]
		public GKILicUserSyncResult GKIAddLicense(List<LicUserData> userLicensePair)
		{
			if (!GKIIsSysAdmin())
			{
				GKILicUserSyncResult errorResult = new GKILicUserSyncResult {
					ErrMsg = GKINoAdminRightsError,
					Success = false,
					LicUserSyncResults = null };
				return errorResult;
			}
			GKILicUserSyncResult licUserSyncResult = GKIAddOrRemoveLicense(userLicensePair, true);
			return licUserSyncResult;
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		ResponseFormat = WebMessageFormat.Json)]
		public GKILicUserSyncResult GKIRemoveLicense(List<LicUserData> userLicensePair)
		{
			if (!GKIIsSysAdmin())
			{
				GKILicUserSyncResult errorResult = new GKILicUserSyncResult
				{
					ErrMsg = GKINoAdminRightsError,
					Success = false,
					LicUserSyncResults = null
				};
				return errorResult;
			}
			GKILicUserSyncResult licUserSyncResult = GKIAddOrRemoveLicense(userLicensePair, false);
			return licUserSyncResult;
		}
		public GKILicUserSyncResult GKIAddOrRemoveLicense(List<LicUserData> userLicensePair, bool isAddLicense)
		{
			GKILicUserSyncResult licUserSyncResult = new GKILicUserSyncResult();
			List<LicUserSyncResult> syncResults = new List<LicUserSyncResult>();
			licUserSyncResult.LicUserSyncResults = syncResults;
			try
			{
				foreach (LicUserData LicUserData in userLicensePair)
				{
					string userLogin = LicUserData.LicUserName;
					string licenseName = LicUserData.LicPackageName;
					try
					{
						if (userLogin.IsNullOrWhiteSpace() || licenseName.IsNullOrWhiteSpace())
						{
							syncResults.Add(new LicUserSyncResult()
							{
								LicUserName = userLogin,
								LicPackageName = licenseName,
								isSuccess = false,
								ErrorMsg = userLogin.IsNullOrWhiteSpace() ? "LicUserName is empty" : "LicPackageName is empty"
							});
							continue;
						}

						var sysadminunitSchema = UserConnection.EntitySchemaManager.FindInstanceByName("SysAdminUnit");
						var esqSysadminunit = new EntitySchemaQuery(sysadminunitSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
						esqSysadminunit.AddColumn("Id");
						esqSysadminunit.Filters.Add(esqSysadminunit.CreateFilterWithParameters(FilterComparisonType.Equal, "Name", userLogin));
						var sysadminunitEntities = esqSysadminunit.GetEntityCollection(UserConnection);
						if (sysadminunitEntities.Count != 1)
						{
							syncResults.Add(new LicUserSyncResult()
							{
								LicUserName = userLogin,
								LicPackageName = licenseName,
								isSuccess = false,
								ErrorMsg = sysadminunitEntities.Count < 1 ? "There are no users with this login" : "There are more than one user with this login"
							});
							continue;
						}
						Entity record = sysadminunitEntities.First();
						Guid userIdGuid = record.GetTypedColumnValue<Guid>("Id");

						var licenseString = new Collection<string>();
						licenseString.Add(licenseName);

						if (isAddLicense) {
							var grantedLicenses = UserConnection.LicHelper.AddUserAvailableLicences(userIdGuid, licenseString);
							if (grantedLicenses == null || grantedLicenses.Count < 1)
							{
								syncResults.Add(new LicUserSyncResult()
								{
									LicUserName = userLogin,
									LicPackageName = licenseName,
									isSuccess = false,
									ErrorMsg = "License was not granted"
								});
								continue;
							}
							else
							{
								syncResults.Add(new LicUserSyncResult()
								{
									LicUserName = userLogin,
									LicPackageName = licenseName,
									isSuccess = true,
									ErrorMsg = ""
								});
							}
						}
						else
						{
							//have license already been removed?
							var sysLicUserESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysLicUser");
							sysLicUserESQ.UseAdminRights = false;
							sysLicUserESQ.AddAllSchemaColumns();
							sysLicUserESQ.Filters.Add(sysLicUserESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "SysUser.Name", userLogin));
							sysLicUserESQ.Filters.Add(sysLicUserESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "SysLicPackage.Name", licenseName));
							var sysLicUserESQCollection = sysLicUserESQ.GetEntityCollection(UserConnection);
							if (sysLicUserESQCollection.Count > 0)
							{

								int removedLicenses = GKIRemoveLicensesRequest(userIdGuid, licenseName);
								if (removedLicenses < 1)
								{
									syncResults.Add(new LicUserSyncResult()
									{
										LicUserName = userLogin,
										LicPackageName = licenseName,
										isSuccess = false,
										ErrorMsg = "License was not removed"
									});
									continue;
								}
							}
							syncResults.Add(new LicUserSyncResult()
							{
								LicUserName = userLogin,
								LicPackageName = licenseName,
								isSuccess = true,
								ErrorMsg = ""
							});
						}
					}
					catch (Exception ex)
					{
						syncResults.Add(new LicUserSyncResult()
						{
							LicUserName = userLogin,
							LicPackageName = licenseName,
							isSuccess = false,
							ErrorMsg = ex.Message
						});
					}
				}
				licUserSyncResult.Success = true;
				licUserSyncResult.ErrMsg = String.Empty;
				return licUserSyncResult;
			}
			catch (Exception ex)
			{
				licUserSyncResult.Success = false;
				licUserSyncResult.ErrMsg = ex.Message;
				return licUserSyncResult;
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
		ResponseFormat = WebMessageFormat.Json)]
		public UsersSyncResult GKIUsersSync()
		{
			if (!GKIIsSysAdmin())
			{
				UsersSyncResult errorResult = new UsersSyncResult
				{
					ErrMsg = GKINoAdminRightsError,
					Success = false,
					UserSyncResultSysAdminUnit = null
				};
				return errorResult;
			}
			UsersSyncResult usersSyncResult = new UsersSyncResult();
			try
			{
				var sysLicPackageESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysLicPackage");
				sysLicPackageESQ.UseAdminRights = false;
				sysLicPackageESQ.AddAllSchemaColumns();
				var sysLicPackageESQCollection = sysLicPackageESQ.GetEntityCollection(UserConnection);

				var sysLicUserESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysLicUser");
				sysLicUserESQ.UseAdminRights = false;
				sysLicUserESQ.AddAllSchemaColumns();
				var SysLicPackageNameClm = sysLicUserESQ.AddColumn("SysLicPackage.Name").Name;
				//TODO: filter by LDAPEntryId is not empty?
				var sysLicUserESQCollection = sysLicUserESQ.GetEntityCollection(UserConnection);
				List<Entity> sysLicUserESQCollectionList = sysLicUserESQCollection.ToList();

				var sysAdminUnitESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysAdminUnit");
				sysAdminUnitESQ.UseAdminRights = false;
				sysAdminUnitESQ.AddAllSchemaColumns();
				var lastActivityDateTimeColumn = sysAdminUnitESQ.AddColumn(sysAdminUnitESQ.CreateAggregationFunction(AggregationTypeStrict.Max,
					"[SysUserSession:SysUser].SessionStartDate"));
				sysAdminUnitESQ.Filters.Add(sysAdminUnitESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "SysAdminUnitTypeValue", 4)); //users
				//TODO: filter by LDAPEntryId is not empty?
				var sysAdminUnitESQCollection = sysAdminUnitESQ.GetEntityCollection(UserConnection);
				usersSyncResult.UserSyncResultSysAdminUnit = new List<UserSyncResultSysAdminUnit>();

				foreach (Entity sysAdminUnitEntity in sysAdminUnitESQCollection)
				{
					UserSyncResultSysAdminUnit userSAU = new UserSyncResultSysAdminUnit();
					userSAU.Active = sysAdminUnitEntity.GetTypedColumnValue<bool>("Active");
					userSAU.Name = sysAdminUnitEntity.GetTypedColumnValue<string>("Name");
					DateTime? lastActivityDateTime = sysAdminUnitEntity.GetTypedColumnValue<DateTime>(lastActivityDateTimeColumn.Name);
					if (lastActivityDateTime != null && lastActivityDateTime > DateTime.MinValue)
						userSAU.LastActivityDateTime = lastActivityDateTime;
					DateTime? registrationDateTime = sysAdminUnitEntity.GetTypedColumnValue<DateTime>("CreatedOn");
					if (registrationDateTime != null && registrationDateTime > DateTime.MinValue)
						userSAU.RegistrationDateTime = registrationDateTime;
					userSAU.UserSyncResultSysLicUser = new List<UserSyncResultSysLicUser>();
					usersSyncResult.UserSyncResultSysAdminUnit.Add(userSAU);
					foreach (Entity sysLicPackage in sysLicPackageESQCollection)
					{
						UserSyncResultSysLicUser userSLU = new UserSyncResultSysLicUser();
						userSLU.SysLicPackageName = sysLicPackage.GetTypedColumnValue<string>("Name");
						userSLU.Active = sysLicUserESQCollectionList.Find(c => (c.GetTypedColumnValue<Guid>("SysUserId") == sysAdminUnitEntity.GetTypedColumnValue<Guid>("Id")) &&
							(c.GetTypedColumnValue<string>(SysLicPackageNameClm) == sysLicPackage.GetTypedColumnValue<string>("Name"))
						) != null ? true : false;
						userSLU.SysUserName = sysAdminUnitEntity.GetTypedColumnValue<string>("Name");
						userSAU.UserSyncResultSysLicUser.Add(userSLU);
					}
				}
				usersSyncResult.Success = true;
				return usersSyncResult;
			}
			catch (Exception ex)
			{
				usersSyncResult.Success = false;
				usersSyncResult.ErrMsg = ex.Message;
				return usersSyncResult;
			}

		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
		ResponseFormat = WebMessageFormat.Json)]
		public string GKIAuthCheck()
		{
			return "ok";
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		ResponseFormat = WebMessageFormat.Json)]
		public LicenseInfoResult GKIGetInstalledLicensesInfo()
		{
			LicenseInfoResult licenseInfoResult = new LicenseInfoResult();
			licenseInfoResult.LicenseInfoList = new List<LicenseInfo>();
			if (!GKIIsSysAdmin())
			{
				licenseInfoResult.ErrMsg = GKINoAdminRightsError;
				licenseInfoResult.Success = false;
				licenseInfoResult.LicenseInfoList = null;
				return licenseInfoResult;
			}
			//Active
			var sysLicESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysLic");
			sysLicESQ.UseAdminRights = false;
			sysLicESQ.AddAllSchemaColumns();
			var SysLicPackageNameColumn = sysLicESQ.AddColumn("SysLicPackage.Name").Name;
			EntitySchemaQuery subEsq = null;
			var countUsedColumn = sysLicESQ.AddColumn("[SysLicUser:SysLicPackage:SysLicPackage].Id", AggregationTypeStrict.Count, out subEsq).Name;
			sysLicESQ.Filters.Add(
				sysLicESQ.CreateFilterWithParameters(FilterComparisonType.LessOrEqual, "StartDate", DateTime.Now));
			sysLicESQ.Filters.Add(
				sysLicESQ.CreateFilterWithParameters(FilterComparisonType.Greater, "DueDate", DateTime.Now));
			var sysLicESQCollection = sysLicESQ.GetEntityCollection(UserConnection);
			foreach (Entity sysLicEntity in sysLicESQCollection)
			{
				licenseInfoResult.LicenseInfoList.Add(new LicenseInfo
				{
					SysLicPackageName = sysLicEntity.GetTypedColumnValue<string>(SysLicPackageNameColumn),
					StartDate = sysLicEntity.GetTypedColumnValue<DateTime>("StartDate"),
					LicType = sysLicEntity.GetTypedColumnValue<int>("LicType"),
					DueDate = sysLicEntity.GetTypedColumnValue<DateTime>("DueDate"),
					Count = sysLicEntity.GetTypedColumnValue<int>("Count"),
					CountUsed = sysLicEntity.GetTypedColumnValue<int>(countUsedColumn),
					Active = true
				});
			}

			//Not Active
			var sysLicInactiveESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysLic");
			sysLicInactiveESQ.UseAdminRights = false;
			sysLicInactiveESQ.AddAllSchemaColumns();
			var sysLicInactiveSysLicPackageNameColumn = sysLicInactiveESQ.AddColumn("SysLicPackage.Name").Name;

			var filterCollection = new EntitySchemaQueryFilterCollection(sysLicInactiveESQ, LogicalOperationStrict.Or);
			var startDatefilter = sysLicInactiveESQ.CreateFilterWithParameters(FilterComparisonType.Greater, "StartDate", DateTime.Now);
			var dueDatefilter = sysLicInactiveESQ.CreateFilterWithParameters(FilterComparisonType.Less, "DueDate", DateTime.Now);
			filterCollection.Add(startDatefilter);
			filterCollection.Add(dueDatefilter);
			sysLicInactiveESQ.Filters.Add(filterCollection);

			var sysLicInactivesysLicESQCollection = sysLicInactiveESQ.GetEntityCollection(UserConnection);
			foreach (Entity sysLicEntity in sysLicInactivesysLicESQCollection)
			{
				licenseInfoResult.LicenseInfoList.Add(new LicenseInfo
				{
					SysLicPackageName = sysLicEntity.GetTypedColumnValue<string>(sysLicInactiveSysLicPackageNameColumn),
					StartDate = sysLicEntity.GetTypedColumnValue<DateTime>("StartDate"),
					LicType = sysLicEntity.GetTypedColumnValue<int>("LicType"),
					DueDate = sysLicEntity.GetTypedColumnValue<DateTime>("DueDate"),
					Count = sysLicEntity.GetTypedColumnValue<int>("Count"),
					CountUsed = 0,
					Active = false
				});
			}
			licenseInfoResult.Success = true;
			licenseInfoResult.ErrMsg = String.Empty;
			return licenseInfoResult;
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		ResponseFormat = WebMessageFormat.Json)]
		public TlsCustomInstallResult GKIInstallLicenses(TlsLicData tlsLicData)
		{
			TlsCustomInstallResult tlsCustomInstallResult = new TlsCustomInstallResult();
			if (!GKIIsSysAdmin())
			{
				tlsCustomInstallResult.errorInfo = GKINoAdminRightsError;
				tlsCustomInstallResult.success = false;
				return tlsCustomInstallResult;
			}
			bool success;
			try
			{
				byte[] msgBytes = Encoding.UTF8.GetBytes(tlsLicData.licData);
				Stream fileStream = new MemoryStream(msgBytes);
				success = UserConnection.AppConnection.LicManager.LoadLicense(fileStream);
			}
			catch (Exception ex)
			{
				tlsCustomInstallResult.success = false;
				tlsCustomInstallResult.errorInfo = ex.Message;
				return tlsCustomInstallResult;
			}
			if (!success)
			{
				tlsCustomInstallResult.success = false;
				tlsCustomInstallResult.errorInfo = "Licenses were not installed";
				return tlsCustomInstallResult;
			}
			try
			{
				var ESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysLic");
				ESQ.UseAdminRights = false;
				ESQ.AddAllSchemaColumns();
				ESQ.Filters.Add(ESQ.CreateFilterWithParameters(FilterComparisonType.LessOrEqual, "StartDate", DateTime.Now));
				ESQ.Filters.Add(ESQ.CreateFilterWithParameters(FilterComparisonType.Greater, "DueDate", DateTime.Now));
				var ESQCollection = ESQ.GetEntityCollection(UserConnection);
				IEnumerable<Guid> sysLicPackageIds = new Guid[] { };
				foreach (Entity sysLic in ESQCollection) {
					if (sysLicPackageIds.Find(x => x == sysLic.GetTypedColumnValue<Guid>("SysLicPackageId")) == Guid.Empty)
					{
						sysLicPackageIds = sysLicPackageIds.Append(sysLic.GetTypedColumnValue<Guid>("SysLicPackageId"));
					}
				}
				UserConnection.AppConnection.LicManager.AddUsersAvailableLicences(UserConnection, new[] { UserConnection.CurrentUser.Id }, sysLicPackageIds);
			}
			catch (Exception ex)
			{
				tlsCustomInstallResult.success = true;
				tlsCustomInstallResult.errorInfo = ex.Message;
				return tlsCustomInstallResult;
			}

			tlsCustomInstallResult.success = true;
			tlsCustomInstallResult.errorInfo = String.Empty;
			return tlsCustomInstallResult;
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		ResponseFormat = WebMessageFormat.Json)]
		public PulseData GKIPulse()
		{
			PulseData pulseData = new PulseData();
			if (!GKIIsSysAdmin())
			{
				pulseData.isSuccess = false;
				pulseData.errorMsg = GKINoAdminRightsError;
				return pulseData;
			}

			//outgoing list
			pulseData.pulseLicUserNames = new List<string>();

			string pulseAuditLogDescBeginningRu = new LocalizableString(UserConnection.Workspace.ResourceStorage,
						"GKILicensingRegularService",
						"LocalizableStrings.PulseAuditLogDescBeginningRu.Value");
			string pulseAuditLogDescEndingRu = new LocalizableString(UserConnection.Workspace.ResourceStorage,
						"GKILicensingRegularService",
						"LocalizableStrings.PulseAuditLogDescEndingRu.Value");
			string pulseAuditLogDescRu = new LocalizableString(UserConnection.Workspace.ResourceStorage,
						"GKILicensingRegularService",
						"LocalizableStrings.PulseAuditLogDescRu.Value");
			
			DateTime searchFromDateTime = Terrasoft.Core.Configuration.SysSettings.GetValue(UserConnection, "GKILicensingLastPulseDateTime", DateTime.MinValue);
			Terrasoft.Core.Configuration.SysSettings.SetDefValue(UserConnection, "GKILicensingLastPulseDateTime", DateTime.UtcNow);
			//audit log select
			var auditSelect =
				new Select(UserConnection)
					.Column("Description")
				.From("SysOperationAudit")
				.Where("ResultId").IsEqual(Column.Parameter(sysOperationResultAuthorizationDeniedId))
				.And("TypeId").IsEqual(Column.Parameter(sysOperationTypeUserAuthorizationId))
				.And("OwnerId").IsNull()
				.And("CreatedOn").IsGreaterOrEqual(Column.Parameter(searchFromDateTime))
				.And().OpenBlock("Description").IsLike(Column.Parameter(pulseAuditLogDescEn))
				.Or("Description").IsLike(Column.Parameter(pulseAuditLogDescRu)).CloseBlock()
				as Select;
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
			{
				using (IDataReader dr = auditSelect.ExecuteReader(dbExecutor))
				{
					string bufferString;
					while (dr.Read())
					{
						bufferString = dr.GetString(0);
						if (bufferString.StartsWith(pulseAuditLogDescBeginningEn) || bufferString.StartsWith(pulseAuditLogDescBeginningRu))
						{
							if (bufferString.StartsWith(pulseAuditLogDescBeginningEn))
							{
								bufferString = bufferString.Substring(pulseAuditLogDescBeginningEn.Length,
									bufferString.IndexOf(pulseAuditLogDescEndingEn) - (pulseAuditLogDescBeginningEn.Length));
							}
							if (bufferString.StartsWith(pulseAuditLogDescBeginningRu))
							{
								bufferString = bufferString.Substring(pulseAuditLogDescBeginningRu.Length,
									bufferString.IndexOf(pulseAuditLogDescEndingRu) - (pulseAuditLogDescBeginningRu.Length));
							}

							if (!pulseData.pulseLicUserNames.Contains(bufferString))
								pulseData.pulseLicUserNames.Add(bufferString);
						}
					}
				}
			}

			pulseData.isSuccess = true;
			pulseData.errorMsg = String.Empty;
			return pulseData;
		}

		private int GKIRemoveLicensesRequest(Guid userIdGuid, string licenseName)
		{
			var delete = new Delete(UserConnection).From("SysLicUser")
				.Where("SysUserId").IsEqual(Column.Parameter(userIdGuid))
				.And("SysLicPackageId").In(
					new Select(UserConnection).Column("Id")
					.From("SysLicPackage")
					.Where("Name")
					.IsEqual(Column.Parameter(licenseName)));
			var deleteResult = delete.Execute();
			return deleteResult;
		}

		private List<string> GetMissingLicenses(IEnumerable<string> grantedLicenses, IEnumerable<string> licenses)
		{
			return licenses.Except(grantedLicenses).ToList();
		}
		
		private List<string> GetGrantedLicenseNames(Collection<Guid> licenses)
		{
			var licenseNames = new List<string>();
			if (licenses.IsEmpty())
			{
				return licenseNames;
			}
			IEnumerable<QueryParameter> uids =
				licenses.Select(id => new QueryParameter(id)).ToList();
			var select =
				new Select(UserConnection)
					.Column("SysLicPackage", "Name")
				.From("SysLic")
				.InnerJoin("SysLicPackage").On("SysLic", "SysLicPackageId")
					.IsEqual("SysLicPackage", "Id")
				.Where("SysLicPackage", "Id").In(uids)
				.And("SysLicPackage", "Operations")
					.Not().IsLike(Column.Parameter("%IsServerLicPackage%"))
				.OrderByAsc("SysLicPackage", "Name")
				as Select;
			select.ExecuteReader(reader => {
				var licenseName = reader.GetColumnValue<string>("Name");
				licenseNames.Add(licenseName);
			});
			return licenseNames;
		}

		private bool GKIIsSysAdmin()
		{
			var userId = UserConnection.CurrentUser.Id;
			if (userId == null || userId == Guid.Empty)
			{
				return false;
			}
			var sysAdminid = BaseConsts.SystemAdministratorsSysAdminUnitId;
			var schema = UserConnection.EntitySchemaManager.GetInstanceByName("SysUserInRole");
			var entity = schema.CreateEntity(UserConnection);
			entity.UseAdminRights = false;
			var sysCondition = new Dictionary<string, object> {
				{ "SysUser", userId },
				{ "SysRole", sysAdminid }
			};
			return entity.ExistInDB(sysCondition);
		}

		public class PulseData
		{
			public bool isSuccess { get; set; }
			public string errorMsg { get; set; }
			public List<string> pulseLicUserNames { get; set; } = null;
		}

		public class TlrLicData
		{
			public string licData { get; set; }
			public string errorInfo { get; set; }
			public bool success { get; set; }
		}

		public class TlsCustomInstallResult
		{
			public string errorInfo { get; set; }
			public bool success { get; set; }
		}
		public class TlsLicData
		{
			public string licData { get; set; }
		}

		public class LicenseInfoResult
		{
			public bool Success { get; set; }
			public string ErrMsg { get; set; }
			public List<LicenseInfo> LicenseInfoList { get; set; }
		}
		public class LicenseInfo
		{
			public string SysLicPackageName { get; set; }
			public DateTime StartDate { get; set; }
			public int LicType { get; set; }
			public DateTime DueDate { get; set; }
			public int Count { get; set; }
			public int CountUsed { get; set; }
			public bool Active { get; set; }
		}
		public class LicUserSyncResult
		{
			public string LicUserName { get; set; }
			public string LicPackageName { get; set; }
			public bool isSuccess { get; set; }
			public string ErrorMsg { get; set; }
		}
		public class GKILicUserSyncResult
		{
			public string ErrMsg { get; set; }
			public bool Success { get; set; }
			public List<LicUserSyncResult> LicUserSyncResults { get; set; }
		}
		public class LicUserData
		{
			public string LicUserName { get; set; }
			public string LicPackageName { get; set; }
		}
		public class UserSyncResultSysAdminUnit
		{
			public string Name { get; set; }
			public bool Active { get; set; }
			public DateTime? LastActivityDateTime { get; set; }
			public DateTime? RegistrationDateTime { get; set; }
			public List<UserSyncResultSysLicUser> UserSyncResultSysLicUser { get; set; }
		}
		public class UserSyncResultSysLicUser
		{
			public string SysUserName { get; set; }
			public bool Active { get; set; }
			public string SysLicPackageName { get; set; }
		}
		public class UsersSyncResult
		{
			public List<UserSyncResultSysAdminUnit> UserSyncResultSysAdminUnit { get; set; }
			public bool Success { get; set; }
			public string ErrMsg { get; set; }
		}

		public static readonly string GKITlsInstallRegularServiceUrl = "/0/ServiceModel/LicenseService.svc/UploadLicenses";
		public static readonly string GKINoAdminRightsError = "Provided user has no System administrator rights. Operation has been aborted";
		public static readonly Guid sysOperationTypeUserAuthorizationId = new Guid(("1c128638-0dbc-45f7-a6d2-77600296b854").ToUpper());
		public static readonly Guid sysOperationResultAuthorizationDeniedId = new Guid(("247ad3fe-c5e9-4ee9-b3d6-b1ec2ed12fa2").ToUpper());
		public static readonly string pulseAuditLogDescEn = "%license is missing or expired%";
		public static readonly string pulseAuditLogDescBeginningEn = "Authorization denied for user ";
		public static readonly string pulseAuditLogDescEndingEn = ". IP Address";
	}
}