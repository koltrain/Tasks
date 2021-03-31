namespace Terrasoft.Configuration
{
    using Quartz;
    using Quartz.Impl.Triggers;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Terrasoft.Common;
    using Terrasoft.Core;
    using Terrasoft.Core.DB;
    using Terrasoft.Core.Entities;
    using Terrasoft.Core.Entities.Events;
    using Terrasoft.Core.Scheduler;

    [EntityEventListener(SchemaName = "GKILicUserInstanceLicPackage")]
	public class GKILicUserInstanceLicPackageEntityEventListener : BaseEntityEventListener
	{
		/// <summary>
        /// добавление фильтрации по полям
        /// </summary>
        /// <param name="sender">отправитель</param>
        /// <param name="e">аргументы</param>
        public override void OnInserted(object sender, EntityAfterEventArgs e)
        {
            base.OnInserted(sender, e);
            var entity = (Entity)sender;
            var userConnection = entity.UserConnection;

            try
            {
                //getting values
                var userLicId = entity.GetTypedColumnValue<Guid>("GKILicUserId");
                var licActive = entity.GetTypedColumnValue<bool>("GKIActive");
                var licInstance = entity.GetTypedColumnValue<Guid>("GKIInstanceId");
                var licPackage = entity.GetTypedColumnValue<Guid>("GKILicPackageId");
                var licSynced = entity.GetTypedColumnValue<bool>("GKISyncedState");

                var rootSchema = userConnection.EntitySchemaManager.FindInstanceByName("GKIInstance");
                var esqActive = new EntitySchemaQuery(rootSchema);
                esqActive.AddAllSchemaColumns();
                esqActive.UseAdminRights = false;
                esqActive.Filters.Add(
                    esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "Id", licPackage));
                Entity summaryEntity = esqActive.GetSummaryEntity(userConnection);
                var customerId = summaryEntity.GetTypedColumnValue<Guid>("GKICustomerID");


                var rootSchemaLicPackageUser = userConnection.EntitySchemaManager.FindInstanceByName("GKILicPackageUser");
                var esqSchemaLicPackageUser = new EntitySchemaQuery(rootSchema);
                esqSchemaLicPackageUser.AddAllSchemaColumns();
                esqSchemaLicPackageUser.UseAdminRights = false;
                esqSchemaLicPackageUser.Filters.Add(
                    esqSchemaLicPackageUser.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicPackage", licPackage));
                esqSchemaLicPackageUser.Filters.Add(
                    esqSchemaLicPackageUser.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUserId", userLicId));
				esqSchemaLicPackageUser.Filters.Add(
                    esqSchemaLicPackageUser.CreateFilterWithParameters(FilterComparisonType.Equal, "GKICustomerID", customerId));
                EntityCollection entityLicPackageUser = esqSchemaLicPackageUser.GetEntityCollection(userConnection);

                #region GKILicUserInstanceLicPackage

                if (entityLicPackageUser.Count == 0)
                {
                    try
                    {
                        var insertLicPackageUser = new Insert(userConnection).Into("GKILicPackageUser")
                                             .Set("Id", Column.Parameter(Guid.NewGuid()))
                                            .Set("CreatedOn", new QueryParameter(DateTime.UtcNow))
                                            .Set("ModifiedOn", new QueryParameter(DateTime.UtcNow))
                                            .Set("GKIActive", new QueryParameter(licActive))
                                            .Set("GKICustomerIDId", new QueryParameter(customerId))
                                            .Set("GKISyncedState", new QueryParameter(licSynced))
                                            .Set("GKILicPackageId", Column.Parameter(licPackage))
                                            .Set("GKILicUserId", Column.Parameter(userLicId));

                        insertLicPackageUser.Execute();
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

		/// <summary>
		/// обновление по условиям фильтрации
		/// </summary>
		/// <param name="sender">отправитель</param>
		/// <param name="e">аргументы</param>
		public override void OnUpdating(object sender, EntityBeforeEventArgs e)
		{
			base.OnUpdating(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;
			try
			{
				var oldStatus = entity.GetTypedOldColumnValue<bool>("GKIActive");
				var curStatus = entity.GetTypedColumnValue<bool>("GKIActive");
				var oldBySyncStatus = entity.GetTypedOldColumnValue<bool>("GKIDeactivatedBySync");
				var curBySyncStatus = entity.GetTypedColumnValue<bool>("GKIDeactivatedBySync");
				var oldIsReservedStatus = entity.GetTypedOldColumnValue<bool>("GKIReserved");
				var curIsReservedStatus = entity.GetTypedColumnValue<bool>("GKIReserved");
				#region GKILicUserInstanceLicPackageQueue
				try
				{
					if (oldStatus != curStatus)
					{
						var rootSchema = userConnection.EntitySchemaManager.GetInstanceByName("GKILicUserInstanceLicPackageQueue");
						Entity queueEntity = rootSchema.CreateEntity(userConnection);
						if (!queueEntity.FetchFromDB("GKILicUserInstanceLicPackage", entity.GetTypedOldColumnValue<Guid>("Id")))
						{
							queueEntity.SetDefColumnValues();
							queueEntity.SetColumnValue("GKILicUserInstanceLicPackageId", entity.GetTypedOldColumnValue<Guid>("Id"));
							queueEntity.Save();
						}

						//if it's not auto deactivation anymore
						if (curBySyncStatus == oldBySyncStatus && curBySyncStatus == true)
						{
							entity.SetColumnValue("GKIDeactivatedBySync", false);
							entity.SetColumnValue("GKIDeactivationReasonLookupId", null);
						}
					}
				}
				catch (Exception ex)
				{
					Console.Write(ex.Message);
				}
				#endregion
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
			}
			#region GKILicUserInstanceLicPackage
			var esqEntity = userConnection.EntitySchemaManager.GetInstanceByName("GKILicUser").CreateEntity(userConnection);
			var userId = entity.GetTypedColumnValue<Guid>("GKILicUserId");
			string errorMsg = new LocalizableString(userConnection.Workspace.ResourceStorage,
				"GKILicenseListeners",
				"LocalizableStrings.GKILicUserNotActiveException.Value");
			if (esqEntity.FetchFromDB("Id", userId))
            {
				var isUserActive = esqEntity.GetTypedColumnValue<bool>("GKIActive");
				if (isUserActive == false)
		        {
					throw new InvalidOperationException(errorMsg);
				}	
			}
			#endregion
		}

		/// <summary>
		/// сохранение записей, удовлетворяющих условиям фильтрации
		/// </summary>
		/// <param name="sender">отправитель</param>
		/// <param name="e">аргументы</param>
		public override void OnSaving(object sender, EntityBeforeEventArgs e)
		{
			base.OnSaving(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;
			try
			{
				var oldIsReservedStatus = entity.GetTypedOldColumnValue<bool>("GKIReserved");
				var curIsReservedStatus = entity.GetTypedColumnValue<bool>("GKIReserved");
				#region GKILicUser
				try
				{
					if (oldIsReservedStatus != curIsReservedStatus)
					{
						bool aggrStatus;
						if (!curIsReservedStatus)
						{
							//this record isn't active anymore, so we're looking for any other record to be active
							//is there any active record at all except this one? (because it is still active! it's not saved!)
							var rootSchema = userConnection.EntitySchemaManager.FindInstanceByName("GKILicUserInstanceLicPackage");
							var esqActive = new EntitySchemaQuery(rootSchema);
							EntitySchemaQueryColumn countActive = esqActive.AddColumn("Id");
							countActive.SummaryType = AggregationType.Count;
							esqActive.Filters.Add(
								esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIReserved", true));
							esqActive.Filters.Add(
								esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser", entity.GetTypedColumnValue<Guid>("GKILicUserId")));
							esqActive.Filters.Add(
								esqActive.CreateFilterWithParameters(FilterComparisonType.NotEqual, "Id", entity.GetTypedColumnValue<Guid>("Id")));
							Entity summaryEntity = esqActive.GetSummaryEntity(userConnection);
							
							int activeRecs = summaryEntity.GetTypedColumnValue<int>(countActive.Name);
							aggrStatus = activeRecs > 0 ? true : false;

						}
						else
						{
							//if ar least one record is active (in this case our record), then the GKILicUser should be active
							aggrStatus = curIsReservedStatus;
						}
						//update
						var helperGKILicensingListenersHelper = new GKILicensingListenersHelper();
						helperGKILicensingListenersHelper.GKILicUserIsVIPUpdate(userConnection, entity.GetTypedOldColumnValue<Guid>("GKILicUserId"), aggrStatus);
					}

				}
				catch (Exception ex)
				{
					Console.Write(ex.Message);
				}
				#endregion
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
			}
		}

		/// <summary>
		/// удаление записей, удовлетворяющих условиям фильтра
		/// </summary>
		/// <param name="sender">отправитель</param>
		/// <param name="e">аргументы</param>
		public override void OnDeleting(object sender, EntityBeforeEventArgs e)
		{
			base.OnDeleting(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;
			try
			{
				#region GKILicUser
				try
				{
					bool aggrStatus;
					//this record isn't active anymore, so we're looking for any other record to be active
					//is there any active record at all except this one? (because it is still active! it's not saved!)
					var rootSchema = userConnection.EntitySchemaManager.FindInstanceByName("GKILicUserInstanceLicPackage");
					var esqActive = new EntitySchemaQuery(rootSchema);
					EntitySchemaQueryColumn countActive = esqActive.AddColumn("Id");
					countActive.SummaryType = AggregationType.Count;
					esqActive.Filters.Add(
						esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIReserved", true));
					esqActive.Filters.Add(
						esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser", entity.GetTypedOldColumnValue<Guid>("GKILicUserId")));
					esqActive.Filters.Add(
						esqActive.CreateFilterWithParameters(FilterComparisonType.NotEqual, "Id", entity.GetTypedOldColumnValue<Guid>("Id")));
					Entity summaryEntity = esqActive.GetSummaryEntity(userConnection);

					int activeRecs = summaryEntity.GetTypedColumnValue<int>(countActive.Name);
					aggrStatus = activeRecs > 0 ? true : false;
					//update
					var helperGKILicensingListenersHelper = new GKILicensingListenersHelper();
					helperGKILicensingListenersHelper.GKILicUserIsVIPUpdate(userConnection, entity.GetTypedOldColumnValue<Guid>("GKILicUserId"), aggrStatus);

				}
				catch (Exception ex)
				{
					Console.Write(ex.Message);
				}
				#endregion
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
			}
		}
	}

	[EntityEventListener(SchemaName = "GKIInstanceLicUser")]
	public class GKIInstanceLicUserEntityEventListener : BaseEntityEventListener
	{
		/// <summary>
		/// сохранение записей, удовлетворяющих условиям фильтрации
		/// </summary>
		/// <param name="sender">отправитель</param>
		/// <param name="e">аргументы</param>
		public override void OnSaving(object sender, EntityBeforeEventArgs e)
		{
			base.OnSaving(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;
			try
			{
				var oldStatus = entity.GetTypedOldColumnValue<bool>("GKIActive");
				var curStatus = entity.GetTypedColumnValue<bool>("GKIActive");
				if (oldStatus != curStatus)
				{
					#region GKILicUser
					try
					{
						bool aggrStatus;
						if (curStatus != true)
						{
							//this record isn't active anymore, so we're looking for any other record to be active
							//is there any active record at all except this one? (because it is still active! it's not saved!)
							var rootSchema = userConnection.EntitySchemaManager.FindInstanceByName("GKIInstanceLicUser");
							var esqActive = new EntitySchemaQuery(rootSchema);
							EntitySchemaQueryColumn countActive = esqActive.AddColumn("Id");
							countActive.SummaryType = AggregationType.Count;
							esqActive.Filters.Add(
								esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIActive", true));
							esqActive.Filters.Add(
								esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser", entity.GetTypedColumnValue<Guid>("GKILicUserId")));
							esqActive.Filters.Add(
								esqActive.CreateFilterWithParameters(FilterComparisonType.NotEqual, "Id", entity.GetTypedColumnValue<Guid>("Id")));
							Entity summaryEntity = esqActive.GetSummaryEntity(userConnection);

							int activeRecs = summaryEntity.GetTypedColumnValue<int>(countActive.Name);
							aggrStatus = activeRecs > 0 ? true : false;
						}
						else
						{
							//if ar least one record is active (in this case our record), then the GKILicUser should be active
							aggrStatus = curStatus;
						}
						//update
						var GKILicUserUpdate = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKILicUser");
						GKILicUserUpdate.AddColumn("GKIActive");
						var GKILicUserUpdateEntity = GKILicUserUpdate.GetEntity(userConnection, entity.GetTypedColumnValue<Guid>("GKILicUserId"));
						GKILicUserUpdateEntity.SetColumnValue("GKIActive", aggrStatus);
						GKILicUserUpdateEntity.Save();
					}
					catch (Exception ex)
					{
						Console.Write(ex.Message);
					}
					#endregion

					#region GKILicUserInstanceLicPackage
					try
					{
						if (curStatus != true)
						{
							List<Guid> licUser = new List<Guid> { entity.GetTypedColumnValue<Guid>("GKILicUserId") };
							var gKILicensingAdminService = new GKILicensingAdminService();
							gKILicensingAdminService.GKIDeactivatedUsersUpdate(licUser, entity.GetTypedColumnValue<Guid>("GKIInstanceId"), GKILicensingConstantsCs.GKIDeactivationReasonLookup.Inactive);
						}
					}
					catch (Exception ex)
					{
						Console.Write(ex.Message);
					}
					#endregion
				}
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
			}
		}
	}

	[EntityEventListener(SchemaName = "GKILicUser")]
	public class GKILicUserEntityEventListener : BaseEntityEventListener
	{
		/// <summary>
		/// сохранение записей, удовлетворяющих условиям фильтрации
		/// </summary>
		/// <param name="sender">отправитель</param>
		/// <param name="e">аргументы</param>
		public override void OnSaving(object sender, EntityBeforeEventArgs e)
		{
			base.OnSaving(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;
			try
			{
				var oldStatus = entity.GetTypedOldColumnValue<bool>("GKIActive");
				var curStatus = entity.GetTypedColumnValue<bool>("GKIActive");
				if (oldStatus != curStatus)
				{
					#region GKILicUser
					//важно, что ниже есть update при обновлении который работает без учета условий этого событийного слоя (дублирует их по состоянию на 13.12.2020)!!
					try
					{
						var rootSchema = userConnection.EntitySchemaManager.FindInstanceByName("GKILicUserInstanceLicPackage");
						var esqActive = new EntitySchemaQuery(rootSchema);
						EntitySchemaQueryColumn countActive = esqActive.AddColumn("Id");
						countActive.SummaryType = AggregationType.Count;
						esqActive.Filters.Add(
							esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIActive", true));
						esqActive.Filters.Add(
							esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser", entity.GetTypedColumnValue<Guid>("Id")));
						Entity summaryEntity = esqActive.GetSummaryEntity(userConnection);

						int activeRecs = summaryEntity.GetTypedColumnValue<int>(countActive.Name);

						Guid licUserStatus = !curStatus ? GKILicensingConstantsCs.GKILicUserStatus.Inactive :
							GKILicensingConstantsCs.GKILicUserStatus.Active;

							entity.SetColumnValue("GKIStatusId", licUserStatus);
					}
					catch (Exception ex)
					{
						Console.Write(ex.Message);
					}
					#endregion
				}
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
			}
		}
		public override void OnSaved(object sender, EntityAfterEventArgs e)
		{
			base.OnSaved(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;
			try
			{
				#region GKILicUser
				try
				{
					bool aggrStatus;
					//this record isn't active anymore, so we're looking for any other record to be active
					//is there any active record at all except this one? (because it is still active! it's not saved!)
					var rootSchema = userConnection.EntitySchemaManager.FindInstanceByName("GKILicUserInstanceLicPackage");
					var esqActive = new EntitySchemaQuery(rootSchema);
					EntitySchemaQueryColumn countActive = esqActive.AddColumn("Id");
					countActive.SummaryType = AggregationType.Count;
					esqActive.Filters.Add(
						esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIReserved", true));
					esqActive.Filters.Add(
						esqActive.CreateFilterWithParameters(FilterComparisonType.Equal, "GKILicUser", entity.GetTypedColumnValue<Guid>("Id")));
					Entity summaryEntity = esqActive.GetSummaryEntity(userConnection);

					int activeRecs = summaryEntity.GetTypedColumnValue<int>(countActive.Name);
					aggrStatus = activeRecs > 0 ? true : false;
					//update
					var helperGKILicensingListenersHelper = new GKILicensingListenersHelper();
					helperGKILicensingListenersHelper.GKILicUserIsVIPUpdate(userConnection, entity.GetTypedColumnValue<Guid>("Id"), aggrStatus);
				}
				catch (Exception ex)
				{
					Console.Write(ex.Message);
				}
				#endregion
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
			}
		}
	}

	[EntityEventListener(SchemaName = "GKILic")]
	public class GKILicEntityEventListener : BaseEntityEventListener
	{
		/// <summary>
		/// сохранение записей, удовлетворяющих условиям фильтрации
		/// </summary>
		/// <param name="sender">отправитель</param>
		/// <param name="e">аргументы</param>
		public override void OnSaving(object sender, EntityBeforeEventArgs e)
		{
			base.OnSaving(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;

			try
			{
				//getting values
				var oldStartDate = entity.GetTypedOldColumnValue<DateTime>("GKIStartDate");
				var newStartDate = entity.GetTypedColumnValue<DateTime>("GKIStartDate");
				var oldDueDate = entity.GetTypedOldColumnValue<DateTime>("GKIExpirationDate");
				var newDueDate = entity.GetTypedColumnValue<DateTime>("GKIExpirationDate");
				var newStatus = entity.GetTypedColumnValue<Guid>("GKILicStatusId");
				var oldStatus = entity.GetTypedOldColumnValue<Guid>("GKILicStatusId");

				#region GKILic
				//GKILicStatus active / inactive
				try
				{
					if (newStartDate != oldStartDate || newDueDate != oldDueDate)
					{
						Guid licStatus = DateTime.Now >= newStartDate && DateTime.Now < newDueDate ? GKILicensingConstantsCs.GKILicStatus.Active : GKILicensingConstantsCs.GKILicStatus.Inactive;
						entity.SetColumnValue("GKILicStatusId", licStatus);
					}
				}
				catch (Exception ex)
				{
					Console.Write(ex.Message);
				}
				#endregion
				#region GKIInstanceLicPackage
				try
				{
					if (newStatus == GKILicensingConstantsCs.GKILicStatus.Inactive)
					{
						Delete instanceLicPackageDelete = new Delete(userConnection)
							.From("GKIInstanceLicPackage")
							.Where("GKILicPackageId")
							.Not().In(new Select(userConnection)
								.Column("GKILicPackageId")
								.From("GKILic")
								.Where("GKILicStatusId").IsEqual(Column.Parameter(GKILicensingConstantsCs.GKILicStatus.Active)))
							as Delete;
						instanceLicPackageDelete.Execute();
					}
					if (newStatus == GKILicensingConstantsCs.GKILicStatus.Active)
					{
						//TODO: GKIInstanceLicPackage and GKIInstanceLicense add records logic;
					}
				}
				catch (Exception ex)
				{
					Console.Write(ex.Message);
				}
				#endregion
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
			}
		}
	}

	[EntityEventListener(SchemaName = "GKIInstanceGroupAD")]
	public class GKIInstanceGroupADEntityEventListener : BaseEntityEventListener
	{
		/// <summary>
		/// сохранение записей, удовлетворяющих условиям фильтрации
		/// </summary>
		/// <param name="sender">отправитель</param>
		/// <param name="e">аргументы</param>
		public override void OnSaving(object sender, EntityBeforeEventArgs e)
		{
			base.OnSaving(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;

			try
			{
				//getting values
				var oldGroup = entity.GetTypedOldColumnValue<Guid>("GKIGroupADId");
				var newGroup = entity.GetTypedColumnValue<Guid>("GKIGroupADId");
				var oldInstance = entity.GetTypedOldColumnValue<Guid>("GKIInstanceId");
				var newInstance = entity.GetTypedColumnValue<Guid>("GKIInstanceId");

				#region GKIGroupADUsers
				if (oldGroup != newGroup || oldInstance != newInstance)
                {
					var helperGKILicensingListenersHelper = new GKILicensingListenersHelper();
					helperGKILicensingListenersHelper.GKIGroupADUsersDelete(userConnection, oldGroup, oldInstance);
				}
                #endregion
            }
            catch (Exception ex)
			{
				Console.Write(ex.Message);
			}
		}

		/// <summary>
		/// удаление записей, удовлетворяющих условиям фильтра
		/// </summary>
		/// <param name="sender">отправитель</param>
		/// <param name="e">аргументы</param>
		public override void OnDeleting(object sender, EntityBeforeEventArgs e)
		{
			base.OnDeleting(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;

			try
			{
				//getting values
				var oldGroup = entity.GetTypedOldColumnValue<Guid>("GKIGroupADId");
				var oldInstance = entity.GetTypedOldColumnValue<Guid>("GKIInstanceId");

				#region GKIGroupADUsers
				var helperGKILicensingListenersHelper = new GKILicensingListenersHelper();
				helperGKILicensingListenersHelper.GKIGroupADUsersDelete(userConnection, oldGroup, oldInstance);
				#endregion
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
			}
		}
	}

    [EntityEventListener(SchemaName = "GKILicPackageUser")]
    public class GKILicPackageUserEventListener : BaseEntityEventListener
    {
		/// <summary>
		/// обновление по условиям фильтрации
		/// </summary>
		/// <param name="sender">отправитель</param>
		/// <param name="e">аргументы</param>
		public override void OnUpdating(object sender, EntityBeforeEventArgs e)
        {
            base.OnUpdating(sender, e);
            var entity = (Entity)sender;
            var userConnection = entity.UserConnection;

            try
            {
                var oldLicActive = entity.GetTypedOldColumnValue<bool>("GKIActive");
                var newLicActive = entity.GetTypedColumnValue<bool>("GKIActive");

                var userLicId = entity.GetTypedColumnValue<Guid>("GKILicUserId");
                var licInstance = entity.GetTypedColumnValue<Guid>("GKIInstanceId");
                var licPackage = entity.GetTypedColumnValue<Guid>("GKILicPackageId");
                var customerId = entity.GetTypedColumnValue<Guid>("GKICustomerIDId");

                #region GKILicUserInstanceLicPackage
                if (oldLicActive != newLicActive)
                {
                    try
                    {
                        var insertLicPackageUser = new Update(userConnection, "GKILicUserInstanceLicPackage")
                                            .Set("GKIActive", new QueryParameter(newLicActive))
                                            .Where("GKILicPackageId").IsEqual(Column.Parameter(licPackage))
                                            .And("GKILicUserId").IsEqual(Column.Parameter(userLicId))
                                            .And("GKIInstanceId").In(
                            new Select(userConnection)
                                .Column("Id")
                                .From("GKIInstance")
                                .Where("GKICustomerIDId").IsEqual(Column.Parameter(customerId))
                                                );
                        insertLicPackageUser.Execute();
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }

	[EntityEventListener(SchemaName = "GKIInstance")]
	public class GKIInstanceEventListener : BaseEntityEventListener
	{
		/// <summary>
		/// обновление по условиям фильтрации
		/// </summary>
		/// <param name="sender">отправитель</param>
		/// <param name="e">аргументы</param>
		public override void OnSaving(object sender, EntityBeforeEventArgs e)
		{
			base.OnSaving(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;

			try
			{
				var oldGKIIsLimitApllied = entity.GetTypedOldColumnValue<bool>("GKIIsLimitApllied");
				var newLGKIIsLimitApllied = entity.GetTypedColumnValue<bool>("GKIIsLimitApllied");

				var recordId = entity.GetTypedColumnValue<Guid>("Id");

				#region GKIInstanceLicense
				if (oldGKIIsLimitApllied != newLGKIIsLimitApllied && !newLGKIIsLimitApllied)
				{
					//clear all instance's GKIInstanceLicense.GKILimitVIP
					var updateGKILimitVIP = new Update(userConnection, "GKIInstanceLicense")
						.Set("GKILimitVIP", Column.Parameter(0))
						.Where("GKIInstanceId").IsEqual(Column.Parameter(recordId));

					updateGKILimitVIP.Execute();
				}
				if (oldGKIIsLimitApllied != newLGKIIsLimitApllied && newLGKIIsLimitApllied)
				{
					//apply the formula to the all GKIInstanceLicense.GKILimitVIP
					//a
					var esqGKIInstanceLicUser = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKIInstanceLicUser") ;
					EntitySchemaQueryColumn countColumn = esqGKIInstanceLicUser.AddColumn("Id");
					countColumn.SummaryType = AggregationType.Count;
					esqGKIInstanceLicUser.Filters.Add(
						esqGKIInstanceLicUser.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", recordId));
					Entity summary = esqGKIInstanceLicUser.GetSummaryEntity(userConnection);
					int _a = (summary != null) ? summary.GetTypedColumnValue<int>(countColumn.Name) : 0;
					
					//b
					int _b = Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "GKILicensingVIPPercentage", 0);

					//formula
					int _c = Convert.ToInt32(Math.Round((double)(_a * _b / 100)));

					var updateGKILimitVIP = new Update(userConnection, "GKIInstanceLicense")
						.Set("GKILimitVIP", Column.Parameter(_c))
						.Where("GKIInstanceId").IsEqual(Column.Parameter(recordId))
						.And("GKILimit").IsGreater(Column.Parameter(0))
						.And("GKILicId").In(
							new Select(userConnection)
							.Column("Id")
							.From("GKILic")
							.Where("GKILicStatusId").IsEqual(Column.Parameter(GKILicensingConstantsCs.GKILicStatus.Active))
						);

					updateGKILimitVIP.Execute();
				}
				#endregion
			}
			catch (Exception ex)
			{
				Console.Write(ex.Message);
			}
		}
	}

	[EntityEventListener(SchemaName = "GKILicensingProcessSchedulerTime")]
	public class GKILicensingProcessSchedulerTimeEventListener : BaseEntityEventListener
	{

		private UserConnection _userConnection;
		private Entity _entity;

		protected List<EntityColumnValue> ChangedColumns
		{
			get; private set;
		}

		#region Methods: Private

		private void InitState(Entity entity)
		{
			_entity = entity;
			_userConnection = entity.UserConnection;
			ChangedColumns = entity.GetChangedColumnValues().ToList();
		}

		private string GetProcessName(Guid processId)
		{
			var select = new Select(_userConnection).Top(1)
				.Column("Name")
				.From("VwSysProcess")
					.Where("Id").IsEqual(Column.Parameter(processId)) as Select;
			return select.ExecuteScalar<string>();
		}
		
		private bool GetIsColumnChanged(string columnName)
		{
			return ChangedColumns.Any(x => x.Name == columnName);
		}

		private string GetJobName(string processName)
		{
			return $"{processName}Job";
		}

		private string GetTriggerName(string processName)
		{
			return $"{processName}Trigger";
		}

		private string GetCronExpression(DateTime startTime)
		{
			return String.Format("0 {0} {1} * * ? *", startTime.Minute, startTime.Hour);
		}

		private void AddQuartzJob()
		{
			Guid processId = _entity.GetTypedColumnValue<Guid>("GKIProcessId");
			DateTime fireDateTime = _entity.GetTypedColumnValue<DateTime>("GKIFireTime");
			string processName = GetProcessName(processId);
			string jobName = GetJobName(processName);
			string triggerName = GetTriggerName(processName);
			string croneExpression = GetCronExpression(fireDateTime);
			AppScheduler.RemoveJob(jobName, "Main");
			IJobDetail jobDetail = AppScheduler.CreateProcessJob(jobName, "Main", processName, "Default", "Supervisor");
			ITrigger trigger = new CronTriggerImpl(triggerName, "Main", croneExpression);
			AppScheduler.Instance.ScheduleJob(jobDetail, trigger);
		}

		private void RemoveQuartzJob()
		{
			string processName = GetProcessName(_entity.GetTypedColumnValue<Guid>("GKIProcessId"));
			var jobName = GetJobName(processName);
			AppScheduler.RemoveJob(jobName, "Main");
		}

		#endregion

		#region Methods: Public

		public override void OnSaving(object sender, EntityBeforeEventArgs e)
		{
			base.OnSaving(sender, e);
			InitState((Entity)sender);
		}

		public override void OnSaved(object sender, EntityAfterEventArgs e)
		{
			base.OnSaved(sender, e);
			if (GetIsColumnChanged("GKIFireTime"))
			{
				AddQuartzJob();
			}
		}

		public override void OnDeleted(object sender, EntityAfterEventArgs e)
		{
			base.OnDeleted(sender, e);
			InitState((Entity)sender);
			RemoveQuartzJob();
		}

		#endregion

	}
	public class GKILicensingListenersHelper
	{
		/// <summary>
		/// "удаление "устаревших" Users
		/// </summary>
		/// <param name="userConnection"></param>
		/// <param name="oldGroup"></param>
		/// <param name="oldInstance"></param>
		public void GKIGroupADUsersDelete(UserConnection userConnection, Guid oldGroup, Guid oldInstance)
		{
			//check if there's any records with the old group and the old instance
			var esqGKIInstanceGroupAD = new EntitySchemaQuery(userConnection.EntitySchemaManager, "GKIInstanceGroupAD");
			esqGKIInstanceGroupAD.UseAdminRights = false;
			esqGKIInstanceGroupAD.AddAllSchemaColumns();
			esqGKIInstanceGroupAD.Filters.Add(
				esqGKIInstanceGroupAD.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIGroupAD", oldGroup));
			esqGKIInstanceGroupAD.Filters.Add(
				esqGKIInstanceGroupAD.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIInstance", oldInstance));
			var esqCollection = esqGKIInstanceGroupAD.GetEntityCollection(userConnection);
			if (esqCollection.Count < 2)
			{
				try
				{
					Delete requestRecordsDelete = new Delete(userConnection)
						.From("GKIGroupADUsers")
						.Where("GKIGroupADId")
						.IsEqual(Column.Parameter(oldGroup))
						.And("GKIInstanceId")
						.IsEqual(Column.Parameter(oldInstance))
					as Delete;
					requestRecordsDelete.Execute();
				}
				catch (Exception ex)
				{
					Console.Write(ex.Message);
				}
			}
		}

		/// <summary>
		/// обновление записей по полю isVIP
		/// </summary>
		/// <param name="userConnection"></param>
		/// <param name="recordId"></param>
		/// <param name="isVIP"></param>
		/// <returns></returns>
		public bool GKILicUserIsVIPUpdate(UserConnection userConnection, Guid recordId, bool isVIP)
        {
			Update update = new Update(userConnection, "GKILicUser")
				.Set("GKIIsVIP", Column.Parameter(isVIP))
				.Where("Id")
				.IsEqual(Column.Parameter(recordId))
				as Update;
			bool updateSuccess = update.Execute() > 0 ? true : false;
			return updateSuccess;
		}
	}


}