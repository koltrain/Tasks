define("GPNDcmSchemaStage", [],
	function() {
		Ext.define("Terrasoft.Designers.GPNDcmSchemaStage", {
			alternateClassName: "Terrasoft.GPNDcmSchemaStage",
			override: "Terrasoft.DcmSchemaStage",
 
			/**
			 * Default stage header color.
			 * #BPMS-29
			 * @overridden
			 * @type {String}
			 */
			defaultColor: "#00569B",
			
			/**
			 * Stage header color.
			 * #BPMS-29
			 * @overridden
			 * @type {String}
			 */
			color: "#00569B",
		});
	});