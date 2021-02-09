namespace Terrasoft.Configuration
{
	using System;
	using Terrasoft.Core;
	using Terrasoft.Core.Entities;
	using Terrasoft.Core.Entities.Events;
	using System.Collections.Generic;
	using Terrasoft.Core.DB;
	using Terrasoft.Common;


	[EntityEventListener(SchemaName = "GKILicUserInstanceLicPackage")]
	public class GKILicUserInstanceLicPackageEntityEventListener : BaseEntityEventListener
	{
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
		}
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

	public class GKILicensingListenersHelper
	{
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