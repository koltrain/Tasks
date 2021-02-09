define("GKILicensingButtonsMixin", ["ProcessModuleUtilities", "GKILicensingButtonsMixinResources"],
	function(ProcessModuleUtilities, resources) { 
	Ext.define("Terrasoft.configuration.mixins.GKILicensingButtonsMixin", {
		alternateClassName: "Terrasoft.GKILicensingButtonsMixin",
			/**
			 * @public
			 * @desc: подтверждение вызова бизнес-процесса, который синхронизирует лицензии
			 */
			onGKILicUserSyncButtonClick: function() {
				var cfg = {
					style: Terrasoft.MessageBoxStyles.BLUE
				};
				this.showConfirmationDialog(resources.localizableStrings.GKILicUserSyncConfirmation,
				function getSelectedButton(returnCode) {
					if (returnCode === Terrasoft.MessageBoxButtons.YES.returnCode) {
						this.GKILicUserSyncServiceCall();
					}
				}, [Terrasoft.MessageBoxButtons.YES, Terrasoft.MessageBoxButtons.NO], null, cfg);
			},
			
			/**
			 * @public
			 * @desc: вызывает бизнес-процесс, который синхронизирует лицензии
			 */
			GKILicUserSyncServiceCall: function() {
				var args = {
					name: "GKILicensingLicUserSyncProcess",
					parameters: null
				};
				ProcessModuleUtilities.startBusinessProcess(args);
				this.showInformationDialog(resources.localizableStrings.GKISyncIsInProcessReminder);
				return;
			},
	});
	return Ext.create(Terrasoft.GKILicensingButtonsMixin);
});