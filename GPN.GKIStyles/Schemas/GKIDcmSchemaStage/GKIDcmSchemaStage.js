define("GKIDcmSchemaStage", [],
	function() {
		Ext.define("Terrasoft.Designers.GKIDcmSchemaStage", {
			alternateClassName: "Terrasoft.GKIDcmSchemaStage",
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