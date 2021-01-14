namespace Terrasoft.Configuration
{
	using Newtonsoft.Json;
	using System;
	using System.Data;
	using System.IO;
	using System.Net;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Web;
	using System.Text;
	using System.Web;
	using Terrasoft.Web.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.Entities;
	using Terrasoft.Core.Entities.Events;
	using System.Collections.Generic;
	using Terrasoft.Core.DB;
	using Terrasoft.Common;
	using System.Xml.Serialization;

	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class GKILicensingLDAPService : BaseService
	{
		public string GKILDAPSync()
		{
			string report;
			var groupSelect =
				new Select(UserConnection)
					.Column("Id")
					.Column("Name")
				.From("GKIGroupAD")
				.Where("Id").In(new Select(UserConnection)
					.Column("GKIGroupADId").From("GKIInstanceGroupAD").Where("GKIInstanceId").Not().IsNull()) as Select;
			var groupList = new List<GKILicensingLdapGroup>();
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
			{
				using (IDataReader dr = groupSelect.ExecuteReader(dbExecutor))
				{
					while (dr.Read())
					{
						string groupId = dr.GetValue(0).ToString();
						string groupDn = dr.GetValue(1).ToString();
						groupList.Add(new GKILicensingLdapGroup(groupId, groupDn));
					}
				}
			}
			report = String.Concat("Groups count: ", groupList.Count.ToString());

			#region ExistingUsersLoad

			Select existedUsersSelect =
				new Select(UserConnection)
					.Column("GKIName")
				.From("GKILicUser") as Select;
			var existingUsers = new List<string>();
			using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
			{
				using (IDataReader dr = existedUsersSelect.ExecuteReader(dbExecutor))
				{
					while (dr.Read())
					{
						existingUsers.Add(dr.GetValue(0).ToString());
					}
				}
			}
			report += String.Concat(". Existing users count: ", existingUsers.Count.ToString());
			#endregion
			var newUserIds = new List<Entity>();
			foreach (var groupItem in groupList)
			{
				var groupFilter = "(memberOf=" + ReplaceSpecialCharacters(groupItem.Dn) + ")";
				report += String.Concat(". Group: ", groupFilter);
				DateTime? ldapModifiedDate = default(DateTime?); // ksenzov: filtering by modified date? i'll look into it later. methods CheckUsersInGroup and GetMinModifiedDateOfNewUsers
				string modifiedUsersFilter = GetUserFilterWithMinModifiedOnAttributeOrDate(ldapModifiedDate); //ksenzov: all users for now
				var filter = "(&" + modifiedUsersFilter + groupFilter + ")";
				report += String.Concat(". Filter: ", filter);
				List<Terrasoft.Configuration.LDAP.LdapUser> ldapUsers;
				using (var ldapUtils = new Terrasoft.Configuration.LDAP.LdapUtilities(UserConnection))
				{
					ldapUsers = ldapUtils.GetUsersByLdapFilter(filter);
					report += String.Concat(". Acquired users count: ", ldapUsers.Count);
				}
				DateTime lastUserModifiedOn = DateTime.MinValue;
				foreach (var user in ldapUsers)
				{
					report += String.Concat(". User: ", user.Name);
					if (string.IsNullOrEmpty(user.Name))
					{
						continue;
					}
					if (lastUserModifiedOn < user.ModifiedOn)
					{
						lastUserModifiedOn = user.ModifiedOn;
					}
					report += String.Concat(", ModifiedOn: ", user.ModifiedOn.ToString());
					var gkiLicUserSchema = UserConnection.EntitySchemaManager.GetInstanceByName("GKILicUser");
					var gkiLicUserQuery = new EntitySchemaQuery(gkiLicUserSchema);
					gkiLicUserQuery.UseAdminRights = false;
					gkiLicUserQuery.AddAllSchemaColumns();
					gkiLicUserQuery.Filters.Add(gkiLicUserQuery.CreateFilterWithParameters(FilterComparisonType.Equal, "GKIName", user.Name));
					var gkiLicUserCollection = gkiLicUserQuery.GetEntityCollection(UserConnection);
					if (gkiLicUserCollection.Count < 1)
					{
						var gkiLicUserEntity = gkiLicUserSchema.CreateEntity(UserConnection);
						gkiLicUserEntity.SetDefColumnValues();
						gkiLicUserEntity.SetColumnValue("GKIName", user.Name);
						gkiLicUserEntity.Save();
						report += String.Concat(" saved new");
					}
					else
					{
						report += String.Concat(" found");
					}
				}
				DateTime? maxModificationDateOfLDAPEntry = lastUserModifiedOn > DateTime.MinValue ? lastUserModifiedOn :
					default(DateTime?);
				if (maxModificationDateOfLDAPEntry.HasValue && Core.Configuration.SysSettings.Exists(UserConnection, "GKILicensingLDAPEntryMaxModifiedOn"))
				{
					Core.Configuration.SysSettings.SetDefValue(UserConnection, "GKILicensingLDAPEntryMaxModifiedOn", maxModificationDateOfLDAPEntry.Value);
				}
			}
			return report;
		}

		private static DateTime GetGKIEntryMaxModifiedOn(UserConnection userConnection)
		{
			if (!Terrasoft.Core.Configuration.SysSettings.TryGetValue(userConnection, "GKILicensingLDAPEntryMaxModifiedOn",
				out var entryMaxModifiedOn))
			{
				return DateTime.MinValue;
			}
			return entryMaxModifiedOn != null
				? TimeZoneInfo.ConvertTimeToUtc((DateTime)entryMaxModifiedOn, userConnection.CurrentUser.TimeZone)
				: DateTime.MinValue;
		}

		private string GetUserFilterWithMinModifiedOnAttributeOrDate(DateTime? fromDate)
		{
			/*
			DateTime GKILdapEntryMaxModifiedOn = GetGKIEntryMaxModifiedOn(UserConnection);
			string GKILdapEntryModifiedOnAttribute = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(
				UserConnection, "GKILicensingLDAPEntryModifiedOnAttribute", String.Empty);
			*/
			string GKILicensingLDAPUsersFilter = Terrasoft.Core.Configuration.SysSettings.GetValue<string>(
				UserConnection, "GKILicensingLDAPUsersFilter", String.Empty);
			/*
			if (GKILdapEntryMaxModifiedOn == DateTime.MinValue || string.IsNullOrEmpty(GKILdapEntryModifiedOnAttribute))
			{
				return GKILicensingLDAPUsersFilter;
			}
			*/
			return GKILicensingLDAPUsersFilter;
			/*
			DateTime fromDateValue = fromDate ?? DateTime.MaxValue;
			DateTime syncFromDate = fromDateValue < _ldapEntryMaxModifiedOn ? fromDateValue : _ldapEntryMaxModifiedOn;
			string lastSyncDateInLdapFormat = ConvertToLdapFormat(syncFromDate);
			var modifiedOnAttributeFilter = string.Format("({0}>={1})", _ldapEntryModifiedOnAttribute,
				lastSyncDateInLdapFormat);
			return "(&" + _ldapUsersFilter + modifiedOnAttributeFilter + ")";
			*/
		}

		private string ReplaceSpecialCharacters(string filterString)
		{
			return filterString.Replace("*", "\\2A")
				.Replace("(", "\\28")
				.Replace(")", "\\29")
				.Replace("\\", "\\5C")
				.Replace("Nul", "\\00");
		}

		private string GKIGetUserInGroupFilterString(string filter, GKILicensingLdapGroup group)
		{
			string groupDn = ReplaceSpecialCharacters(group.Dn);
			return filter.Replace(gkiLdapGroupMacroName, groupDn);
		}

		public struct GKILicensingLdapGroup
		{

			#region Fields: Public

			public string Id;
			public string Dn;
			public DateTime ModifiedOn;

			#endregion

			#region Constructors: Public

			public GKILicensingLdapGroup(string id, string dn)
			{
				Id = id;
				Dn = dn;
				ModifiedOn = DateTime.MinValue;
			}

			#endregion

		}

		public struct GKILicensingLdapUser
		{

			#region Fields: Public

			public string Id;
			public string Name;
			public string FullName;
			public string Company;
			public string Email;
			public string Phone;
			public string JobTitle;
			public bool IsActive;
			public string Dn;
			public DateTime ModifiedOn;

			#endregion

		}
		private const string gkiLdapGroupMacroName = "[#LDAPGroupDN#]";
	}


}