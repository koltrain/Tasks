define("MainHeaderSchema", [], function() {
	return {
		attributes: {},
		methods: {
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
					var result = this.Ext.decode(message.Body);
					if (result.MessageText){
						this.showInformationDialog(result.MessageText);
					}
				}
			}
		},
		diff: []
	};
});