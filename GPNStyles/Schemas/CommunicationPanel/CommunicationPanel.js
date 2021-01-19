define("CommunicationPanel", ["CommunicationPanelResources", "GPNResourcesSwapperResources", "css!SyncSettingsErrorsCSS"],
	function(resources, gkiResources) {
		/**
		 * Для совместимости с 7.17: появилась новая панель со своей иконкой
		*/
		if (resources.localizableImages.OmniChatMenuIconSVG){
			resources.localizableImages.OmniChatMenuIconSVG = gkiResources.localizableImages.OmniChatMenuIconSVG;
		}
		return {
			messages: {},
			mixins: {},
			attributes: {},
			methods: {},
			diff: []
		};
	});
