define("MainHeaderSchema", [], function() {
	return {
		attributes: {},
		methods: {
			init: function() {
				this.callParent(arguments);
				this.Terrasoft.ServerChannel.on(Terrasoft.EventName.ON_MESSAGE, this.onGKIMessageReceived, this);
			},
			destroy: function() {
				this.Terrasoft.ServerChannel.un(Terrasoft.EventName.ON_MESSAGE, this.onGKIMessageReceived, this);
				this.callParent(arguments);
			},
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