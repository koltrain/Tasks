define("UsersSectionV2", ["ServiceHelper", "RightUtilities"], function(ServiceHelper, RightUtilities) {
	return {
		entitySchemaName: "SysAdminUnit",
		diff: [],
		attributes: {
			/**
			 * Флаг видимости кнопок лицензирования
			 */
			"GKIShowLicensesDistribution": {
				"dataValueType": this.Terrasoft.DataValueType.BOOLEAN,
				"value": false
			},
			/**
			 * Флаг доступности кнопок бронирования лицензий (участвует в расчете доступности)
			 */
			"GKIIsVIPLicensingEnabled": {
				"dataValueType": this.Terrasoft.DataValueType.BOOLEAN,
				"value": false
			},
		},
		messages: {},
		mixins: {},
		methods: {
			/**
			 * @overridden
			 */
			init: function() {
				this.callParent(arguments);
				this.getGKIShowLicensesDistribution();
				this.getGKIIsVIPLicensingEnabled();
			},

			/**
			 * Убрана видимость кнопок лицензирования
			 * @overridden
			 */
			setLicDistributionButtonVisibility: function() {
				this.set("ShowLicensesDistribution", false);
			},

			/**
			 * Изменена привязка видимости кнопки "Выдать лицензии"
			 * @overridden
			 */
			createLicDistributionButton: function() {
				var button = this.callParent(arguments);
				if (button && button.values && button.values.Visible){
					button.values.Visible = {"bindTo": "GKIShowLicensesDistribution"};
				}
				return button;
			},

			/**
			 * Изменена привязка видимости кнопки "Отозвать лицензии"
			 * @overridden
			 */
			createLicRecallButton: function() {
				var button = this.callParent(arguments);
				if (button && button.values && button.values.Visible){
					button.values.Visible = {"bindTo": "GKIShowLicensesDistribution"};
				}
				return button;
			},

			/**
			 * @overridden
			 */
			getSectionActions: function() {
				var actionMenuItems = this.callParent(arguments);
				actionMenuItems.addItem(this.getButtonMenuSeparator());
				actionMenuItems.addItem(this.createGKIVIPAssignButton());
				actionMenuItems.addItem(this.createGKIVIPWithdrawButton());
				return actionMenuItems;
			},

			/**
			 * Конфиг кнопки "Назначить VIP"
			 * @protected
			 */
			createGKIVIPAssignButton: function() {
				return this.getButtonMenuItem({
					"Caption": {"bindTo": "Resources.Strings.GKIVIPAssignButtonCaption"},
					"Click": {"bindTo": "onGKIVIPAssign"},
					"Visible": true,
					"Enabled": {"bindTo": "onGKIIsVIPLicensingEnabled"},
					"IsEnabledForSelectedAll": true
				});
			},

			/**
			 * Конфиг кнопки "Отозвать VIP"
			 * @protected
			 */
			createGKIVIPWithdrawButton: function() {
				return this.getButtonMenuItem({
					"Caption": {"bindTo": "Resources.Strings.GKIVIPWithdrawButtonCaption"},
					"Click": {"bindTo": "onGKIVIPWithdraw"},
					"Visible": true,
					"Enabled": {"bindTo": "onGKIIsVIPLicensingEnabled"},
					"IsEnabledForSelectedAll": true
				});
			},

			/**
			 * Доступность кнопок бронирования
			 */
			onGKIIsVIPLicensingEnabled: function() {
				const isEnabled = this.get("GKIIsVIPLicensingEnabled");
				if (!this.$MultiSelect) {
					const activeRow = this.getActiveRow();
					return activeRow && activeRow.$Active && isEnabled;
				}
				return this.isAnySelected() && isEnabled;
			},

			/**
			 * Нажатие кнопки "Назначить VIP"
			 * @protected
			 */
			onGKIVIPAssign: function() {
				if (!this.$MultiSelect) {
					this.openVIPLicensesToDistribute();
					return;
				}
				this.getUserIds(function(userIds) {
					const maxQueryParamCount = 2000;
					if (userIds.length > maxQueryParamCount) {
						this.showInformationDialog(this.get("Resources.Strings.GKIVIPToManyUsers"));
						return;
					}
					this.openVIPLicensesToDistribute();
				}, this);
			},

			/**
			 * Нажатие кнопки "Отозвать VIP"
			 * @protected
			 */
			onGKIVIPWithdraw: function() {
				if (!this.$MultiSelect) {
					this.openVIPLicensesToWithdraw();
					return;
				}
				this.getUserIds(function(userIds) {
					const maxQueryParamCount = 2000;
					if (userIds.length > maxQueryParamCount) {
						this.showInformationDialog(this.get("Resources.Strings.GKIVIPToManyUsers"));
						return;
					}
					this.openVIPLicensesToWithdraw();
				}, this);
			},

			/**
			 * Открытие окна отзыва VIP-лицензий
			 */
			openVIPLicensesToWithdraw: function() {
				var config = this.getLicDistributionLookupConfig();
				this.openLookup(config, this.onVIPLicPackagesToWithdrawSelected, this);
			},

			/**
			 * Открытие окна назначения VIP-лицензий
			 */
			openVIPLicensesToDistribute: function() {
				var config = this.getLicDistributionLookupConfig();
				this.openLookup(config, this.onVIPLicPackagesToDistibuteSelected, this);
			},

			/**
			 * После выбора лицензий для отзыва
			 */
			onVIPLicPackagesToWithdrawSelected: function(args) {
				var packageIds = args.selectedRows.getKeys();
				if (!packageIds.length) {
					return;
				}
				this.getUserIds(function(userIds) {
					this.withdrawVIPLicenses(userIds, packageIds);
				}, this);
			},

			/**
			 * После выбора лицензий для бронирования
			 */
			onVIPLicPackagesToDistibuteSelected: function(args) {
				var packageIds = args.selectedRows.getKeys();
				if (!packageIds.length) {
					return;
				}
				this.getUserIds(function(userIds) {
					this.distributeVIPLicenses(userIds, packageIds);
				}, this);
			},

			/**
			 * Вызов веб-сервиса отзыва VIP-лицензий
			 * @protected
			 * @param {Array} userIds массив пользователей
			 * @param {Array} packageIds массив продуктов
			 */
			withdrawVIPLicenses: function(userIds, packageIds) {
				var data = {
					"userIds": userIds,
					"packageIds": packageIds
				};
				this.showBodyMask();
				ServiceHelper.callService("GKILicensingRegularService", "GKIWithdrawVIPLicenses",  function(response) {
					this.hideBodyMask();
					if (!response.GKIWithdrawVIPLicensesResult) {
						var responseMessage = this.GKIServiceErrorsHandling(response);
						this.showInformationDialog(responseMessage);
						return;
					}
					if (response.GKIWithdrawVIPLicensesResult === "ok"){
						this.showInformationDialog(this.get("Resources.Strings.GKIVIPLicensesSuccess"));
					}
					else{
						this.showInformationDialog(response.GKIWithdrawVIPLicensesResult);
					}
				}, data, this);
			},

			/**
			 * Вызов веб-сервиса распределения VIP-лицензий
			 * @protected
			 * @param {Array} userIds массив пользователей
			 * @param {Array} packageIds массив продуктов
			 */
			distributeVIPLicenses: function(userIds, packageIds) {
				var data = {
					"userIds": userIds,
					"packageIds": packageIds
				};
				this.showBodyMask();
				ServiceHelper.callService("GKILicensingRegularService", "GKIDistributeVIPLicenses",  function(response) {
					this.hideBodyMask();
					if (!response.GKIDistributeVIPLicensesResult) {
						var responseMessage = this.GKIServiceErrorsHandling(response);
						this.showInformationDialog(responseMessage);
						return;
					}
					var isSuccess = response.GKIDistributeVIPLicensesResult.IsSuccess;
					var licLimitErrors = response.GKIDistributeVIPLicensesResult.SysLicPackageErrors;
					if (!isSuccess){
						this.showInformationDialog(response.GKIDistributeVIPLicensesResult.ErrorMsg);
					}
					if (isSuccess && (!licLimitErrors || licLimitErrors.length === 0)){
						this.showInformationDialog(this.get("Resources.Strings.GKIVIPLicensesSuccess"));
					}
					else{
						var licLimitsString;
						Terrasoft.each(licLimitErrors, function(item) {
							var licLimitString = licLimitsString ?? new String('\n'); 
							licLimitsString = licLimitString.concat('\n', 
								this.Ext.String.format(this.get("Resources.Strings.GKIDistributeVIPLicensesErrorLimits"), item.Key, item.Value));
						}, this);
						licLimitsString += '\n\n';
						var errorMsg = this.Ext.String.format(this.get("Resources.Strings.GKIDistributeVIPLicensesError"), licLimitsString);
						this.showInformationDialog(errorMsg);
					}
				}, data, this);
			},

			/**
			 * Обработка ошибок сервиса
			 * @public
			 * @param {Object} response ответ сервиса
			 * @returns {String} сообщение ошибки
			 */
			GKIServiceErrorsHandling: function(response){
				var responseMessage;
				if (response.responseText) {
					console.log(response.responseText);
				}
				if (response.message){
					responseMessage = response.message;
				}
				else if (response.statusText) {
					responseMessage = response.statusText;
				}
				else {
					responseMessage = "Unknown error";
				}
				return responseMessage;
			},
			/**
			 * Видимость кнопок лицензирования
			 * @protected
			 */
			getGKIShowLicensesDistribution: function() {
				ServiceHelper.callService("GKILicensingRegularService", "GKIIsTheSlaveFree",  function(response) {
					this.set("GKIShowLicensesDistribution", response);
				}, null, this);
			},

			/**
			 * Доступность кнопок бронирования лицензий
			 * @protected
			 */
			getGKIIsVIPLicensingEnabled: function() {
				const operationCode = "CanManageLicUsers";
				RightUtilities.checkCanExecuteOperations([operationCode], function(result) {
					this.set("GKIIsVIPLicensingEnabled", result && result[operationCode]);
				}, this);
			},
		}
	};
});
