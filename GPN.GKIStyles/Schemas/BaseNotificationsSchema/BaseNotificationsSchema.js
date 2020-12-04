define("BaseNotificationsSchema", [],
	function() {
		return {
			entitySchemaName: "Reminding",
			mixins: {},
			messages: {},
			attributes: {},
			properties: {},
			methods: {
				/**
				 * Sets data message of an empty list.
				 * @param {Function} callback Function to be called after set empty message config.
				 * @param {Object} scope Scope for the callback function.
				 * @private
				 * @overridden
				 */
				setEmptyMessageConfig: function(callback, scope) {
					this.callParent(arguments);
					this.emptyMessageConfig = {
						title: this.get("Resources.Strings.EmptyResultTitle"),
						description: "",
						image: this.get("Resources.Images.EmptyResultImage")
					};
				},
			},
			diff: []
		};
	});
