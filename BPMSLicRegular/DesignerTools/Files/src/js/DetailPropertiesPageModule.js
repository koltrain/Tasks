define("DetailPropertiesPageModule", [
		"ViewModelSchemaDesignerDetailModalBoxResources",
		"BaseSchemaViewModelResources",
		"BasePropertiesPageModule"
	],
	function(resources, baseSchemaViewModelResources) {
		Ext.define("Terrasoft.DetailPropertiesPageModule", {
			extend: "Terrasoft.BasePropertiesPageModule",

			messages: {
				/**
				 * @message GetDetailConfig
				 */
				"GetDetailConfig": {
					mode: Terrasoft.MessageMode.PTP,
					direction: Terrasoft.MessageDirectionType.PUBLISH
				},
				/**
				 * @message SaveDetailConfig
				 */
				"SaveDetailConfig": {
					mode: Terrasoft.MessageMode.PTP,
					direction: Terrasoft.MessageDirectionType.PUBLISH
				}
			},

			mixins: {
				customEvent: "Terrasoft.CustomEventDomMixin"
			},

			/**
			 * @private
			 */
			_masterEntitySchema: null,

			/**
			 * @private
			 */
			_packageUId: null,

			/**
			 * @private
			 */
			_caption: null,

			/**
			 * @private
			 */
			_getValidationConfig: function(config) {
				return {
					maxColumnNameLength: 50,
					maxColumnCaptionLength: 50,
					schemaColumnsNames: config.detailNames
				};
			},

			/**
			 * @private
			 */
			_getColumnConfig: function() {
				const config = this.sandbox.publish("GetDetailConfig", null, [this.sandbox.id]);
				this._masterEntitySchema = config.masterEntitySchema;
				this._packageUId = config.packageUId;
				const customEvent = this.mixins.customEvent;
				const validationConfig = this._getValidationConfig(config);
				if (!Ext.isEmpty(config.detailConfig)) {
					this._getDetailAttributes(config, (detailAttributes) => {
						this._caption = detailAttributes.caption;
						customEvent.publish("GetColumnConfig", {
							itemType: this.getPageItemType(),
							validationConfig,
							detailConfig: detailAttributes.detail,
							caption: this.toLocalizableStringModel(detailAttributes.caption),
							name: detailAttributes.name,
							isInherited: !config.canEdit,
							masterColumnConfig: detailAttributes.masterColumnConfig,
							detailColumnConfig: detailAttributes.detailColumnConfig
						}, this);
					});
				} else {
					customEvent.publish("GetColumnConfig", {
						itemType: this.getPageItemType(),
						validationConfig
					}, this);
				}
			},

			/**
			 * @private
			 */
			_getDetailAttributes: function(config, callback, scope) {
				const detailConfig = config.detailConfig;
				const detail = this._getDetail(detailConfig.detailSchemaName, detailConfig.detailEntitySchemaName);
				let entitySchema;
				const result = {
					detail: Object.assign(detail, {canEdit: config.canEdit}),
					name: detailConfig.detailKey
				};
				Terrasoft.chain(
					function(next) {
						this._getDetailEntitySchema({
							schemaUId: detail.value.entitySchemaUId,
							packageUId: config.packageUId
						}, next, this);
					},
					function(next, schema) {
						entitySchema = schema;
						this._getDetailCaption({
							localizableDetailCaption: detailConfig.localizableDetailCaption,
							packageUId: config.packageUId,
							detailSchemaUId: detail.value.detailSchemaUId
						}, next, this);
					},
					function(next, caption) {
						const detailColumnConfig = this._getDetailEntitySchemaColumn(entitySchema, detailConfig.detailEntitySchemaColumn) || {};
						detailColumnConfig.canEdit = config.canEdit;
						const masterColumnConfig = this._getMasterEntitySchemaColumn(config.masterEntitySchema,
							detailConfig.masterEntitySchemaColumn) || {};
						masterColumnConfig.canEdit = config.canEdit;
						Object.assign(result, {
							caption,
							masterColumnConfig,
							detailColumnConfig
						});
						callback.call(scope, result);
					},
					this
				);
			},

			/**
			 * @private
			 */
			_getMasterEntitySchemaColumn: function(masterEntitySchema, masterEntitySchemaColumnName) {
				const masterEntitySchemaColumn = masterEntitySchema.columns.firstOrDefault(function(column) {
					return column.name === masterEntitySchemaColumnName;
				}, this);
				let config;
				if (masterEntitySchemaColumn) {
					config= this._convertEntitySchemaColumnToLookupEntity(masterEntitySchemaColumn);
				}
				return config;
			},

			/**
			 * @private
			 */
			_getDetailCaption: function(config, callback, scope) {
				if (config.localizableDetailCaption) {
					callback.call(scope, config.localizableDetailCaption.clone());
				} else {
					const findConfig = {schemaUId: config.detailSchemaUId, packageUId: config.packageUId};
					Terrasoft.ClientUnitSchemaManager.findBundleSchemaInstance(findConfig, function(instance) {
						const localizableDetailCaption = instance.localizableStrings.get("Caption");
						callback.call(scope, localizableDetailCaption.clone());
					}, this);
				}
			},

			/**
			 * @private
			 */
			_getCaption: function(uId) {
				this._getDetailCaption({detailSchemaUId: uId, packageUId: this._packageUId}, (caption) => {
					const customEvent = this.mixins.customEvent;
					customEvent.publish("GetCaption", this.toLocalizableStringModel(caption));
				}, this);
			},

			/**
			 * @private
			 */
			_getDetail: function(detailSchemaName, detailEntitySchemaName) {
				const items = Terrasoft.DetailManager.getItems();
				const detailManagerItem = items.firstOrDefault(function(item) {
					return item.getDetailSchemaName() === detailSchemaName &&
						(!detailEntitySchemaName || item.getEntitySchemaName() === detailEntitySchemaName);
				}, this);
				return this._convertDetailManagerItemToLookupEntity(detailManagerItem);
			},

			/**
			 * @private
			 */
			_getDetailEntitySchema: function(config, callback, scope) {
				Terrasoft.EntitySchemaManager.findBundleSchemaInstance(config, (entitySchema) => {
					callback.call(scope, entitySchema);
				});
			},

			/**
			 * @private
			 */
			_getDetailEntitySchemaColumn: function(entitySchema, columnName) {
				const detailEntitySchemaColumn = entitySchema.columns.firstOrDefault(function(column) {
					return column.name === columnName;
				}, this);
				let config;
				if (detailEntitySchemaColumn) {
					config = this._convertEntitySchemaColumnToLookupEntity(detailEntitySchemaColumn);
				}
				return config;
			},

			/**
			 * @private
			 */
			_convertEntitySchemaColumnToLookupEntity: function(entitySchemaColumn) {
				return {
					name: entitySchemaColumn.caption.getValue(),
					value: {
						name: entitySchemaColumn.name,
						dataValueType: entitySchemaColumn.dataValueType,
						referenceSchemaUId: entitySchemaColumn.referenceSchemaUId
					}
				};
			},

			/**
			 * @private
			 */
			_convertDetailManagerItemToLookupEntity: function(detailManagerItem) {
				return {
					name: detailManagerItem.getCaption(),
					value: {
						id: detailManagerItem.getId(),
						detailSchemaName: detailManagerItem.getDetailSchemaName(),
						detailSchemaUId: detailManagerItem.getDetailSchemaUId(),
						entitySchemaName: detailManagerItem.getEntitySchemaName(),
						entitySchemaUId: detailManagerItem.getEntitySchemaUId()
					}
				};
			},

			/**
			 * @private
			 */
			_getDetailList: function() {
				const detailCollection = Terrasoft.DetailManager.getItems();
				const columns = detailCollection
					.filterByFn(function(detailManagerItem) {
						return detailManagerItem.getDetailSchemaName() && detailManagerItem.getEntitySchemaName();
					}, this)
					.select(function(detailManagerItem) {
						return this._convertDetailManagerItemToLookupEntity(detailManagerItem);
					}, this);
				columns.sortByFn(this._lookupSortFn);
				const customEvent = this.mixins.customEvent;
				customEvent.publish("GetDetailList", columns.getItems());
			},

			/**
			 * @private
			 */
			_filterDataSourceColumn: function ({ dataValueType }) {
				return dataValueType === Terrasoft.DataValueType.GUID || dataValueType === Terrasoft.DataValueType.LOOKUP
			},

			/**
			 * @private
			 */
			_filterMaterColumn: function (column, detailValue) {
				if (!detailValue || detailValue.dataValueType === Terrasoft.DataValueType.GUID || column.dataValueType === Terrasoft.DataValueType.GUID) {
					return true;
				} else {
					return detailValue.referenceSchemaUId && detailValue.referenceSchemaUId === column.referenceSchemaUId;
				}
			},

			/**
			 * @private
			 */
			_getMasterColumnList: function(detailColumnOption) {
				const columns = this._masterEntitySchema.columns
					.filterByFn((column) => this._filterDataSourceColumn(column) && this._filterMaterColumn(column, detailColumnOption?.value))
					.select(function(column) {
					return this._convertEntitySchemaColumnToLookupEntity(column);
				}, this);
				columns.sortByFn(this._lookupSortFn);
				const customEvent = this.mixins.customEvent;
				customEvent.publish("GetMasterColumnList", columns.getItems());
			},

			/**
			 * @private
			 */
			_getDetailColumnList: function(uId) {
				this._getDetailEntitySchema({
					schemaUId: uId,
					packageUId: this._packageUId
				}, (entitySchema) => {
					const columns = entitySchema.columns
						.filterByFn((column) => this._filterDataSourceColumn(column))
						.select(function(column) {
						return this._convertEntitySchemaColumnToLookupEntity(column);
					}, this);
					columns.sortByFn(this._lookupSortFn);
					const customEvent = this.mixins.customEvent;
					customEvent.publish("GetDetailColumnList", columns.getItems());
				}, this);
			},

			/**
			 * @private
			 */
			_lookupSortFn: function(item1, item2) {
				return item1.name.localeCompare(item2.name);
			},

			/**
			 * @private
			 */
			_getDetailConfig: function(data) {
				const caption = new Terrasoft.LocalizableString({
					cultureValues: this.getCultureValues(data.caption)
				});
				const {detail, detailColumn, masterColumn} = data;
				return {
					detailSchemaName: detail.detailSchemaName,
					detailEntitySchemaName: detail.entitySchemaName,
					detailEntitySchemaColumn: detailColumn.value,
					masterEntitySchemaColumn: masterColumn.value,
					localizableDetailCaption: caption,
					isDetailCaptionChanged: !this._isEqualLocalizableStrings(this._caption, caption),
					detailKey: data.name
				};
			},

			/**
			 * @private
			 */
			_isEqualLocalizableStrings: function(string1, string2) {
				if (!string1 || !string2) {
					return false;
				}
				const object1 = {};
				const object2 = {};
				string1.getSerializableObject(object1);
				string2.getSerializableObject(object2);
				return Terrasoft.isEqual(object1, object2);
			},

			/**
			 * @inheritdoc BasePropertiesPageModule#getPageItemType
			 * @override
			 */
			getPageItemType: function() {
				return "detail";
			},

			/**
			 * @inheritdoc BasePropertiesPageModule#save
			 * @override
			 */
			save: function(data) {
				this.callParent(arguments);
				data.detail = data.detail.value;
				this.sandbox.publish("SaveDetailConfig", this._getDetailConfig(data), [this.sandbox.id]);
				this.close();
			},

			/**
			 * @inheritdoc BasePropertiesPageModule#initCustomEvents
			 * @override
			 */
			initCustomEvents: function() {
				this.callParent(arguments);
				const customEvent = this.mixins.customEvent;
				customEvent.subscribe("GetColumnConfig").subscribe(this._getColumnConfig.bind(this));
				customEvent.subscribe("GetDetailList").subscribe(this._getDetailList.bind(this));
				customEvent.subscribe("GetMasterColumnList").subscribe(this._getMasterColumnList.bind(this));
				customEvent.subscribe("GetDetailColumnList").subscribe(this._getDetailColumnList.bind(this));
				customEvent.subscribe("GetCaption").subscribe(this._getCaption.bind(this));
			},


			/**
			 * @inheritdoc BasePropertiesPageModule#getPropertiesPageTranslation
			 * @override
			 */
			getPropertiesPageTranslation: function() {
				const baseConfig = this.callParent(arguments);
				const config = {
					caption: this.resources.localizableStrings.DetailModalBoxHeader,
					detailCaption: this.resources.localizableStrings.DetailNameCaption,
					captionLabel: this.resources.localizableStrings.DetailCaptionOnThePage,
					selectValueCaption: this.resources.localizableStrings.SelectValueCaption,
					nameLabel: this.resources.localizableStrings.NameLabelCaption,
					masterColumnCaption: this.resources.localizableStrings.MasterSchemaTitle,
					detailColumnCaption: this.resources.localizableStrings.DetailSchemaColumnCaption,
					dataSource: this.resources.localizableStrings.DataSourceCaption,
					duplicateColumnName: this.resources.localizableStrings.DuplicateDetailNameMessage
				};
				return Object.assign({}, baseConfig, config);
			},

			/**
			 * @override
			 */
			init: function() {
				this.initResources(baseSchemaViewModelResources);
				this.initResources(resources);
				this.callParent(arguments);
			}

		});
		return Terrasoft.DetailPropertiesPageModule;
	});
