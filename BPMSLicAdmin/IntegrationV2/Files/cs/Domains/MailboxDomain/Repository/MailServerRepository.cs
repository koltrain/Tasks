namespace IntegrationV2.MailboxDomain.Repository
{
	using System;
	using System.Collections.Generic;
	using IntegrationApi.MailboxDomain.Model;
	using IntegrationV2.MailboxDomain.Interfaces;
	using Terrasoft.Core;
	using Terrasoft.Core.DB;
	using Terrasoft.Core.Factories;
	using Terrasoft.Common;

	#region Class: MailServerRepository

	/// <summary>
	/// Mail server repository implementation.
	/// </summary>
	[DefaultBinding(typeof(IMailServerRepository))]
	internal class MailServerRepository : IMailServerRepository
	{

		#region Fields: Private

		private UserConnection _userConnection;

		#endregion

		#region Conctructors: Public

		public MailServerRepository(UserConnection uc) {
			_userConnection = uc;
		}

		#endregion

		#region Methods: Private

		private Select GetMailServersQuery() {
			return new Select(_userConnection)
					.Column("MS", "Id")
					.Column("MS", "IsExchengeAutodiscover")
					.Column("MS", "ExchangeEmailAddress")
					.Column("MS", "TypeId")
					.Column("MS", "AllowEmailDownloading")
					.Column("MS", "AllowEmailSending")
					.Column("MS", "Address")
					.Column("MS", "Port")
					.Column("MS", "SMTPServerAddress")
					.Column("MS", "SMTPPort")
					.Column("MS", "UseSSL")
					.Column("MS", "Strategy")
					.Column("OAA", "ClientClassName")
					.Column("MS", "IsStartTls")
				.From("MailServer").As("MS")
				.LeftOuterJoin("OAuthApplications").As("OAA").On("MS", "OAuthApplicationId").IsEqual("OAA", "Id") as Select;
		}

		private void AddPrimaryFilter(Select query, Guid mailServerId) {
			query.Where("MS", "Id").IsEqual(Column.Parameter(mailServerId));
		}

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="IMailServerRepository.GetAll"/>
		public IEnumerable<MailServer> GetAll(bool useForSynchronization = true) {
			var query = GetMailServersQuery();
			using (var dbExecutor = _userConnection.EnsureDBConnection()) {
				using (var reader = query.ExecuteReader(dbExecutor)) {
					while (reader.Read()) {
						yield return new MailServer(reader, useForSynchronization);
					}
				}
			}
		}

		/// <inheritdoc cref="IMailServerRepository.GetById(Guid)"/>
		public MailServer GetById(Guid mailServerId) {
			var query = GetMailServersQuery();
			AddPrimaryFilter(query, mailServerId);
			MailServer mailserver = null;
			using (var dbExecutor = _userConnection.EnsureDBConnection()) {
				using (var reader = query.ExecuteReader(dbExecutor)) {
					if (reader.Read()) {
						mailserver = new MailServer(reader);
					}
				}
			}
			return mailserver;
		}

		#endregion

	}

	#endregion

}
