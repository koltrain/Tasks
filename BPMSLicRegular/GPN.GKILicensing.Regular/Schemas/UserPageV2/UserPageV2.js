define("UserPageV2", ["RightUtilities", "ConfigurationConstants", "ViewUtilities", "ServiceHelper"],
	function(RightUtilities, ConfigurationConstants, ViewUtilities, ServiceHelper) {
		return {
			entitySchemaName: "VwSysAdminUnit",
			messages: {},
			details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
			diff: /**SCHEMA_DIFF*/[
				{
					"operation": "insert",
					"parentName": "Tabs",
					"propertyName": "tabs",
					"index": 2,
					"name": "GKIVIPLicenseTab",
					"values": {
						"caption": {"bindTo": "Resources.Strings.GKIVIPLicenseTabCaption"},
						"items": []
					}
				},
				{
					"operation": "insert",
					"parentName": "GKIVIPLicenseTab",
					"name": "GKIVIPLicenseControlGroup",
					"propertyName": "items",
					"values": {
						"itemType": this.Terrasoft.ViewItemType.CONTROL_GROUP,
						"caption": {"bindTo": "Resources.Strings.GKIVIPLicenseTabCaption"},
						"items": [],
						"tools": [],
						"controlConfig": {
							"collapsed": false
						}
					}
				},
				{
					"operation": "insert",
					"name": "GKIVIPLicenseList",
					"parentName": "GKIVIPLicenseControlGroup",
					"propertyName": "items",
					"values": {
						"generator": "ConfigurationItemGenerator.generateContainerList",
						"idProperty": "Id",
						"collection": "GKIVIPLicenseCollection",
						"observableRowNumber": 15,
						"onGetItemConfig": "getGKIItemViewConfig",
						"dataItemIdPrefix": "lic-item"
					}
				},
			]/**SCHEMA_DIFF*/,
			attributes: {
				/**
				 * Коллекция VIP-лицензий
				 */
				"GKIVIPLicenseCollection": {
					dataValueType: this.Terrasoft.DataValueType.COLLECTION
				},
				/**
				 * VIP-лицензии загружены или нет
				 */
				"IsGKIVIPLicenseDataLoaded": {
					dataValueType: this.Terrasoft.DataValueType.BOOLEAN,
					type: this.Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN
				},
				/**
				 * Количество плохих запросов
				 */
				"GKIBadRequestsCounter": {
					dataValueType: this.Terrasoft.DataValueType.INTEGER,
					type: this.Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
					value: 0
				},
			},
			rules: {},
			methods: {
				/**
				 * @overridden
				 * @param {Function} callback
				 * @param {Object} scope
				 */
				init: function() {
					this.callParent(arguments);
					this.initGKIVIPCollection();
					this.getGKIVIPCollection();
				},

				/**
				 * @overridden
				 */
				onEntityInitialized: function() {
					this.callParent(arguments);
					this.initTabsVisibility();
				},

				/**
				 * @overridden
				 */
				updateDetails: function() {
					this.callParent(arguments);
					this.getGKIVIPCollection();
				},

				/**
				 * @overridden
				 */
				onSaved: function() {
					this.callParent(arguments);
					if (this.isEditMode()) {
						this.getGKIVIPCollection();
					}
				},

				/**
				 * Видимость вкладки "Лицензии"
				 * @protected
				 */
				getGKIShowLicenseTab: function(callback, scope) {
					ServiceHelper.callService("GKILicensingRegularService", "GKIIsTheSlaveFree",  function(response) {
						callback.call(scope || this, response);
					}, null, this);
				},

				/**
				 * @protected
				 * @desc скрытие вкладки Лицензирование и управление видимостью вкладки Бронирование лицензий
				 */
				initTabsVisibility: function() {
					var licenseTab = this.$TabsCollection.get("LicenseTab");
					licenseTab.set("Visible", false); 
					this.getGKIShowLicenseTab(
						function(response){
							licenseTab.set("Visible", response);
						}, this
					);
					var GKIVIPlicenseTab = this.$TabsCollection.get("GKIVIPLicenseTab");
					GKIVIPlicenseTab.set("Visible", false); 
					this.getUserIsSysAdmin(
						function(isSysAdmin){
							GKIVIPlicenseTab.set("Visible", isSysAdmin);
						}, this
					);
				},

				/**
				 * @protected
				 * viewConfig для детали Бронирования лицензий
				 * @param {Object} itemConfig viewConfig элемента
				 * @param {Terrasoft.BaseViewModel} item элемент модели
				 */
				getGKIItemViewConfig: function(itemConfig, item) {
					var labelClass = ["license-label"];
					if (!item.get("Enabled")) {
						labelClass.push("disabled-label");
					}
					var config = ViewUtilities.getContainerConfig("license-view");
					var labelConfig = {
						className: "Terrasoft.Label",
						caption: {bindTo: "Name"},
						classes: {labelClass: labelClass},
						inputId: item.get("Id") + "-el"
					};
					var editConfig = {
						className: "Terrasoft.CheckBoxEdit",
						id: item.get("Id"),
						checked: {bindTo: "Checked"},
						enabled: {bindTo: "Enabled"}
					};
					var countConfig = {
						className: "Terrasoft.Label",
						caption: {bindTo: "AvailableCountCaption"},
						classes: {labelClass: labelClass.concat(["count-label"])}
					};
					config.items.push(labelConfig, editConfig, countConfig);
					itemConfig.config = config;
				},

				/**
				 * Получает коллекцию доступных VIP-лицензий для пользователя
				 * @protected
				 */
				getGKIVIPCollection: function() {
					var userId = this.isAddMode()
						? this.Terrasoft.GUID_EMPTY
						: (this.get("PrimaryColumnValue") || this.get(this.entitySchema.primaryColumnName));
					var dataSend = {
						userId: userId
					};
					var config = {
						serviceName: "GKILicensingRegularService",
						methodName: "GetUserGKIVIPLicPackages",
						data: dataSend
					};
					this.set("IsGKIVIPLicenseDataLoaded", false);
					this.showMaskOnGKIVIPLicenses();
					this.callService(config, function(response) {
						if(!response.GetUserGKIVIPLicPackagesResult && this.get("GKIBadRequestsCounter") < 20){
							this.getGKIVIPCollection();
							this.set("GKIBadRequestsCounter", this.get("GKIBadRequestsCounter")+1);
							return;
						}
						this.onGetGKIVIPCollection(response);
					}, this);
				},

				/**
				 * Обработка VIP-лицензий пользователя
				 * @protected
				 * @param {Object} response ответ сервиса
				 */
				onGetGKIVIPCollection: function(response, callback, scope) {
					var collection = this.get("GKIVIPLicenseCollection");
					collection.clear();
					if (response && response.GetUserGKIVIPLicPackagesResult) {
						this.Terrasoft.each(response.GetUserGKIVIPLicPackagesResult, function(item) {
							var licenseItem = this.getGKIVIPLicenseCollectionItem(item);
							collection.add(item.Id, licenseItem);
						}, this);
					}
					this.set("IsGKIVIPLicenseDataLoaded", true);
					this.hideMaskOnGKIVIPLicenses();
					if (this.Ext.isFunction(callback)) {
						callback.call(scope);
					}
				},
				/**
				 * Формирование наполнения элемента детали Бронирование лицензий
				 * @protected
				 * @param {Object} config конфигурация элемента
				 * @return {Terrasoft.BaseViewModel} экземпляр класса BaseViewModel для включения в коллекцию
				 */
				getGKIVIPLicenseCollectionItem: function(config) {
					var licText = this.Ext.String.format(
						this.get("Resources.Strings.GKIVIPLicAvailableCountCaption"),
						config.Available, config.Limit);
					var collectionItem = this.Ext.create("Terrasoft.BaseViewModel", {
						values: {
							Id: config.Id,
							Name: config.Caption,
							Checked: config.Checked,
							Enabled: config.Enabled,
							Available: config.Available,
							Limit: config.Limit,
							AvailableCountCaption: licText
						}
					});
					collectionItem.sandbox = this.sandbox;
					return collectionItem;
				},
				/**
				 * Сохранение изменений в VIP-лицензиях
				 * @protected
				 */
				saveGKIVIPLicenses: function() {
					var licenseItems = {};
					var collection = this.get("GKIVIPLicenseCollection");
					collection.each(function(model) {
						if (this.isAddMode() || model.getIsChanged()) {
							licenseItems[model.get("Id")] = model.get("Checked");
						}
					}, this);
					if (this.Terrasoft.isEmptyObject(licenseItems)) {
						return;
					} else {
						var dataSend = {
							licenseItems: this.Ext.encode(licenseItems)
						};
						var config = {
							serviceName: "GKILicensingRegularService",
							methodName: "SaveGKIVIPChanges",
							data: dataSend
						};
						this.showMaskOnGKIVIPLicenses();
						this.callService(config, function(response) {
							this.onGKIVIPSaveLicenses(response);
						}, this);
					}
				},
				/**
				 * коллбек после попытки сохранения VIP-лицензий
				 * @param {Object} response ответ сервиса
				 */
				onGKIVIPSaveLicenses: function(response) {
					this.hideMaskOnGKIVIPLicenses();
					if (response) {
						response.success = this.Ext.isEmpty(response.SaveGKIVIPChangesResult);
						if (!response.success) {	
							this.showInformationDialog(response.SaveGKIVIPChangesResult);
						}
						this.getGKIVIPCollection();
					}
				},
				/**
				 * @private
				 * @desc проверка является ли пользователь системным администратором
				 * @param {Function} callback 
				 * @param {Object} scope 
				 */
				getUserIsSysAdmin: function (callback, scope) {
					var currentUser = Terrasoft.SysValue.CURRENT_USER.value;
					var sysAdmins = ConfigurationConstants.SysAdminUnit.Id.SysAdministrators;
					var esq = Ext.create("Terrasoft.EntitySchemaQuery", {
						rootSchemaName: "SysUserInRole"
					});
					esq.addColumn("SysRole");
					esq.addColumn("SysUser");
					esq.filters.add("SysUser", Terrasoft.createColumnFilterWithParameter(
						Terrasoft.ComparisonType.EQUAL, "SysUser", currentUser));
					esq.filters.add("SysRole", Terrasoft.createColumnFilterWithParameter(
						Terrasoft.ComparisonType.EQUAL, "SysRole", sysAdmins));
					esq.getEntityCollection(function(response) {
						if (response && response.success) {
							var result = response.collection;
							var isSysAdmin = (result.collection.length !== 0);
							callback.call(scope || this, isSysAdmin);
						}
					}, this);
				},
				/**
				 * Инициализирует коллекцию VIP-лицензий
				 * @private
				 */
				initGKIVIPCollection: function() {
					var collection = this.Ext.create("Terrasoft.BaseViewModelCollection");
					collection.on("itemChanged", this.onGKICheckChange, this);
					this.set("GKIVIPLicenseCollection", collection);
				},

				/**
				 * тихое сохранение при нажатии чекбокса
				 * @private
				 */
				onGKICheckChange: function() {
					if (this.get("IsGKIVIPLicenseDataLoaded") && this.isEditMode()) {
						this.saveGKIVIPLicenses();
					}
				},
				
				/**
				 * маска на летали Бронирование лицензий
				 * @private
				 */
				showMaskOnGKIVIPLicenses: function() {
					var maskConfig = {
						selector: "#UserPageV2GKIVIPLicenseListContainerList",
						timeout: 0
					};
					var elements = this.Ext.select(maskConfig.selector);
					if (elements.item(0)) {
						this.GKIVIPlicenseListMaskId = this.Terrasoft.Mask.show(maskConfig);
					}
				},

				/**
				 * скрытие маски на детали Бронирование лицензий
				 * @private
				 */
				hideMaskOnGKIVIPLicenses: function() {
					this.Terrasoft.Mask.hide(this.GKIVIPlicenseListMaskId);
				},
			}
		};
	});
