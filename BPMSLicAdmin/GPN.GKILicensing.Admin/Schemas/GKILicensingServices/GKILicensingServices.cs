namespace Terrasoft.Configuration
{
    using Newtonsoft.Json;

    using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
    using System.Net;
	using System.Linq;
	using System.Runtime.Serialization.Json;
	using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Text;
	using System.Xml.Serialization;

	using Terrasoft.Configuration.FileUpload;
	using Terrasoft.Web.Common.ServiceRouting;
	using Terrasoft.Web.Http.Abstractions;
	using Terrasoft.Web.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.Entities;
	using Terrasoft.Core.Factories;
	using Terrasoft.Core.DB;
	using Terrasoft.Core.Process;
	using Terrasoft.Core.Scheduler;
	using Terrasoft.Common;
	using Terrasoft.Mail.Sender;


	#region Service

	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class GKILicensingAdminService : BaseService
	{
		public GKILicensingAdminService() {}
		public GKILicensingAdminService(UserConnection userConnection)
        {
			UserConnection = userConnection;
		}

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

			var rootSchema = UserConnection.EntitySchemaManager.FindInstanceByName("GKILicUserInstanceLicPackage");
			var esqEnhanced = new EntitySchemaQuery(rootSchema);
			esqEnhanced.AddColumn("Id");
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
			var enhancedCollection = esqEnhanced.GetEntityCollection(UserConnection);
			GKIMakeThemChangeLicense(isAddLicense, enhancedCollection, GKIUrlClm, GKILicUserNameClm, GKILicPackageNameClm);
		}

		public bool GKILicUserSyncAll()
		{
			string successMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKILicUserSyncSuccess.Value");
			string processErrorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKILicUserSyncProcessError.Value");

				GKIGoMakeThemChangeLicenseQueueFilterMethod(null, null, null, "GKILicensingLicUserSyncProcess");

			return true;
		}

		public bool GKIGoMakeThemChangeLicenseQueueFilterMethod(List<Guid> licPackageIds, List<Guid> instanceIds, List<Guid> licUserIds,
			string processName = null)
		{
			string successMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKILicUserSyncSuccess.Value");
			string processErrorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKILicUserSyncProcessError.Value");
			try
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

				var rootSchema = UserConnection.EntitySchemaManager.FindInstanceByName("GKILicUserInstanceLicPackage");

				//GKIActive == true
				var esqEnhancedIsAdd = new EntitySchemaQuery(rootSchema);
				esqEnhancedIsAdd.AddColumn("Id");
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
				var enhancedIsAddCollection = esqEnhancedIsAdd.GetEntityCollection(UserConnection);

				//GKIActive == false
				var esqEnhancedIsRemove = new EntitySchemaQuery(rootSchema);
				esqEnhancedIsRemove.AddColumn("Id");
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
				var enhancedIsRemoveCollection = esqEnhancedIsRemove.GetEntityCollection(UserConnection);

				if (enhancedIsAddCollection.Count + enhancedIsRemoveCollection.Count > 0)
				{
					GKIMakeThemChangeLicense(true, enhancedIsAddCollection, GKIUrlClmIsAdd, GKILicUserNameClmIsAdd, GKILicPackageNameClmIsAdd);
					GKIMakeThemChangeLicense(false, enhancedIsRemoveCollection, GKIUrlClmIsRemove, GKILicUserNameClmIsRemove, GKILicPackageNameClmIsRemove);
				}
				if (processName != null && processName.Length > 0)
					SendLicAdminProcessReminding(processName, successMsg);
			}
			catch (Exception ex)
			{
				if (processName != null && processName.Length > 0)
					SendLicAdminProcessReminding(processName, processErrorMsg);
				throw ex;
			}
			return true;
		}
		
		#endregion

		#region UsersSync

		public void GKILicenseSyncRegularByUrl(string baseUrl)
		{
			var GKIInstanceESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
			GKIInstanceESQ.UseAdminRights = false;
			GKIInstanceESQ.AddAllSchemaColumns();
			GKIInstanceESQ.Filters.Add(
				GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIUrl", baseUrl));
			var GKIInstanceESQCollection = GKIInstanceESQ.GetEntityCollection(UserConnection);
			if (GKIInstanceESQCollection.Count > 0)
			{
				Entity GKIInstanceEntity = GKIInstanceESQCollection.First();
				GKILicenseSyncRegular(GKIInstanceEntity.GetTypedColumnValue<Guid>("Id"));
			}
			else
			{
				return;
			}
		}

		public void GKILicenseSyncRegular(Guid? instanceIdFilter)
		{
			string errorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKILicenseSyncRegularError.Value");
			string successMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKILicenseSyncRegularSuccess.Value");
			string processErrorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKILicenseSyncRegularProcessError.Value");

			string returnMsg = successMsg;
			try
			{
				List<LicInstanceSyncResult> licInstanceSyncResults = new List<LicInstanceSyncResult>();

				var instanceSchema = UserConnection.EntitySchemaManager.FindInstanceByName("GKIInstance");
				var esqInstance = new EntitySchemaQuery(instanceSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
				esqInstance.AddAllSchemaColumns();
				if (instanceIdFilter != null && instanceIdFilter != Guid.Empty)
				{
					esqInstance.Filters.Add(
						esqInstance.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", instanceIdFilter));
				}
				var instantEntities = esqInstance.GetEntityCollection(UserConnection);

				foreach (Entity instance in instantEntities)
				{
					string baseUrl = instance.GetTypedColumnValue<string>("GKIUrl");
					string serviceUrl = GKIUsersSyncServiceUrl;
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
						returnMsg = errorMsg;
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
						returnMsg = successMsg;
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
						returnMsg = errorMsg;
						continue;
					}
				}
				foreach (LicInstanceSyncResult licInstanceSyncResult in licInstanceSyncResults)
				{
					//instance sync result
					Update instanceUpdate = new Update(UserConnection, "GKIInstance")
						.Set("GKILastSyncSuccess", Column.Parameter(licInstanceSyncResult.Success))
						.Set("GKILastSyncError", Column.Parameter(licInstanceSyncResult.ErrMsg))
						.Where("Id").IsEqual(Column.Parameter(licInstanceSyncResult.InstanceId))
					as Update;
					instanceUpdate.Execute();
				}

				GKIGetInstallLicensesInfoAll();

				RemindingServerUtilities.CreateRemindingByProcess(UserConnection, "GKILicenseSyncRegularProcess", returnMsg);
			}
			catch(Exception ex)
            {
				RemindingServerUtilities.CreateRemindingByProcess(UserConnection, "GKILicenseSyncRegularProcess", processErrorMsg);
				throw ex;
			}
			return;
		}

		public void GKILicenseGetUsersToDeactivate(Guid? instanceIdFilter)
		{
			string processErrorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKILicenseGetUsersToDeactivateProcessError.Value");
			try
			{
				List<LicInstanceSyncResult> licInstanceSyncResults = new List<LicInstanceSyncResult>();

				var instanceSchema = UserConnection.EntitySchemaManager.FindInstanceByName("GKIInstance");
				var esqInstance = new EntitySchemaQuery(instanceSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
				esqInstance.AddAllSchemaColumns();
				if (instanceIdFilter != null && instanceIdFilter != Guid.Empty)
				{
					esqInstance.Filters.Add(
						esqInstance.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", instanceIdFilter));
				}
				var instantEntities = esqInstance.GetEntityCollection(UserConnection);

				foreach (Entity instance in instantEntities)
				{
					string baseUrl = instance.GetTypedColumnValue<string>("GKIUrl");
					string serviceUrl = GKIUsersSyncServiceUrl;
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
						string syncResult = GKILicenseGetUsersToDeactivateRequest(baseUrl, serviceUrl, instanceId);
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
					Update instanceUpdate = new Update(UserConnection, "GKIInstance")
						.Set("GKILastSyncSuccess", Column.Parameter(licInstanceSyncResult.Success))
						.Set("GKILastSyncError", Column.Parameter(licInstanceSyncResult.ErrMsg))
						.Where("Id").IsEqual(Column.Parameter(licInstanceSyncResult.InstanceId))
					as Update;
					instanceUpdate.Execute();
				}
			}
			catch (Exception ex)
			{
				RemindingServerUtilities.CreateRemindingByProcess(UserConnection, "GKILicenseGetUsersToDeactivateProcess", processErrorMsg);
				throw ex;
			}
			return;
		}


		#endregion

		#region Licensing
		[OperationContract]
		[WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
		public string GKIImportTlsFile(Stream fileContent)
        {
			try
			{
				GKIUserConnectionCheck();
				IFileUploadInfo fileUploadInfo = ClassFactory.Get<FileUploadInfo>(
					new ConstructorArgument("fileContent", fileContent),
					new ConstructorArgument("request", HttpContext.Current.Request),
					new ConstructorArgument("storage", UserConnection.Workspace.ResourceStorage));

				Stream fileStream = fileUploadInfo.Content;

				StreamReader streamReader = new StreamReader(fileStream);
				string tlsString = streamReader.ReadToEnd();
				TLSLicenses tlsLicenses = new TLSLicenses();

				fileStream.Seek(0, SeekOrigin.Begin);
				byte[] fileBytes = StreamToByteArray(fileStream);

				//validate the file
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
					throw new Exception("Wrong file content");
				}
				if (tlsLicenses.FileVersion == String.Empty || tlsLicenses.CustomerId == String.Empty)
				{
					throw new Exception("Wrong file content");
				}
				var update = new Update(UserConnection, "GKIInstance")
					.Set("GKITlsFile", Column.Parameter(fileBytes))
					.Where("GKICustomerIDId").In(
						new Select(UserConnection)
						.Column("Id")
						.From("GKICustomerID")
						.Where("Name")
						.IsEqual(Column.Parameter(tlsLicenses.CustomerId))
					).And("GKIVersion")
					.IsEqual(Column.Parameter(tlsLicenses.FileVersion)) as Update;
				int recordsUpdated = update.Execute();
				if (recordsUpdated == 0)
				{
					throw new Exception("No suitable instances");
				}
			}
			catch(Exception ex)
            {
				return ex.Message;
			}
			return String.Empty;
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
			ResponseFormat = WebMessageFormat.Json)]
		public string GKITlsInstall(Guid recordId)
		{
			GKIUserConnectionCheck();

			string returnMsg = "";
			string errorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
					"GKILicensingServices",
					"LocalizableStrings.GKITlsInstallError.Value");
			string successMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
					"GKILicensingServices",
					"LocalizableStrings.GKITlsInstallSuccess.Value");

			var GKIInstanceESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
			GKIInstanceESQ.UseAdminRights = false;
			string idColumnName = GKIInstanceESQ.AddColumn("Id").Name;
			GKIInstanceESQ.AddColumn("GKIUrl");
			GKIInstanceESQ.AddColumn("GKITlsFile");
			var customerNameClm = GKIInstanceESQ.AddColumn("GKICustomerID.Name").Name;
			GKIInstanceESQ.Filters.Add(
				GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", recordId));
			var ESQCollection = GKIInstanceESQ.GetEntityCollection(UserConnection);

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
						string installationResult = GKILicenseHttpRequest(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), GKITlsInstallRegularServiceUrl, licMessage);
						TlsCustomInstallResult tlsInstallResult = JsonConvert.DeserializeObject<TlsCustomInstallResult>(installationResult);
						if (tlsInstallResult.success != true)
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

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
			ResponseFormat = WebMessageFormat.Json)]
		public bool GKITlsDownloadEnable()
        {
			try
			{
				GKIUserConnectionCheck();
				var systemUserConn = UserConnection.AppConnection.SystemUserConnection;
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
		public string GKITlrRequestByCustomerID(Guid CustomerId)
		{
			GKIUserConnectionCheck();
			string returnMsg = "";
			string errorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKITlrRequestError.Value");
			string successMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKITlrRequestSuccess.Value");
			string processErrorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKITlrRequestProcessError.Value");
			string wrongCIDErrorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKITlrRequestWrongCIDError.Value");
			string emailNotSentErrorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKITlrRequestEmailNotSentError.Value");
			

			returnMsg = successMsg;

			try
			{
				Dictionary<string, byte[]> versionsList = new Dictionary<string, byte[]>();

				var GKIInstanceESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
				GKIInstanceESQ.UseAdminRights = false;
				string idColumnName = GKIInstanceESQ.AddColumn("Id").Name;
				GKIInstanceESQ.AddColumn("GKIUrl");
				GKIInstanceESQ.AddColumn("GKIVersion");
				var cidColumn = GKIInstanceESQ.AddColumn("GKICustomerID.Name");
				GKIInstanceESQ.Filters.Add(
					GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", CustomerId));
				var GKIInstanceESQCollection = GKIInstanceESQ.GetEntityCollection(UserConnection);

				foreach (Entity GKIInstanceEntity in GKIInstanceESQCollection)
				{
					GKIWriteGKIInstanceTlrInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), String.Empty);
					string baseUrl = GKIInstanceEntity.GetTypedColumnValue<string>("GKIUrl");
					//url format validation:
					Uri uriResult;
					bool uriCheckResult = Uri.TryCreate(baseUrl, UriKind.Absolute, out uriResult)
						&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

					if (baseUrl != String.Empty && uriCheckResult)
					{
						try
						{
							string fileString = GKILicenseHttpGetRequest(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), GKITlrRequestUrl);
							TLRLicenses tlrLicenses;
							string version = GKIInstanceEntity.GetTypedColumnValue<string>("GKIVersion");
							XmlSerializer serializer = new XmlSerializer(typeof(TLRLicenses));
							using (var reader = new StringReader(fileString))
							{
								tlrLicenses = (TLRLicenses)serializer.Deserialize(reader);
							}
							if (tlrLicenses.CustomerId != GKIInstanceEntity.GetTypedColumnValue<string>(cidColumn.Name))
                            {
								throw new Exception(wrongCIDErrorMsg);
							}
							
							byte[] tlrFile = Encoding.UTF8.GetBytes(fileString);
							var update = new Update(UserConnection, "GKIInstance")
								.Set("GKITlrFile", Column.Parameter(tlrFile))
								.Set("GKIVersion", Column.Parameter(tlrLicenses.FileVersion))
								.Where("Id").IsEqual(Column.Parameter(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName))) as Update;
							int recordsUpdated = update.Execute();
							if (recordsUpdated == 0)
							{
								throw new Exception("Loading tlr failure");
							}

							if (!versionsList.ContainsKey(tlrLicenses.FileVersion))
							{
								versionsList.Add(tlrLicenses.FileVersion, tlrFile);
							}

							//update or insert GKIInstanceFile's LicenseRequest.tlr
							var updateGKIInstanceFile = new Update(UserConnection, "GKIInstanceFile")
								.Set("Data", Column.Parameter(tlrFile))
								.Where("GKIInstanceId").IsEqual(Column.Parameter(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName)))
								.And("Name").IsEqual(Column.Parameter("LicenseRequest.tlr")) as Update;
							int filesGKIInstanceFileUpdated = updateGKIInstanceFile.Execute();
							if (filesGKIInstanceFileUpdated == 0)
                            {
								var esqGKIInstanceFileSchema = UserConnection.EntitySchemaManager.GetInstanceByName("GKIInstanceFile");
								var esqGKIInstanceFileEntity = esqGKIInstanceFileSchema.CreateEntity(UserConnection);
								esqGKIInstanceFileEntity.SetDefColumnValues();
								esqGKIInstanceFileEntity.SetColumnValue("GKIInstanceId", GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName));
								esqGKIInstanceFileEntity.SetColumnValue("TypeId", Terrasoft.WebApp.FileConsts.FileTypeUId);
								esqGKIInstanceFileEntity.SetColumnValue("Name", "LicenseRequest.tlr");
								esqGKIInstanceFileEntity.SetColumnValue("Data", tlrFile);
								esqGKIInstanceFileEntity.Save();
							}
						}
						catch (Exception ex)
						{
							GKIWriteGKIInstanceTlrInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), ex.Message);
							returnMsg = errorMsg;
							continue;
						}
					}
					else
					{
						GKIWriteGKIInstanceTlrInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), "Invalid or empty URL");
						returnMsg = errorMsg;
						continue;
					}

					GKIWriteGKIInstanceTlrInstallError(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), String.Empty);
					continue;
				}
				//sending an email request to tech support
				if (returnMsg == successMsg)
				{
					try
					{
						List<byte[]> filesData = (from kvp in versionsList select kvp.Value).Distinct().ToList();
						GKISendTlrEmail(filesData);
					}
					catch (Exception ex)
					{
						//we fetch them again to not overwrite previous error messages
						var secondGKIInstanceESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
						secondGKIInstanceESQ.UseAdminRights = false;
						string secondIdColumnName = secondGKIInstanceESQ.AddColumn("Id").Name;
						secondGKIInstanceESQ.AddColumn("GKILastTlrInstallError");
						secondGKIInstanceESQ.Filters.Add(
							secondGKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", CustomerId));
						secondGKIInstanceESQ.Filters.Add(
							secondGKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILastTlrInstallError", String.Empty));
						var secondGKIInstanceESQCollection = secondGKIInstanceESQ.GetEntityCollection(UserConnection);
						foreach (Entity secondGKIInstanceEntity in secondGKIInstanceESQCollection)
						{
							GKIWriteGKIInstanceTlrInstallError(secondGKIInstanceEntity.GetTypedColumnValue<Guid>(secondIdColumnName),
								String.Concat(emailNotSentErrorMsg, ". ", ex.Message));
						}
						returnMsg = errorMsg;
					}
				}
				return returnMsg;
			}
			catch (Exception ex)
			{
				RemindingServerUtilities.CreateRemindingByProcess(UserConnection, "GKITlrRequestProcess", processErrorMsg);
				throw ex;
			}
		}

		#endregion

		#region Pulse

		public void GKIPulseStart()
		{
			AppScheduler.ScheduleMinutelyProcessJob("GKILicenseCheckThePulseProcess", "GKILicensePulse", "GKILicenseCheckThePulseProcess",
				UserConnection.Workspace.Name, UserConnection.AppConnection.SystemUserConnection.CurrentUser.Name, 1, null, true, true);
		}
		public void GKIPulseTimerInit()
        {
			int secondsInterval = Terrasoft.Core.Configuration.SysSettings.GetValue(UserConnection, "GKILicensingAdminToRegularRequestTimer", 0);
			bool isPulseActive = secondsInterval > 0;
			if (isPulseActive)
			{
				GKIPulseTimer = new System.Timers.Timer(secondsInterval * 1000);
				GKIPulseTimer.Elapsed += (sender, e) => GKIPulseBeatBusinessProcessInit();
				GKIPulseTimer.Enabled = true;
			}
		}
		public void GKIPulseBeatBusinessProcessInit()
        {
			ProcessSchema schema = UserConnection.AppConnection.SystemUserConnection.ProcessSchemaManager.GetInstanceByName("GKILicensingPulseBusinessProcess");
			var flowEngine = new FlowEngine(UserConnection.AppConnection.SystemUserConnection);
			var param = new Dictionary<string, string>();
			flowEngine.RunProcess(schema, param);
		}

		public void GKIPulseBeat()
		{
			string returnMsg = String.Empty;

			var esqGKIInstance = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
			esqGKIInstance.UseAdminRights = false;
			string idColumnName = esqGKIInstance.AddColumn("Id").Name;
			esqGKIInstance.AddColumn("GKIUrl");
			esqGKIInstance.AddColumn("GKIName");
			esqGKIInstance.AddColumn("GKICustomerID");
			var esqGKIInstanceCollection = esqGKIInstance.GetEntityCollection(UserConnection);

			foreach (Entity esqGKIInstanceEntity in esqGKIInstanceCollection)
			{
				string instanceName = esqGKIInstanceEntity.GetTypedColumnValue<string>("GKIName");
				string baseUrl = esqGKIInstanceEntity.GetTypedColumnValue<string>("GKIUrl");
				Guid instanceId = esqGKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName);
				//url format validation:
				Uri uriResult;
				bool uriCheckResult = Uri.TryCreate(baseUrl, UriKind.Absolute, out uriResult)
					&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

				if (baseUrl != String.Empty && uriCheckResult)
				{
					try
					{
						//get VIP limits
						PulseOutgoingData pulseOutgoingData = new PulseOutgoingData();
						pulseOutgoingData.vipLicensesLimits = GetVIPLicensesLimits(instanceId);

						//get lastMasterCheckInWaitMinutes
						pulseOutgoingData.lastMasterCheckInWaitMinutes = Core.Configuration.SysSettings.GetValue<int>(
							UserConnection, "GKILastMasterCheckInWaitMinutes", GKILicensingConstantsCs.Misc.lastMasterCheckInWaitMinutes);

						//JsonConverter is serializing dictionaries inproperly!
						var stream = new MemoryStream();
						var serializer = new DataContractJsonSerializer(typeof(PulseOutgoingData));
						serializer.WriteObject(stream, pulseOutgoingData);
						stream.Position = 0;
						var streamReader = new StreamReader(stream);
						string message = streamReader.ReadToEnd();

						//send pulse data
						PulseData pulseData = JsonConvert.DeserializeObject<PulseData>(
							GKILicenseHttpRequest(instanceId, GKIPulseUrl, message));
						if (pulseData.vipLimitsErrorMsg != String.Empty)
						{
							returnMsg += String.Concat("\n", instanceName, ": VIP license error: ", pulseData.vipLimitsErrorMsg, ". ");
						}

						if (pulseData.isSuccess != true)
                        {
							returnMsg += String.Concat("\n", instanceName, ": ", pulseData.errorMsg, ". ");
							continue;
						}

						// request for licensing
						if (pulseData.pulseLicUserNames.Count > 0)
                        {
							PulseLicensing(pulseData, esqGKIInstanceEntity, instanceId, baseUrl);
						}
					}
					catch (Exception ex)
					{
						returnMsg += String.Concat("\n", instanceName, ": ", ex.Message, ". ");
						continue;
					}
				}
				else
				{
					returnMsg += String.Concat("\n", instanceName, ": Invalid or empty URL. ");
					continue;
				}
				continue;
			}
			if (returnMsg.Length > 0)
            {
				throw new Exception(returnMsg);
            }
			return;
		}
		public void GKILicenseCheckThePulse()
		{
			if (GKIPulseTimer == null)
			{
				GKIPulseTimerInit();
			}
		}
		public void GKIPulseStop()
		{
			AppScheduler.RemoveJob("GKILicenseCheckThePulseProcess", "GKILicensePulse");
			if (GKIPulseTimer != null)
			{
				GKIPulseTimer.Stop();
				GKIPulseTimer.Dispose();
				GKIPulseTimer = null;
			}
		}

        #endregion

        #region VIP users

		public void GKIGetVIPUsers()
        {
			string returnMsg = String.Empty;

			var esqGKIInstance = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
			esqGKIInstance.UseAdminRights = false;
			string idColumnName = esqGKIInstance.AddColumn("Id").Name;
			esqGKIInstance.AddColumn("GKIUrl");
			esqGKIInstance.AddColumn("GKIName");
			esqGKIInstance.AddColumn("GKICustomerID");
			var esqGKIInstanceCollection = esqGKIInstance.GetEntityCollection(UserConnection);

			foreach (Entity esqGKIInstanceEntity in esqGKIInstanceCollection)
			{
				string instanceName = esqGKIInstanceEntity.GetTypedColumnValue<string>("GKIName");
				string baseUrl = esqGKIInstanceEntity.GetTypedColumnValue<string>("GKIUrl");
				Guid instanceId = esqGKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName);
				//url format validation:
				Uri uriResult;
				bool uriCheckResult = Uri.TryCreate(baseUrl, UriKind.Absolute, out uriResult)
					&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

				if (baseUrl != String.Empty && uriCheckResult)
				{
					try
					{
						GKIVIPUsers vipUsers;
						string response = GKILicenseHttpRequest(instanceId, GKIVIPUsersUrl);
						using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(response)))
						{
							DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(GKIVIPUsers));
							vipUsers = (GKIVIPUsers)deserializer.ReadObject(ms);
						}

						if (vipUsers.isSuccess != true)
						{
							returnMsg += String.Concat("\n", instanceName, ": ", vipUsers.errorMsg, ". ");
							continue;
						}

						//each limit license
						Dictionary<string, int> vipLimits = GetVIPLicensesLimits(instanceId);

						foreach (var productName in vipLimits.Keys)
                        {
							SetVIPLicenses(productName, vipUsers.vipUsers.ContainsKey(productName) ? vipUsers.vipUsers[productName] : null, instanceId);
							ValidateVIPLimits(instanceId, productName);
                        }
					}
					catch (Exception ex)
					{
						returnMsg += String.Concat("\n", instanceName, ": ", ex.Message, ". ");
						continue;
					}
				}
				else
				{
					returnMsg += String.Concat("\n", instanceName, ": Invalid or empty URL. ");
					continue;
				}
				continue;
			}
			if (returnMsg.Length > 0)
			{
				throw new Exception(returnMsg);
			}
			return;
		}

		#endregion

		#region Public Methods
		public void GKIDeactivatedUsersUpdate(List<Guid> deactivateLicUsers, Guid instanceId, Guid reason)
		{
			IEnumerable<QueryParameter> allDeactivateLicUsersIdsEnum = deactivateLicUsers.ConvertAll(x => new QueryParameter(x));
			if (allDeactivateLicUsersIdsEnum.Count() > 0)
			{
				Update updateLicensesInactive = new Update(UserConnection, "GKILicUserInstanceLicPackage")
					.Set("GKIActive", Column.Parameter(false))
					.Set("GKIDeactivatedBySync", Column.Parameter(true))
					.Set("GKIDeactivationReasonLookupId", Column.Parameter(reason))
					.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
					.And("GKILicUserId").In(allDeactivateLicUsersIdsEnum)
					.And()
						.OpenBlock("GKIActive").IsEqual(Column.Parameter(true))
						.Or("GKISyncedState").IsEqual(Column.Parameter(true))
					.CloseBlock()
				as Update;
				updateLicensesInactive.Execute();

				var insertIntoQueue = new InsertSelect(UserConnection)
					.Into("GKILicUserInstanceLicPackageQueue")
					.Set("GKILicUserInstanceLicPackageId")
					.FromSelect(
						new Select(UserConnection)
							.Column("Id")
								.From("GKILicUserInstanceLicPackage")
								.Where("GKIDeactivatedBySync").IsEqual(Column.Parameter(true))
								.And("GKIActive").IsEqual(Column.Parameter(false))
								.And("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
								.And("GKILicUserId").In(allDeactivateLicUsersIdsEnum)
								.And("Id").Not().In(new Select(UserConnection)
									.Column("GKILicUserInstanceLicPackageId")
									.From("GKILicUserInstanceLicPackageQueue"))
									as Select);
				insertIntoQueue.Execute();
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
			ResponseFormat = WebMessageFormat.Json)]
		public string GKINewInstancePassword(Guid instanceId, string password)
		{
			try
			{
				var securityEngine = UserConnection.DBSecurityEngine;
				securityEngine.CheckCanExecuteOperation("GKICanManageLicensingSettings");

				var schemaGKIInstance = UserConnection.EntitySchemaManager.GetInstanceByName("GKIInstance");
				Entity entityGKIInstance = schemaGKIInstance.CreateEntity(UserConnection);
				if (entityGKIInstance.FetchFromDB("Id", instanceId))
				{
					entityGKIInstance.SetColumnValue("GKIPassword", password);
					entityGKIInstance.Save();
				}
				else
                {
					throw new NullOrEmptyException();
                }
			}
			catch(Exception ex)
            {
				return ex.Message;
            }
			return String.Empty;
		}

		public void GKISlaveAndADNotInSync()
        {
			Guid mailboxId = Terrasoft.Core.Configuration.SysSettings.GetValue(UserConnection, "GKILicensingMailBox", Guid.Empty);
			GetSubjectAndBodyFromEmailTemplate(GKILicensingConstantsCs.Misc.SlaveAndADNotInSyncEmailTemplate, out string emailSubject, out string emailBody);
			var esqGKIInstance = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
			esqGKIInstance.UseAdminRights = false;
			esqGKIInstance.AddColumn("Id");
			esqGKIInstance.AddColumn("GKIAdminEmail");
			esqGKIInstance.AddColumn("GKIName");
			esqGKIInstance.Filters.Add(
				esqGKIInstance.CreateFilterWithParameters(FilterComparisonType.Equal, "[GKIInstanceLicUser:GKIInstance:Id].GKIMSADActive", true));
			esqGKIInstance.Filters.Add(
				esqGKIInstance.CreateFilterWithParameters(FilterComparisonType.Equal, "[GKIInstanceLicUser:GKIInstance:Id].GKIActive", false));
				
			var esqGKIInstanceCollection = esqGKIInstance.GetEntityCollection(UserConnection);
			foreach(var instanceEntity in esqGKIInstanceCollection)
            {
				if (instanceEntity.GetTypedColumnValue<string>("GKIAdminEmail").Length > 0)
				{
					string instanceEmailBody = emailBody.Replace("[#GKIName#]", instanceEntity.GetTypedColumnValue<string>("GKIName"));
					GKISendActivityEmail(UserConnection, mailboxId, instanceEntity.GetTypedColumnValue<string>("GKIAdminEmail"), emailSubject, instanceEmailBody, null);
				}
            }
		}

		#endregion

		#region Private Methods
		public void CreateReminding(string remindingSubject, string schemaName, Guid contactId, Guid? recordId = null)
		{
			Reminding remindingEntity = new Reminding(UserConnection);
			var manager = UserConnection.GetSchemaManager("EntitySchemaManager");
			var targetSchema = manager.FindItemByName(schemaName);
			DateTime currentDateTime = UserConnection.CurrentUser.GetCurrentDateTime();
			Guid userContactId = UserConnection.CurrentUser.ContactId;
			remindingEntity.SetDefColumnValues();
			if (!recordId.IsNullOrEmpty())
			{
				remindingEntity.SetColumnValue("SubjectId", recordId);
			}
			remindingEntity.SetColumnValue("ModifiedOn", currentDateTime);
			remindingEntity.SetColumnValue("AuthorId", userContactId);
			remindingEntity.SetColumnValue("ContactId", contactId);
			remindingEntity.SetColumnValue("SourceId", RemindingConsts.RemindingSourceAuthorId);
			remindingEntity.SetColumnValue("RemindTime", currentDateTime);
			remindingEntity.SetColumnValue("Description", remindingSubject);
			remindingEntity.SetColumnValue("SubjectCaption", remindingSubject);
			remindingEntity.SetColumnValue("NotificationTypeId", RemindingConsts.NotificationTypeNotificationId);
			remindingEntity.SetColumnValue("SysEntitySchemaId", targetSchema.UId);
			remindingEntity.SetColumnValue("IsRead", false);
			remindingEntity.Save();
		}
		private void CreateRemindingByProcess(Guid contactId, string processName, string subject,
				string description = null)
		{
			ProcessSchema processSchema = UserConnection.ProcessSchemaManager.FindInstanceByName(processName);
			EntitySchema processLogSchema = UserConnection.EntitySchemaManager.FindInstanceByName("VwSysProcessLog");
			Guid processLogRecordId = GetProcessLogRecordId(processSchema);
			if (processLogRecordId == Guid.Empty)
			{
				return;
			}
			Entity remindingEntity = UserConnection.EntitySchemaManager.GetInstanceByName("Reminding")
				.CreateEntity(UserConnection);
			remindingEntity.SetDefColumnValues();
			remindingEntity.SetColumnValue("AuthorId", UserConnection.CurrentUser.ContactId);
			remindingEntity.SetColumnValue("ContactId", contactId);
			remindingEntity.SetColumnValue("SourceId", RemindingConsts.RemindingSourceAuthorId);
			remindingEntity.SetColumnValue("RemindTime", UserConnection.CurrentUser.GetCurrentDateTime());
			remindingEntity.SetColumnValue("Description", description);
			remindingEntity.SetColumnValue("SubjectCaption", subject);
			remindingEntity.SetColumnValue("SysEntitySchemaId", processLogSchema.UId);
			remindingEntity.SetColumnValue("SubjectId", processLogRecordId);
			remindingEntity.Save();
		}
		private Guid GetProcessLogRecordId(ProcessSchema processSchema)
		{
			Guid recorId = Guid.Empty;
			var subSelect = new Select(UserConnection)
							.Column("SPS", "Id")
							.From("SysProcessStatus").As("SPS")
							.Where("SPS", "Id").IsEqual("SPL", "StatusId")
							.And("SPS", "Value").IsEqual(Column.Parameter(1));
			Select select = new Select(UserConnection).Top(1)
							.Column("SPL", "Id").As("RecordId")
							.From("SysProcessLog").As("SPL")
							.Where("SPL", "SysWorkspaceId").IsEqual(Column.Parameter(UserConnection.Workspace.Id))
							.And("SPL", "OwnerId").IsEqual(Column.Parameter(UserConnection.CurrentUser.ContactId))
							.And("SPL", "Name").IsEqual(Column.Parameter(processSchema.Caption.ToString()))
							.And().Exists(subSelect)
							.OrderBy(OrderDirectionStrict.Descending, "SPL", "CreatedOn") as Select;
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
			{
				using (IDataReader reader = select.ExecuteReader(dbExecutor))
				{
					if (reader.Read())
					{
						recorId = reader.GetColumnValue<Guid>("RecordId");
					}
				}
			}
			return recorId;
		}

		private void SendLicAdminReminding(string message, string schemaName, Guid? recordId = null)
        {
			var esqSysAdminUnitCollection = GetLicAdminCollection(out string contactColumnName);
			foreach (var entitySysAdminUnit in esqSysAdminUnitCollection)
            {
				CreateReminding(message, schemaName, entitySysAdminUnit.GetTypedColumnValue<Guid>(contactColumnName), recordId);
			}
		}

		private void SendLicAdminProcessReminding(string processName, string message)
		{
			var esqSysAdminUnitCollection = GetLicAdminCollection(out string contactColumnName);
			foreach (var entitySysAdminUnit in esqSysAdminUnitCollection)
			{
				CreateRemindingByProcess(entitySysAdminUnit.GetTypedColumnValue<Guid>(contactColumnName), processName, message);
			}
		}
		private EntityCollection GetLicAdminCollection(out string contactColumnName)
        {
			var esqSysAdminUnit = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysAdminUnitInRole");
			esqSysAdminUnit.UseAdminRights = false;
			//aggregation column for a "group by" in the query
			esqSysAdminUnit.AddColumn(esqSysAdminUnit.CreateAggregationFunction(AggregationTypeStrict.Count,
				"Id"));
			var contactColumn = esqSysAdminUnit.AddColumn("SysAdminUnit.Contact.Id");
			contactColumnName = contactColumn.Name;
			esqSysAdminUnit.Filters.Add(
				esqSysAdminUnit.CreateFilterWithParameters(FilterComparisonType.Equal, "SysAdminUnitRoleId",
					GKILicensingConstantsCs.SysAdminUnitRole.LicAdmin));
			var esqSysAdminUnitCollection = esqSysAdminUnit.GetEntityCollection(UserConnection);
			return esqSysAdminUnitCollection;
		}
		private void ValidateVIPLimits(Guid instanceId, string productName)
        {
			Dictionary<string, int> vipLimits = GetVIPLicensesLimits(instanceId);

			var esqGKILicUserInstanceLicPackage = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKILicUserInstanceLicPackage");
			esqGKILicUserInstanceLicPackage.UseAdminRights = false;
			var countUsedColumn = esqGKILicUserInstanceLicPackage.AddColumn(
				esqGKILicUserInstanceLicPackage.CreateAggregationFunction(AggregationTypeStrict.Count, "Id")).Name;
			esqGKILicUserInstanceLicPackage.Filters.Add(
				esqGKILicUserInstanceLicPackage.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
			esqGKILicUserInstanceLicPackage.Filters.Add(
				esqGKILicUserInstanceLicPackage.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIReserved", true));
			esqGKILicUserInstanceLicPackage.Filters.Add(
				esqGKILicUserInstanceLicPackage.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage.GKIName", productName));
			var esqGKILicUserInstanceLicPackageCollection = esqGKILicUserInstanceLicPackage.GetEntityCollection(UserConnection);
			int? actuallyReserved = esqGKILicUserInstanceLicPackageCollection.FirstOrDefault()?.GetTypedColumnValue<int>(countUsedColumn);
			//is limit exceeded?
			if (actuallyReserved != null && actuallyReserved > 0 && (!vipLimits.ContainsKey(productName) || vipLimits[productName] < actuallyReserved))
            {
				var schemaGKIInstance = UserConnection.EntitySchemaManager.GetInstanceByName("GKIInstance");
				Entity entityGKIInstance = schemaGKIInstance.CreateEntity(UserConnection);
				if (entityGKIInstance.FetchFromDB("Id", instanceId))
				{
					SendLicAdminReminding(String.Format(new LocalizableString(UserConnection.Workspace.ResourceStorage,
							"GKILicensingServices",
							"LocalizableStrings.GKIVIPLimitIsExceededMessage.Value"), 
						productName, 
						entityGKIInstance.GetTypedColumnValue<string>("GKIName")), "GKIInstance", instanceId);
				}
            }
		}
		private void SetVIPLicenses(string productName, List<string> vipUsersNames, Guid instanceId)
        {
			if (vipUsersNames == null)
			{
				Update updateVIPLicensesNotReservedToAll = new Update(UserConnection, "GKILicUserInstanceLicPackage")
					.Set("GKIReserved", Column.Parameter(false))
					.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
					.And("GKILicPackageId").In(
						new Select(UserConnection)
						.Column("Id")
						.From("GKILicPackage")
						.Where("GKIName").IsEqual(Column.Parameter(productName))
					)
					.And("GKIReserved").IsEqual(Column.Parameter(true))
				as Update;
				updateVIPLicensesNotReservedToAll.Execute();
			}
			else
			{
				IEnumerable<QueryParameter> enumVIPUserNames = vipUsersNames.ConvertAll(x => new QueryParameter(x));

				Update updateVIPLicensesReserved = new Update(UserConnection, "GKILicUserInstanceLicPackage")
					.Set("GKIReserved", Column.Parameter(true))
					.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
					.And("GKILicPackageId").In(
						new Select(UserConnection)
						.Column("Id")
						.From("GKILicPackage")
						.Where("GKIName").IsEqual(Column.Parameter(productName))
					)
					.And("GKIReserved").IsEqual(Column.Parameter(false))
					.And("GKILicUserId").In(
						new Select(UserConnection)
						.Column("Id")
						.From("GKILicUser")
						.Where("GKIName").In(enumVIPUserNames)
					)
				as Update;
				updateVIPLicensesReserved.Execute();

				Update updateVIPLicensesNotReserved = new Update(UserConnection, "GKILicUserInstanceLicPackage")
					.Set("GKIReserved", Column.Parameter(false))
					.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
					.And("GKILicPackageId").In(
						new Select(UserConnection)
						.Column("Id")
						.From("GKILicPackage")
						.Where("GKIName").IsEqual(Column.Parameter(productName))
					)
					.And("GKIReserved").IsEqual(Column.Parameter(true))
					.And("GKILicUserId").In(
						new Select(UserConnection)
						.Column("Id")
						.From("GKILicUser")
						.Where("GKIName").Not().In(enumVIPUserNames)
					)
				as Update;
				updateVIPLicensesNotReserved.Execute();
			}
		}

		private void PulseLicensing(PulseData pulseData, Entity esqGKIInstanceEntity, Guid instanceId, string baseUrl)
        {
			List<LicUserData> licUserData = new List<LicUserData>();
			object[] pulseLicUserNamesParams = pulseData.pulseLicUserNames.Cast<object>().ToArray();
			List<QueryParameter> recordsToActivate = new List<QueryParameter>();

			//licenses available
			Dictionary<string, int> licPackageAvailable = GetAvailableLicenses(esqGKIInstanceEntity.GetTypedColumnValue<Guid>("GKICustomerIDId"));

			var esqGKILicUserInstanceLicPackage = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKILicUserInstanceLicPackage");
			esqGKILicUserInstanceLicPackage.UseAdminRights = false;
			var idCol = esqGKILicUserInstanceLicPackage.AddColumn("Id");
			var userNameCol = esqGKILicUserInstanceLicPackage.AddColumn("GKILicUser.GKIName");
			var packNameCol = esqGKILicUserInstanceLicPackage.AddColumn("GKILicPackage.GKIName");
			esqGKILicUserInstanceLicPackage.AddColumn("GKIReserved");
			esqGKILicUserInstanceLicPackage.AddColumn("GKILicUser");
			esqGKILicUserInstanceLicPackage.AddColumn("GKILicPackage");
			esqGKILicUserInstanceLicPackage.AddColumn("GKIInstance");

			esqGKILicUserInstanceLicPackage.Filters.Add(
				esqGKILicUserInstanceLicPackage.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
			esqGKILicUserInstanceLicPackage.Filters.Add(
				esqGKILicUserInstanceLicPackage.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser.GKIName", pulseLicUserNamesParams));
			esqGKILicUserInstanceLicPackage.Filters.Add(
				esqGKILicUserInstanceLicPackage.CreateExistsFilter("[GKILic:GKILicPackage:GKILicPackage].Id"));
			esqGKILicUserInstanceLicPackage.Filters.Add(
				esqGKILicUserInstanceLicPackage.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance.GKICustomerID",
					esqGKIInstanceEntity.GetTypedColumnValue<Guid>("GKICustomerIDId")));

			var esqGKILicUserInstanceLicPackageCollection = esqGKILicUserInstanceLicPackage.GetEntityCollection(UserConnection);
			if (esqGKILicUserInstanceLicPackageCollection.Count > 0)
			{
				foreach (var esqGKILicUserInstanceLicPackageEntity in esqGKILicUserInstanceLicPackageCollection)
				{
					Guid recordId = esqGKILicUserInstanceLicPackageEntity.GetTypedColumnValue<Guid>(idCol.Name);
					string userName = esqGKILicUserInstanceLicPackageEntity.GetTypedColumnValue<string>(userNameCol.Name);
					string packName = esqGKILicUserInstanceLicPackageEntity.GetTypedColumnValue<string>(packNameCol.Name);
					if (esqGKILicUserInstanceLicPackageEntity.GetTypedColumnValue<bool>("GKIReserved"))
					{
						licUserData.Add(new LicUserData { LicUserName = userName, LicPackageName = packName });
						recordsToActivate.Add(new QueryParameter(recordId));
						licPackageAvailable[packName] += -1;
					}
					else
					{
						bool isEligible = GetUserGroupADLicenseEligible(esqGKILicUserInstanceLicPackageEntity);
						if (licPackageAvailable[packName] > 0 && isEligible)
						{
							licUserData.Add(new LicUserData { LicUserName = userName, LicPackageName = packName });
							recordsToActivate.Add(new QueryParameter(recordId));
							licPackageAvailable[packName] += -1;
						}
					}
				}

				//activate records
				if (recordsToActivate.Count > 0)
				{
					IEnumerable<QueryParameter> allActivateRecordsEnum = recordsToActivate.AsEnumerable();

					Update updateLicensesActive = new Update(UserConnection, "GKILicUserInstanceLicPackage")
						.Set("GKIActive", Column.Parameter(true))
						.Where("Id").In(allActivateRecordsEnum)
					as Update;
					updateLicensesActive.Execute();
				}
				//request to add licenses
				if (licUserData.Count > 0)
				{
					GKILicenseAssignmentRequest(instanceId, true, licUserData);
				}
			}
		}

		private bool GetUserGroupADLicenseEligible(Entity esqGKILicUserInstanceLicPackageEntity)
        {
			var esqGKIGroupAD = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIGroupAD");
			esqGKIGroupAD.UseAdminRights = false;
			esqGKIGroupAD.AddColumn("Id");
			esqGKIGroupAD.Filters.Add(
				esqGKIGroupAD.CreateFilterWithParameters(FilterComparisonType.Equal, "[GKIGroupADUsers:GKIGroupAD:Id].GKIInstance", 
					esqGKILicUserInstanceLicPackageEntity.GetTypedColumnValue<Guid>("GKIInstanceId")));
			esqGKIGroupAD.Filters.Add(
				esqGKIGroupAD.CreateFilterWithParameters(FilterComparisonType.Equal, "[GKIGroupADUsers:GKIGroupAD:Id].GKILicUser",
					esqGKILicUserInstanceLicPackageEntity.GetTypedColumnValue<Guid>("GKILicUserId")));
			esqGKIGroupAD.Filters.Add(
				esqGKIGroupAD.CreateFilterWithParameters(FilterComparisonType.Equal, "[GKIGroupADInstanceLicense:GKIGroupAD:Id].GKILicPackage",
					esqGKILicUserInstanceLicPackageEntity.GetTypedColumnValue<Guid>("GKILicPackageId")));
			var esqGKIGroupADCollection = esqGKIGroupAD.GetEntityCollection(UserConnection);
			bool result = esqGKIGroupADCollection != null && esqGKIGroupADCollection.Count > 0 ? true : false;
			return result;
        }

		private Dictionary<string, int> GetVIPLicensesLimits(Guid instanceId)
        {
			Dictionary<string, int> vipLicensesLimits = new Dictionary<string, int>();
			var esqGKIInstanceLicense = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstanceLicense");
			esqGKIInstanceLicense.UseAdminRights = false;
			var columnGKILicPackageName = esqGKIInstanceLicense.AddColumn("GKILic.GKILicPackage.GKIName");
			esqGKIInstanceLicense.AddColumn("GKILimitVIP");
			esqGKIInstanceLicense.Filters.Add(
				esqGKIInstanceLicense.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
			esqGKIInstanceLicense.Filters.Add(
				esqGKIInstanceLicense.CreateFilterWithParameters(FilterComparisonType.Greater, "GKILimitVIP", 0));
			esqGKIInstanceLicense.Filters.Add(
				esqGKIInstanceLicense.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILic.GKILicStatus", GKILicensingConstantsCs.GKILicStatus.Active));
			var esqGKIInstanceLicenseCollection = esqGKIInstanceLicense.GetEntityCollection(UserConnection);
			foreach (var esqGKIInstanceLicenseEntity in esqGKIInstanceLicenseCollection)
            {
				vipLicensesLimits[esqGKIInstanceLicenseEntity.GetTypedColumnValue<string>(columnGKILicPackageName.Name)] =
					esqGKIInstanceLicenseEntity.GetTypedColumnValue<int>("GKILimitVIP");
			}

			return vipLicensesLimits;
		}

		private Dictionary<string, int> GetAvailableLicenses(Guid GKICustomerIDId)
        {
			Dictionary<string, int> licPackageAvailable = new Dictionary<string, int>();
			var licESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKILic");
			licESQ.UseAdminRights = false;
			var licPackageName = licESQ.AddColumn("GKILicPackage.GKIName");
			licESQ.AddColumn("GKIAvailableCount");
			licESQ.Filters.Add(
				licESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", GKICustomerIDId));
			var licESQCollection = licESQ.GetEntityCollection(UserConnection);
			foreach (var licEntity in licESQCollection)
			{
				string licPackName = licEntity.GetTypedColumnValue<string>(licPackageName.Name);
				if (licPackageAvailable.ContainsKey(licPackName))
				{
					licPackageAvailable[licPackName] += licEntity.GetTypedColumnValue<int>("GKIAvailableCount");
				}
				else
				{
					licPackageAvailable.Add(licPackName, licEntity.GetTypedColumnValue<int>("GKIAvailableCount"));
				}
			}
			return licPackageAvailable;
		}

		private void GKIMakeThemChangeLicense(bool isAddLicense, EntityCollection enhancedCollection, string GKIUrlClm, 
			string GKILicUserNameClm, string GKILicPackageNameClm)
		{

			GKIUserConnectionCheck();

			List<InstanceSyncData> instancesSyncData = new List<InstanceSyncData>();

			foreach (Entity enhancedEntity in enhancedCollection)
			{
				string whosEmpty = enhancedEntity.GetTypedColumnValue<string>(GKIUrlClm) == null ? "Url" :
						enhancedEntity.GetTypedColumnValue<string>(GKILicUserNameClm) == null ? "UserName" :
						enhancedEntity.GetTypedColumnValue<string>(GKILicPackageNameClm) == null ? "LicPackageName" : String.Empty;
				if (whosEmpty != String.Empty)
				{
					GKIWriteLicUserInstanceLicPackageError(enhancedEntity.GetTypedColumnValue<Guid>("Id"), whosEmpty + " is empty");
					continue;
				}
				Guid instanceId = enhancedEntity.GetTypedColumnValue<Guid>("Id");
				InstanceSyncData curInstance = new InstanceSyncData();
				curInstance = instancesSyncData.Find(item => item.InstanceId == instanceId);
				if (curInstance == null)
				{
					curInstance = new InstanceSyncData();
					curInstance.InstanceId = instanceId;
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
				try
				{
					string userMessage = GKILicenseAssignmentRequest(instanceSyncData.InstanceId, isAddLicense, instanceSyncData.LicUserDataList);
					continue;
				}
				catch (Exception ex)
				{
					//instance sync error
					Update instanceUpdate = new Update(UserConnection, "GKIInstance")
						.Set("GKILastSyncSuccess", Column.Parameter(false))
						.Set("GKILastSyncError", Column.Parameter(ex.Message))
						.Where("Id").IsEqual(Column.Parameter(instanceSyncData.InstanceId))
					as Update;
					instanceUpdate.Execute();
					continue;
				}
			}
			GKIGetInstallLicensesInfoAll();
		}
		private void GKIGetInstallLicensesInfoAll()
		{
			var GKICustomerIDESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKICustomerID");
			GKICustomerIDESQ.UseAdminRights = false;
			string idColumnName = GKICustomerIDESQ.AddColumn("Id").Name;
			var GKICustomerIDESQCollection = GKICustomerIDESQ.GetEntityCollection(UserConnection);

			foreach (Entity GKICustomerIDEntity in GKICustomerIDESQCollection)
			{
				GKIGetInstallLicensesInfoByCustomerID(GKICustomerIDEntity.GetTypedColumnValue<Guid>(idColumnName));
			}
		}
		private void GKIGetInstallLicensesInfoByCustomerID(Guid CustomerId)
		{
			var GKIInstanceESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
			GKIInstanceESQ.UseAdminRights = false;
			string idColumnName = GKIInstanceESQ.AddColumn("Id").Name;
			GKIInstanceESQ.AddColumn("GKIUrl");
			GKIInstanceESQ.Filters.Add(
				GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", CustomerId));
			var GKIInstanceESQCollection = GKIInstanceESQ.GetEntityCollection(UserConnection);

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
				var update = new Update(UserConnection, "GKIInstance")
					.Set("GKILastTlsInstallError", Column.Parameter(message))
					.Where("Id").IsEqual(Column.Parameter(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName))) as Update;
				update.Execute();
			}
		}
		private string GKIGetInstallLicensesInfo(string baseUrl, Guid CustomerId, Guid instanceId)
		{
			try
			{
				string bareLicenseInfoResult = GKILicenseHttpRequest(instanceId, GKIGetInstalledLicensesInfoServiceUrl);
				LicenseInfoResult licenseInfoResult = JsonConvert.DeserializeObject<LicenseInfoResult>(bareLicenseInfoResult);
				if (licenseInfoResult.Success == true)
				{
					foreach (LicenseInfo licenseInfo in licenseInfoResult.LicenseInfoList)
					{
						var licESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKILic");
						licESQ.UseAdminRights = false;
						licESQ.AddAllSchemaColumns();
						licESQ.Filters.Add(
							licESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", CustomerId));
						licESQ.Filters.Add(
							licESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage.GKIName", licenseInfo.SysLicPackageName));
						var licESQCollection = licESQ.GetEntityCollection(UserConnection);

						var GKILicPackageSchema = UserConnection.EntitySchemaManager.GetInstanceByName("GKILicPackage");
						Entity GKILicPackageEntity = GKILicPackageSchema.CreateEntity(UserConnection);
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
							var select = new Select(UserConnection)
								.Column("Id")
								.From("GKILicPackage")
								.Where("GKIName").IsEqual(Column.Parameter(licenseInfo.SysLicPackageName)) as Select;
							licPackageId = select.ExecuteScalar<Guid>();
						}

						var rootSchema = UserConnection.EntitySchemaManager.GetInstanceByName("GKILic");
						Entity GKILicEntity = rootSchema.CreateEntity(UserConnection);

						if (licESQCollection.Count > 0)
						{
							GKILicEntity = licESQCollection.First();
						}
						else
						{
							GKILicEntity.SetDefColumnValues();
						}

						//update GKICountUsed in GKIInstanceLicense for future max function
						var GKIInstanceLicenseESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstanceLicense");
						GKIInstanceLicenseESQ.UseAdminRights = false;
						GKIInstanceLicenseESQ.AddAllSchemaColumns();
						GKIInstanceLicenseESQ.Filters.Add(
							GKIInstanceLicenseESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
						GKIInstanceLicenseESQ.Filters.Add(
							GKIInstanceLicenseESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILic", GKILicEntity.GetTypedColumnValue<Guid>("Id")));
						var GKIInstanceLicenseESQCollection = GKIInstanceLicenseESQ.GetEntityCollection(UserConnection);

						var schemaGKIInstanceLicense = UserConnection.EntitySchemaManager.GetInstanceByName("GKIInstanceLicense");
						var entityGKIInstanceLicense = schemaGKIInstanceLicense.CreateEntity(UserConnection);

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
						var maxGKIInstanceLicenseESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstanceLicense");
						maxGKIInstanceLicenseESQ.UseAdminRights = false;
						var countUsedColumn = maxGKIInstanceLicenseESQ.AddColumn(
							maxGKIInstanceLicenseESQ.CreateAggregationFunction(AggregationTypeStrict.Max, "GKICountUsed"));
						maxGKIInstanceLicenseESQ.Filters.Add(
							maxGKIInstanceLicenseESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILic", GKILicEntity.GetTypedColumnValue<Guid>("Id")));
						var maxGKIInstanceLicenseESQCollection = maxGKIInstanceLicenseESQ.GetEntityCollection(UserConnection);
						var entityMaxGKIInstanceLicense = maxGKIInstanceLicenseESQCollection.First();
						int maxLicenseCountUsed = entityMaxGKIInstanceLicense.GetTypedColumnValue<int>(countUsedColumn.Name);

						//LicType
						var licTypeSchema = UserConnection.EntitySchemaManager.GetInstanceByName("GKILicType");
						Entity licTypeEntity = licTypeSchema.CreateEntity(UserConnection);
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
							var instanceLicPackageESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstanceLicPackage");
							instanceLicPackageESQ.UseAdminRights = false;
							instanceLicPackageESQ.AddAllSchemaColumns();
							instanceLicPackageESQ.Filters.Add(
								instanceLicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
							instanceLicPackageESQ.Filters.Add(
								instanceLicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage", licPackageId));
							var instanceLicPackageESQCollection = instanceLicPackageESQ.GetEntityCollection(UserConnection);
							if (instanceLicPackageESQCollection.Count == 0)
							{
								var instanceLicPackageSchema = UserConnection.EntitySchemaManager.GetInstanceByName("GKIInstanceLicPackage");
								var instanceLicPackageEntity = instanceLicPackageSchema.CreateEntity(UserConnection);
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
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
		private void GKIDeactivateLicUsers(GKIUsersSyncResult syncResult, Guid instanceId)
		{
			Dictionary<string, Guid> instanceLicUsersList = new Dictionary<string, Guid>();
			List<Guid> deactivateLicUsersList = new List<Guid>();
			List<Guid> deactivateIdleLicUsersList = new List<Guid>();
			List<Guid> deactivatePastDueLicUsersList = new List<Guid>();
			double timespanSysValue = Convert.ToDouble(Core.Configuration.SysSettings.GetValue(UserConnection, "GKISysLicUserInactivenessControlTimespan"));
			double cooldownSysValue = Convert.ToDouble(Core.Configuration.SysSettings.GetValue(UserConnection, "GKISysLicUserInactivenessControlCooldown"));

			//select all users from an instance - it's faster
			var instanceUsersSelect =
					new Select(UserConnection)
						.Column("GKILicUserId")
						.Column(new Select(UserConnection).Column("GKIName").From("GKILicUser").Where("Id").IsEqual("GKILicUserId"))
						.From("GKIInstanceLicUser")
						.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId)) as Select;
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
			{
				using (IDataReader dr = instanceUsersSelect.ExecuteReader(dbExecutor))
				{
					while (dr.Read())
					{
						if (!instanceLicUsersList.ContainsKey(dr.GetString(1)))
						{
							instanceLicUsersList.Add(dr.GetString(1), dr.GetGuid(0));
						}
					}
				}
			}

			foreach (UserSyncResultSysAdminUnit userSyncResultSysAdminUnit in syncResult.UserSyncResultSysAdminUnit)
			{
				if (!instanceLicUsersList.ContainsKey(userSyncResultSysAdminUnit.Name))
				{
					continue;
				}
				Guid GKILicUserId = instanceLicUsersList[userSyncResultSysAdminUnit.Name];
				//if not active then put in deactivate queue
				if (!userSyncResultSysAdminUnit.Active)
				{
					deactivateLicUsersList.Add(GKILicUserId);
				}
				else
				{
					//if it's not system user
					if (userSyncResultSysAdminUnit.Name != "Supervisor" && userSyncResultSysAdminUnit.Name != "SysPortalConnection")
					{
						//if user was created and haven't entered ever since (idle)
						if (userSyncResultSysAdminUnit.LastActivityDateTime == null
							&&
							userSyncResultSysAdminUnit.RegistrationDateTime <=
							DateTime.Now
								.AddMonths(-Convert.ToInt32(Math.Truncate(cooldownSysValue)))
								.AddDays(-Convert.ToInt32((cooldownSysValue - Math.Truncate(cooldownSysValue)) * 100))
							)
						{
							deactivateIdleLicUsersList.Add(GKILicUserId);
						}
						//if user haven't entered in syssetting-stated period of time (past due)
						else if (userSyncResultSysAdminUnit.LastActivityDateTime <=
							DateTime.Now
								.AddMonths(-Convert.ToInt32(Math.Truncate(timespanSysValue)))
								.AddDays(-Convert.ToInt32((timespanSysValue - Math.Truncate(timespanSysValue)) * 100))
							)
						{
							deactivatePastDueLicUsersList.Add(GKILicUserId);
						}
					}
				}
			}

			GKIDeactivatedUsersUpdate(deactivateLicUsersList, instanceId, GKILicensingConstantsCs.GKIDeactivationReasonLookup.Inactive);
			GKIDeactivatedUsersUpdate(deactivateIdleLicUsersList, instanceId, GKILicensingConstantsCs.GKIDeactivationReasonLookup.DidntEnter);
			GKIDeactivatedUsersUpdate(deactivatePastDueLicUsersList, instanceId, GKILicensingConstantsCs.GKIDeactivationReasonLookup.HaventEnteredInTheTimespan);
		}
		private string GKILicenseGetUsersToDeactivateRequest(string baseUrl, string serviceUrl, Guid instanceId)
		{
			string userMessage = "";
			var msgResponseText = GKILicenseHttpRequest(instanceId, serviceUrl);
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
					GKIDeactivateLicUsers(syncResult, instanceId);
				}
			}
			else
			{
				userMessage = msgResponseText;
			}

			return userMessage;
		}
		private string GKILicenseSyncRegularRequest(string baseUrl, string serviceUrl, Guid instanceId)
		{
			var msgResponseText = GKILicenseHttpRequest(instanceId, serviceUrl);
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

					List<Guid> allReceivedLicUsers = new List<Guid>();

					foreach (UserSyncResultSysAdminUnit userSyncResultSysAdminUnit in syncResult.UserSyncResultSysAdminUnit)
					{

						#region User
						string sysAdminUnitName = userSyncResultSysAdminUnit.Name;
						Guid GKILicUserId;

						var GKIInstanceLicUserESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstanceLicUser");
						GKIInstanceLicUserESQ.UseAdminRights = false;
						GKIInstanceLicUserESQ.AddAllSchemaColumns();
						GKIInstanceLicUserESQ.Filters.Add(
							GKIInstanceLicUserESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
						GKIInstanceLicUserESQ.Filters.Add(
							GKIInstanceLicUserESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser.GKIName", sysAdminUnitName));
						var GKIInstanceLicUserESQCollection = GKIInstanceLicUserESQ.GetEntityCollection(UserConnection);
						if (GKIInstanceLicUserESQCollection.Count > 0)
						{
							Entity GKIInstanceLicUserEntity = GKIInstanceLicUserESQCollection.First();
							GKIInstanceLicUserEntity.SetColumnValue("GKIActive", userSyncResultSysAdminUnit.Active);
							bool setClm = userSyncResultSysAdminUnit.LastActivityDateTime != null ?
								GKIInstanceLicUserEntity.SetColumnValue("GKILastActivityDateTime", userSyncResultSysAdminUnit.LastActivityDateTime)
								: false;
							setClm = userSyncResultSysAdminUnit.RegistrationDateTime != null ?
								GKIInstanceLicUserEntity.SetColumnValue("GKIRegistrationDate", userSyncResultSysAdminUnit.RegistrationDateTime)
								: false;
							GKIInstanceLicUserEntity.Save();
							GKILicUserId = GKIInstanceLicUserEntity.GetTypedColumnValue<Guid>("GKILicUserId");
						}
						else
						{
							var GKILicUserESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKILicUser");
							GKILicUserESQ.UseAdminRights = false;
							GKILicUserESQ.AddAllSchemaColumns();
							GKILicUserESQ.Filters.Add(
								GKILicUserESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIName", sysAdminUnitName));
							var GKILicUserESQCollection = GKILicUserESQ.GetEntityCollection(UserConnection);
							if (GKILicUserESQCollection.Count > 0)
							{
								GKILicUserId = GKILicUserESQCollection.First().GetTypedColumnValue<Guid>("Id");
							}
							else
							{
								var GKILicUserSchema = UserConnection.EntitySchemaManager.GetInstanceByName("GKILicUser");
								var GKILicUserEntity = GKILicUserSchema.CreateEntity(UserConnection);
								GKILicUserEntity.SetDefColumnValues();
								GKILicUserEntity.SetColumnValue("GKIName", sysAdminUnitName);
								GKILicUserEntity.SetColumnValue("GKIPlatformActive", true);
								GKILicUserEntity.SetColumnValue("GKIActive", userSyncResultSysAdminUnit.Active);
								GKILicUserEntity.Save();
								GKILicUserId = GKILicUserEntity.GetTypedColumnValue<Guid>("Id");
							}
							var GKIInstanceLicUserSchema = UserConnection.EntitySchemaManager.GetInstanceByName("GKIInstanceLicUser");
							var GKIInstanceLicUserEntity = GKIInstanceLicUserSchema.CreateEntity(UserConnection);
							GKIInstanceLicUserEntity.SetDefColumnValues();
							GKIInstanceLicUserEntity.SetColumnValue("GKIActive", userSyncResultSysAdminUnit.Active);
							GKIInstanceLicUserEntity.SetColumnValue("GKIInstanceId", instanceId);
							GKIInstanceLicUserEntity.SetColumnValue("GKILicSyncSourceId", GKILicensingConstantsCs.GKILicSyncSource.Regular);
							GKIInstanceLicUserEntity.SetColumnValue("GKILicUserId", GKILicUserId);
							bool setClm = userSyncResultSysAdminUnit.LastActivityDateTime != null ?
								GKIInstanceLicUserEntity.SetColumnValue("GKILastActivityDateTime", userSyncResultSysAdminUnit.LastActivityDateTime)
								: false;
							setClm = userSyncResultSysAdminUnit.RegistrationDateTime != null ?
								GKIInstanceLicUserEntity.SetColumnValue("GKIRegistrationDate", userSyncResultSysAdminUnit.RegistrationDateTime)
								: false;
							GKIInstanceLicUserEntity.Save();
						}

						//for comparing with current instance list
						allReceivedLicUsers.Add(GKILicUserId);

						#endregion
						#region Licenses
						foreach (UserSyncResultSysLicUser userSyncResultSysLicUser in userSyncResultSysAdminUnit.UserSyncResultSysLicUser)
						{
							var GKILicUserInstanceLicPackageESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKILicUserInstanceLicPackage");
							GKILicUserInstanceLicPackageESQ.UseAdminRights = false;
							GKILicUserInstanceLicPackageESQ.AddAllSchemaColumns();
							GKILicUserInstanceLicPackageESQ.Filters.Add(
								GKILicUserInstanceLicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", instanceId));
							GKILicUserInstanceLicPackageESQ.Filters.Add(
								GKILicUserInstanceLicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser", GKILicUserId));
							GKILicUserInstanceLicPackageESQ.Filters.Add(
								GKILicUserInstanceLicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage.GKIName", userSyncResultSysLicUser.SysLicPackageName));
							var GKILicUserInstanceLicPackageESQCollection = GKILicUserInstanceLicPackageESQ.GetEntityCollection(UserConnection);
							if (GKILicUserInstanceLicPackageESQCollection.Count > 0)
							{
								Entity GKILicUserInstanceLicPackageEntity = GKILicUserInstanceLicPackageESQCollection.First();
								GKILicUserInstanceLicPackageEntity.SetColumnValue("GKISyncedState", userSyncResultSysLicUser.Active);
								GKILicUserInstanceLicPackageEntity.Save();
							}
							else
							{
								var GKILicPackageESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKILicPackage");
								GKILicPackageESQ.UseAdminRights = false;
								GKILicPackageESQ.AddAllSchemaColumns();
								GKILicPackageESQ.Filters.Add(
									GKILicPackageESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIName", userSyncResultSysLicUser.SysLicPackageName));
								Entity GKILicPackageESQEntity = GKILicPackageESQ.GetEntityCollection(UserConnection).First();
								if (GKILicPackageESQEntity == null)
								{
									continue;
								}

								var GKILicUserInstanceLicPackageSchema = UserConnection.EntitySchemaManager.GetInstanceByName("GKILicUserInstanceLicPackage");
								var GKILicUserInstanceLicPackageEntity = GKILicUserInstanceLicPackageSchema.CreateEntity(UserConnection);
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

					//set GKIInstanceLicUsers that are deleted from the instance not active
					IEnumerable<QueryParameter> allReceivedLicUsersIdsEnum = allReceivedLicUsers.ConvertAll(x => new QueryParameter(x));
					if (allReceivedLicUsersIdsEnum.Count() > 0)
					{
						Update requestGKIInstanceLicUsersUpdate = new Update(UserConnection, "GKIInstanceLicUser")
							.Set("GKIActive", Column.Parameter(false))
							.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
							.And("GKILicUserId")
							.Not().In(allReceivedLicUsersIdsEnum)
							as Update;
						requestGKIInstanceLicUsersUpdate.Execute();
					}

					GKILicUserGKIPlatformActiveUpdate();

					GKILicUserGKIActiveUpdate();

					GKIDeactivateLicUsers(syncResult, instanceId);


					#endregion
				}
			}
			else
			{
				userMessage = msgResponseText;
			}

			return userMessage;
		}
		private string GKILicenseAssignmentRequest(Guid instanceId, bool isAddLicense, List<LicUserData> licUserDataList)
		{
			string message = JsonConvert.SerializeObject(licUserDataList);
			string serviceUrl = isAddLicense
				? GKIAddLicenseServiceUrl
				: GKIRemoveLicenseServiceUrl;
			string userMessage = "";
			try
			{
				var schemaGKIInstance = UserConnection.EntitySchemaManager.GetInstanceByName("GKIInstance");
				Entity entityGKIInstance = schemaGKIInstance.CreateEntity(UserConnection);
				entityGKIInstance.FetchFromDB("Id", instanceId);
				string instanceName = entityGKIInstance.GetTypedColumnValue<string>("GKIName");

				var msgResponseText = GKILicenseHttpRequest(instanceId, serviceUrl, message);
				GKILicUserSyncResult GKILicUserSyncResult = JsonConvert.DeserializeObject<GKILicUserSyncResult>(msgResponseText);
				List<LicUserSyncResult> syncResults = GKILicUserSyncResult.LicUserSyncResults;
				if (syncResults != null)
				{
					userMessage = String.Concat(instanceName, ": ", userMessage);
					userMessage = String.Concat(userMessage, GKILicUserSyncResult.Success == true ? "" : "Critical error occured! " + GKILicUserSyncResult.ErrMsg);

					LicSyncResultProcessing(syncResults, isAddLicense, out userMessage, instanceId);

					//instance sync result
					Update instanceUpdate = new Update(UserConnection, "GKIInstance")
						.Set("GKILastSyncSuccess", Column.Parameter(GKILicUserSyncResult.Success))
						.Set("GKILastSyncError", Column.Parameter(GKILicUserSyncResult.Success == true ? String.Empty : GKILicUserSyncResult.ErrMsg))
						.Where("Id").IsEqual(Column.Parameter(instanceId))
					as Update;
					instanceUpdate.Execute();
				}
				else
				{
					string errorMsg = GKILicUserSyncResult.Success == true ? "Instance has not returned any user" : GKILicUserSyncResult.ErrMsg;
					//instance sync result
					Update instanceUpdate = new Update(UserConnection, "GKIInstance")
						.Set("GKILastSyncSuccess", Column.Parameter(GKILicUserSyncResult.Success))
						.Set("GKILastSyncError", Column.Parameter(errorMsg))
						.Where("Id").IsEqual(Column.Parameter(instanceId))
					as Update;
					instanceUpdate.Execute();

					IEnumerable<string> licPackageNames = licUserDataList.Select(x => x.LicPackageName).Distinct();
					foreach (string licPackageName in licPackageNames)
					{
						IEnumerable<QueryParameter> syncedErrorUserNamesEnum = licUserDataList.ConvertAll(x => new QueryParameter(x.LicUserName));
						if (syncedErrorUserNamesEnum.Count() > 0)
						{
							Update syncedUserErrorsUpdate = new Update(UserConnection, "GKILicUserInstanceLicPackage")
								.Set("GKILastSyncDateTime", Column.Parameter(DateTime.MinValue))
								.Set("GKILastErrorDateTime", Column.Parameter(DateTime.Now))
								.Set("GKIDeactivatedBySync", Column.Parameter(false))
								.Set("GKISyncError", Column.Parameter(errorMsg))
								.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
								.And("GKILicPackageId").In(new Select(UserConnection).Column("Id").From("GKILicPackage").Where("GKIName").IsEqual(Column.Parameter(licPackageName)))
								.And("GKILicUserId").In(new Select(UserConnection).Column("Id").From("GKILicUser").Where("GKIName").In(syncedErrorUserNamesEnum))
							as Update;
							syncedUserErrorsUpdate.Execute();
						}
					}

					userMessage = "Instance has not returned any user. ";
					userMessage = String.Concat(instanceName, ": ", userMessage);
					userMessage = String.Concat(userMessage, GKILicUserSyncResult.Success == true ? "" : "Critical error occured! " + GKILicUserSyncResult.ErrMsg);
				}
			}
			catch (Exception ex)
			{
				//instance sync result
				Update instanceUpdate = new Update(UserConnection, "GKIInstance")
					.Set("GKILastSyncSuccess", Column.Parameter(false))
					.Set("GKILastSyncError", Column.Parameter(ex.Message))
					.Where("Id").IsEqual(Column.Parameter(instanceId))
				as Update;
				instanceUpdate.Execute();

				IEnumerable<QueryParameter> syncedErrorUserNamesEnum = licUserDataList.ConvertAll(x => new QueryParameter(x.LicUserName));
				if (syncedErrorUserNamesEnum.Count() > 0)
				{
					Update syncedUserErrorsUpdate = new Update(UserConnection, "GKILicUserInstanceLicPackage")
						.Set("GKILastSyncDateTime", Column.Parameter(DateTime.MinValue))
						.Set("GKILastErrorDateTime", Column.Parameter(DateTime.Now))
						.Set("GKIDeactivatedBySync", Column.Parameter(false))
						.Set("GKISyncError", Column.Parameter(ex.Message))
						.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
						.And("GKILicUserId").In(new Select(UserConnection).Column("Id").From("GKILicUser").Where("GKIName").In(syncedErrorUserNamesEnum))
					as Update;
					syncedUserErrorsUpdate.Execute();
				}

				userMessage = ex.Message;
			}
			return userMessage;
		}
		private void LicSyncResultProcessing(List<LicUserSyncResult> syncResults, bool isAddLicense, out string userMessage, Guid instanceId)
        {

			List<LicUserSyncResult> trueSyncResults = syncResults.FindAll(item => item.isSuccess == true);
			List<LicUserSyncResult> errorSyncResults = syncResults.FindAll(item => item.isSuccess == false);

			userMessage = String.Concat(
				trueSyncResults.Count.ToString(),
				" licenses were ", isAddLicense ? "granted. " : "removed. ",
				errorSyncResults.Count.ToString(), " user sync errors. ");

			IEnumerable<string> licPackageNames = syncResults.Select(x => x.LicPackageName).Distinct();
			IEnumerable<string> errorNames = errorSyncResults.Select(x => x.ErrorMsg).Distinct();

			foreach (string licPackageName in licPackageNames)
			{
				//update valid results
				IEnumerable<QueryParameter> syncedUserNamesEnum = trueSyncResults.ConvertAll(x => new QueryParameter(x.LicUserName));
				if (syncedUserNamesEnum.Count() > 0)
				{
					Update syncedUserActivenessUpdate = new Update(UserConnection, "GKILicUserInstanceLicPackage")
						.Set("GKISyncedState", Column.Parameter(isAddLicense))
						.Set("GKISyncError", Column.Parameter(String.Empty))
						.Set("GKIDeactivatedBySync", Column.Parameter(false))
						.Set("GKILastSyncDateTime", Column.Parameter(DateTime.Now))
						.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
						.And("GKILicPackageId").In(new Select(UserConnection).Column("Id").From("GKILicPackage").Where("GKIName").IsEqual(Column.Parameter(licPackageName)))
						.And("GKILicUserId").In(new Select(UserConnection).Column("Id").From("GKILicUser").Where("GKIName").In(syncedUserNamesEnum))
					as Update;
					syncedUserActivenessUpdate.Execute();

					Delete syncedUserQueueDelete = new Delete(UserConnection)
						.From("GKILicUserInstanceLicPackageQueue")
						.Where("GKILicUserInstanceLicPackageId")
						.In(new
							Select(UserConnection)
							.Column("Id")
							.From("GKILicUserInstanceLicPackage")
							.Where("GKILicUserId").In(new Select(UserConnection).Column("Id").From("GKILicUser").Where("GKIName").In(syncedUserNamesEnum)))
					as Delete;
					syncedUserQueueDelete.Execute();
				}

				//update errors
				foreach (string errorName in errorNames)
				{
					IEnumerable<QueryParameter> syncedErrorUserNamesEnum = errorSyncResults.ConvertAll(x => new QueryParameter(x.LicUserName));
					if (syncedErrorUserNamesEnum.Count() > 0)
					{
						Update syncedUserErrorsUpdate = new Update(UserConnection, "GKILicUserInstanceLicPackage")
							.Set("GKILastErrorDateTime", Column.Parameter(DateTime.Now))
							.Set("GKISyncError", Column.Parameter(errorName))
							.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
							.And("GKILicPackageId").In(new Select(UserConnection).Column("Id").From("GKILicPackage").Where("GKIName").IsEqual(Column.Parameter(licPackageName)))
							.And("GKILicUserId").In(new Select(UserConnection).Column("Id").From("GKILicUser").Where("GKIName").In(syncedErrorUserNamesEnum))
						as Update;
						syncedUserErrorsUpdate.Execute();
					}
				}
			}
		}
		private bool GKIIsBusinessProcessAlive(string processName)
		{
			bool isAlive = false;
			var processRunningStatusId = new Guid("ED2AE277-B6E2-DF11-971B-001D60E938C6");
			var select = new Select(UserConnection.AppConnection.SystemUserConnection)
				.Column("pl", "Id").As("ProcessLogId")
				.From("SysProcessLog", "pl")
					.InnerJoin("SysSchema").As("s")
						.On("s", "Id").IsEqual("pl", "SysSchemaId")
				.Where("s", "Name").IsEqual(Column.Const(processName))
					.And("pl", "StatusId").IsEqual(Column.Const(processRunningStatusId)) as Select;

			var processesIds = new List<Guid>();
			using (var dbExecutor = UserConnection.AppConnection.SystemUserConnection.EnsureDBConnection())
			{
				using (var dataReader = select.ExecuteReader(dbExecutor))
				{
					while (dataReader.Read())
					{
						isAlive = true;
					}
				}
			}
			return isAlive;
		}
		private void GKICancelBusinessProcesses(string processName)
        {
			var notLockHint = new NoLockHint();
			var processRunningStatusId = new Guid("ED2AE277-B6E2-DF11-971B-001D60E938C6");
			var select = new Select(UserConnection.AppConnection.SystemUserConnection)
				.Column("pl", "Id").As("ProcessLogId")
				.From("SysProcessLog", "pl")
					.InnerJoin("SysSchema").As("s").WithHints(notLockHint)
						.On("s", "Id").IsEqual("pl", "SysSchemaId")
				.Where("s", "Name").IsEqual(Column.Const(processName))
					.And("pl", "StatusId").IsEqual(Column.Const(processRunningStatusId)) as Select;

			var processesIds = new List<Guid>();
			using (var dbExecutor = UserConnection.AppConnection.SystemUserConnection.EnsureDBConnection())
			{
				using (var dataReader = select.ExecuteReader(dbExecutor))
				{
					while (dataReader.Read())
					{
						processesIds.Add(dataReader.GetColumnValue<Guid>("ProcessLogId"));
					}
				}
			}

			foreach (var processId in processesIds)
			{
				UserConnection.AppConnection.SystemUserConnection.ProcessEngine.ProcessExecutor.CancelExecutionAsync(processId);
			}
		}
		private void GKISendTlrEmail(List<byte[]> filesData)
        {
			Guid mailboxId = Terrasoft.Core.Configuration.SysSettings.GetValue(UserConnection, "GKILicensingMailBox", Guid.Empty);
			string recipientEmail = Terrasoft.Core.Configuration.SysSettings.GetValue(UserConnection, "GKILicensingTerrasoftSupportAddress", String.Empty);
			GetSubjectAndBodyFromEmailTemplate(GKILicensingConstantsCs.Misc.TlrRequestEmailTemplate, out string emailSubject, out string emailBody);

			GKISendActivityEmail(UserConnection, mailboxId, recipientEmail, emailSubject, emailBody, filesData, null, "LicenseRequest", ".tlr");
		}

		private void GetSubjectAndBodyFromEmailTemplate(Guid templateId, out string emailSubject, out string emailBody)
        {
			string emailTemplateErrorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKITlrRequestEmailTemplateError.Value");

			var emailTemplateSchema = UserConnection.EntitySchemaManager.GetInstanceByName("EmailTemplate");
			Entity emailTemplateEntity = emailTemplateSchema.CreateEntity(UserConnection);
			if (!emailTemplateEntity.FetchFromDB("Id", templateId))
			{
				throw new Exception(emailTemplateErrorMsg);
			}
			emailSubject = emailTemplateEntity.GetTypedColumnValue<string>("Subject");
			emailBody = emailTemplateEntity.GetTypedColumnValue<string>("Body");
		}

		private void GKISendActivityEmail(UserConnection UserConnection, Guid mailboxId, string recipientEmail,
			string subject, string body, List<byte[]> filesData, List<string> fileNames = null, string defaultFileName = "File", string defaultFileExtension = null)
        {
			string noMailBoxErrorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKISendActivityEmailNoMailBoxError.Value");
			string noRecipientEmailErrorMsg = new LocalizableString(UserConnection.Workspace.ResourceStorage,
				"GKILicensingServices",
				"LocalizableStrings.GKISendActivityEmailNoRecipientError.Value");

			if (string.IsNullOrEmpty(recipientEmail))
            {
				throw new Exception(noRecipientEmailErrorMsg);
			}
			var mailboxTableESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "MailboxSyncSettings");
			var senderEmailAddressColumn = mailboxTableESQ.AddColumn("SenderEmailAddress");
			mailboxTableESQ.Filters.Add(mailboxTableESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", mailboxId));
			var mailboxEntities = mailboxTableESQ.GetEntityCollection(UserConnection);
			if (mailboxEntities == null || mailboxEntities.Count < 1)
			{
				throw new Exception(noMailBoxErrorMsg);
			}
			string senderEmail = mailboxEntities[0].GetTypedColumnValue<string>(senderEmailAddressColumn.Name);

			var activity = new Activity(UserConnection);
			activity.SetDefColumnValues();
			activity.TypeId = ActivityConsts.EmailTypeUId;
			activity.Sender = senderEmail;
			activity.Recepient = recipientEmail;
			activity.Title = subject;
			activity.Body = body;
			activity.IsHtmlBody = true;
			activity.StartDate = UserConnection.CurrentUser.GetCurrentDateTime();
			activity.DueDate = UserConnection.CurrentUser.GetCurrentDateTime();
			activity.OwnerId = UserConnection.CurrentUser.ContactId;
			activity.AuthorId = UserConnection.CurrentUser.ContactId;
			activity.StatusId = ActivityConsts.NewStatusUId;
			activity.Save();

			if (filesData != null && filesData.Count > 0)
			{
				foreach (var fileData in filesData)
				{
					int curElementPosition = filesData.FindIndex(x => x == fileData);
					string fileName = fileNames?.ElementAtOrDefault(curElementPosition) ?? String.Concat(defaultFileName,
						curElementPosition == 0 ? String.Empty : String.Concat("(", curElementPosition.ToString(), ")"), defaultFileExtension);
					var fileEntity = new ActivityFile(UserConnection);
					fileEntity.SetDefColumnValues();
					fileEntity.SetColumnValue("ActivityId", activity.Id);
					fileEntity.SetColumnValue("TypeId", Terrasoft.WebApp.FileConsts.FileTypeUId);
					fileEntity.SetColumnValue("Name", fileName);
					fileEntity.SetColumnValue("Data", fileData);
					fileEntity.Save();
				}
			}

			var emailClientFactory = ClassFactory.Get<EmailClientFactory>(new ConstructorArgument("UserConnection", UserConnection));
			var activityEmailSender = new ActivityEmailSender(emailClientFactory, UserConnection);
			activityEmailSender.Send(activity.Id);
		}


        private void GKILicUserGKIPlatformActiveUpdate()
        {
			//GKIPlatformActive:
			//make GKILicUser.GKIPlatformActive inactive if it hasn't got any GKIInstanceLicUsers
			Update updateGKILicUserPlatformNotActive = new Update(UserConnection, "GKILicUser")
				.Set("GKIPlatformActive", Column.Parameter(false))
				.Where("Id").Not().In(new Select(UserConnection).Column("GKILicUserId").From("GKIInstanceLicUser"))
			as Update;
			updateGKILicUserPlatformNotActive.Execute();

			//make GKILicUser.GKIPlatformActive for every acquired instance user
			Update updateGKILicUserPlatformActive = new Update(UserConnection, "GKILicUser")
				.Set("GKIPlatformActive", Column.Parameter(true))
				.Where("Id").In(new Select(UserConnection).Column("GKILicUserId").From("GKIInstanceLicUser"))
			as Update;
			updateGKILicUserPlatformActive.Execute();
		}

		private void GKILicUserGKIActiveUpdate()
        {
			//GKIActive:
			//make GKILicUser.GKIActive inactive if it hasn't got any GKIInstanceLicUsers.GKIActive == true
			Update updateGKILicUserNotActive = new Update(UserConnection, "GKILicUser")
				.Set("GKIActive", Column.Parameter(false))
				.Set("GKIStatusId", Column.Parameter(GKILicensingConstantsCs.GKILicUserStatus.Inactive))
				.Where("Id").Not().In(new Select(UserConnection).Column("GKILicUserId")
					.From("GKIInstanceLicUser")
					.Where("GKIActive").IsEqual(Column.Parameter(true)))
			as Update;
			updateGKILicUserNotActive.Execute();

			//make GKILicUser.GKIActive for every user with active GKIInstanceLicUser
			//part 1: active with licenses
			Update updateGKILicUserActive = new Update(UserConnection, "GKILicUser")
				.Set("GKIActive", Column.Parameter(true))
				.Set("GKIStatusId", Column.Parameter(GKILicensingConstantsCs.GKILicUserStatus.Active))
				.Where("Id").In(new Select(UserConnection).Column("GKILicUserId")
					.From("GKIInstanceLicUser")
					.Where("GKIActive").IsEqual(Column.Parameter(true)))
			//#BPMS-139
			//.And("Id").In(new Select(UserConnection).Column("GKILicUserId").From("GKILicUserInstanceLicPackage").Where("GKIActive").IsEqual(Column.Parameter(true)))
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
			Update update = new Update(UserConnection, "GKILicUserInstanceLicPackage")
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
			Update update = new Update(UserConnection, "GKIInstance")
				.Set("GKILastTlsInstallError", Column.Parameter(errorMsg))
				.Where("Id")
				.IsEqual(Column.Parameter(Id))
				as Update;
			update.Execute();
		}

		private void GKIWriteGKIInstanceTlrInstallError(Guid Id, string errorMsg)
		{
			Update update = new Update(UserConnection, "GKIInstance")
				.Set("GKILastTlrInstallError", Column.Parameter(errorMsg))
				.Where("Id")
				.IsEqual(Column.Parameter(Id))
				as Update;
			update.Execute();
		}

		private string GKIAuthorize(Guid instanceId)
		{
			var esqGKIInstance = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
			esqGKIInstance.UseAdminRights = false;
			esqGKIInstance.AddColumn("GKIUrl");
			esqGKIInstance.AddColumn("GKIPassword");
			var loginColumn = esqGKIInstance.AddColumn("GKILogin.GKIName");
			esqGKIInstance.Filters.Add(
				esqGKIInstance.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", instanceId));
			var esqGKIInstanceCollection = esqGKIInstance.GetEntityCollection(UserConnection);
			var instanceEntity = esqGKIInstanceCollection.FirstOrDefault();
			string baseUrl = instanceEntity.GetTypedColumnValue<string>("GKIUrl");
			string login = instanceEntity.GetTypedColumnValue<string>(loginColumn.Name);
			string password = instanceEntity.GetTypedColumnValue<string>("GKIPassword");

			string authUrl = string.Concat(baseUrl, authServicePath);
			string authMessage = String.Format(authTemplate, login, password);
			
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
							string authResult = GKIAuthorizeForced(baseUrl, authUrl, authMessage);
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
				string authCookieValue = response.Cookies[authName]?.Value ?? String.Empty;
				string crsfCookieValue = response.Cookies[crsfName]?.Value ?? String.Empty;
				if (authCookieValue != String.Empty)
				{
					AuthCookies.Add(new Uri(baseUrl), new Cookie(authName, authCookieValue));
				}
				if (crsfCookieValue != String.Empty)
				{
					AuthCookies.Add(new Uri(baseUrl), new Cookie(crsfName, crsfCookieValue));
				}
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
		private string GKIAuthorizeForced(string baseUrl, string authUrl, string authMessage)
		{
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
				string authCookieValue = response.Cookies[authName]?.Value ?? String.Empty;
				string crsfCookieValue = response.Cookies[crsfName]?.Value ?? String.Empty;
				if (authCookieValue != String.Empty)
				{
					AuthCookies.Add(new Uri(baseUrl), new Cookie(authName, authCookieValue));
				}
				if (crsfCookieValue != String.Empty)
				{
					AuthCookies.Add(new Uri(baseUrl), new Cookie(crsfName, crsfCookieValue));
				}
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
		private string GKILicenseHttpGetRequest(Guid instanceId, string serviceUrl)
		{
			string baseUrl = GKIGetInstanceUrl(instanceId);
			string authResult = GKIAuthorize(instanceId);
			if (authResult != String.Empty)
			{
				throw new Exception(authResult);
			}
			HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(String.Concat(baseUrl, serviceUrl));
			httprequest.Method = "GET";
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
		private string GKILicenseHttpRequest(Guid instanceId, string serviceUrl)
		{
			string baseUrl = GKIGetInstanceUrl(instanceId);
			string authResult = GKIAuthorize(instanceId);
			if (authResult != String.Empty)
			{
				throw new Exception(authResult);
			}
			HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(String.Concat(baseUrl, serviceUrl));
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
		private string GKILicenseHttpRequest(Guid instanceId, string serviceUrl, string message)
		{
			string baseUrl = GKIGetInstanceUrl(instanceId);
			string authResult = GKIAuthorize(instanceId);
			if (authResult != String.Empty)
			{
				throw new Exception(authResult);
			}
			byte[] msgBytes = Encoding.UTF8.GetBytes(message);

			HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(String.Concat(baseUrl, serviceUrl));
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
		private string GKIFileByteRequest(Guid instanceId, string serviceUrl, byte[] file)
		{
			string baseUrl = GKIGetInstanceUrl(instanceId);
			string authResult = GKIAuthorize(instanceId);
			if (authResult != String.Empty)
			{
				throw new Exception(authResult);
			}
			byte[] msgBytes = file;

			HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(String.Concat(baseUrl, serviceUrl));
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

		private string GKIGetInstanceUrl(Guid instanceId)
        {
			string baseUrl;
			var schemaGKIInstance = UserConnection.EntitySchemaManager.GetInstanceByName("GKIInstance");
			Entity entityGKIInstance = schemaGKIInstance.CreateEntity(UserConnection);
			if (entityGKIInstance.FetchFromDB("Id", instanceId))
			{
				baseUrl = entityGKIInstance.GetTypedColumnValue<string>("GKIUrl");
			}
            else 
			{
				baseUrl = String.Empty;
			}
			return baseUrl;
		}
		private UserConnection GKIGetUserConnection()
		{
			return UserConnection ?? (UserConnection)HttpContext.Current.Session["UserConnection"];
		}
		private void GKIUserConnectionCheck()
		{
			if (UserConnection == null)
			{
				UserConnection = GKIGetUserConnection();
			}
		}

		#endregion

		#region Classes

		public class GKIVIPUsers
		{
			public bool isSuccess { get; set; }
			public string errorMsg { get; set; }
			public Dictionary<string, List<string>> vipUsers { get; set; } //product, logins
		}
		public class PulseOutgoingData
        {
			public int lastMasterCheckInWaitMinutes { get; set; }
			public Dictionary<string, int> vipLicensesLimits { get; set; }
		}
		public class PulseData
		{
			public bool isSuccess { get; set; }
			public string errorMsg { get; set; }
			public string vipLimitsErrorMsg { get; set; }
			public List<string> pulseLicUserNames { get; set; } = null;
		}

		[XmlRoot(ElementName = "Licenses")]
		public class TLRLicenses
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
		public class CustomerIdResponse
		{
			public string сustomerId { get; set; }
		}
		public class TlrLicData
		{
			public string licData { get; set; }
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
			public Guid InstanceId { get; set; }
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
			public DateTime? RegistrationDateTime { get; set; }
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
		public static CookieContainer AuthCookies;
		public static System.Timers.Timer GKIPulseTimer;

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
		#endregion
	}
	#endregion
}