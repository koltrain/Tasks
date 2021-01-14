define("BasePropertiesPageModule", [
	"MaskHelper",
	"ModalBox",
	"BaseDesignerResources",
	"page-wizard-component"
	],
	function(MaskHelper, ModalBox, resources) {
		Ext.define("Terrasoft.BasePropertiesPageModule", {
			extend: "Terrasoft.configuration.BaseModule",

			mixins: {
				customEvent: "Terrasoft.CustomEventDomMixin"
			},

			/**
			 * @type {Object} Resources.
			 * @protected
			 */
			resources: null,

			//region Methods: Protected

			/**
			 * Return a type of editable page item.
			 * @protected
			 * @abstract
			 * @return {String} Page item type.
			 */
			getPageItemType: Terrasoft.abstractFn,

			/**
			 * Returned some not empty culture value.
			 * @protected
			 */
			getSomeNotEmptyCultureValue: function(cultureValues) {
				return Object.values(cultureValues).find(function(cultureValue) {
					return !Ext.isEmpty(cultureValue);
				});
			},

			/**
			 * Returned page translations.
			 * @protected
			 */
			getPropertiesPageTranslation: function() {
				return {
					"saveButton": this.resources.localizableStrings.SaveButtonCaption,
					"cancelButton": this.resources.localizableStrings.CancelButtonCaption,
					"LocalizableStringsDialog.Cancel": this.resources.localizableStrings.CancelButtonCaption,
					"LocalizableStringsDialog.Apply": this.resources.localizableStrings.ApplyButtonCaption,
					"LocalizableStringsDialog.ShowAllLanguages": this.resources.localizableStrings.ShowAllLanguages,
					"LocalizableStringsDialog.HideInactiveLanguages": this.resources.localizableStrings.HideInactiveLanguages,
					"generalCaption": this.resources.localizableStrings.GeneralCaption,
					"wrongCaptionLength": this.resources.localizableStrings.WrongCaptionLengthMessage,
					"columnEmpty": Terrasoft.Resources.BaseViewModel.columnRequiredValidationMessage,
					"wrongColumnName": this.resources.localizableStrings.WrongColumnNameMessage
				};
			},

			/**
			 * Initializes resources.
			 * @param {Object} schemaResources Resources
			 * @protected
			 */
			initResources: function(schemaResources) {
				Ext.applyIf(this.resources.localizableImages, schemaResources.localizableImages);
				Ext.applyIf(this.resources.localizableStrings, schemaResources.localizableStrings);
			},

			/**
			 * Initializes custom event.
			 * @protected
			 */
			initCustomEvents: function() {
				this.sandbox.registerMessages(this.messages);
				const customEvent = this.mixins.customEvent;
				customEvent.init();
				customEvent.subscribe("Cancel").subscribe(this.cancel.bind(this));
				customEvent.subscribe("Save").subscribe(this.save.bind(this));
			},

			/**
			 * Save event handler.
			 * @protected
			 */
			save: Terrasoft.emptyFn,

			/**
			 * Cancel event handler.
			 * @protected
			 */
			cancel: function() {
				this.close();
			},

			/**
			 * Close MessageBox.
			 * @protected
			 */
			close: function() {
				ModalBox.close();
			},

			/**
			 * Initializes translation.
			 * @protected
			 */
			initPageDesignerTranslation: function() {
				const translation = this.getPropertiesPageTranslation();
				this.mixins.customEvent.publish("GetPageWizardTranslation", translation);
			},

			/**
			 * Returns culture values.
			 * @protected
			 * @param localizableStringModel
			 */
			getCultureValues: function(localizableStringModel) {
				const cultureValues = {};
				const primaryCulture = Terrasoft.SysValue.PRIMARY_CULTURE.displayValue;
				const currentUserCulture = Terrasoft.SysValue.CURRENT_USER_CULTURE.displayValue;
				localizableStringModel.forEach(function (model) {
					cultureValues[model.cultureName] = model.value;
				}, this);
				if (!cultureValues[primaryCulture]) {
					if (cultureValues[currentUserCulture]) {
						cultureValues[primaryCulture] = cultureValues[currentUserCulture];
					} else {
						cultureValues[primaryCulture] = this.getSomeNotEmptyCultureValue(cultureValues);
					}
				}
				return cultureValues;
			},

			/**
			 * Convert localizable string to localizable string model.
			 * @param {Terrasoft.LocalizableString} localizableString Localizable string
			 */
			toLocalizableStringModel: function (localizableString) {
				const cultureValues = (localizableString && localizableString.cultureValues) || {};
				return Object.entries(cultureValues).map(function(cultureValue) {
					return {
						cultureName: cultureValue[0],
						value: cultureValue[1]
					};
				});
			},

			// endregion

			//region Methods: Public

			/**
			 * @inheritdoc Terrasoft.BaseObject#constructor
			 * @override
			 */
			constructor: function() {
				this.callParent(arguments);
				this.resources = {
					localizableImages: {},
					localizableStrings: {}
				};
			},

			/**
			 * @override
			 */
			render: function(renderTo) {
				this.callParent(arguments);
				const propertiesPage = document.createElement("ts-page-item-properties-page-host");
				propertiesPage.setAttribute("item-type", this.getPageItemType());
				propertiesPage.setAttribute("current-culture",
					Terrasoft.SysValue.CURRENT_USER_CULTURE.displayValue);
				renderTo.appendChild(propertiesPage);
				MaskHelper.hideBodyMask();
			},

			/**
			 * @override
			 */
			init: function() {
				this.callParent(arguments);
				this.initResources(resources);
				this.initCustomEvents();
				this.initPageDesignerTranslation();
			},

			/**
			 * @override
			 */
			onDestroy: function() {
				this.mixins.customEvent.destroy();
				this.callParent(arguments);
			}

			// endregion


		});
		return Terrasoft.BasePropertiesPageModule;
	});
