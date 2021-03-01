namespace Terrasoft.Configuration
{
	using System;
	using Terrasoft.Core;
	using Terrasoft.Core.Entities;
	using Terrasoft.Core.Entities.Events;
	using System.Collections.Generic;
	using Terrasoft.Core.DB;
	using Terrasoft.Common;


	[EntityEventListener(SchemaName = "GKIVIPUsersLicenses")]
	public class GKIVIPUsersLicensesEntityEventListener : BaseEntityEventListener
	{
		public override void OnSaving(object sender, EntityBeforeEventArgs e)
		{
			base.OnSaving(sender, e);
			var entity = (Entity)sender;
			var userConnection = entity.UserConnection;
			try
			{
				var oldStatus = entity.GetTypedOldColumnValue<bool>("GKIisVIP");
				var curStatus = entity.GetTypedColumnValue<bool>("GKIisVIP");
				
				// when adding a VIP license if available <= 0 then throw an error
				if (oldStatus != curStatus && curStatus)
                {
					var serviceGKILicensingRegularService = new GKILicensingRegularService();
					int usedCount = serviceGKILicensingRegularService.GetUsedVIPCount(entity.GetTypedColumnValue<Guid>("GKISysLicPackageId"));
					int limit = serviceGKILicensingRegularService.GKIGetVIPLimit(entity.GetTypedColumnValue<Guid>("GKISysLicPackageId"));

					if (limit == 0)
					{
						throw new Exception(new LocalizableString(userConnection.Workspace.ResourceStorage,
							"GKILicensingRegularListeners",
							"LocalizableStrings.GKIVIPUsersLicensesLimitRecordDoesntExist.Value")
						);
					}

					if (usedCount >= limit)
                    {
						throw new Exception(new LocalizableString(userConnection.Workspace.ResourceStorage,
							"GKILicensingRegularListeners",
							"LocalizableStrings.GKIVIPUsersLicensesLimitExceeded.Value")
						);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}