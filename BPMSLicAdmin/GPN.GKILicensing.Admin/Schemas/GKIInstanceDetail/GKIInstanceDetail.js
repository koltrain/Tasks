 define("GKIInstanceDetail", [], function() {
    return {
        entitySchemaName: "GKIInstance",
        details: /**SCHEMA_DETAILS*/{}/**SCHEMA_DETAILS*/,
        methods: {
			//region overridden методы убирающие кнопки детали
				getCopyRecordMenuItem: Terrasoft.emptyFn,
				getDeleteRecordMenuItem: Terrasoft.emptyFn,
			//endregion

			/**
			 * @overridden
			 * действия при инициализации схемы
			 */
			init: function() {
				this.callParent(arguments);
				this.Terrasoft.ServerChannel.on(Terrasoft.EventName.ON_MESSAGE, this.onGKIMessageReceived, this);
			},

			/**
			 * @overridden
			 * действия при выгрузке схемы
			 */
			destroy: function() {
				this.Terrasoft.ServerChannel.un(Terrasoft.EventName.ON_MESSAGE, this.onGKIMessageReceived, this);
				this.callParent(arguments);
			},

			/**
			* @protected
			* param {Object} sender
		    * param {String} message
			*/
			onGKIMessageReceived: function(sender, message) {
				if (message && message.Header && message.Header.Sender === "GKILicensingMessage") {
					var result = message.Body;
					if (result){
						switch (result) {
							case "\"GKIInstanceApplicationStatusUpdated\"":
								this.updateDetail({ "reloadAll": true });
								break;
						
							default:
								break;
						}
					}
				}
			},
        },
        diff: /**SCHEMA_DIFF*/[]/**SCHEMA_DIFF*/
    };
});