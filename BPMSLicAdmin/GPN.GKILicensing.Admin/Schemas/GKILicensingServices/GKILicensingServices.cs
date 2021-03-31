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

		#region Constants
		public static CookieContainer AuthCookies;
		public static System.Timers.Timer GKIPulseTimer;
		#endregion

		#region LicenseSync
		/// <summary>
		/// Обновление лицензий с фильтром
		/// </summary>
		/// <param name="isAddLicense">добавление/удаление</param> 
		/// <param name="licPackageIds">список продуктов</param> 
		/// <param name="instanceIds">список экземпляров</param> 
		/// <param name="licUserIds">список пользователей</param> 
		public void GKIGoMakeThemChangeLicenseFilterMethod(bool isAddLicense, List<Guid> licPackageIds, List<Guid> instanceIds, List<Guid> licUserIds)
		{
			if ((licPackageIds != null && licPackageIds.Count == 0) ||
				(instanceIds != null && instanceIds.Count == 0) ||
				(licUserIds != null && licUserIds.Count == 0))
			{
				//what? - none = return, where? - nowhere = return, whom? - noone = return.
				return;
			}

			GKIUserConnectionCheck();

			var rootSchema = UserConnection.EntitySchemaManager.FindInstanceByName("GKILicUserInstanceLicPackage");
			var esqEnhanced = new EntitySchemaQuery(rootSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
			esqEnhanced.AddColumn("GKIInstance");
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

		/// <summary>
		/// Обновление всех лицензий в очереди
		/// </summary>
		/// <returns>Резуьтат обновления</returns>
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

		/// <summary>
		/// Обновление лицензий из очереди с фильтром
		/// </summary>
		/// <param name="licPackageIds">список продуктов</param> 
		/// <param name="instanceIds">список экземпляров</param> 
		/// <param name="licUserIds">список пользователей</param> 
		/// <param name="processName">имя процесса запустившего метод</param> 
		/// <returns> Результат обновления </returns>
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
					return true;
				}

				GKIUserConnectionCheck();

				var rootSchema = UserConnection.EntitySchemaManager.FindInstanceByName("GKILicUserInstanceLicPackage");

				//GKIActive == true
				var esqEnhancedIsAdd = new EntitySchemaQuery(rootSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
				esqEnhancedIsAdd.AddColumn("GKIInstance");
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
				var esqEnhancedIsRemove = new EntitySchemaQuery(rootSchema) { PrimaryQueryColumn = { IsAlwaysSelect = true } };
				esqEnhancedIsRemove.AddColumn("GKIInstance");
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
		/// <summary>
		/// Синхронизация с экземплярами по url экземпляра
		/// </summary>
		/// <param name="baseUrl">url экземпляра</param> 
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
		/// <summary>
		/// Синхронизация с экземплярами с возможностью фильтрации по экземпляру
		/// </summary>
		/// <param name="instanceIdFilter">Id экземпляра</param> 
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
					string serviceUrl = GKILicensingConstantsCs.LicensingServices.GKIUsersSyncServiceUrl;
					Guid instanceId = instance.GetTypedColumnValue<Guid>("Id");
					bool uriCheckResult = GKIUriCheck(baseUrl);

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
				RemindingServerUtilities.CreateRemindingByProcess(UserConnection, "GKIInstanceLicenseSelectedSyncProcess", returnMsg);
			}
			catch(Exception ex)
            {
				RemindingServerUtilities.CreateRemindingByProcess(UserConnection, "GKILicenseSyncRegularProcess", processErrorMsg);
				RemindingServerUtilities.CreateRemindingByProcess(UserConnection, "GKIInstanceLicenseSelectedSyncProcess", processErrorMsg);
				throw ex;
			}
			return;
		}
		/// <summary>
		/// Получение претендентов на деактивацию с возможностью фильтрации по экземпляру
		/// </summary>
		/// <param name="instanceIdFilter">Id экземпляра</param> 
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
					string serviceUrl = GKILicensingConstantsCs.LicensingServices.GKIUsersSyncServiceUrl;
					Guid instanceId = instance.GetTypedColumnValue<Guid>("Id");
					bool uriCheckResult = GKIUriCheck(baseUrl);

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
		/// <summary>
		/// Импорт файла tls
		/// </summary>
		/// <param name="fileContent">файл</param> 
		/// <returns> Описание ошибки </returns>
		[OperationContract]
		[WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
		public string GKIImportTlsFile(Stream fileContent)
		{
			GKIUserConnectionCheck();
			try
			{
				IFileUploadInfo fileUploadInfo;
				try
				{
					fileUploadInfo = ClassFactory.Get<FileUploadInfo>(
						new ConstructorArgument("fileContent", fileContent),
						new ConstructorArgument("request", HttpContext.Current.Request),
						new ConstructorArgument("storage", UserConnection.Workspace.ResourceStorage));
				}
				catch (InvalidCastException ex)
				{
					fileUploadInfo = ClassFactory.Get<FileUploadInfo>(
						new ConstructorArgument("fileContent", fileContent),
#if NETSTANDARD2_0
				new ConstructorArgument("request", HttpContext.Current.Request),
#else
				new ConstructorArgument("request", new System.Web.HttpRequestWrapper(System.Web.HttpContext.Current.Request)),
#endif
				new ConstructorArgument("storage", UserConnection.Workspace.ResourceStorage));
				}


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
				string fileVersion = tlsLicenses.FileVersion.Substring(0, tlsLicenses.FileVersion.LastIndexOf("."));
				var schemaGKICustomerID = UserConnection.EntitySchemaManager.GetInstanceByName("GKICustomerID");
				Entity entityGKICustomerID = schemaGKICustomerID.CreateEntity(UserConnection);
				if (!entityGKICustomerID.FetchFromDB("Name", tlsLicenses.CustomerId))
				{
					throw new Exception("Wrong file content");
				}
				Dictionary<string, object> conditions = new Dictionary<string, object>();
				conditions.Add("GKICustomerID", entityGKICustomerID.GetTypedColumnValue<Guid>("Id"));
				conditions.Add("GKIVersion", fileVersion);

				var schemaGKITlsFileEntity = UserConnection.EntitySchemaManager.GetInstanceByName("GKITlsFileEntity");
				Entity entityGKITlsFileEntity = schemaGKITlsFileEntity.CreateEntity(UserConnection);
				if (!entityGKITlsFileEntity.FetchFromDB(conditions))
				{
					
					entityGKITlsFileEntity.SetDefColumnValues();
					entityGKITlsFileEntity.SetColumnValue("GKICustomerIDId", entityGKICustomerID.GetTypedColumnValue<Guid>("Id"));
					entityGKITlsFileEntity.SetColumnValue("GKIVersion", fileVersion);
					entityGKITlsFileEntity.Save();
				}
				Guid recordId = entityGKITlsFileEntity.GetTypedColumnValue<Guid>("Id");

				Update update = new Update(UserConnection, "GKITlsFileEntity")
					.Set("GKITlsFile", Column.Parameter(fileBytes))
					.Where("Id").IsEqual(Column.Parameter(recordId)) as Update;
				update.Execute();

				var GKIInstanceESQ = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
				GKIInstanceESQ.UseAdminRights = false;
				string idColumnName = GKIInstanceESQ.AddColumn("Id").Name;
				GKIInstanceESQ.Filters.Add(
					GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", entityGKICustomerID.GetTypedColumnValue<Guid>("Id")));
				GKIInstanceESQ.Filters.Add(
					GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.StartWith, "GKIVersion", fileVersion));
				GKIInstanceESQ.Filters.Add(
					GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.NotEqual, "GKIApplicationStatus", GKILicensingConstantsCs.GKIApplicationStatus.TlsInstalled));
				GKIInstanceESQ.Filters.Add(
					GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.NotEqual, "GKIApplicationStatus", GKILicensingConstantsCs.GKIApplicationStatus.TlsNotInstalled));
				var instanceESQCollection = GKIInstanceESQ.GetEntityCollection(UserConnection);
				foreach(var instance in instanceESQCollection)
                {
					GKIChangeGKIInstanceStatus(instance.GetTypedColumnValue<Guid>(idColumnName), GKILicensingConstantsCs.GKIApplicationStatus.TlsLoaded);
				}
				MsgChannelUtilities.PostMessage(UserConnection, "GKILicensingMessage", "\"GKIInstanceApplicationStatusUpdated\"");
			}
			catch (Exception ex)
            {
				return ex.Message;
            }
			return String.Empty;
		}

		/// <summary>
		/// Установки лицензий из tls файла
		/// </summary>
		/// <param name="recordId">Идентификатор клиента</param> 
		/// <returns> Описание ошибки </returns>
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
			GKIInstanceESQ.AddColumn("GKIVersion");
			GKIInstanceESQ.AddColumn("GKIWinNodeAddress");
			var customerNameClm = GKIInstanceESQ.AddColumn("GKICustomerID.Name").Name;
			GKIInstanceESQ.Filters.Add(
				GKIInstanceESQ.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", recordId));
			var ESQCollection = GKIInstanceESQ.GetEntityCollection(UserConnection);

			foreach(Entity GKIInstanceEntity in ESQCollection)
			{
				Guid instanceId = GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName);
				var fileEntity = GKIGetTlsFileEntity(GKIInstanceEntity.GetTypedColumnValue<string>("GKIVersion"), recordId);
				if (fileEntity == null)
                {
					GKIWriteGKIInstanceLastTlsInstallError(instanceId, "Empty file");
					returnMsg = errorMsg;
					continue;
				}

				byte[] file = fileEntity.GetColumnValue("GKITlsFile") as byte[];
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
					GKIWriteGKIInstanceLastTlsInstallError(instanceId, "Wrong file content");
					returnMsg = errorMsg;
					continue;
				}

				if (GKIInstanceEntity.GetTypedColumnValue<string>(customerNameClm) != tlsLicenses.CustomerId)
				{
					GKIWriteGKIInstanceLastTlsInstallError(instanceId, "Wrong CustomerId");
					returnMsg = errorMsg;
					continue;
				}

				string baseUrl = GKIInstanceEntity.GetTypedColumnValue<string>("GKIUrl");
				string winNodeUrl = GKIInstanceEntity.GetTypedColumnValue<string>("GKIWinNodeAddress");

				string mainInstanceErrorInfo = GKITlsInstallRequest(baseUrl, instanceId, licMessage);
				if (mainInstanceErrorInfo != String.Empty)
				{
					GKIWriteGKIInstanceLastTlsInstallError(instanceId, mainInstanceErrorInfo);
					returnMsg = errorMsg;
					continue;
				}

				string winNodeInstanceErrorInfo = GKITlsInstallRequest(winNodeUrl, instanceId, licMessage);
				if (winNodeInstanceErrorInfo != String.Empty)
				{
					string winTemplate = new LocalizableString(UserConnection.Workspace.ResourceStorage,
						"GKILicensingServices",
						"LocalizableStrings.GKIWindowsInstanceTemplate.Value");
					GKIWriteGKIInstanceLastTlsInstallError(instanceId, String.Format(winTemplate, winNodeInstanceErrorInfo));
					returnMsg = errorMsg;
					continue;
				}

				try
				{
					GKIGetInstallLicensesInfo(baseUrl, recordId, instanceId);
				}
				catch (Exception ex)
				{
					string errTemplate = new LocalizableString(UserConnection.Workspace.ResourceStorage,
						"GKILicensingServices",
						"LocalizableStrings.GKITlsInstalledGetInfoErrorTemplate.Value");
					GKIWriteGKIInstanceLastTlsInstallError(instanceId,
						String.Format(errTemplate, ex.Message));
					returnMsg = errorMsg;
					continue;
				}
				GKIWriteGKIInstanceLastTlsInstallError(instanceId, String.Empty);
				continue;
			}
			MsgChannelUtilities.PostMessage(UserConnection, "GKILicensingMessage", "\"GKIInstanceApplicationStatusUpdated\"");
			if (returnMsg == String.Empty)
			{
				returnMsg = successMsg;
			}
			return returnMsg;
		}

		/// <summary>
		/// Установка исключения импорта файлов tls для сервиса /rest/GKILicensingAdminService/GKIImportTlsFile
		/// </summary>
		/// <returns> Успешность установки </returns>
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
				if (!updateEntity.FetchFromDB("Name", GKILicensingConstantsCs.LicensingServices.GKIImportTlsFile))
				{
					updateEntity.SetDefColumnValues();
					updateEntity.SetColumnValue("Name", GKILicensingConstantsCs.LicensingServices.GKIImportTlsFile);
				}
				updateEntity.Save();
				return true;
			}
            catch (Exception ex)
            {
				return false;
            }
		}
		/// <summary>
		/// Запрос tlr по идентификатору клиента
		/// </summary>
		/// <param name="CustomerId">Идентификатор клиента</param> 
		/// <returns></returns>
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
					Guid instanceId = GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName);
					GKIWriteGKIInstanceTlrInstallError(instanceId, String.Empty);
					string baseUrl = GKIInstanceEntity.GetTypedColumnValue<string>("GKIUrl");
					bool uriCheckResult = GKIUriCheck(baseUrl);

					if (baseUrl != String.Empty && uriCheckResult)
					{
						try
						{
							string fileString = GKILicenseHttpGetRequest(instanceId, GKILicensingConstantsCs.LicensingServices.GKITlrRequestUrl);
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
								.Where("Id").IsEqual(Column.Parameter(instanceId)) as Update;
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
								.Where("GKIInstanceId").IsEqual(Column.Parameter(instanceId))
								.And("Name").IsEqual(Column.Parameter("LicenseRequest.tlr")) as Update;
							int filesGKIInstanceFileUpdated = updateGKIInstanceFile.Execute();
							if (filesGKIInstanceFileUpdated == 0)
                            {
								var esqGKIInstanceFileSchema = UserConnection.EntitySchemaManager.GetInstanceByName("GKIInstanceFile");
								var esqGKIInstanceFileEntity = esqGKIInstanceFileSchema.CreateEntity(UserConnection);
								esqGKIInstanceFileEntity.SetDefColumnValues();
								esqGKIInstanceFileEntity.SetColumnValue("GKIInstanceId", instanceId);
								esqGKIInstanceFileEntity.SetColumnValue("TypeId", Terrasoft.WebApp.FileConsts.FileTypeUId);
								esqGKIInstanceFileEntity.SetColumnValue("Name", "LicenseRequest.tlr");
								esqGKIInstanceFileEntity.SetColumnValue("Data", tlrFile);
								esqGKIInstanceFileEntity.Save();
							}
						}
						catch (Exception ex)
						{
							GKIWriteGKIInstanceTlrInstallError(instanceId, ex.Message);
							returnMsg = errorMsg;
							continue;
						}
					}
					else
					{
						GKIWriteGKIInstanceTlrInstallError(instanceId, "Invalid or empty URL");
						returnMsg = errorMsg;
						continue;
					}

					GKIWriteGKIInstanceTlrInstallError(instanceId, String.Empty);
					continue;
				}
				//sending an email request to tech support
				if (returnMsg == successMsg)
				{
					try
					{
						List<byte[]> filesData = (from kvp in versionsList select kvp.Value).Distinct().ToList();
						GKISendTlrEmail(filesData);
						foreach (Entity GKIInstanceEntity in GKIInstanceESQCollection)
						{
							var fileEntity = GKIGetTlsFileEntity(GKIInstanceEntity.GetTypedColumnValue<string>("GKIVersion"), CustomerId);
							if (fileEntity == null)
							{
								GKIChangeGKIInstanceStatus(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), GKILicensingConstantsCs.GKIApplicationStatus.RequestSent);
							}
                            else
                            {
								GKIChangeGKIInstanceStatus(GKIInstanceEntity.GetTypedColumnValue<Guid>(idColumnName), GKILicensingConstantsCs.GKIApplicationStatus.UpdateRequired);
							}
						}
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
				MsgChannelUtilities.PostMessage(UserConnection, "GKILicensingMessage", "\"GKIInstanceApplicationStatusUpdated\"");
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
		/// <summary>
		/// Включение процесса проверки пульса в расписание
		/// </summary>
		public void GKIPulseStart()
		{
			AppScheduler.ScheduleMinutelyProcessJob("GKILicenseCheckThePulseProcess", "GKILicensePulse", "GKILicenseCheckThePulseProcess",
				UserConnection.Workspace.Name, UserConnection.AppConnection.SystemUserConnection.CurrentUser.Name, 1, null, true, true);
		}

		/// <summary>
		/// Инициализация таймера проверки активности пульса
		/// </summary>
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

		/// <summary>
		/// Запуск процесса проверки пульса
		/// </summary>
		public void GKIPulseBeatBusinessProcessInit()
        {
			ProcessSchema schema = UserConnection.AppConnection.SystemUserConnection.ProcessSchemaManager.GetInstanceByName("GKILicensingPulseBusinessProcess");
			var flowEngine = new FlowEngine(UserConnection.AppConnection.SystemUserConnection);
			var param = new Dictionary<string, string>();
			flowEngine.RunProcess(schema, param);
		}

		/// <summary>
		/// Пульс - регулярная проверка связи с экземплярами и передача необходимых данных
		/// </summary>
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
				bool uriCheckResult = GKIUriCheck(baseUrl);

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
							GKILicenseHttpRequest(instanceId, GKILicensingConstantsCs.LicensingServices.GKIPulseUrl, message));
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

		/// <summary>
		/// Проверка активности пульса
		/// </summary>
		public void GKILicenseCheckThePulse()
		{
			if (GKIPulseTimer == null)
			{
				GKIPulseTimerInit();
			}
		}

		/// <summary>
		/// Остановка проверки пульса
		/// </summary>
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

		/// <summary>
		/// Получение VIP-пользователей с экземпляров
		/// </summary>
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
				bool uriCheckResult = GKIUriCheck(baseUrl);

				if (baseUrl != String.Empty && uriCheckResult)
				{
					try
					{
						GKIVIPUsers vipUsers;
						string response = GKILicenseHttpRequest(instanceId, GKILicensingConstantsCs.LicensingServices.GKIVIPUsersUrl);
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

		/// <summary>
		/// Сохранение системных настроек со страницы настроек
		/// </summary>
		/// <param name="request">JSON с настройками</param>
		[OperationContract]
		[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public ConfigurationServiceResponse GKISaveLicensingSysSettings(Dictionary<string, string> request)
		{
			var response = new ConfigurationServiceResponse();
			try
			{
				GKIUserConnectionCheck();
				UserConnection.DBSecurityEngine.CheckCanExecuteOperation("GKICanManageLicensingSettings");
				request.ForEach(sysSettings => GKISetSysSettingsDefValue(sysSettings.Key, sysSettings.Value));
			}
			catch (Exception e)
			{
				response.Exception = e;
			}
			return response;
		}

		/// <summary>
		/// Деактивация пользователей и постановка их в очередь обновления
		/// </summary>
		/// <param name="deactivateLicUsers">список пользователей</param> 
		/// <param name="instanceId">Id экземпляра</param> 
		/// <param name="reason">причина деактивации</param> 
		public void GKIDeactivatedUsersUpdate(List<Guid> deactivateLicUsers, Guid instanceId, Guid reason)
		{
			IEnumerable<QueryParameter> allDeactivateLicUsersIdsEnum = deactivateLicUsers.ConvertAll(x => new QueryParameter(x));
			if (allDeactivateLicUsersIdsEnum.Count() > 0)
			{
				Update updateLicensesInactive = new Update(UserConnection, "GKILicUserInstanceLicPackage")
					.Set("GKIActive", Column.Parameter(false))
					.Set("GKIDeactivatedBySync", Column.Parameter(true))
					.Set("GKIDeactivationReasonLookupId", Column.Parameter(reason))
					.Set("GKISyncedState", Column.Parameter(true))
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

		/// <summary>
		/// Смена пароля для экземпляра
		/// </summary>
		/// <param name="instanceId">Id экземпляра</param> 
		/// <param name="password">пароль</param> 
		/// <returns></returns>
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

		/// <summary>
		/// Оповещение администраторов о рассинхронизации данных из MSAD и экземплярами
		/// </summary>
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

		/// <summary>
		/// Создание уведомления
		/// </summary>
		/// <param name="remindingSubject">заголовок</param> 
		/// <param name="schemaName">имя схемы</param> 
		/// <param name="contactId"> Id контакта</param>
		/// <param name="recordId">Id записи для ссылки</param> 
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
		#endregion

		#region Private Methods

		/// <summary>
		/// Установка значения системной настройки.
		/// </summary>
		/// <param name="code">код</param>
		/// <param name="value">значение</param>
		private void GKISetSysSettingsDefValue(string code, object value)
		{
			var sysSettings = new Terrasoft.Core.Configuration.SysSettings(UserConnection);
			if (sysSettings.FetchFromDB("Code", code))
			{
				Terrasoft.Core.Configuration.SysSettings.SetDefValue(UserConnection, code, value);
			}
		}

		/// <summary>
		/// Установка статуса экземпляра
		/// </summary>
		/// <param name="instanceId">экземпляр</param> 
		/// <param name="status">статус</param> 
		private void GKIChangeGKIInstanceStatus(Guid instanceId, Guid status)
        {
			var schemaGKIInstance = UserConnection.EntitySchemaManager.GetInstanceByName("GKIInstance");
			Entity entityGKIInstance = schemaGKIInstance.CreateEntity(UserConnection);
			if (entityGKIInstance.FetchFromDB("Id", instanceId))
			{
				entityGKIInstance.SetDefColumnValues();
				entityGKIInstance.SetColumnValue("GKIApplicationStatusId", status);
				entityGKIInstance.Save();
			}
		}

		/// <summary>
		/// Запрос на установку tls-файла
		/// </summary>
		/// <param name="baseUrl">url экземпляра</param> 
		/// <param name="instanceId">id экземпляра</param> 
		/// <param name="licMessage">содержимое файла</param> 
		/// <returns>описание ошибки</returns>
		private string GKITlsInstallRequest(string baseUrl, Guid instanceId, string licMessage)
		{
			bool uriCheckResult = GKIUriCheck(baseUrl);
			if (baseUrl != String.Empty && uriCheckResult)
			{
				try
				{
					string installationResult = GKILicenseHttpRequest(instanceId, GKILicensingConstantsCs.LicensingServices.GKITlsInstallRegularServiceUrl, licMessage);
					TlsCustomInstallResult tlsInstallResult = JsonConvert.DeserializeObject<TlsCustomInstallResult>(installationResult);
					if (tlsInstallResult.success != true)
					{
						return tlsInstallResult.errorInfo;
					}
					return String.Empty;
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			}
			else
			{
				return "Invalid or empty URL";
			}
		}

		/// <summary>
		/// Проверка валидности url
		/// </summary>
		/// <param name="baseUrl">url</param> 
		/// <returns> валидность </returns>
		private bool GKIUriCheck(string baseUrl)
		{
			Uri uriResult;
			bool uriCheckResult = Uri.TryCreate(baseUrl, UriKind.Absolute, out uriResult)
				&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
			return uriCheckResult;
		}

		/// <summary>
		/// Получение записи tls файла
		/// </summary>
		/// <param name="fileVersion">версия файла (может быть с билдом и без него)</param> 
		/// <param name="customerId">идентификатор клиента</param> 
		/// <returns>объект</returns>
		private Entity GKIGetTlsFileEntity(string fileVersion, Guid customerId)
        {
			string validFileVersion = fileVersion.Count(f => f == '.') > 2
				? fileVersion.Substring(0, fileVersion.LastIndexOf("."))
				: fileVersion;
			Dictionary<string, object> conditions = new Dictionary<string, object>();
			conditions.Add("GKICustomerID", customerId);
			conditions.Add("GKIVersion", validFileVersion);

			var schemaGKITlsFileEntity = UserConnection.EntitySchemaManager.GetInstanceByName("GKITlsFileEntity");
			Entity entityGKITlsFileEntity = schemaGKITlsFileEntity.CreateEntity(UserConnection);
			if (!entityGKITlsFileEntity.FetchFromDB(conditions))
			{
				return null;
			}
			return entityGKITlsFileEntity;
		}

		/// <summary>
		/// Создание уведомления по процессу
		/// </summary>
		/// <param name="contactId">адресат</param> 
		/// <param name="processName">имя процесса</param> 
		/// <param name="subject">заголовок</param> 
		/// <param name="description">описание</param> 
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

		/// <summary>
		/// Получение записи журнала процессов
		/// </summary>
		/// <param name="processSchema">схема процесса</param> 
		/// <returns> Id записи журнала процессов </returns>
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

		/// <summary>
		/// Отправить уведомление администраторам службы лицензирования
		/// </summary>
		/// <param name="message">текст уведомления</param> 
		/// <param name="schemaName">имя схемы</param> 
		/// <param name="recordId">Id для ссылки на запись</param> 
		private void SendLicAdminReminding(string message, string schemaName, Guid? recordId = null)
        {
			var esqSysAdminUnitCollection = GetLicAdminCollection(out string contactColumnName);
			foreach (var entitySysAdminUnit in esqSysAdminUnitCollection)
            {
				CreateReminding(message, schemaName, entitySysAdminUnit.GetTypedColumnValue<Guid>(contactColumnName), recordId);
			}
		}

		/// <summary>
		/// Отправить уведомление по процессу администраторам службы лицензирования
		/// </summary>
		/// <param name="processName">имя процесса</param> 
		/// <param name="message">текст уведомления</param> 
		private void SendLicAdminProcessReminding(string processName, string message)
		{
			var esqSysAdminUnitCollection = GetLicAdminCollection(out string contactColumnName);
			foreach (var entitySysAdminUnit in esqSysAdminUnitCollection)
			{
				CreateRemindingByProcess(entitySysAdminUnit.GetTypedColumnValue<Guid>(contactColumnName), processName, message);
			}
		}

		/// <summary>
		/// Получение коллекции объектов администраторов службы лицензирования
		/// </summary>
		/// <param name="contactColumnName">имя колонки объекта "Контакт"</param> 
		/// <returns></returns>
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

		/// <summary>
		/// Проверка VIP-лимитов
		/// </summary>
		/// <param name="instanceId">Id экземпляра</param> 
		/// <param name="productName">наимменование продукта</param> 
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

		/// <summary>
		/// Установка VIP-лицензий
		/// </summary>
		/// <param name="productName">наименование продукта</param> 
		/// <param name="vipUsersNames">список VIP-пользователей</param> 
		/// <param name="instanceId">Id экземпляра</param> 
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

		/// <summary>
		/// Лицензирование пользователей после неудачной попытки аутентификации
		/// </summary>
		/// <param name="pulseData">данные сервиса</param> 
		/// <param name="esqGKIInstanceEntity">объект экземпляра</param> 
		/// <param name="instanceId"> Id экземпляра</param>
		/// <param name="baseUrl">url экземпляра</param> 
		private void PulseLicensing(PulseData pulseData, Entity esqGKIInstanceEntity, Guid instanceId, string baseUrl)
        {
			List<LicUserData> licVIPUserData = new List<LicUserData>();
			
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
						licVIPUserData.Add(new LicUserData { LicUserName = userName, LicPackageName = packName });
						recordsToActivate.Add(new QueryParameter(recordId));
						licPackageAvailable[packName] += -1;
					}
					else
					{
						bool isEligible = GetUserGroupADLicenseEligible(esqGKILicUserInstanceLicPackageEntity);
						if (licPackageAvailable[packName] > 0 && isEligible)
						{
							licVIPUserData.Add(new LicUserData { LicUserName = userName, LicPackageName = packName });
							recordsToActivate.Add(new QueryParameter(recordId));
							licPackageAvailable[packName] += -1;
						}
					}
				}

				//recall licenses if there's an overflow
				GKILicPackageShortageHandler(licPackageAvailable, instanceId);

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
				if (licVIPUserData.Count > 0)
				{
					GKILicenseAssignmentRequest(instanceId, true, licVIPUserData);
				}
			}
		}

		/// <summary>
		/// Управление лишением лицензий для раздачи VIP-пользователю
		/// </summary>
		/// <param name="licPackageAvailable">лицензия, доступно</param>
		/// <param name="instanceId">экземпляр</param>
		private void GKILicPackageShortageHandler(Dictionary<string, int> licPackageAvailable, Guid instanceId)
        {
			List<QueryParameter> records = new List<QueryParameter>();
			List<LicUserData> licUserData = new List<LicUserData>();
			foreach (var key in licPackageAvailable.Keys)
            {
				if (licPackageAvailable[key] < 0)
                {
					var selectGKIInstanceLicUser =
						new Select(UserConnection)
						.Top(-licPackageAvailable[key])
						.Column("GKILicUserInstanceLicPackage", "Id")
						.Column(new Select(UserConnection).Column("GKIName")
							.From("GKILicUser")
							.Where("GKILicUser", "Id")
							.IsEqual("GKILicUserInstanceLicPackage", "GKILicUserId"))
						.Column(new Select(UserConnection).Column("GKILastActivityDateTime")
							.From("GKIInstanceLicUser")
							.Where("GKIInstanceLicUser", "GKILicUserId").IsEqual("GKILicUserInstanceLicPackage", "GKILicUserId")
							.And("GKIInstanceLicUser", "GKIInstanceId").IsEqual("GKILicUserInstanceLicPackage", "GKIInstanceId")
						).As("LastActivityDateTime")
						.Column(new Select(UserConnection).Column("GKIRegistrationDate")
							.From("GKIInstanceLicUser")
							.Where("GKIInstanceLicUser", "GKILicUserId").IsEqual("GKILicUserInstanceLicPackage", "GKILicUserId")
							.And("GKIInstanceLicUser", "GKIInstanceId").IsEqual("GKILicUserInstanceLicPackage", "GKIInstanceId")
						).As("RegistrationDate")
						.From("GKILicUserInstanceLicPackage")
						.Where("GKILicUserInstanceLicPackage", "GKIInstanceId").IsEqual(Column.Parameter(instanceId))
						.And("GKILicUserInstanceLicPackage", "GKIActive").IsEqual(Column.Parameter(true))
						.And("GKILicUserInstanceLicPackage", "GKISyncedState").IsEqual(Column.Parameter(true))
						.And("GKILicUserInstanceLicPackage", "GKIReserved").IsEqual(Column.Parameter(false))
						.And("GKILicUserInstanceLicPackage", "GKILicPackageId").In(new Select(UserConnection)
							.Column("GKILicPackage", "Id").From("GKILicPackage").Where("GKILicPackage", "GKIName").IsEqual(Column.Parameter(key)))
						.And("GKILicUserInstanceLicPackage", "GKILicUserId").Not().In(new Select(UserConnection)
							.Column("GKILicUser", "Id").From("GKILicUser").Where("GKILicUser", "GKIName").IsEqual(Column.Parameter("Supervisor"))
								.Or("GKILicUser", "GKIName").IsEqual(Column.Parameter("SysPortalConnection")))
						.OrderByAsc("LastActivityDateTime")
						.OrderByAsc("RegistrationDate")
						as Select;
					using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
					{
						using (IDataReader dr = selectGKIInstanceLicUser.ExecuteReader(dbExecutor))
						{
							while (dr.Read())
							{
								licUserData.Add(new LicUserData { LicUserName = dr.GetString(1), LicPackageName = key });
								records.Add(new QueryParameter(dr.GetGuid(0)));
							}
						}
					}
				}
            }
			if (records.Count > 0)
			{
				IEnumerable<QueryParameter> allDeactivateRecordsEnum = records.AsEnumerable();
				Update updateLicensesNotActive = new Update(UserConnection, "GKILicUserInstanceLicPackage")
					.Set("GKIActive", Column.Parameter(false))
					.Where("Id").In(allDeactivateRecordsEnum)
				as Update;
				updateLicensesNotActive.Execute();
				GKILicenseAssignmentRequest(instanceId, false, licUserData);
			}
		}

		/// <summary>
		/// "Раздали" ли лицензию пользователю в MS AD
		/// </summary>
		/// <param name="esqGKILicUserInstanceLicPackageEntity">объект Лицензии пользователей платформы</param> 
		/// <returns> Раздали </returns>
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

		/// <summary>
		/// Получение лимитов VIP-пользователей для экземпляра
		/// </summary>
		/// <param name="instanceId">экземпляр</param> 
		/// <returns> лицензия, лимит </returns>
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
			esqGKIInstanceLicense.Filters.Add(
				esqGKIInstanceLicense.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILic.GKILicType", GKILicensingConstantsCs.GKILicType.Personal));
			var esqGKIInstanceLicenseCollection = esqGKIInstanceLicense.GetEntityCollection(UserConnection);
			foreach (var esqGKIInstanceLicenseEntity in esqGKIInstanceLicenseCollection)
            {
				vipLicensesLimits[esqGKIInstanceLicenseEntity.GetTypedColumnValue<string>(columnGKILicPackageName.Name)] =
					esqGKIInstanceLicenseEntity.GetTypedColumnValue<int>("GKILimitVIP");
			}

			return vipLicensesLimits;
		}

		/// <summary>
		/// Получение количества доступных лицензий для идентификатора
		/// </summary>
		/// <param name="GKICustomerIDId"> идентификатор компании</param>
		/// <returns> лицензия, количество </returns>
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

		/// <summary>
		/// Запрос на обновление лицензий на экземплярах
		/// </summary>
		/// <param name="isAddLicense">добавить / отозвать</param> 
		/// <param name="enhancedCollection">коллекция данных для обновления</param> 
		/// <param name="GKIUrlClm">наимемнование колонки url экземпляра</param> 
		/// <param name="GKILicUserNameClm">наимемнование колонки пользователя платформы</param> 
		/// <param name="GKILicPackageNameClm">наимемнование колонки продукта</param> 
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
				Guid instanceId = enhancedEntity.GetTypedColumnValue<Guid>("GKIInstanceId");
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

		/// <summary>
		/// Получение информации об установленных лицензиях со всех экземпляров
		/// </summary>
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

		/// <summary>
		/// Получение информации об установленных лицензиях со всех экземпляров идентификатора компании
		/// </summary>
		/// <param name="CustomerId">идентификатор компании</param> 
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
				bool uriCheckResult = GKIUriCheck(baseUrl);
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

		/// <summary>
		/// Получение информации об установленных лицензиях с экземпляра
		/// </summary>
		/// <param name="baseUrl">url экземпляра</param> 
		/// <param name="CustomerId">идентификатор компании</param> 
		/// <param name="instanceId">экземпляр</param> 
		/// <returns> описание ошибок </returns>
		private string GKIGetInstallLicensesInfo(string baseUrl, Guid CustomerId, Guid instanceId)
		{
			try
			{
				string bareLicenseInfoResult = GKILicenseHttpRequest(instanceId, GKILicensingConstantsCs.LicensingServices.GKIGetInstalledLicensesInfoServiceUrl);
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

		/// <summary>
		/// Деактивация лицензий у пользователей по результатам синхронизации
		/// </summary>
		/// <param name="syncResult"> результаты синхронизации</param>
		/// <param name="instanceId">экземпляр</param> 
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
							userSyncResultSysAdminUnit.RegistrationDateTime ==
							DateTime.Now.AddDays(-Convert.ToInt32(cooldownSysValue))
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

		/// <summary>
		/// Запрос претендентов на деактцивацию лицензий
		/// </summary>
		/// <param name="baseUrl">url экземпляра</param> 
		/// <param name="serviceUrl">url сервиса</param> 
		/// <param name="instanceId">экземпляр</param> 
		/// <returns> описание ошибок </returns>
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

		/// <summary>
		/// Запрос синхронизации с экземпляром
		/// </summary>
		/// <param name="baseUrl">url экземпляра</param> 
		/// <param name="serviceUrl">url сервиса</param> 
		/// <param name="instanceId">экземпляр</param> 
		/// <returns> описание ошибок </returns>
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

		/// <summary>
		/// Запрос на обновление лицензий экземпляра
		/// </summary>
		/// <param name="instanceId">экземпляр</param> 
		/// <param name="isAddLicense">добавить / отозвать</param> 
		/// <param name="licUserDataList">список пользователей и лицензий</param> 
		/// <returns> описание ошибок </returns>
		private string GKILicenseAssignmentRequest(Guid instanceId, bool isAddLicense, List<LicUserData> licUserDataList)
		{
			string message = JsonConvert.SerializeObject(licUserDataList);
			string serviceUrl = isAddLicense
				? GKILicensingConstantsCs.LicensingServices.GKIAddLicenseServiceUrl
				: GKILicensingConstantsCs.LicensingServices.GKIRemoveLicenseServiceUrl;
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

		/// <summary>
		/// Обработка результата обновления лицензий 
		/// </summary>
		/// <param name="syncResults">ответ сервиса</param> 
		/// <param name="isAddLicense">добавить / отозвать</param> 
		/// <param name="userMessage">текст сообщения</param> 
		/// <param name="instanceId">экземпляр</param> 
		private void LicSyncResultProcessing(List<LicUserSyncResult> syncResults, bool isAddLicense, out string userMessage, Guid instanceId)
        {

			List<LicUserSyncResult> trueSyncResults = syncResults.FindAll(item => item.isSuccess == true);
			List<LicUserSyncResult> errorSyncResults = syncResults.FindAll(item => item.isSuccess == false);

			userMessage = String.Concat(
				trueSyncResults.Count.ToString(),
				" licenses were ", isAddLicense ? "granted. " : "removed. ",
				errorSyncResults.Count.ToString(), " user sync errors. ");

			IEnumerable<string> licPackageNames = syncResults.Select(x => x.LicPackageName).Distinct();
			
			foreach (string licPackageName in licPackageNames)
			{
				List<LicUserSyncResult> licPackageTrueSyncResults = syncResults.FindAll(item => item.isSuccess == true && item.LicPackageName == licPackageName);
				List<LicUserSyncResult> licPackageErrorSyncResults = syncResults.FindAll(item => item.isSuccess == false && item.LicPackageName == licPackageName);
				IEnumerable<string> errorNames = licPackageErrorSyncResults.Select(x => x.ErrorMsg).Distinct();

				//update valid results
				IEnumerable<QueryParameter> syncedUserNamesEnum = licPackageTrueSyncResults.ConvertAll(x => new QueryParameter(x.LicUserName));
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
					List<LicUserSyncResult> licPackageErrorSpecificSyncResults = licPackageErrorSyncResults.FindAll(item => item.ErrorMsg == errorName);
					IEnumerable<QueryParameter> syncedErrorUserNamesEnum = licPackageErrorSpecificSyncResults.ConvertAll(x => new QueryParameter(x.LicUserName));
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

		/// <summary>
		/// Запущен ли бизнес-процесс
		/// </summary>
		/// <param name="processName">имя процесса</param> 
		/// <returns> запущен </returns>
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

		/// <summary>
		/// Отменить все запущенные экземпляры бизнес-процесса
		/// </summary>
		/// <param name="processName">имя процесса</param> 
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

		/// <summary>
		/// Отправка имейла с tlr-файлами
		/// </summary>
		/// <param name="filesData">tlr-файлы</param> 
		private void GKISendTlrEmail(List<byte[]> filesData)
        {
			Guid mailboxId = Terrasoft.Core.Configuration.SysSettings.GetValue(UserConnection, "GKILicensingMailBox", Guid.Empty);
			string recipientEmail = Terrasoft.Core.Configuration.SysSettings.GetValue(UserConnection, "GKILicensingTerrasoftSupportAddress", String.Empty);
			GetSubjectAndBodyFromEmailTemplate(GKILicensingConstantsCs.Misc.TlrRequestEmailTemplate, out string emailSubject, out string emailBody);

			GKISendActivityEmail(UserConnection, mailboxId, recipientEmail, emailSubject, emailBody, filesData, null, "LicenseRequest", ".tlr");
		}

		/// <summary>
		/// Получение заголовка и тела письма из шаблона
		/// </summary>
		/// <param name="templateId">шаблон</param> 
		/// <param name="emailSubject">заголовок письма</param> 
		/// <param name="emailBody">тело письма</param> 
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

		/// <summary>
		/// Отправка имейла
		/// </summary>
		/// <param name="UserConnection">UserConnection</param> 
		/// <param name="mailboxId">настройки синхронизации с почтовым ящиком</param> 
		/// <param name="recipientEmail">адрес получателя</param> 
		/// <param name="subject">заголовок</param> 
		/// <param name="body">тело</param> 
		/// <param name="filesData">файлы</param> 
		/// <param name="fileNames">имена файлов</param> 
		/// <param name="defaultFileName">имя файла по умолчанию</param> 
		/// <param name="defaultFileExtension">расширение файла по умолчанию</param> 
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

		/// <summary>
		/// Обновление галочки "Активен на экземплярах"
		/// </summary>
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

		/// <summary>
		/// Обновление галочки "Активен"
		/// </summary>
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

		/// <summary>
		/// Поток в байты
		/// </summary>
		/// <param name="stream">поток</param> 
		/// <returns> байты </returns>
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

		/// <summary>
		/// Запись ошибки в Лицензии пользователей платформы
		/// </summary>
		/// <param name="Id">запись</param> 
		/// <param name="errorMsg">текст ошибки</param> 
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

		/// <summary>
		/// Запись ошибки установки лицензий в Экземпляр платформы
		/// </summary>
		/// <param name="Id">запись</param> 
		/// <param name="errorMsg">текст ошибки</param> 
		private void GKIWriteGKIInstanceLastTlsInstallError(Guid Id, string errorMsg)
		{
			Update update = new Update(UserConnection, "GKIInstance")
				.Set("GKILastTlsInstallError", Column.Parameter(errorMsg))
				.Set("GKIApplicationStatusId", Column.Parameter(errorMsg == String.Empty 
					? GKILicensingConstantsCs.GKIApplicationStatus.TlsInstalled
					: GKILicensingConstantsCs.GKIApplicationStatus.TlsNotInstalled))
				.Where("Id")
				.IsEqual(Column.Parameter(Id))
				as Update;
			update.Execute();
		}

		/// <summary>
		/// Запись ошибки запроса лицензий в Экземпляр платформы
		/// </summary>
		/// <param name="Id">запись</param> 
		/// <param name="errorMsg">текст ошибки</param> 
		private void GKIWriteGKIInstanceTlrInstallError(Guid Id, string errorMsg)
		{
			Update update = new Update(UserConnection, "GKIInstance")
				.Set("GKILastTlrInstallError", Column.Parameter(errorMsg))
				.Where("Id")
				.IsEqual(Column.Parameter(Id))
				as Update;
			update.Execute();
		}

		/// <summary>
		/// Авторизация на экземпляре
		/// </summary>
		/// <param name="instanceId">экземпляр</param> 
		/// <returns> описание ошибок </returns>
		private string GKIAuthorize(Guid instanceId)
		{
			var esqGKIInstance = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "GKIInstance");
			esqGKIInstance.UseAdminRights = false;
			esqGKIInstance.AddColumn("GKIUrl");
			esqGKIInstance.AddColumn("GKIPassword");
			var loginColumn = esqGKIInstance.AddColumn("GKILogin");
			esqGKIInstance.Filters.Add(
				esqGKIInstance.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", instanceId));
			var esqGKIInstanceCollection = esqGKIInstance.GetEntityCollection(UserConnection);
			var instanceEntity = esqGKIInstanceCollection.FirstOrDefault();
			string baseUrl = instanceEntity.GetTypedColumnValue<string>("GKIUrl");
			string login = instanceEntity.GetTypedColumnValue<string>(loginColumn.Name);
			string password = instanceEntity.GetTypedColumnValue<string>("GKIPassword");

			string authUrl = string.Concat(baseUrl, GKILicensingConstantsCs.LicensingServices.authServicePath);
			string authMessage = String.Format(GKILicensingConstantsCs.LicensingServices.authTemplate, login, password);
			
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
					HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(String.Concat(baseUrl, GKILicensingConstantsCs.LicensingServices.GKIAuthCheckServiceUrl));
					httprequest.Method = "POST";
					httprequest.Accept = @"application/json";
					httprequest.ContentLength = 0;
					httprequest.ContentType = @"application/json";
					httprequest.CookieContainer = AuthCookies;
					var crsfcookie = httprequest.CookieContainer.GetCookies(new Uri(baseUrl))[GKILicensingConstantsCs.LicensingServices.crsfName];
					if (crsfcookie != null)
					{
						httprequest.Headers.Add(GKILicensingConstantsCs.LicensingServices.crsfName, crsfcookie.Value);
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
				string authCookieValue = response.Cookies[GKILicensingConstantsCs.LicensingServices.authName]?.Value ?? String.Empty;
				string crsfCookieValue = response.Cookies[GKILicensingConstantsCs.LicensingServices.crsfName]?.Value ?? String.Empty;
				if (authCookieValue != String.Empty)
				{
					AuthCookies.Add(new Uri(baseUrl), new Cookie(GKILicensingConstantsCs.LicensingServices.authName, authCookieValue));
				}
				if (crsfCookieValue != String.Empty)
				{
					AuthCookies.Add(new Uri(baseUrl), new Cookie(GKILicensingConstantsCs.LicensingServices.crsfName, crsfCookieValue));
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

		/// <summary>
		/// Повторная авторизация
		/// </summary>
		/// <param name="baseUrl">url экземпляра</param> 
		/// <param name="authUrl">url сервиса авторизации</param> 
		/// <param name="authMessage">сообщение</param> 
		/// <returns> описание ошибок </returns>
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
				string authCookieValue = response.Cookies[GKILicensingConstantsCs.LicensingServices.authName]?.Value ?? String.Empty;
				string crsfCookieValue = response.Cookies[GKILicensingConstantsCs.LicensingServices.crsfName]?.Value ?? String.Empty;
				if (authCookieValue != String.Empty)
				{
					AuthCookies.Add(new Uri(baseUrl), new Cookie(GKILicensingConstantsCs.LicensingServices.authName, authCookieValue));
				}
				if (crsfCookieValue != String.Empty)
				{
					AuthCookies.Add(new Uri(baseUrl), new Cookie(GKILicensingConstantsCs.LicensingServices.crsfName, crsfCookieValue));
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

		/// <summary>
		/// Базовый запрос без данных (Get)
		/// </summary>
		/// <param name="instanceId">экземпляр</param> 
		/// <param name="serviceUrl">url сервиса</param> 
		/// <returns> описание ошибок </returns>
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
			var crsfcookie = httprequest.CookieContainer.GetCookies(new Uri(baseUrl))[GKILicensingConstantsCs.LicensingServices.crsfName];
			if (crsfcookie != null)
			{
				httprequest.Headers.Add(GKILicensingConstantsCs.LicensingServices.crsfName, crsfcookie.Value);
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

		/// <summary>
		/// Базовый запрос без данных (Post)
		/// </summary>
		/// <param name="instanceId">экземпляр</param> 
		/// <param name="serviceUrl">url сервиса</param> 
		/// <returns> описание ошибок </returns>
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
			var crsfcookie = httprequest.CookieContainer.GetCookies(new Uri(baseUrl))[GKILicensingConstantsCs.LicensingServices.crsfName];
			if (crsfcookie != null)
			{
				httprequest.Headers.Add(GKILicensingConstantsCs.LicensingServices.crsfName, crsfcookie.Value);
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

		/// <summary>
		/// Базовый запрос с данными
		/// </summary>
		/// <param name="instanceId">экземпляр</param> 
		/// <param name="serviceUrl">url сервиса</param> 
		/// <param name="message">данные в строковом формате</param> 
		/// <returns> описание ошибок </returns>
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
			var crsfcookie = httprequest.CookieContainer.GetCookies(new Uri(baseUrl))[GKILicensingConstantsCs.LicensingServices.crsfName];
			if (crsfcookie != null)
			{
				httprequest.Headers.Add(GKILicensingConstantsCs.LicensingServices.crsfName, crsfcookie.Value);
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

		/// <summary>
		/// Базовый запрос с данными в байтах
		/// </summary>
		/// <param name="instanceId">экземпляр</param> 
		/// <param name="serviceUrl">url сервиса</param> 
		/// <param name="file">байты</param> 
		/// <returns> описание ошибок </returns>
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
			var crsfcookie = httprequest.CookieContainer.GetCookies(new Uri(baseUrl))[GKILicensingConstantsCs.LicensingServices.crsfName];
			if (crsfcookie != null)
			{
				httprequest.Headers.Add(GKILicensingConstantsCs.LicensingServices.crsfName, crsfcookie.Value);
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

		/// <summary>
		/// Получение url экземпляра
		/// </summary>
		/// <param name="instanceId">экземпляр</param> 
		/// <returns> url </returns>
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

		/// <summary>
		/// Получение UserConnection
		/// </summary>
		/// <returns> UserConnection </returns>
		private UserConnection GKIGetUserConnection()
		{
			return UserConnection ?? (UserConnection)HttpContext.Current.Session["UserConnection"];
		}

		/// <summary>
		/// Проверка наличия UserConnection
		/// </summary>
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

	}
	#endregion
}