define("ControlGroupPropertiesPageModule", [
	"ViewModelSchemaDesignerControlGroupModalBoxResources",
	"BaseSchemaViewModelResources",
	"BasePropertiesPageModule"
	],
	function(resources, baseSchemaViewModelResources) {
		Ext.define("Terrasoft.ControlGroupPropertiesPageModule", {
			extend: "Terrasoft.BasePropertiesPageModule",

			messages: {
				/**
				 * @message GetControlGroupConfig
				 */
				"GetControlGroupConfig": {
					mode: Terrasoft.MessageMode.PTP,
					direction: Terrasoft.MessageDirectionType.PUBLISH
				},
				/**
				 * @message SaveControlGroupConfig
				 */
				"SaveControlGroupConfig": {
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
			_getCaptionStringModel: function (config, callback, scope) {
				if (!config.captionResourcesName) {
					return Ext.callback(callback, scope);
				}
				const pageSchema = config.pageSchema;
				const resourceArray = config.captionResourcesName.split(".");
				const resourceName= resourceArray[resourceArray.length -1];
				let localizableString = pageSchema.localizableStrings.find(resourceName);
				if (!localizableString) {
					const schemaUId = pageSchema.isNew() ? pageSchema.parentSchemaUId : pageSchema.uId;
					const instanceConfig = {schemaUId, packageUId: pageSchema.packageUId};
					Terrasoft.ClientUnitSchemaManager.findBundleSchemaInstance(instanceConfig, (item) => {
						localizableString = item.localizableStrings.find(resourceName);
						const captionModel = this.toLocalizableStringModel(localizableString);
						return Ext.callback(callback, scope, [captionModel]);
					}, this);
				} else {
					return Ext.callback(callback, scope, [this.toLocalizableStringModel(localizableString)]);
				}
			},

			/**
			 * @private
			 */
			_getValidationConfig: function(config) {
				return {
					maxColumnNameLength: 50,
					maxColumnCaptionLength: 50,
					schemaColumnsNames: config.tabNames
				};
			},

			/**
			 * @private
			 */
			_getColumnConfig: function() {
				const config = this.sandbox.publish("GetControlGroupConfig", null, [this.sandbox.id]);
				const customEvent = this.mixins.customEvent;
				const validationConfig = this._getValidationConfig(config);
				this._getCaptionStringModel(config, (caption) => {
					customEvent.publish("GetColumnConfig", {
						itemType: this.getPageItemType(),
						validationConfig,
						name: config.name,
						caption,
						isInherited: !config.canEditName,
						allowEmptyCaption: true
					}, this);
				}, this);
			},

			/**
			 * @inheritdoc BasePropertiesPageModule#getPageItemType
			 * @override
			 */
			getPageItemType: function() {
				return "controlGroup";
			},

			/**
			 * @inheritdoc BasePropertiesPageModule#save
			 * @override
			 */
			save: function(data) {
				this.callParent(arguments);
				let caption;
				if (data.caption.length > 0) {
					caption = new Terrasoft.LocalizableString({
						cultureValues: this.getCultureValues(data.caption)
					});
				} else {
					caption = new Terrasoft.LocalizableString();
				}
				this.sandbox.publish("SaveControlGroupConfig", {caption, name: data.name}, [this.sandbox.id]);
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
			},


			/**
			 * @inheritdoc BasePropertiesPageModule#getPropertiesPageTranslation
			 * @override
			 */
			getPropertiesPageTranslation: function() {
				const baseConfig = this.callParent(arguments);
				const config = {
					"caption": this.resources.localizableStrings.ControlGroupModalBoxHeader,
					"captionLabel": this.resources.localizableStrings.ControlGroupModalBoxLabelCaption,
					"nameLabel": this.resources.localizableStrings.NameLabelCaption,
					"duplicateColumnName": this.resources.localizableStrings.DuplicateGroupNameMessage
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
		return Terrasoft.ControlGroupPropertiesPageModule;
	});
