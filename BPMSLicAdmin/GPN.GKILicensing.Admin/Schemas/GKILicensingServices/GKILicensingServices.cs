namespace Terrasoft.Configuration
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Net;
	using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Text;
	using Terrasoft.Web.Http.Abstractions;
	using Terrasoft.Web.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.Entities;
	using System.Collections.Generic;
	using Terrasoft.Core.DB;
	using Terrasoft.Common;
	using System.Xml.Serialization;

	#region Service
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class GKILicensingAdminService : BaseService
	{
		#region LicenseSync
		public void GKIGoMakeThemChangeLicenseFilterMethod(bool isAddLicense, List<Guid> licPackageIds, List<Guid> instanceIds, List<Guid> licUserIds)
		{

			if ((licPackageIds != null && licPackageIds.Count == 0) ||
				(instanceIds != null && instanceIds.Count == 0) ||
				(licUserIds != null && licUserIds.Count == 0))
			{
				//what? - none = return, where? - nowhere = return, whom? - noone = return.
				//TODO: reminder
				return;
			}

			GKIUserConnectionCheck();

			var rootSchema = userConnection.EntitySchemaManager.FindInstanceByName("GKILicUserInstanceLicPackage");
			var esqEnhanced = new EntitySchemaQuery(rootSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
			string GKIUrlClm = esqEnhanced.AddColumn("GKIInstance.GKIUrl").Name;
			string GKILicUserNameClm = esqEnhanced.AddColumn("GKILicUser.GKIName").Name;
			string GKILicPackageNameClm = esqEnhanced.AddColumn("GKILicPackage.GKIName").Name;
			if (licPackageIds != null && licPackageIds.Count > 0)
			{
				esqEnhanced.Filters.Add(
					esqEnhanced.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage", licPackageIds.Cast<object>().ToArray()));
			}
			if (instanceIds != null && instanceIds.Count > 0)
			{
				esqEnhanced.Filters.Add(
					esqEnhanced.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceIds.Cast<object>().ToArray()));
			}
			if (licUserIds != null && licUserIds.Count > 0)
			{
				esqEnhanced.Filters.Add(
					esqEnhanced.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser", licUserIds.Cast<object>().ToArray()));
			}
			var enhancedCollection = esqEnhanced.GetEntityCollection(userConnection);
			GKIMakeThemChangeLicense(isAddLicense, enhancedCollection, GKIUrlClm, GKILicUserNameClm, GKILicPackageNameClm);
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
		ResponseFormat = WebMessageFormat.Json)]
		public bool GKILicUserSyncAll()
		{
			GKIGoMakeThemChangeLicenseQueueFilterMethod(null, null, null);
			return true;
		}
		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
		ResponseFormat = WebMessageFormat.Json)]
		public bool GKIGoMakeThemChangeLicenseQueueFilterMethod(List<Guid> licPackageIds, List<Guid> instanceIds, List<Guid> licUserIds)
		{

			if ((licPackageIds != null && licPackageIds.Count == 0) ||
				(instanceIds != null && instanceIds.Count == 0) ||
				(licUserIds != null && licUserIds.Count == 0))
			{
				//what? - none = return, where? - nowhere = return, whom? - noone = return.
				//TODO: reminder
				return true;
			}

			GKIUserConnectionCheck();
			
			var rootSchema = userConnection.EntitySchemaManager.FindInstanceByName("GKILicUserInstanceLicPackage");

			//GKIActive == true
			var esqEnhancedIsAdd = new EntitySchemaQuery(rootSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
			string GKIUrlClmIsAdd = esqEnhancedIsAdd.AddColumn("GKIInstance.GKIUrl").Name;
			string GKILicUserNameClmIsAdd = esqEnhancedIsAdd.AddColumn("GKILicUser.GKIName").Name;
			string GKILicPackageNameClmIsAdd = esqEnhancedIsAdd.AddColumn("GKILicPackage.GKIName").Name;
			if (licPackageIds != null && licPackageIds.Count > 0)
			{
				esqEnhancedIsAdd.Filters.Add(
					esqEnhancedIsAdd.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage", licPackageIds.Cast<object>().ToArray()));
			}
			if (instanceIds != null && instanceIds.Count > 0)
			{
				esqEnhancedIsAdd.Filters.Add(
					esqEnhancedIsAdd.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceIds.Cast<object>().ToArray()));
			}
			if (licUserIds != null && licUserIds.Count > 0)
			{
				esqEnhancedIsAdd.Filters.Add(
					esqEnhancedIsAdd.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser", licUserIds.Cast<object>().ToArray()));
			}
			esqEnhancedIsAdd.Filters.Add(
					esqEnhancedIsAdd.CreateExistsFilter("[GKILicUserInstanceLicPackageQueue:GKILicUserInstanceLicPackage:Id].Id"));
			esqEnhancedIsAdd.Filters.Add(
					esqEnhancedIsAdd.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIActive", true));
			var enhancedIsAddCollection = esqEnhancedIsAdd.GetEntityCollection(userConnection);

			//GKIActive == false
			var esqEnhancedIsRemove = new EntitySchemaQuery(rootSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
			string GKIUrlClmIsRemove = esqEnhancedIsRemove.AddColumn("GKIInstance.GKIUrl").Name;
			string GKILicUserNameClmIsRemove = esqEnhancedIsRemove.AddColumn("GKILicUser.GKIName").Name;
			string GKILicPackageNameClmIsRemove = esqEnhancedIsRemove.AddColumn("GKILicPackage.GKIName").Name;
			if (licPackageIds != null && licPackageIds.Count > 0)
			{
				esqEnhancedIsRemove.Filters.Add(
					esqEnhancedIsRemove.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage", licPackageIds.Cast<object>().ToArray()));
			}
			if (instanceIds != null && instanceIds.Count > 0)
			{
				esqEnhancedIsRemove.Filters.Add(
					esqEnhancedIsRemove.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceIds.Cast<object>().ToArray()));
			}
			if (licUserIds != null && licUserIds.Count > 0)
			{
				esqEnhancedIsRemove.Filters.Add(
					esqEnhancedIsRemove.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser", licUserIds.Cast<object>().ToArray()));
			}
			esqEnhancedIsRemove.Filters.Add(
					esqEnhancedIsRemove.CreateExistsFilter("[GKILicUserInstanceLicPackageQueue:GKILicUserInstanceLicPackage:Id].Id"));
			esqEnhancedIsRemove.Filters.Add(
					esqEnhancedIsRemove.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIActive", false));
			var enhancedIsRemoveCollection = esqEnhancedIsRemove.GetEntityCollection(userConnection);
			
			if (enhancedIsAddCollection.Count + enhancedIsRemoveCollection.Count == 0)
            {
				//TODO: reminder
				return true;
			}

			GKIMakeThemChangeLicense(true, enhancedIsAddCollection, GKIUrlClmIsAdd, GKILicUserNameClmIsAdd, GKILicPackageNameClmIsAdd);
			GKIMakeThemChangeLicense(false, enhancedIsRemoveCollection, GKIUrlClmIsRemove, GKILicUserNameClmIsRemove, GKILicPackageNameClmIsRemove);

			return true;
		}

		public void GKIMakeThemChangeLicense(bool isAddLicense, EntityCollection enhancedCollection, string GKIUrlClm, string GKILicUserNameClm, string GKILicPackageNameClm)
		{
			
			GKIUserConnectionCheck();

			List<InstanceSyncData> instancesSyncData = new List<InstanceSyncData>();

			foreach (Entity enhancedEntity in enhancedCollection) {
				string whosEmpty = enhancedEntity.GetTypedColumnValue<string>(GKIUrlClm) == null ? "Url" :
						enhancedEntity.GetTypedColumnValue<string>(GKILicUserNameClm) == null ? "UserName" :
						enhancedEntity.GetTypedColumnValue<string>(GKILicPackageNameClm) == null ? "LicPackageName" : String.Empty;
				if (whosEmpty != String.Empty) {
					GKIWriteLicUserInstanceLicPackageError(enhancedEntity.GetTypedColumnValue<Guid>("Id"), whosEmpty + " is empty");
					continue;
				}
				string baseUrl = enhancedEntity.GetTypedColumnValue<string>(GKIUrlClm);
				InstanceSyncData curInstance = new InstanceSyncData();
				curInstance = instancesSyncData.Find(item => item.BaseUrl == baseUrl);
				if (curInstance == null)
                {
					curInstance = new InstanceSyncData();
					curInstance.BaseUrl = baseUrl;
					instancesSyncData.Add(curInstance);
				}
				
				LicUserData userLicensePair = new LicUserData
				{
					LicUserName = enhancedEntity.GetTypedColumnValue<string>(GKILicUserNameClm),
					LicPackageName = enhancedEntity.GetTypedColumnValue<string>(GKILicPackageNameClm)
				};
				if (curInstance.LicUserDataList == null)
                {
					curInstance.LicUserDataList = new List<LicUserData>();
				}
				curInstance.LicUserDataList.Add(userLicensePair);

			}
			foreach (InstanceSyncData instanceSyncData in instancesSyncData)
			{
				string message = JsonConvert.SerializeObject(instanceSyncData.LicUserDataList);
				string serviceUrl = isAddLicense ?
						string.Concat(instanceSyncData.BaseUrl, GKIAddLicenseServiceUrl) :
						string.Concat(instanceSyncData.BaseUrl, GKIRemoveLicenseServiceUrl);
				try
				{
					string userMessage = GKILicenseAssignmentRequest(message, instanceSyncData.BaseUrl, serviceUrl, isAddLicense, instanceSyncData);
					//TODO: reminder
					continue;
				}
				catch (Exception ex)
				{
					//TODO: reminder ex.Message
					continue;
				}
			}
			GKIGetInstallLicensesInfoAll();
		}
		private string GKILicenseAssignmentRequest(string message, string baseUrl, string serviceUrl, bool isAddLicense, InstanceSyncData instanceSyncData)
		{
			string userMessage = "";
			try
			{
				var msgResponseText = GKILicenseHttpRequest(baseUrl, serviceUrl, message);
				GKILicUserSyncResult GKILicUserSyncResult = JsonConvert.DeserializeObject<GKILicUserSyncResult>(msgResponseText);
				List<LicUserSyncResult> syncResults = GKILicUserSyncResult.LicUserSyncResults;
				if (syncResults != null)
				{
					List<LicUserSyncResult> trueSyncResults = syncResults.FindAll(item => item.isSuccess == true);
					List<LicUserSyncResult> errorSyncResults = syncResults.FindAll(item => item.isSuccess == false);

					userMessage = String.Concat(
						trueSyncResults.Count.ToString(),
						" licenses were ", isAddLicense ? "granted. " : "removed. ",
						errorSyncResults.Count.ToString(), " user sync errors. ");

					userMessage = String.Concat(instanceSyncData.BaseUrl, ": ", userMessage);
					userMessage = String.Concat(userMessage, GKILicUserSyncResult.Success == true ? "" : "Critical error occured! " + GKILicUserSyncResult.ErrMsg);

					IEnumerable<string> licPackageNames = syncResults.Select(x => x.LicPackageName).Distinct();
					IEnumerable<string> errorNames = errorSyncResults.Select(x => x.ErrorMsg).Distinct();
					
					foreach (string licPackageName in licPackageNames) {
						
						//update valid results
						List<QueryParameter> syncedUserNames = new List<QueryParameter>();
						foreach (LicUserSyncResult trueSyncResult in trueSyncResults)
						{
							if (trueSyncResult.LicPackageName == licPackageName)
							{
								syncedUserNames.Add(new QueryParameter(trueSyncResult.LicUserName));
							}
						}
						IEnumerable<QueryParameter> syncedUserNamesEnum = syncedUserNames.AsEnumerable();
						if (syncedUserNamesEnum.Count() > 0)
						{
							Update syncedUserActivenessUpdate = new Update(userConnection, "GKILicUserInstanceLicPackage")
								.Set("GKISyncedState", Column.Parameter(isAddLicense))
								.Set("GKISyncError", Column.Parameter(String.Empty))
								.Set("GKILastSyncDateTime", Column.Parameter(DateTime.Now))
								.Where("GKIInstanceId").In(new Select(userConnection).Column("Id").From("GKIInstance").Where("GKIUrl").IsEqual(Column.Parameter(instanceSyncData.BaseUrl)))
								.And("GKILicPackageId").In(new Select(userConnection).Column("Id").From("GKILicPackage").Where("GKIName").IsEqual(Column.Parameter(licPackageName)))
								.And("GKILicUserId").In(new Select(userConnection).Column("Id").From("GKILicUser").Where("GKIName").In(syncedUserNamesEnum))
							as Update;
							syncedUserActivenessUpdate.Execute();

							Delete syncedUserQueueDelete = new Delete(userConnection)
								.From("GKILicUserInstanceLicPackageQueue")
								.Where("GKILicUserInstanceLicPackageId")
								.In(new
									Select(userConnection)
									.Column("Id")
									.From("GKILicUserInstanceLicPackage")
									.Where("GKILicUserId").In(new Select(userConnection).Column("Id").From("GKILicUser").Where("GKIName").In(syncedUserNamesEnum)))
							as Delete;
							syncedUserQueueDelete.Execute();
						}

						//update errors
						foreach (string errorName in errorNames)
						{
							List<QueryParameter> syncedErrorUserNames = new List<QueryParameter>();
							foreach (LicUserSyncResult errorSyncResult in errorSyncResults)
							{
								if (errorSyncResult.LicPackageName == licPackageName && errorSyncResult.ErrorMsg == errorName)
								{
									syncedErrorUserNames.Add(new QueryParameter(errorSyncResult.LicUserName));
								}
							}
							IEnumerable<QueryParameter> syncedErrorUserNamesEnum = syncedErrorUserNames.AsEnumerable();
							if (syncedErrorUserNamesEnum.Count() > 0)
							{
								Update syncedUserErrorsUpdate = new Update(userConnection, "GKILicUserInstanceLicPackage")
									.Set("GKILastErrorDateTime", Column.Parameter(DateTime.Now))
									.Set("GKISyncError", Column.Parameter(errorName))
									.Where("GKIInstanceId").In(new Select(userConnection).Column("Id").From("GKIInstance").Where("GKIUrl").IsEqual(Column.Parameter(instanceSyncData.BaseUrl)))
									.And("GKILicPackageId").In(new Select(userConnection).Column("Id").From("GKILicPackage").Where("GKIName").IsEqual(Column.Parameter(licPackageName)))
									.And("GKILicUserId").In(new Select(userConnection).Column("Id").From("GKILicUser").Where("GKIName").In(syncedErrorUserNamesEnum))
								as Update;
								syncedUserErrorsUpdate.Execute();
							}
						}
					}

					//instance sync result
					Update instanceUpdate = new Update(userConnection, "GKIInstance")
						.Set("GKILastSyncSuccess", Column.Parameter(GKILicUserSyncResult.Success))
						.Set("GKILastSyncError", Column.Parameter(GKILicUserSyncResult.Success == true ? String.Empty : GKILicUserSyncResult.ErrMsg))
						.Where("GKIUrl").IsEqual(Column.Parameter(instanceSyncData.BaseUrl))
					as Update;
					instanceUpdate.Execute();
				}
				else
				{
					

					string errorMsg = GKILicUserSyncResult.Success == true ? "Instance has not returned any user" : GKILicUserSyncResult.ErrMsg;
					//instance sync result
					Update instanceUpdate = new Update(userConnection, "GKIInstance")
						.Set("GKILastSyncSuccess", Column.Parameter(GKILicUserSyncResult.Success))
						.Set("GKILastSyncError", Column.Parameter(errorMsg))
						.Where("GKIUrl").IsEqual(Column.Parameter(instanceSyncData.BaseUrl))
					as Update;
					instanceUpdate.Execute();

					IEnumerable<string> licPackageNames = instanceSyncData.LicUserDataList.Select(x => x.LicPackageName).Distinct();
					foreach (string licPackageName in licPackageNames)
					{
						List<QueryParameter> syncedErrorUserNames = new List<QueryParameter>();
						foreach (LicUserData errorSyncResult in instanceSyncData.LicUserDataList)
						{
							if (errorSyncResult.LicPackageName == licPackageName)
							{
								syncedErrorUserNames.Add(new QueryParameter(errorSyncResult.LicUserName));
							}
						}
						IEnumerable<QueryParameter> syncedErrorUserNamesEnum = syncedErrorUserNames.AsEnumerable();
						if (syncedErrorUserNamesEnum.Count() > 0)
						{
							Update syncedUserErrorsUpdate = new Update(userConnection, "GKILicUserInstanceLicPackage")
								.Set("GKILastSyncDateTime", Column.Parameter(DateTime.MinValue))
								.Set("GKILastErrorDateTime", Column.Parameter(DateTime.Now))
								.Set("GKISyncError", Column.Parameter(errorMsg))
								.Where("GKIInstanceId").In(new Select(userConnection).Column("Id").From("GKIInstance").Where("GKIUrl").IsEqual(Column.Parameter(instanceSyncData.BaseUrl)))
								.And("GKILicPackageId").In(new Select(userConnection).Column("Id").From("GKILicPackage").Where("GKIName").IsEqual(Column.Parameter(licPackageName)))
								.And("GKILicUserId").In(new Select(userConnection).Column("Id").From("GKILicUser").Where("GKIName").In(syncedErrorUserNamesEnum))
							as Update;
							syncedUserErrorsUpdate.Execute();
						}
					}

					userMessage = "Instance has not returned any user. ";
					userMessage = String.Concat(instanceSyncData.BaseUrl, ": ", userMessage);
					userMessage = String.Concat(userMessage, GKILicUserSyncResult.Success == true ? "" : "Critical error occured! " + GKILicUserSyncResult.ErrMsg);
				}
			}
			catch(Exception ex)
			{
				//instance sync result
				Update instanceUpdate = new Update(userConnection, "GKIInstance")
					.Set("GKILastSyncSuccess", Column.Parameter(false))
					.Set("GKILastSyncError", Column.Parameter(ex.Message))
					.Where("GKIUrl").IsEqual(Column.Parameter(instanceSyncData.BaseUrl))
				as Update;
				instanceUpdate.Execute();

				List<QueryParameter> syncedErrorUserNames = new List<QueryParameter>();
				foreach (LicUserData errorSyncResult in instanceSyncData.LicUserDataList)
				{
					syncedErrorUserNames.Add(new QueryParameter(errorSyncResult.LicUserName));
				}
				IEnumerable<QueryParameter> syncedErrorUserNamesEnum = syncedErrorUserNames.AsEnumerable();
				if (syncedErrorUserNamesEnum.Count() > 0)
				{
					Update syncedUserErrorsUpdate = new Update(userConnection, "GKILicUserInstanceLicPackage")
						.Set("GKILastSyncDateTime", Column.Parameter(DateTime.MinValue))
						.Set("GKILastErrorDateTime", Column.Parameter(DateTime.Now))
						.Set("GKISyncError", Column.Parameter(ex.Message))
						.Where("GKIInstanceId").In(new Select(userConnection).Column("Id").From("GKIInstance").Where("GKIUrl").IsEqual(Column.Parameter(instanceSyncData.BaseUrl)))
						.And("GKILicUserId").In(new Select(userConnection).Column("Id").From("GKILicUser").Where("GKIName").In(syncedErrorUserNamesEnum))
					as Update;
					syncedUserErrorsUpdate.Execute();
				}

				userMessage = ex.Message;
			}
			return userMessage;
		}
		#endregion

		#region UsersSync

		public string GKILicenseSyncRegularByUrl(string baseUrl)
		{
			GKIUserConnectionCheck();

			var GKIInstanceESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKIInstance");
			GKIInstanceESQ.UseAdminRights = false;
			GKIInstanceESQ.AddAllSchemaColumns();
			GKIInstanceESQ.Filters.Add(
				GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIUrl", baseUrl));
			var GKIInstanceESQCollection = GKIInstanceESQ.GetEntityCollection(userConnection);
			if (GKIInstanceESQCollection.Count > 0)
			{
				Entity GKIInstanceEntity = GKIInstanceESQCollection.First();
				return GKILicenseSyncRegular(GKIInstanceEntity.GetTypedColumnValue<Guid>("Id"));
			}
			else
			{
				return "Empty url";
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
		ResponseFormat = WebMessageFormat.Json)]
		public string GKILicenseSyncRegular(Guid instanceIdFilter)
		{
			GKIUserConnectionCheck();
			
			List<LicInstanceSyncResult> licInstanceSyncResults = new List<LicInstanceSyncResult>();

			var instanceSchema = userConnection.EntitySchemaManager.FindInstanceByName("GKIInstance");
			var esqInstance = new EntitySchemaQuery(instanceSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
			esqInstance.AddAllSchemaColumns();
			if (instanceIdFilter != Guid.Empty)
            {
				esqInstance.Filters.Add(
					esqInstance.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", instanceIdFilter));
			}
			var instantEntities = esqInstance.GetEntityCollection(userConnection);

			foreach (Entity instance in instantEntities)
			{
				string baseUrl = instance.GetTypedColumnValue<string>("GKIUrl");
				string serviceUrl = String.Concat(baseUrl, GKIUsersSyncServiceUrl);
				Guid instanceId = instance.GetTypedColumnValue<Guid>("Id");

				//url format validation:
				Uri uriResult;
				bool uriCheckResult = Uri.TryCreate(baseUrl, UriKind.Absolute, out uriResult)
					&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
				if (baseUrl == String.Empty || !uriCheckResult)
				{
					licInstanceSyncResults.Add(new LicInstanceSyncResult()
					{
						InstanceId = instanceId,
						Success = false,
						ErrMsg = "Invalid or empty URL"
					});
					continue;
				}

				try
				{
					string syncResult = GKILicenseSyncRegularRequest(baseUrl, serviceUrl, instanceId);
					licInstanceSyncResults.Add(new LicInstanceSyncResult()
					{
						InstanceId = instanceId,
						Success = syncResult == String.Empty ? true : false,
						ErrMsg = syncResult
					});
					continue;
				}
				catch (Exception ex)
				{
					licInstanceSyncResults.Add(new LicInstanceSyncResult()
					{
						InstanceId = instance.GetTypedColumnValue<Guid>("Id"),
						Success = false,
						ErrMsg = ex.Message
					});
					continue;
				}
			}
			foreach (LicInstanceSyncResult licInstanceSyncResult in licInstanceSyncResults)
			{
				//instance sync result
				Update instanceUpdate = new Update(userConnection, "GKIInstance")
					.Set("GKILastSyncSuccess", Column.Parameter(licInstanceSyncResult.Success))
					.Set("GKILastSyncError", Column.Parameter(licInstanceSyncResult.ErrMsg))
					.Where("Id").IsEqual(Column.Parameter(licInstanceSyncResult.InstanceId))
				as Update;
				instanceUpdate.Execute();

				//TODO: reminder + valid message
			}

			GKIGetInstallLicensesInfoAll();

			return JsonConvert.SerializeObject(licInstanceSyncResults);
		}

		private string GKILicenseSyncRegularRequest(string baseUrl, string serviceUrl, Guid instanceId)
		{
			GKIUserConnectionCheck();
			var msgResponseText = GKILicenseHttpRequest(baseUrl, serviceUrl);
			string userMessage = "";
			GKIUsersSyncRoot syncResultObj = JsonConvert.DeserializeObject<GKIUsersSyncRoot>(msgResponseText);
			GKIUsersSyncResult syncResult = syncResultObj.GKIUsersSyncResult;
			if (syncResult != null)
			{
				if (!syncResult.Success)
				{
					userMessage = syncResult.ErrMsg;
				}
				if (syncResult.UserSyncResultSysAdminUnit != null && syncResult.UserSyncResultSysAdminUnit.Count > 0)
				{
					List<Guid> deactivateLicUsers = new List<Guid>();
					List<Guid> deactivateIdleLicUsers = new List<Guid>();
					List<Guid> deactivatePastDueLicUsers = new List<Guid>();
					List<Guid> allReceivedLicUsers = new List<Guid>();

					double timespanSysValue = Convert.ToDouble(Core.Configuration.SysSettings.GetValue(userConnection, "GKISysLicUserInactivenessControlTimespan"));

					foreach (UserSyncResultSysAdminUnit userSyncResultSysAdminUnit in syncResult.UserSyncResultSysAdminUnit)
					{
                        #region User
                        string sysAdminUnitName = userSyncResultSysAdminUnit.Name;
						Guid GKILicUserId;

						var GKIInstanceLicUserESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKIInstanceLicUser");
						GKIInstanceLicUserESQ.UseAdminRights = false;
						GKIInstanceLicUserESQ.AddAllSchemaColumns();
						GKIInstanceLicUserESQ.Filters.Add(
							GKIInstanceLicUserESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
						GKIInstanceLicUserESQ.Filters.Add(
							GKIInstanceLicUserESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser.GKIName", sysAdminUnitName));
						var GKIInstanceLicUserESQCollection = GKIInstanceLicUserESQ.GetEntityCollection(userConnection);
						if (GKIInstanceLicUserESQCollection.Count > 0)
						{
							Entity GKIInstanceLicUserEntity = GKIInstanceLicUserESQCollection.First();
							GKIInstanceLicUserEntity.SetColumnValue("GKIActive", userSyncResultSysAdminUnit.Active);
							bool setClm = userSyncResultSysAdminUnit.LastActivityDateTime != null ? 
								GKIInstanceLicUserEntity.SetColumnValue("GKILastActivityDateTime", userSyncResultSysAdminUnit.LastActivityDateTime)
								: false;
							GKIInstanceLicUserEntity.Save();
							GKILicUserId = GKIInstanceLicUserEntity.GetTypedColumnValue<Guid>("GKILicUserId");
						}
						else
						{
							var GKILicUserESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKILicUser");
							GKILicUserESQ.UseAdminRights = false;
							GKILicUserESQ.AddAllSchemaColumns();
							GKILicUserESQ.Filters.Add(
								GKILicUserESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIName", sysAdminUnitName));
							var GKILicUserESQCollection = GKILicUserESQ.GetEntityCollection(userConnection);
							if (GKILicUserESQCollection.Count > 0)
							{
								GKILicUserId = GKILicUserESQCollection.First().GetTypedColumnValue<Guid>("Id");
							}
							else
							{
								var GKILicUserSchema = userConnection.EntitySchemaManager.GetInstanceByName("GKILicUser");
								var GKILicUserEntity = GKILicUserSchema.CreateEntity(userConnection);
								GKILicUserEntity.SetDefColumnValues();
								GKILicUserEntity.SetColumnValue("GKIName", sysAdminUnitName);
								GKILicUserEntity.SetColumnValue("GKIPlatformActive", true);
								GKILicUserEntity.SetColumnValue("GKIActive", userSyncResultSysAdminUnit.Active);
								GKILicUserEntity.Save();
								GKILicUserId = GKILicUserEntity.GetTypedColumnValue<Guid>("Id");
							}
							var GKIInstanceLicUserSchema = userConnection.EntitySchemaManager.GetInstanceByName("GKIInstanceLicUser");
							var GKIInstanceLicUserEntity = GKIInstanceLicUserSchema.CreateEntity(userConnection);
							GKIInstanceLicUserEntity.SetDefColumnValues();
							GKIInstanceLicUserEntity.SetColumnValue("GKIActive", userSyncResultSysAdminUnit.Active);
							GKIInstanceLicUserEntity.SetColumnValue("GKIInstanceId", instanceId);
							GKIInstanceLicUserEntity.SetColumnValue("GKILicSyncSourceId", GKILicensingConstantsCs.GKILicSyncSource.Regular);
							GKIInstanceLicUserEntity.SetColumnValue("GKILicUserId", GKILicUserId);
							bool setClm = userSyncResultSysAdminUnit.LastActivityDateTime != null ?
								GKIInstanceLicUserEntity.SetColumnValue("GKILastActivityDateTime", userSyncResultSysAdminUnit.LastActivityDateTime)
								: false;
							GKIInstanceLicUserEntity.Save();
						}
                        
						//for comparing with current instance list
                        allReceivedLicUsers.Add(GKILicUserId);

						//if not active then put in deactivate queue
						if (!userSyncResultSysAdminUnit.Active)
						{
							deactivateLicUsers.Add(GKILicUserId);
						}
                        else
                        {
							//if it's not system user
							if (userSyncResultSysAdminUnit.Name != "Supervisor" && userSyncResultSysAdminUnit.Name != "SysPortalConnection")
							{
								//if user was created and haven't entered ever since (idle)
								if (userSyncResultSysAdminUnit.LastActivityDateTime == null)
								{
									deactivateIdleLicUsers.Add(GKILicUserId);
								}
								//if user haven't entered in syssetting-stated period of time (past due)
								else if (userSyncResultSysAdminUnit.LastActivityDateTime <=
									DateTime.Now
										.AddMonths(-Convert.ToInt32(Math.Truncate(timespanSysValue)))
										.AddDays(-Convert.ToInt32((timespanSysValue - Math.Truncate(timespanSysValue)) * 100))
									)
								{
									deactivatePastDueLicUsers.Add(GKILicUserId);
								}
							}
                        }

                        #endregion
						#region Licenses
                        foreach (UserSyncResultSysLicUser userSyncResultSysLicUser in userSyncResultSysAdminUnit.UserSyncResultSysLicUser)
						{
							var GKILicUserInstanceLicPackageESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKILicUserInstanceLicPackage");
							GKILicUserInstanceLicPackageESQ.UseAdminRights = false;
							GKILicUserInstanceLicPackageESQ.AddAllSchemaColumns();
							GKILicUserInstanceLicPackageESQ.Filters.Add(
								GKILicUserInstanceLicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
							GKILicUserInstanceLicPackageESQ.Filters.Add(
								GKILicUserInstanceLicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser", GKILicUserId));
							GKILicUserInstanceLicPackageESQ.Filters.Add(
								GKILicUserInstanceLicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage.GKIName", userSyncResultSysLicUser.SysLicPackageName));
							var GKILicUserInstanceLicPackageESQCollection = GKILicUserInstanceLicPackageESQ.GetEntityCollection(userConnection);
							if (GKILicUserInstanceLicPackageESQCollection.Count > 0)
							{
								Entity GKILicUserInstanceLicPackageEntity = GKILicUserInstanceLicPackageESQCollection.First();
								GKILicUserInstanceLicPackageEntity.SetColumnValue("GKISyncedState", userSyncResultSysLicUser.Active);
								GKILicUserInstanceLicPackageEntity.Save();
							}
							else
							{
								var GKILicPackageESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKILicPackage");
								GKILicPackageESQ.UseAdminRights = false;
								GKILicPackageESQ.AddAllSchemaColumns();
								GKILicPackageESQ.Filters.Add(
									GKILicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIName", userSyncResultSysLicUser.SysLicPackageName));
								Entity GKILicPackageESQEntity = GKILicPackageESQ.GetEntityCollection(userConnection).First();
								if (GKILicPackageESQEntity == null)
								{
									continue;
								}

								var GKILicUserInstanceLicPackageSchema = userConnection.EntitySchemaManager.GetInstanceByName("GKILicUserInstanceLicPackage");
								var GKILicUserInstanceLicPackageEntity = GKILicUserInstanceLicPackageSchema.CreateEntity(userConnection);
								GKILicUserInstanceLicPackageEntity.SetDefColumnValues();
								GKILicUserInstanceLicPackageEntity.SetColumnValue("GKIActive", userSyncResultSysLicUser.Active);
								GKILicUserInstanceLicPackageEntity.SetColumnValue("GKIInstanceId", instanceId);
								GKILicUserInstanceLicPackageEntity.SetColumnValue("GKILicPackageId", GKILicPackageESQEntity.GetTypedColumnValue<Guid>("Id"));
								GKILicUserInstanceLicPackageEntity.SetColumnValue("GKILicUserId", GKILicUserId);
								GKILicUserInstanceLicPackageEntity.SetColumnValue("GKISyncedState", userSyncResultSysLicUser.Active);
								GKILicUserInstanceLicPackageEntity.Save();
							}
						}
                        #endregion
                    }

					#region AfterSyncActions

					//delete GKIInstanceLicUsers that are deleted from the instance
					List<QueryParameter> allReceivedLicUsersIds = new List<QueryParameter>();
					foreach (Guid allReceivedLicUser in allReceivedLicUsers)
					{
						allReceivedLicUsersIds.Add(new QueryParameter(allReceivedLicUser));
					}
					IEnumerable<QueryParameter> allReceivedLicUsersIdsEnum = allReceivedLicUsersIds.AsEnumerable();
					if (allReceivedLicUsersIdsEnum.Count() > 0)
					{
						Delete requestRecordsDelete = new Delete(userConnection)
							.From("GKIInstanceLicUser")
							.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
							.And("GKILicUserId")
							.Not().In(allReceivedLicUsersIdsEnum)
							as Delete;
						requestRecordsDelete.Execute();
					}

					GKILicUserGKIPlatformActiveUpdate();

					GKILicUserGKIActiveUpdate();

					GKIDeactivatedUsersUpdate(deactivateLicUsers, instanceId, GKILicensingConstantsCs.GKIDeactivationReasonLookup.Inactive);
					GKIDeactivatedUsersUpdate(deactivateIdleLicUsers, instanceId, GKILicensingConstantsCs.GKIDeactivationReasonLookup.DidntEnter);
					GKIDeactivatedUsersUpdate(deactivatePastDueLicUsers, instanceId, GKILicensingConstantsCs.GKIDeactivationReasonLookup.HaventEnteredInTheTimespan);

					#endregion
				}
            }
			else
			{
				userMessage = msgResponseText;
			}

			//TODO: reminder

			return userMessage;
		}
		#endregion

		#region Licensing
		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
			ResponseFormat = WebMessageFormat.Json)]
		public void GKIImportTlsFile()
        {
			var userConnection = UserConnection;
			var request = HttpContext.Current.Request;
			var queryString = request.Form;

			string fileName = queryString["fileName"];
			Guid recordId = new Guid(queryString["parentColumnValue"]);
			Stream fileStream = request.Files["files"].InputStream;
			
			StreamReader streamReader = new StreamReader(fileStream);
			string tlsString = streamReader.ReadToEnd();
			TLSLicenses tlsLicenses = new TLSLicenses();
			
			fileStream.Seek(0, SeekOrigin.Begin);
			byte[] fileBytes = StreamToByteArray(fileStream);

			//validate the file
			//extension
			if (fileName.Substring(fileName.Length > 4 ? fileName.Length - 4 : 0) != ".tls")
			{
				throw new Exception("#ExceptionStart#Wrong file format#ExceptionEnd#");
			}
			//content
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(TLSLicenses));
				using (var reader = new StringReader(tlsString))
				{
					tlsLicenses = (TLSLicenses)serializer.Deserialize(reader);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("#ExceptionStart#Wrong file content#ExceptionEnd#");
			}
			if (tlsLicenses.FileVersion == String.Empty || tlsLicenses.CustomerId == String.Empty)
            {
				throw new Exception("#ExceptionStart#Wrong file content#ExceptionEnd#");
			}

			var update = new Update(userConnection, "GKIInstance")
				.Set("GKITlsFile", Column.Parameter(fileBytes))
				.Where("GKICustomerIDId").In(
					new Select(userConnection)
					.Column("Id")
					.From("GKICustomerID")
					.Where("Name")
					.IsEqual(Column.Parameter(tlsLicenses.CustomerId))
				).And("GKIVersion")
				.IsEqual(Column.Parameter(tlsLicenses.FileVersion)) as Update;
			int recordsUpdated = update.Execute();
			if (recordsUpdated == 0)
			{
				throw new Exception("#ExceptionStart#No suitable instances#ExceptionEnd#");
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
			ResponseFormat = WebMessageFormat.Json)]
		public string GKITlsInstall(Guid recordId)
		{
			GKIUserConnectionCheck();

			string returnMsg = "";
			string errorMsg = new LocalizableString(userConnection.Workspace.ResourceStorage,
					"GKILicensingServices",
					"LocalizableStrings.GKITlsInstallError.Value");
			string successMsg = new LocalizableString(userConnection.Workspace.ResourceStorage,
					"GKILicensingServices",
					"LocalizableStrings.GKITlsInstallSuccess.Value");

			var GKIInstanceESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKIInstance");
			GKIInstanceESQ.UseAdminRights = false;
			string idColumnName = GKIInstanceESQ.AddColumn("Id").Name;
			GKIInstanceESQ.AddColumn("GKIUrl");
			GKIInstanceESQ.AddColumn("GKITlsFile");
			var customerNameClm = GKIInstanceESQ.AddColumn("GKICustomerID.Name").Name;
			GKIInstanceESQ.Filters.Add(
				GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", recordId));
			var ESQCollection = GKIInstanceESQ.GetEntityCollection(userConnection);

			foreach(Entity GKIInstanceEntity in ESQCollection)
			{
				var fileValue = GKIInstanceEntity.GetColumnValue("GKITlsFile");
				if (fileValue == null)
                {
					GKIWriteGKIInstanceLastTlsInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), "Empty file");
					returnMsg = errorMsg;
					continue;
				}

				byte[] file = GKIInstanceEntity.GetColumnValue("GKITlsFile") as byte[];
				string fileString = Encoding.UTF8.GetString(file);
				var licObject = new TlsLicData { licData = fileString };
				string licMessage = JsonConvert.SerializeObject(licObject);

				TLSLicenses tlsLicenses = new TLSLicenses();
				try
				{
					XmlSerializer serializer = new XmlSerializer(typeof(TLSLicenses));
					using (var reader = new MemoryStream(file))
					{
						tlsLicenses = (TLSLicenses)serializer.Deserialize(reader);
					}
				}
				catch (Exception ex)
				{
					GKIWriteGKIInstanceLastTlsInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), "Wrong file content");
					returnMsg = errorMsg;
					continue;
				}

				if (GKIInstanceEntity.GetTypedColumnValue<string>(customerNameClm) != tlsLicenses.CustomerId)
				{
					GKIWriteGKIInstanceLastTlsInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), "Wrong CustomerId");
					returnMsg = errorMsg;
					continue;
				}

				string baseUrl = GKIInstanceEntity.GetTypedColumnValue<string>("GKIUrl");
				//url format validation:
				Uri uriResult;
				bool uriCheckResult = Uri.TryCreate(baseUrl, UriKind.Absolute, out uriResult)
					&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
						
				if (baseUrl != String.Empty && uriCheckResult)
				{
					try
					{
						string installationResult = GKILicenseHttpRequest(baseUrl, String.Concat(baseUrl, GKITlsInstallRegularServiceUrl), licMessage);
						TlsCustomInstallResult tlsInstallResult = JsonConvert.DeserializeObject<TlsCustomInstallResult>(installationResult);
						if (tlsInstallResult.errorInfo != String.Empty)
                        {
							GKIWriteGKIInstanceLastTlsInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), tlsInstallResult.errorInfo);
							returnMsg = errorMsg;
							continue;
						}
						if (tlsInstallResult.success == true)
						{
						try
                            {
								GKIGetInstallLicensesInfo(baseUrl, recordId, GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName));
							}
							catch(Exception ex)
                            {
								GKIWriteGKIInstanceLastTlsInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), String.Concat("Licesnes have been installed, but attempt to get the information about them has failed: ", ex.Message));
								returnMsg = errorMsg;
								continue; 
							}
                        }
					}
					catch(Exception ex)
                    {
						GKIWriteGKIInstanceLastTlsInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), ex.Message);
						returnMsg = errorMsg;
						continue;
					}
				}
                else
                {
					GKIWriteGKIInstanceLastTlsInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), "Invalid or empty URL");
					returnMsg = errorMsg;
					continue;
				}

				GKIWriteGKIInstanceLastTlsInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), String.Empty);
				continue;
			}
			if (returnMsg == String.Empty)
			{
				returnMsg = successMsg;
			}
			return returnMsg;
		}

		public void GKIGetInstallLicensesInfoAll()
		{
			var GKICustomerIDESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKICustomerID");
			GKICustomerIDESQ.UseAdminRights = false;
			string idColumnName = GKICustomerIDESQ.AddColumn("Id").Name;
			var GKICustomerIDESQCollection = GKICustomerIDESQ.GetEntityCollection(userConnection);

			foreach (Entity GKICustomerIDEntity in GKICustomerIDESQCollection)
			{
				GKIGetInstallLicensesInfoByCustomerID(GKICustomerIDEntity.GetTypedColumnValue<Guid>(idColumnName));
			}
		}
		public void GKIGetInstallLicensesInfoByCustomerID(Guid CustomerId)
		{
			var GKIInstanceESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKIInstance");
			GKIInstanceESQ.UseAdminRights = false;
			string idColumnName = GKIInstanceESQ.AddColumn("Id").Name;
			GKIInstanceESQ.AddColumn("GKIUrl");
			GKIInstanceESQ.Filters.Add(
				GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", CustomerId));
			var GKIInstanceESQCollection = GKIInstanceESQ.GetEntityCollection(userConnection);

			foreach (Entity GKIInstanceEntity in GKIInstanceESQCollection)
			{
				string baseUrl = GKIInstanceEntity.GetTypedColumnValue<string>("GKIUrl");
				//url format validation:
				Uri uriResult;
				bool uriCheckResult = Uri.TryCreate(baseUrl, UriKind.Absolute, out uriResult)
					&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
				string message = "";
				if (baseUrl != String.Empty && uriCheckResult)
				{
					try
					{
						GKIGetInstallLicensesInfo(baseUrl, CustomerId, GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName));
					}
					catch (Exception ex)
					{
						message = ex.Message;
					}
				}
                else
                {
					message = "Invalid or empty URL";
				}
				var update = new Update(userConnection, "GKIInstance")
					.Set("GKILastTlsInstallError", Column.Parameter(message))
					.Where("Id").IsEqual(Column.Parameter(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName))) as Update;
				update.Execute();
			}
		}
		public string GKIGetInstallLicensesInfo(string baseUrl, Guid CustomerId, Guid instanceId)
		{
			try
			{
				string bareLicenseInfoResult = GKILicenseHttpRequest(baseUrl, String.Concat(baseUrl, GKIGetInstalledLicensesInfoServiceUrl));
				LicenseInfoResult licenseInfoResult = JsonConvert.DeserializeObject<LicenseInfoResult>(bareLicenseInfoResult);
				if (licenseInfoResult.Success == true)
				{
					foreach (LicenseInfo licenseInfo in licenseInfoResult.LicenseInfoList)
					{
						var licESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKILic");
						licESQ.UseAdminRights = false;
						licESQ.AddAllSchemaColumns();
						licESQ.Filters.Add(
							licESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", CustomerId));
						licESQ.Filters.Add(
							licESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage.GKIName", licenseInfo.SysLicPackageName));
						var licESQCollection = licESQ.GetEntityCollection(userConnection);

						var GKILicPackageSchema = userConnection.EntitySchemaManager.GetInstanceByName("GKILicPackage");
						Entity GKILicPackageEntity = GKILicPackageSchema.CreateEntity(userConnection);
						Guid licPackageId;
						if (!GKILicPackageEntity.FetchFromDB("GKIName", licenseInfo.SysLicPackageName))
						{
							GKILicPackageEntity.SetDefColumnValues();
							GKILicPackageEntity.SetColumnValue("GKIName", licenseInfo.SysLicPackageName);
							GKILicPackageEntity.SetColumnValue("GKIDescription", licenseInfo.SysLicPackageName);
							GKILicPackageEntity.Save();
							licPackageId = GKILicPackageEntity.GetTypedColumnValue<Guid>("Id");
						}
						else
						{
							var select = new Select(userConnection)
								.Column("Id")
								.From("GKILicPackage")
								.Where("GKIName").IsEqual(Column.Parameter(licenseInfo.SysLicPackageName)) as Select;
							licPackageId = select.ExecuteScalar<Guid>();
						}

						var rootSchema = userConnection.EntitySchemaManager.GetInstanceByName("GKILic");
						Entity GKILicEntity = rootSchema.CreateEntity(userConnection);

						if (licESQCollection.Count > 0)
						{
							GKILicEntity = licESQCollection.First();
						}
                        else
                        {
							GKILicEntity.SetDefColumnValues();
						}

						//update GKICountUsed in GKIInstanceLicense for future max function
						var GKIInstanceLicenseESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKIInstanceLicense");
						GKIInstanceLicenseESQ.UseAdminRights = false;
						GKIInstanceLicenseESQ.AddAllSchemaColumns();
						GKIInstanceLicenseESQ.Filters.Add(
							GKIInstanceLicenseESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
						GKIInstanceLicenseESQ.Filters.Add(
							GKIInstanceLicenseESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILic", GKILicEntity.GetTypedColumnValue<Guid>("Id")));
						var GKIInstanceLicenseESQCollection = GKIInstanceLicenseESQ.GetEntityCollection(userConnection);

						var schemaGKIInstanceLicense = userConnection.EntitySchemaManager.GetInstanceByName("GKIInstanceLicense");
						var entityGKIInstanceLicense = schemaGKIInstanceLicense.CreateEntity(userConnection);

						if (GKIInstanceLicenseESQCollection.Count > 0)
                        {
							entityGKIInstanceLicense = GKIInstanceLicenseESQCollection.First();
						}
                        else
                        {
							entityGKIInstanceLicense.SetDefColumnValues();
							entityGKIInstanceLicense.SetColumnValue("GKIInstanceId", instanceId);
							entityGKIInstanceLicense.SetColumnValue("GKILicId", GKILicEntity.GetTypedColumnValue<Guid>("Id"));
						}
						entityGKIInstanceLicense.SetColumnValue("GKICountUsed", licenseInfo.CountUsed);
						//we will save entityGKIInstanceLicense later on.

						//max CountUsed in GKIInstanceLicenses
						var maxGKIInstanceLicenseESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKIInstanceLicense");
						maxGKIInstanceLicenseESQ.UseAdminRights = false;
						var countUsedColumn = maxGKIInstanceLicenseESQ.AddColumn(
							maxGKIInstanceLicenseESQ.CreateAggregationFunction(AggregationTypeStrict.Max, "GKICountUsed"));
						maxGKIInstanceLicenseESQ.Filters.Add(
							maxGKIInstanceLicenseESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILic", GKILicEntity.GetTypedColumnValue<Guid>("Id")));
						var maxGKIInstanceLicenseESQCollection = maxGKIInstanceLicenseESQ.GetEntityCollection(userConnection);
						var entityMaxGKIInstanceLicense = maxGKIInstanceLicenseESQCollection.First();
						int maxLicenseCountUsed = entityMaxGKIInstanceLicense.GetTypedColumnValue<int>(countUsedColumn.Name);

						//LicType
						var licTypeSchema = userConnection.EntitySchemaManager.GetInstanceByName("GKILicType");
						Entity licTypeEntity = licTypeSchema.CreateEntity(userConnection);
						Guid licTypeId = licTypeEntity.FetchFromDB("GKILicTypeInt", licenseInfo.LicType) ? 
							licTypeEntity.GetTypedColumnValue<Guid>("Id") : Guid.Empty;

						GKILicEntity.SetColumnValue("GKIAvailableCount", licenseInfo.Count - maxLicenseCountUsed);
						GKILicEntity.SetColumnValue("GKICount", licenseInfo.Count);
						GKILicEntity.SetColumnValue("GKICustomerIDId", CustomerId);
						GKILicEntity.SetColumnValue("GKIExpirationDate", licenseInfo.DueDate);
						GKILicEntity.SetColumnValue("GKILicPackageId", licPackageId);
						GKILicEntity.SetColumnValue("GKIName", licenseInfo.SysLicPackageName);
						GKILicEntity.SetColumnValue("GKIStartDate", licenseInfo.StartDate);
						GKILicEntity.SetColumnValue("GKIUsedCount", maxLicenseCountUsed);
						bool clmSet = licTypeId != Guid.Empty ? GKILicEntity.SetColumnValue("GKILicTypeId", licTypeId) : false;
						GKILicEntity.SetColumnValue("GKILicStatusId", DateTime.Now >= licenseInfo.StartDate && DateTime.Now < licenseInfo.DueDate 
							? GKILicensingConstantsCs.GKILicStatus.Active 
							: GKILicensingConstantsCs.GKILicStatus.Inactive);
						GKILicEntity.Save();
						entityGKIInstanceLicense.Save();

						if (DateTime.Now >= licenseInfo.StartDate && DateTime.Now < licenseInfo.DueDate) //license is active
						{
							var instanceLicPackageESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKIInstanceLicPackage");
							instanceLicPackageESQ.UseAdminRights = false;
							instanceLicPackageESQ.AddAllSchemaColumns();
							instanceLicPackageESQ.Filters.Add(
								instanceLicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
							instanceLicPackageESQ.Filters.Add(
								instanceLicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage", licPackageId));
							var instanceLicPackageESQCollection = instanceLicPackageESQ.GetEntityCollection(userConnection);
							if (instanceLicPackageESQCollection.Count == 0)
							{
								var instanceLicPackageSchema = userConnection.EntitySchemaManager.GetInstanceByName("GKIInstanceLicPackage");
								var instanceLicPackageEntity = instanceLicPackageSchema.CreateEntity(userConnection);
								instanceLicPackageEntity.SetDefColumnValues();
								instanceLicPackageEntity.SetColumnValue("GKIInstanceId", instanceId);
								instanceLicPackageEntity.SetColumnValue("GKILicPackageId", licPackageId);
								instanceLicPackageEntity.Save();
							}
						}
					}
				}
                else
                {
					return licenseInfoResult.ErrMsg;
                }
				return String.Empty;
			}
			catch(Exception ex)
            {
				return ex.Message;
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
			ResponseFormat = WebMessageFormat.Json)]
		public bool GKITlsDownloadEnable()
        {
			try
			{
				GKIUserConnectionCheck();
				var systemUserConn = userConnection.AppConnection.SystemUserConnection;
				var rootSchema = systemUserConn.EntitySchemaManager.GetInstanceByName("FileSecurityExcludedUri");
				Entity updateEntity = rootSchema.CreateEntity(systemUserConn);
				if (!updateEntity.FetchFromDB("Name", "/rest/GKILicensingAdminService/GKIImportTlsFile"))
				{
					updateEntity.SetDefColumnValues();
					updateEntity.SetColumnValue("Name", "/rest/GKILicensingAdminService/GKIImportTlsFile");
				}
				updateEntity.Save();
				return true;
			}
            catch (Exception ex)
            {
				return false;
            }
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
			ResponseFormat = WebMessageFormat.Json)]
		public string GKITlrRequestByCustomerID (Guid CustomerId)
		{
			try
			{
				string returnMsg = "";

				var GKIInstanceESQ = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKIInstance");
				GKIInstanceESQ.UseAdminRights = false;
				string idColumnName = GKIInstanceESQ.AddColumn("Id").Name;
				GKIInstanceESQ.AddColumn("GKIUrl");
				GKIInstanceESQ.Filters.Add(
					GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", CustomerId));
				var GKIInstanceESQCollection = GKIInstanceESQ.GetEntityCollection(userConnection);

				foreach (Entity GKIInstanceEntity in GKIInstanceESQCollection)
				{
					string baseUrl = GKIInstanceEntity.GetTypedColumnValue<string>("GKIUrl");
					//url format validation:
					Uri uriResult;
					bool uriCheckResult = Uri.TryCreate(baseUrl, UriKind.Absolute, out uriResult)
						&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

					string result = "";
					if (baseUrl != String.Empty && uriCheckResult)
					{
						try
						{
							//TODO: tlr request
						}
						catch (Exception ex)
						{
							result = ex.Message;
						}
					}
					else
					{
						result = "Invalid or empty URL";
					}
					if (result == String.Empty)
					{
						result = "Success";
					}
					else
					{
						returnMsg = "Errors has been detected. Check instance section";
					}
					var update = new Update(userConnection, "GKIInstance")
						.Set("GKILastTlsInstallError", Column.Parameter(result))
						.Where("Id").IsEqual(Column.Parameter(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName))) as Update;
					update.Execute();
				}
				if (returnMsg == String.Empty)
				{
					returnMsg = "Success";
				}
				return returnMsg;
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
        #endregion

        #region Private Methods

       public void GKIDeactivatedUsersUpdate(List<Guid> deactivateLicUsers, Guid instanceId, Guid reason)
       {
			List<QueryParameter> allDeactivateLicUsersIds = new List<QueryParameter>();
			foreach (Guid deactivateLicUser in deactivateLicUsers)
			{
				allDeactivateLicUsersIds.Add(new QueryParameter(deactivateLicUser));
			}
			IEnumerable<QueryParameter> allDeactivateLicUsersIdsEnum = allDeactivateLicUsersIds.AsEnumerable();
			if (allDeactivateLicUsersIdsEnum.Count() > 0)
			{
				Update updateLicensesInactive = new Update(userConnection, "GKILicUserInstanceLicPackage")
					.Set("GKIActive", Column.Parameter(false))
					.Set("GKIDeactivatedBySync", Column.Parameter(true))
					.Set("GKIDeactivationReasonLookupId", Column.Parameter(reason))
					.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
					.And("GKILicUserId").In(allDeactivateLicUsersIdsEnum)
				as Update;
				updateLicensesInactive.Execute();

				var insertIntoQueue = new InsertSelect(userConnection)
					.Into("GKILicUserInstanceLicPackageQueue")
					.Set("GKILicUserInstanceLicPackageId")
					.FromSelect(
						new Select(userConnection)
							.Column("Id")
								.From("GKILicUserInstanceLicPackage")
								.Where("GKIDeactivatedBySync").IsEqual(Column.Parameter(true))
								.And("GKIActive").IsEqual(Column.Parameter(false))
								.And("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
								.And("GKILicUserId").In(allDeactivateLicUsersIdsEnum)
								.And("Id").Not().In(new Select(userConnection)
									.Column("GKILicUserInstanceLicPackageId")
									.From("GKILicUserInstanceLicPackageQueue"))
									as Select);
				insertIntoQueue.Execute();
			}
		}

		private void GKILicUserGKIPlatformActiveUpdate()
        {
			//GKIPlatformActive:
			//make GKILicUser.GKIPlatformActive inactive if it hasn't got any GKIInstanceLicUsers
			Update updateGKILicUserPlatformNotActive = new Update(userConnection, "GKILicUser")
				.Set("GKIPlatformActive", Column.Parameter(false))
				.Where("Id").Not().In(new Select(userConnection).Column("GKILicUserId").From("GKIInstanceLicUser"))
			as Update;
			updateGKILicUserPlatformNotActive.Execute();

			//make GKILicUser.GKIPlatformActive for every acquired instance user
			Update updateGKILicUserPlatformActive = new Update(userConnection, "GKILicUser")
				.Set("GKIPlatformActive", Column.Parameter(true))
				.Where("Id").In(new Select(userConnection).Column("GKILicUserId").From("GKIInstanceLicUser"))
			as Update;
			updateGKILicUserPlatformActive.Execute();
		}

		private void GKILicUserGKIActiveUpdate()
        {
			//GKIActive:
			//make GKILicUser.GKIActive inactive if it hasn't got any GKIInstanceLicUsers.GKIActive == true
			Update updateGKILicUserNotActive = new Update(userConnection, "GKILicUser")
				.Set("GKIActive", Column.Parameter(false))
				.Set("GKIStatusId", Column.Parameter(GKILicensingConstantsCs.GKILicUserStatus.Inactive))
				.Where("Id").Not().In(new Select(userConnection).Column("GKILicUserId")
					.From("GKIInstanceLicUser")
					.Where("GKIActive").IsEqual(Column.Parameter(true)))
			as Update;
			updateGKILicUserNotActive.Execute();

			//make GKILicUser.GKIActive for every user with active GKIInstanceLicUser
			//part 1: active with licenses
			Update updateGKILicUserActive = new Update(userConnection, "GKILicUser")
				.Set("GKIActive", Column.Parameter(true))
				.Set("GKIStatusId", Column.Parameter(GKILicensingConstantsCs.GKILicUserStatus.Active))
				.Where("Id").In(new Select(userConnection).Column("GKILicUserId")
					.From("GKIInstanceLicUser")
					.Where("GKIActive").IsEqual(Column.Parameter(true)))
				//#BPMS-139
				//.And("Id").In(new Select(userConnection).Column("GKILicUserId").From("GKILicUserInstanceLicPackage").Where("GKIActive").IsEqual(Column.Parameter(true)))
			as Update;
			updateGKILicUserActive.Execute();
		}
		private byte[] StreamToByteArray(Stream stream)
		{
			byte[] byteArray = new byte[16 * 1024];
			using (MemoryStream mStream = new MemoryStream())
			{
				int bit;
				while ((bit = stream.Read(byteArray, 0, byteArray.Length)) > 0)
				{
					mStream.Write(byteArray, 0, bit);
				}
				return mStream.ToArray();
			}
		}
		private void GKIWriteLicUserInstanceLicPackageError(Guid Id, string errorMsg)
        {
			Update update = new Update(this.userConnection, "GKILicUserInstanceLicPackage")
				.Set("GKISyncError", Column.Parameter(errorMsg.Length > 250 ?
							errorMsg.Substring(0, 250) : errorMsg))
				.Set("GKILastErrorDateTime", Column.Parameter(DateTime.Now))
				.Where("Id")
				.IsEqual(Column.Parameter(Id))
				as Update;
			update.Execute();
		}
		private void GKIWriteGKIInstanceLastTlsInstallError(Guid Id, string errorMsg)
		{
			Update update = new Update(this.userConnection, "GKIInstance")
				.Set("GKILastTlsInstallError", Column.Parameter(errorMsg.Length > 250 ?
					errorMsg.Substring(0, 250) : errorMsg))
				.Where("Id")
				.IsEqual(Column.Parameter(Id))
				as Update;
			update.Execute();
		}

		private string GKIAuthorize(string baseUrl)
		{
			string authUrl = string.Concat(baseUrl, authServicePath);
			string authMessage = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "GKILicensingCredentials");
			
			if (authMessage == String.Empty)
			{
				return "Error: no authentification credentials in GKILicensingCredentials";
			}

			if (AuthCookies == null)
			{
				AuthCookies = new CookieContainer();
			}

			if (AuthCookies.GetCookies(new Uri(baseUrl)).Count > 0)
			{
				try
				{
					HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(String.Concat(baseUrl, GKIAuthCheckServiceUrl));
					httprequest.Method = "POST";
					httprequest.Accept = @"application/json";
					httprequest.ContentLength = 0;
					httprequest.ContentType = @"application/json";
					httprequest.CookieContainer = AuthCookies;
					var crsfcookie = httprequest.CookieContainer.GetCookies(new Uri(baseUrl))[crsfName];
					if (crsfcookie != null)
					{
						httprequest.Headers.Add(crsfName, crsfcookie.Value);
					}

					using (HttpWebResponse response = (HttpWebResponse)httprequest.GetResponse())
					{
						using (var streamReader = new StreamReader(response.GetResponseStream()))
						{
							var msgResponseText = streamReader.ReadToEnd();
						}
					}
				}
				catch (WebException ex)
				{
					using (var stream = ex.Response.GetResponseStream())
					using (var reader = new StreamReader(stream))
					{
						string exceptionMsg = reader.ReadToEnd();
						if (exceptionMsg.Contains("401"))
						{
							string authResult = GKIAuthorizeForced(baseUrl);
							if (authResult != String.Empty)
							{
								return authResult;
							}
							else
							{
								return String.Empty;
							}
						}
						else
						{
							return exceptionMsg;
						}
					}
				}
				catch(Exception ex)
                {
					return ex.Message;
				}
				return String.Empty;
			}

			byte[] bytes = Encoding.UTF8.GetBytes(authMessage);

			HttpWebRequest authrequest = (HttpWebRequest)WebRequest.Create(authUrl);
			authrequest.Method = "POST";
			authrequest.Accept = @"application/json";
			authrequest.ContentLength = bytes.Length;
			authrequest.ContentType = @"application/json";
			authrequest.CookieContainer = AuthCookies;

			using (Stream streamresponse = authrequest.GetRequestStream())
			{
				streamresponse.Write(bytes, 0, bytes.Length);
			}
			string responseText;
			ResponseStatus status = null;

			using (HttpWebResponse response = (HttpWebResponse)authrequest.GetResponse())
			{
				string authCookeValue = response.Cookies[authName].Value;
				string crsfCookeValue = response.Cookies[crsfName].Value;
				AuthCookies.Add(new Uri(baseUrl), new Cookie(authName, authCookeValue));
				AuthCookies.Add(new Uri(baseUrl), new Cookie(crsfName, crsfCookeValue));
				using (var streamReader = new StreamReader(response.GetResponseStream()))
				{
					responseText = streamReader.ReadToEnd();
					status = JsonConvert.DeserializeObject<ResponseStatus>(responseText);
				}
			}

			if (status == null || status.Code != 0)
			{
				AuthCookies = new CookieContainer();
				return responseText;
			}
			else
			{
				return String.Empty;
			}
		}
		private string GKIAuthorizeForced(string baseUrl)
		{
			string authUrl = string.Concat(baseUrl, authServicePath);
			string authMessage = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "GKILicensingCredentials");

			if (authMessage == String.Empty)
			{
				return "Error: no authentification credentials in GKILicensingCredentials";
			}

			AuthCookies = new CookieContainer();

			byte[] bytes = Encoding.UTF8.GetBytes(authMessage);

			HttpWebRequest authrequest = (HttpWebRequest)WebRequest.Create(authUrl);
			authrequest.Method = "POST";
			authrequest.Accept = @"application/json";
			authrequest.ContentLength = bytes.Length;
			authrequest.ContentType = @"application/json";
			authrequest.CookieContainer = AuthCookies;

			using (Stream streamresponse = authrequest.GetRequestStream())
			{
				streamresponse.Write(bytes, 0, bytes.Length);
			}
			string responseText;
			ResponseStatus status = null;

			using (HttpWebResponse response = (HttpWebResponse)authrequest.GetResponse())
			{
				string authCookeValue = response.Cookies[authName].Value;
				string crsfCookeValue = response.Cookies[crsfName].Value;
				AuthCookies.Add(new Uri(baseUrl), new Cookie(authName, authCookeValue));
				AuthCookies.Add(new Uri(baseUrl), new Cookie(crsfName, crsfCookeValue));
				using (var streamReader = new StreamReader(response.GetResponseStream()))
				{
					responseText = streamReader.ReadToEnd();
					status = JsonConvert.DeserializeObject<ResponseStatus>(responseText);
				}
			}

			if (status == null || status.Code != 0)
			{
				AuthCookies = new CookieContainer();
				return responseText;
			}
			else
			{
				return String.Empty;
			}
		}
		private string GKILicenseHttpRequest(string baseUrl, string serviceUrl)
		{
			string authResult = GKIAuthorize(baseUrl);
			if (authResult != String.Empty)
            {
				throw new Exception(authResult);
            }
			HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(serviceUrl);
			httprequest.Method = "POST";
			httprequest.Accept = @"application/json";
			httprequest.ContentLength = 0;
			httprequest.ContentType = @"application/json";
			httprequest.CookieContainer = AuthCookies;
			var crsfcookie = httprequest.CookieContainer.GetCookies(new Uri(baseUrl))[crsfName];
			if (crsfcookie != null)
			{
				httprequest.Headers.Add(crsfName, crsfcookie.Value);
			}

			using (HttpWebResponse response = (HttpWebResponse)httprequest.GetResponse())
			{
				using (var streamReader = new StreamReader(response.GetResponseStream()))
				{
					var msgResponseText = streamReader.ReadToEnd();
					return msgResponseText;
				}
			}
		}
		private string GKILicenseHttpRequest(string baseUrl, string serviceUrl, string message)
		{
			string authResult = GKIAuthorize(baseUrl);
			if (authResult != String.Empty)
			{
				throw new Exception(authResult);
			}
			byte[] msgBytes = Encoding.UTF8.GetBytes(message);

			HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(serviceUrl);
			httprequest.Method = "POST";
			httprequest.Accept = @"application/json";
			httprequest.ContentLength = msgBytes.Length;
			httprequest.ContentType = @"application/json";
			httprequest.CookieContainer = AuthCookies;
			var crsfcookie = httprequest.CookieContainer.GetCookies(new Uri(baseUrl))[crsfName];
			if (crsfcookie != null)
			{
				httprequest.Headers.Add(crsfName, crsfcookie.Value);
			}
			using (Stream streamresponse = httprequest.GetRequestStream())
			{
				streamresponse.Write(msgBytes, 0, msgBytes.Length);
			}
			using (HttpWebResponse response = (HttpWebResponse)httprequest.GetResponse())
			{
				using (var streamReader = new StreamReader(response.GetResponseStream()))
				{
					var msgResponseText = streamReader.ReadToEnd();
					return msgResponseText;
				}
			}
		}
		private string GKIFileByteRequest(string baseUrl, string serviceUrl, byte[] file)
		{
			string authResult = GKIAuthorize(baseUrl);
			if (authResult != String.Empty)
			{
				throw new Exception(authResult);
			}
			byte[] msgBytes = file;

			HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(serviceUrl);
			httprequest.Method = "POST";
			httprequest.Accept = @"application/json";
			httprequest.ContentLength = msgBytes.Length;
			httprequest.ContentType = @"application/json";
			httprequest.CookieContainer = AuthCookies;
			var crsfcookie = httprequest.CookieContainer.GetCookies(new Uri(baseUrl))[crsfName];
			if (crsfcookie != null)
			{
				httprequest.Headers.Add(crsfName, crsfcookie.Value);
			}
			using (Stream streamresponse = httprequest.GetRequestStream())
			{
				streamresponse.Write(msgBytes, 0, msgBytes.Length);
			}
			using (HttpWebResponse response = (HttpWebResponse)httprequest.GetResponse())
			{
				using (var streamReader = new StreamReader(response.GetResponseStream()))
				{
					var msgResponseText = streamReader.ReadToEnd();
					return msgResponseText;
				}
			}
		}
		private UserConnection GKIGetUserConnection()
		{
			return UserConnection;
		}
		private void GKIUserConnectionCheck()
		{
			if (userConnection == null)
			{
				userConnection = GKIGetUserConnection();
			}
		}

		#endregion

		#region Classes

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

		public class TlsErrorInfo
		{
			public string errorCode { get; set; }
			public string message { get; set; }
			public object stackTrace { get; set; }
		}

		public class TlsInstallResult
		{
			public TlsErrorInfo errorInfo { get; set; }
			public bool success { get; set; }
		}
		public class TlsCustomInstallResult
		{
			public string errorInfo { get; set; }
			public bool success { get; set; }
		}


		[XmlRoot(ElementName = "Licenses")]
		public class TLSLicenses
		{
			[XmlAttribute(AttributeName = "CustomerId")]
			public string CustomerId { get; set; }
			[XmlAttribute(AttributeName = "FileVersion")]
			public string FileVersion { get; set; }
			[XmlAttribute(AttributeName = "Direction")]
			public string Direction { get; set; }
			[XmlAttribute(AttributeName = "Data")]
			public string Data { get; set; }
		}
		private class ResponseStatus
		{
			public int Code { get; set; }
			public string Message { get; set; }
			public object Exception { get; set; }
			public object PasswordChangeUrl { get; set; }
			public object RedirectUrl { get; set; }
		}

		public class InstanceSyncData
        {
			public List<LicUserData> LicUserDataList { get; set; }
			public string BaseUrl { get; set; }
			public string Message { get; set; }
		}
		public class LicUserData
		{
			public string LicUserName { get; set; }
			public string LicPackageName { get; set; }
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
		public class GKILicUserSyncResultRoot
		{ 
			public GKILicUserSyncResult GKILicUserSyncResult { get; set; }
		}
		public class LicInstanceSyncResult
        {
			public Guid InstanceId { get; set; }
			public bool Success { get; set; }
			public string ErrMsg { get; set; }
		}

		public class UserSyncResultSysLicUser
		{
			public bool Active { get; set; }
			public string SysLicPackageName { get; set; }
			public string SysUserName { get; set; }
		}

		public class UserSyncResultSysAdminUnit
		{
			public bool Active { get; set; }
			public string Name { get; set; }
			public DateTime? LastActivityDateTime { get; set; }
			public List<UserSyncResultSysLicUser> UserSyncResultSysLicUser { get; set; }
		}

		public class GKIUsersSyncResult
		{
			public string ErrMsg { get; set; }
			public bool Success { get; set; }
			public List<UserSyncResultSysAdminUnit> UserSyncResultSysAdminUnit { get; set; }
		}

		public class GKIUsersSyncRoot
		{
			public GKIUsersSyncResult GKIUsersSyncResult { get; set; }
		}
		#endregion

		#region Constants
		UserConnection userConnection;
		public static CookieContainer AuthCookies;
		string authName = ".ASPXAUTH";
		string crsfName = "BPMCSRF";
		string authServicePath = "/ServiceModel/AuthService.svc/Login";
		string GKIUsersSyncServiceUrl = "/0/rest/GKILicensingRegularService/GKIUsersSync";
		string GKIAddLicenseServiceUrl = "/0/rest/GKILicensingRegularService/GKIAddLicense";
		string GKIRemoveLicenseServiceUrl = "/0/rest/GKILicensingRegularService/GKIRemoveLicense";
		string GKITlsInstallRegularServiceUrl = "/0/rest/GKILicensingRegularService/GKIInstallLicenses";
		string GKIAuthCheckServiceUrl = "/0/rest/GKILicensingRegularService/GKIAuthCheck";
		string GKIGetInstalledLicensesInfoServiceUrl = "/0/rest/GKILicensingRegularService/GKIGetInstalledLicensesInfo";
		#endregion
	}
	#endregion
}