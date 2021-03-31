namespace Terrasoft.Configuration
{
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.Collections.Concurrent;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Linq;
	using System.IO;
	using System.Text;
	using System.ServiceModel;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Web;
	using System.Threading.Tasks;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.DB;
	using Terrasoft.Web.Common;
	using Terrasoft.Core.Entities;
	
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class GKILicensingRegularService : BaseService
	{
		private static Dictionary<Guid, int> dictGKIVIPLicenses = new Dictionary<Guid, int>();
		private static DateTime? lastMasterCheckInDateTime;
		private static int? lastMasterCheckInWaitMinutes;
		private static DateTime? lastUnknowStatusDateTime;

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		ResponseFormat = WebMessageFormat.Json)]
		public GKILicUserSyncResult GKIAddLicense(List<LicUserData> userLicensePair)
		{
			try
			{
				SetLastMasterCheckIn();
			}
			catch (Exception)
			{ }
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
			try
			{
				SetLastMasterCheckIn();
			}
			catch (Exception)
			{ }
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
			
			try
			{
				ConcurrentQueue<LicUserSyncResult> syncResultsQueue = new ConcurrentQueue<LicUserSyncResult>();
				Parallel.ForEach(userLicensePair, LicUserData => GKIProcessLicUserData(LicUserData, syncResultsQueue, isAddLicense));
				List<LicUserSyncResult> syncResults = new List<LicUserSyncResult>(syncResultsQueue.ToList());
				licUserSyncResult.LicUserSyncResults = syncResults;
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
			try
			{
				SetLastMasterCheckIn();
			}
			catch (Exception)
			{ }
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
				var sysLicUserESQCollection = sysLicUserESQ.GetEntityCollection(UserConnection);
				List<Entity> sysLicUserESQCollectionList = sysLicUserESQCollection.ToList();

				var sysAdminUnitESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysAdminUnit");
				sysAdminUnitESQ.UseAdminRights = false;
				sysAdminUnitESQ.AddAllSchemaColumns();
				var lastActivityDateTimeColumn = sysAdminUnitESQ.AddColumn(sysAdminUnitESQ.CreateAggregationFunction(AggregationTypeStrict.Max,
					"[SysUserSession:SysUser].SessionStartDate"));
				sysAdminUnitESQ.Filters.Add(sysAdminUnitESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "SysAdminUnitTypeValue", 4)); //users
				var sysAdminUnitESQCollections = sysAdminUnitESQ.GetEntityCollectionIterator(UserConnection);
				usersSyncResult.UserSyncResultSysAdminUnit = new List<UserSyncResultSysAdminUnit>();
				foreach (var sysAdminUnitESQCollection in sysAdminUnitESQCollections)
				{
					ConcurrentQueue<UserSyncResultSysAdminUnit> userQueue = new ConcurrentQueue<UserSyncResultSysAdminUnit>();
					Parallel.ForEach(sysAdminUnitESQCollection, sysAdminUnitEntity =>
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
						userQueue.Enqueue(userSAU);
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
					});
					usersSyncResult.UserSyncResultSysAdminUnit.AddRange(userQueue);
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
			try
			{
				SetLastMasterCheckIn();
			}
			catch (Exception)
			{ }
			return "ok";
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		ResponseFormat = WebMessageFormat.Json)]
		public LicenseInfoResult GKIGetInstalledLicensesInfo()
		{
			try
			{
				SetLastMasterCheckIn();
			}
			catch (Exception)
			{ }
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
			var licTypeColumn = sysLicESQ.AddColumn("SysLicPackage.Operations").Name;
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
					LicType = sysLicEntity.GetTypedColumnValue<string>(licTypeColumn).Contains("IsServerLicPackage") ? 1 : 0,
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
		public PulseData GKIPulse(PulseIncomingData pulseIncomingData)
		{
			try
			{
				SetLastMasterCheckIn();
			}
			catch (Exception)
			{ }
			PulseData pulseData = new PulseData();
			if (!GKIIsSysAdmin())
			{
				pulseData.isSuccess = false;
				pulseData.errorMsg = GKINoAdminRightsError;
				return pulseData;
			}

			try
			{
				AssignVIPLimits(pulseIncomingData.vipLicensesLimits);
			}
			catch (Exception ex)
			{
				pulseData.vipLimitsErrorMsg = ex.Message;
			}

			AssignLastMasterCheckInWaitMinutes(pulseIncomingData.lastMasterCheckInWaitMinutes);

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
			List<string> bufferStrings = new List<string>();
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
			{
				using (IDataReader dr = auditSelect.ExecuteReader(dbExecutor))
				{
					
					while (dr.Read())
					{
						bufferStrings.Add(dr.GetString(0));
					}
				}
			}
			ConcurrentQueue<string> pulseLicUserNames = new ConcurrentQueue<string>();
			Parallel.ForEach(bufferStrings, bufferString =>
			{
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

					if (!pulseLicUserNames.Contains(bufferString))
						pulseLicUserNames.Enqueue(bufferString);
				}
			});
			pulseData.pulseLicUserNames.AddRange(pulseLicUserNames);

			pulseData.isSuccess = true;
			pulseData.errorMsg = String.Empty;
			pulseData.vipLimitsErrorMsg = String.Empty;
			return pulseData;
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
		ResponseFormat = WebMessageFormat.Json)]
		public List<GKIVIPUsersLicensesData> GetUserGKIVIPLicPackages(Guid userId)
		{
			List<GKIVIPUsersLicensesData> listGKIVIPUsersLicensesData = new List<GKIVIPUsersLicensesData>();
			if (GKIIsSysAdmin())
			{
				var dictGKIVIPLicensesUsedCount = GetUsedVIPCount();
				var queryParamListGKIVIPLicensesEnum = GetVIPLicensesFilter();
				var enrolledSysLicPackages = new List<Guid>();

				var listGKIVIPUsersLicensesDataSelect =
					new Select(UserConnection)
					.Column("Id")
					.Column(
						new Select(UserConnection)
						.Column("Name")
						.From("SysLicPackage")
						.Where("Id")
						.IsEqual("GKISysLicPackageId"))
					.Column("GKIisVIP")
					.Column("GKISysLicPackageId")
					.From("GKIVIPUsersLicenses")
					.Where("GKISysAdminUnitId").IsEqual(Column.Parameter(userId))
					.And("GKISysLicPackageId").In(queryParamListGKIVIPLicensesEnum) as Select;
				using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
				{
					using (IDataReader dr = listGKIVIPUsersLicensesDataSelect.ExecuteReader(dbExecutor))
					{
						while (dr.Read())
						{
							int available = dictGKIVIPLicenses[dr.GetGuid(3)] - dictGKIVIPLicensesUsedCount[dr.GetGuid(3)];
							if (!enrolledSysLicPackages.Contains(dr.GetGuid(3)))
							{
								enrolledSysLicPackages.Add(dr.GetGuid(3));
							}
							listGKIVIPUsersLicensesData.Add(new GKIVIPUsersLicensesData
							{
								Id = dr.GetGuid(0),
								Caption = dr.GetString(1),
								Checked = (bool)dr.GetValue(2),
								Enabled = available > 0 || (bool)dr.GetValue(2) ? true : false,
								Available = available,
								Limit = dictGKIVIPLicenses[dr.GetGuid(3)]
							});
						}
					}
				}

				//adding missing records
				IEnumerable<Guid> enrolledSysLicPackagesEnum = enrolledSysLicPackages.AsEnumerable();
				var missingSysLicPackages = dictGKIVIPLicenses.Keys.ToList().Except(enrolledSysLicPackagesEnum);
				if (missingSysLicPackages.Count() > 0)
				{
					var schemaGKIVIPUsersLicenses = UserConnection.EntitySchemaManager.GetInstanceByName("GKIVIPUsersLicenses");
					var schemaSysLicPackage = UserConnection.EntitySchemaManager.GetInstanceByName("SysLicPackage");

					foreach (Guid missingSysLicPackage in missingSysLicPackages)
					{
						Entity entitySysLicPackage = schemaSysLicPackage.CreateEntity(UserConnection);
						entitySysLicPackage.FetchFromDB("Id", missingSysLicPackage);
						string licPackageName = entitySysLicPackage.GetTypedColumnValue<string>("Name");

						var entityGKIVIPUsersLicenses = schemaGKIVIPUsersLicenses.CreateEntity(UserConnection);
						entityGKIVIPUsersLicenses.SetDefColumnValues();
						entityGKIVIPUsersLicenses.SetColumnValue("GKIisVIP", false);
						entityGKIVIPUsersLicenses.SetColumnValue("GKISysAdminUnitId", userId);
						entityGKIVIPUsersLicenses.SetColumnValue("GKISysLicPackageId", missingSysLicPackage);
						entityGKIVIPUsersLicenses.Save();

						int availableMissing = dictGKIVIPLicenses[missingSysLicPackage] - dictGKIVIPLicensesUsedCount[missingSysLicPackage];
						listGKIVIPUsersLicensesData.Add(new GKIVIPUsersLicensesData
						{
							Id = entityGKIVIPUsersLicenses.GetTypedColumnValue<Guid>("Id"),
							Caption = licPackageName,
							Checked = false,
							Enabled = availableMissing > 0 ? true : false,
							Available = availableMissing,
							Limit = dictGKIVIPLicenses[missingSysLicPackage]
						});
					}
				}

				listGKIVIPUsersLicensesData = listGKIVIPUsersLicensesData.OrderBy(x => x.Caption).ToList();
			}
			return listGKIVIPUsersLicensesData;
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
		ResponseFormat = WebMessageFormat.Json)]
		public string SaveGKIVIPChanges(string licenseItems)
		{
			try
			{
				if (!GKIIsSysAdmin())
				{
					throw new Exception(GKINoAdminRightsError);
				}
				Dictionary<string, bool> licenses = Terrasoft.Common.Json.Json.Deserialize<Dictionary<string, bool>>(licenseItems);
				var sb = new StringBuilder();
				foreach (KeyValuePair<string, bool> licItem in licenses)
				{
					var recordId = new Guid(licItem.Key);
					try
					{
						var schemaGKIVIPUsersLicenses = UserConnection.EntitySchemaManager.GetInstanceByName("GKIVIPUsersLicenses");
						Entity entityGKIVIPUsersLicenses = schemaGKIVIPUsersLicenses.CreateEntity(UserConnection);
						if (entityGKIVIPUsersLicenses.FetchFromDB("Id", recordId))
						{
							entityGKIVIPUsersLicenses.SetColumnValue("GKIisVIP", licItem.Value);
							entityGKIVIPUsersLicenses.Save();
						}
					}
					catch (Exception ex)
					{
						sb.Append(ex.Message);
					}
				}
				if (sb.Length > 0)
				{
					return string.Format(
						new LocalizableString(UserConnection.Workspace.ResourceStorage,
							"GKILicensingRegularService",
							"LocalizableStrings.GKIVIPChangesErrors.Value"),
						sb);
				}
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return string.Empty;
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		ResponseFormat = WebMessageFormat.Json)]
		public GKIVIPUsers GKIGetVIPUsers()
		{
			try
			{
				SetLastMasterCheckIn();
			}
			catch (Exception)
			{ }
			GKIVIPUsers vipUsers = new GKIVIPUsers();
			try
			{
				if (!GKIIsSysAdmin())
				{
					throw new Exception(GKINoAdminRightsError);
				}
				vipUsers.vipUsers = new Dictionary<string, List<string>>();
				var esqGKIVIPUsersLicenses = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIVIPUsersLicenses");
				esqGKIVIPUsersLicenses.UseAdminRights = false;
				var sysPackageNameColumn = esqGKIVIPUsersLicenses.AddColumn("GKISysLicPackage.Name");
				var userNameColumn = esqGKIVIPUsersLicenses.AddColumn("GKISysAdminUnit.Name");
				esqGKIVIPUsersLicenses.Filters.Add(
					esqGKIVIPUsersLicenses.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIisVIP", true));
				var esqGKIVIPUsersLicensesCollection = esqGKIVIPUsersLicenses.GetEntityCollection(UserConnection);
				foreach (var entityGKIVIPUsersLicenses in esqGKIVIPUsersLicensesCollection)
				{
					string sysPackageName = entityGKIVIPUsersLicenses.GetTypedColumnValue<string>(sysPackageNameColumn.Name);
					string sysUserName = entityGKIVIPUsersLicenses.GetTypedColumnValue<string>(userNameColumn.Name);
					if (!vipUsers.vipUsers.ContainsKey(sysPackageName) || vipUsers.vipUsers[sysPackageName].IsNullOrEmpty())
					{
						vipUsers.vipUsers[sysPackageName] = new List<string>();
					}
					vipUsers.vipUsers[sysPackageName].Add(sysUserName);
				}

				vipUsers.isSuccess = true;
				vipUsers.errorMsg = String.Empty;
			}
			catch (Exception ex)
			{
				vipUsers.isSuccess = false;
				vipUsers.errorMsg = ex.Message;
				vipUsers.vipUsers = null;
			}
			return vipUsers;
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		ResponseFormat = WebMessageFormat.Json)]
		public bool GKIIsTheSlaveFree()
		{
			try
			{
				if (GKIIsSysAdmin())
				{
					lastMasterCheckInWaitMinutes = lastMasterCheckInWaitMinutes ?? lastMasterCheckInWaitMinutesDefault;
					if (lastMasterCheckInDateTime != null)
                    {
						bool isFree = (DateTime.Now - lastMasterCheckInDateTime.Value).TotalMinutes > lastMasterCheckInWaitMinutes
							? true
							: false;
						return isFree;
					}
					if (lastMasterCheckInDateTime == null && lastUnknowStatusDateTime == null)
					{
						lastUnknowStatusDateTime = DateTime.Now;
						return false;
					}
					if (lastMasterCheckInDateTime == null && lastUnknowStatusDateTime != null)
					{
						bool isFree = (DateTime.Now - lastUnknowStatusDateTime.Value).TotalMinutes > lastMasterCheckInWaitMinutes
							? true
							: false;
						return isFree;
					}
					return false;
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
		ResponseFormat = WebMessageFormat.Json)]
		public GKIDistributeVIPLicensesResult GKIDistributeVIPLicenses(Guid[] userIds, Guid[] packageIds)
		{
			GKIDistributeVIPLicensesResult vipLicensesResult = new GKIDistributeVIPLicensesResult();
			vipLicensesResult.SysLicPackageErrors = new Dictionary<string, int>();
			try
			{
				if (!GKIIsSysAdmin())
				{
					throw new Exception(GKINoAdminRightsError);
				}
				//assess the volume
				var schemaSysLicPackage = UserConnection.EntitySchemaManager.GetInstanceByName("SysLicPackage");
				List<Guid> validPackages = new List<Guid>();
				int countUsers = userIds.Length;
				
				foreach (Guid sysPackage in packageIds)
                {
					int limit = GKIGetVIPLimit(sysPackage);
					int usedCount = GetUsedVIPCount(sysPackage, userIds.ToList());
					if (countUsers > limit - usedCount)
                    {
						Entity entitySysLicPackage = schemaSysLicPackage.CreateEntity(UserConnection);
						entitySysLicPackage.FetchFromDB("Id", sysPackage);
						string packageName = entitySysLicPackage.GetTypedColumnValue<string>("Name");
						vipLicensesResult.SysLicPackageErrors[packageName] = limit - usedCount;
					}
                    else
                    {
						validPackages.Add(sysPackage);
					}
				}

				foreach(Guid licPackage in validPackages)
                {
					//missing records insert
					var insertIntoGKIVIPUsersLicenses = new InsertSelect(UserConnection)
						.Into("GKIVIPUsersLicenses")
						.Set("GKISysAdminUnitId")
						.Set("GKISysLicPackageId")
						.FromSelect(
							new Select(UserConnection)
								.Column("Id")
								.Column(Column.Const(licPackage))
								.From("SysAdminUnit")
								.Where("Id").Not().In(
									new Select(UserConnection)
									.Column("GKISysAdminUnitId")
									.From("GKIVIPUsersLicenses")
									.Where("GKISysLicPackageId")
									.IsEqual(Column.Parameter(licPackage))
									.And("GKISysAdminUnitId").Not().IsNull()
									)
						as Select);
					insertIntoGKIVIPUsersLicenses.Execute();

					//existing records update
					IEnumerable<QueryParameter> usersEnum = userIds.ToList().ConvertAll(x => new QueryParameter(x));

					Update updateGKIVIPUsersLicenses = new Update(UserConnection, "GKIVIPUsersLicenses")
						.Set("GKIisVIP", Column.Parameter(true))
						.Where("GKISysLicPackageId")
						.IsEqual(Column.Parameter(licPackage))
						.And("GKISysAdminUnitId").In(usersEnum)
						.And("GKIisVIP").IsEqual(Column.Parameter(false))
						as Update;
					updateGKIVIPUsersLicenses.Execute();

					
				}
				vipLicensesResult.IsSuccess = true;
			}
			catch (Exception ex)
			{
				vipLicensesResult.IsSuccess = false;
				vipLicensesResult.ErrorMsg = ex.Message;
				return vipLicensesResult;
			}
			return vipLicensesResult;
		}
		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
		ResponseFormat = WebMessageFormat.Json)]
		public string GKIWithdrawVIPLicenses(Guid[] userIds, Guid[] packageIds)
		{
			try
			{
				if (!GKIIsSysAdmin())
				{
					throw new Exception(GKINoAdminRightsError);
				}
				//existing records update
				IEnumerable<QueryParameter> usersEnum = userIds.ToList().ConvertAll(x => new QueryParameter(x));
				IEnumerable<QueryParameter> licPackagesEnum = packageIds.ToList().ConvertAll(x => new QueryParameter(x));

				Update updateGKIVIPUsersLicenses = new Update(UserConnection, "GKIVIPUsersLicenses")
					.Set("GKIisVIP", Column.Parameter(false))
					.Where("GKISysLicPackageId").In(licPackagesEnum)
					.And("GKISysAdminUnitId").In(usersEnum)
					.And("GKIisVIP").IsEqual(Column.Parameter(true))
					as Update;
				updateGKIVIPUsersLicenses.Execute();
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return "ok";
		}
		public int GKIGetVIPLimit(Guid sysLicPackageId)
        {
			int value = dictGKIVIPLicenses.ContainsKey(sysLicPackageId) ? dictGKIVIPLicenses[sysLicPackageId] : 0;
			return value;
		}
		private void AssignLastMasterCheckInWaitMinutes(int lastMasterCheckInWaitMinutesToSet)
        {
			lastMasterCheckInWaitMinutes = lastMasterCheckInWaitMinutesToSet;
		}

		private void SetLastMasterCheckIn()
		{
			lastMasterCheckInDateTime = DateTime.Now;
			lastUnknowStatusDateTime = null;
		}

		private void AssignVIPLimits(Dictionary<string, int> vipLicensesLimits = null)
        {
			if (dictGKIVIPLicenses == null)
            {
				dictGKIVIPLicenses = new Dictionary<Guid, int>();
			}

			if (vipLicensesLimits.IsNullOrEmpty())
            {
				dictGKIVIPLicenses.Clear();
				DeleteAllGKIVIPUsersLicenses();
				return;
            }

			List<Guid> sysLicPackagesFound = new List<Guid>();
			//search SysLicPackages by their names
			foreach (var vipLicenseLimitName in vipLicensesLimits.Keys)
            {
				var schemaSysLicPackage = UserConnection.EntitySchemaManager.GetInstanceByName("SysLicPackage");
				Entity entitySysLicPackage = schemaSysLicPackage.CreateEntity(UserConnection);
				if (entitySysLicPackage.FetchFromDB("Name", vipLicenseLimitName))
				{
					Guid recordId = entitySysLicPackage.GetTypedColumnValue<Guid>("Id");
					sysLicPackagesFound.Add(recordId);
					dictGKIVIPLicenses[recordId] = vipLicensesLimits[vipLicenseLimitName];
				}
			}

			//remove redundant keys
			IEnumerable<Guid> sysLicPackagesFoundEnum = sysLicPackagesFound.AsEnumerable();
			var missingSysLicPackages = dictGKIVIPLicenses.Keys.ToList().Except(sysLicPackagesFoundEnum);
			foreach (var missingKey in missingSysLicPackages.ToList())
			{
				dictGKIVIPLicenses.Remove(missingKey);
			}
			DeleteGKIVIPUsersLicenses(missingSysLicPackages);

			//assign empty rows so usersection filter would work
			var schemaGKIVIPUsersLicenses = UserConnection.EntitySchemaManager.GetInstanceByName("GKIVIPUsersLicenses");
			foreach (var key in dictGKIVIPLicenses.Keys)
            {
				Entity entityGKIVIPUsersLicenses = schemaGKIVIPUsersLicenses.CreateEntity(UserConnection);
				if (!entityGKIVIPUsersLicenses.FetchFromDB("GKISysLicPackage", key))
				{
					entityGKIVIPUsersLicenses.SetDefColumnValues();
					entityGKIVIPUsersLicenses.SetColumnValue("GKISysLicPackageId", key);
					entityGKIVIPUsersLicenses.Save();
				}
			}
		}

		private void DeleteGKIVIPUsersLicenses(IEnumerable<Guid> missingSysLicPackages = null)
        {
			if (missingSysLicPackages != null && missingSysLicPackages.Count() > 0)
			{
				List<QueryParameter> packagesToDelete = new List<QueryParameter>();
				foreach (Guid missingPackage in missingSysLicPackages)
				{
					packagesToDelete.Add(new QueryParameter(missingPackage));
				}
				IEnumerable<QueryParameter> packagesToDeleteEnum = packagesToDelete.AsEnumerable();
				Delete requestRecordsDelete = new Delete(UserConnection)
					.From("GKIVIPUsersLicenses")
					.Where("GKISysLicPackageId")
					.In(packagesToDeleteEnum)
				as Delete;
				requestRecordsDelete.Execute();
			}
		}
		private void DeleteAllGKIVIPUsersLicenses()
		{
			Delete requestRecordsDelete = new Delete(UserConnection)
				.From("GKIVIPUsersLicenses")
			as Delete;
			requestRecordsDelete.Execute();
		}
		public Dictionary<Guid, int> GetUsedVIPCount()
        {
			var dictGKIVIPLicensesUsedCount = new Dictionary<Guid, int>(dictGKIVIPLicenses);
			foreach (var key in dictGKIVIPLicensesUsedCount.Keys.ToList())
			{
				dictGKIVIPLicensesUsedCount[key] = 0;
			}
			var queryParamListGKIVIPLicensesEnum = GetVIPLicensesFilter();
			
			var usedVIPLicensesSelect =
				new Select(UserConnection)
				.Column(Func.Count("Id"))
				.Column("GKISysLicPackageId")
				.From("GKIVIPUsersLicenses")
				.Where("GKISysLicPackageId").In(queryParamListGKIVIPLicensesEnum)
				.And("GKIisVIP").IsEqual(Column.Parameter(true))
				.GroupBy("GKISysLicPackageId") as Select;
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
			{
				using (IDataReader dr = usedVIPLicensesSelect.ExecuteReader(dbExecutor))
				{
					while (dr.Read())
					{
						dictGKIVIPLicensesUsedCount[dr.GetGuid(1)] = dr.GetInt32(0);
					}
				}
			}
			return dictGKIVIPLicensesUsedCount;
		}
		public int GetUsedVIPCount(Guid sysLicPackageId)
		{
			int usedCount = 0;
			var usedVIPLicensesSelect =
				new Select(UserConnection)
				.Column(Func.Count("Id"))
				.Column("GKISysLicPackageId")
				.From("GKIVIPUsersLicenses")
				.Where("GKISysLicPackageId").IsEqual(Column.Parameter(sysLicPackageId))
				.And("GKIisVIP").IsEqual(Column.Parameter(true))
				.GroupBy("GKISysLicPackageId") as Select;
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
			{
				using (IDataReader dr = usedVIPLicensesSelect.ExecuteReader(dbExecutor))
				{
					while (dr.Read())
					{
						usedCount = dr.GetInt32(0);
					}
				}
			}
			return usedCount;
		}
		public int GetUsedVIPCount(Guid sysLicPackageId, List<Guid> sysUserIds)
		{
			int usedCount = 0;
			IEnumerable<QueryParameter> queryParameterUserIds = sysUserIds.ConvertAll(x => new QueryParameter(x));
			var usedVIPLicensesSelect =
				new Select(UserConnection)
				.Column(Func.Count("Id"))
				.Column("GKISysLicPackageId")
				.From("GKIVIPUsersLicenses")
				.Where("GKISysLicPackageId").IsEqual(Column.Parameter(sysLicPackageId))
				.And("GKIisVIP").IsEqual(Column.Parameter(true))
				.And("GKISysAdminUnitId").Not().In(queryParameterUserIds)
				.GroupBy("GKISysLicPackageId") as Select;
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
			{
				using (IDataReader dr = usedVIPLicensesSelect.ExecuteReader(dbExecutor))
				{
					while (dr.Read())
					{
						usedCount = dr.GetInt32(0);
					}
				}
			}
			return usedCount;
		}
		private void GKIProcessLicUserData(LicUserData LicUserData, ConcurrentQueue<LicUserSyncResult> syncResultsQueue, bool isAddLicense)
        {
			string userLogin = LicUserData.LicUserName;
			string licenseName = LicUserData.LicPackageName;
			try
			{
				if (userLogin.IsNullOrWhiteSpace() || licenseName.IsNullOrWhiteSpace())
				{
					syncResultsQueue.Enqueue(new LicUserSyncResult()
					{
						LicUserName = userLogin,
						LicPackageName = licenseName,
						isSuccess = false,
						ErrorMsg = userLogin.IsNullOrWhiteSpace() ? "LicUserName is empty" : "LicPackageName is empty"
					});
					return;
				}

				var sysadminunitSchema = UserConnection.EntitySchemaManager.FindInstanceByName("SysAdminUnit");
				var esqSysadminunit = new EntitySchemaQuery(sysadminunitSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
				esqSysadminunit.AddColumn("Id");
				esqSysadminunit.Filters.Add(esqSysadminunit.CreateFilterWithParameters(FilterComparisonType.Equal, "Name", userLogin));
				var sysadminunitEntities = esqSysadminunit.GetEntityCollection(UserConnection);
				if (sysadminunitEntities.Count != 1)
				{
					syncResultsQueue.Enqueue(new LicUserSyncResult()
					{
						LicUserName = userLogin,
						LicPackageName = licenseName,
						isSuccess = false,
						ErrorMsg = sysadminunitEntities.Count < 1 ? "There are no users with this login" : "There are more than one user with this login"
					});
					return;
				}
				Entity record = sysadminunitEntities.First();
				Guid userIdGuid = record.GetTypedColumnValue<Guid>("Id");

				var licenseString = new Collection<string>();
				licenseString.Add(licenseName);

				bool hasTheLicense = GKIDoesUserHaveTheLicense(userIdGuid, licenseName);

				if (isAddLicense)
				{
					if (!hasTheLicense)
                    {
						var grantedLicenses = UserConnection.LicHelper.AddUserAvailableLicences(userIdGuid, licenseString);
						if (!GKIDoesUserHaveTheLicense(userIdGuid, licenseName))
						{
							syncResultsQueue.Enqueue(new LicUserSyncResult()
							{
								LicUserName = userLogin,
								LicPackageName = licenseName,
								isSuccess = false,
								ErrorMsg = "License was not granted"
							});
							return;
						}
					}
					syncResultsQueue.Enqueue(new LicUserSyncResult()
					{
						LicUserName = userLogin,
						LicPackageName = licenseName,
						isSuccess = true,
						ErrorMsg = ""
					});
				}
				else
				{
					if (hasTheLicense)
					{

						int removedLicenses = GKIRemoveLicensesRequest(userIdGuid, licenseName);
						if (removedLicenses < 1)
						{
							syncResultsQueue.Enqueue(new LicUserSyncResult()
							{
								LicUserName = userLogin,
								LicPackageName = licenseName,
								isSuccess = false,
								ErrorMsg = "License was not removed"
							});
							return;
						}
					}
					syncResultsQueue.Enqueue(new LicUserSyncResult()
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
				syncResultsQueue.Enqueue(new LicUserSyncResult()
				{
					LicUserName = userLogin,
					LicPackageName = licenseName,
					isSuccess = false,
					ErrorMsg = ex.Message
				});
			}
		}

		private bool GKIDoesUserHaveTheLicense(Guid userIdGuid, string licenseName)
        {
			//esq for calculating have the license been removed or granted already
			var sysLicUserESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysLicUser") { PrimaryQueryColumn = { IsAlwaysSelect = true } };
			sysLicUserESQ.UseAdminRights = false;
			sysLicUserESQ.Filters.Add(sysLicUserESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "SysUser", userIdGuid));
			sysLicUserESQ.Filters.Add(sysLicUserESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "SysLicPackage.Name", licenseName));
			var sysLicUserESQCollection = sysLicUserESQ.GetEntityCollection(UserConnection);
			bool hasTheLicense = sysLicUserESQCollection.Count > 0 ? true : false;
			return hasTheLicense;
		}

		private IEnumerable<QueryParameter> GetVIPLicensesFilter()
        {
			IEnumerable<QueryParameter> queryParamListGKIVIPLicensesEnum = dictGKIVIPLicenses.Keys.ToList().ConvertAll(x => new QueryParameter(x));
			return queryParamListGKIVIPLicensesEnum;
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

		public class GKIDistributeVIPLicensesResult
        {
			public bool IsSuccess { get; set; }
			public string ErrorMsg { get; set; }
			public Dictionary<string, int> SysLicPackageErrors { get; set; }
		}

		public class GKIVIPUsers
		{
			public bool isSuccess { get; set; }
			public string errorMsg { get; set; }
			public Dictionary<string, List<string>> vipUsers { get; set; } //product, logins
		}

		public class PulseIncomingData
		{
			public int lastMasterCheckInWaitMinutes { get; set; }
			public Dictionary<string, int> vipLicensesLimits { get; set; }
		}
		public class GKIVIPUsersLicensesData
		{
			public Guid Id { get; set; }
			public string Caption { get; set; }
			public bool Checked { get; set; }
			public bool Enabled { get; set; }
			public int Available { get; set; }
			public int Limit { get; set; }
		}
		public class GKIVIPLicenses
		{
			public Guid SysLicPackageId { get; set; }
			public int Limit { get; set; }
			public int Available { get; set; }
		}

		public class PulseData
		{
			public bool isSuccess { get; set; }
			public string errorMsg { get; set; }
			public string vipLimitsErrorMsg { get; set; }
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
		public static readonly int lastMasterCheckInWaitMinutesDefault = 5;
	}
}