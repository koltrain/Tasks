define("GPNFolderManagerViewConfigGenerator", ["GPNFolderManagerViewConfigGeneratorResources", 
"FolderManagerViewConfigGeneratorResources"],
	function(resources, nuiResources) {
		var resourcesBuffer = resources;
		resources = nuiResources;
		resources.localizableImages.SettingsImage = resourcesBuffer.localizableImages.SettingsImage;
		resources.localizableImages.AddToFavoritesImage = resourcesBuffer.localizableImages.AddToFavoritesImage;
		resources.localizableImages.RemoveFromFavorites = resourcesBuffer.localizableImages.RemoveFromFavorites;

		Ext.define("Terrasoft.GPNFolderManagerViewConfigGenerator", {
			override: "Terrasoft.FolderManagerViewConfigGenerator",
			
			/**
			 * @overridden
			 * Returns folder menu action config.
			 * @return {Object} Folder menu actions config.
			 */
			getFolderMenuActionConfig: function() {
				var returnObj = this.callParent(arguments);
				if (returnObj && returnObj.imageConfig){
					returnObj.imageConfig = resources.localizableImages.SettingsImage;
				}
				return returnObj;
			},
			
			/**
			 * @overridden
			 * Returns folder favorite action config.
			 * @return {Object} Folder favorite actions config.
			 */
			getFolderFavoriteActionConfig: function() {
				var returnObj = this.callParent(arguments);
				if (returnObj && returnObj.imageConfig){
					returnObj.imageConfig = {
						bindTo: "isInFavorites",
						bindConfig: {
							converter: function(isInFavorites) {
								return isInFavorites
										? resources.localizableImages.RemoveFromFavorites
										: resources.localizableImages.AddToFavoritesImage;
							}
						}
					};
				}
				return returnObj;
			},
		});
	});