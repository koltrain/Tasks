define("GKIResourcesSwapper", ["GKIResourcesSwapperResources", "ExtendedFilterEditViewV2Resources", "ChartModuleHelperResources", "QuickFilterViewV2Resources", "DcmSchemaDesignerResources"],
	function(gkiResources, extendedFilterEditViewV2Resources, chartModuleHelperResources, quickFilterViewV2Resources, dcmSchemaDesignerResources) {
		
		//#region: ExtendedFilterEditViewV2Resources
		extendedFilterEditViewV2Resources.localizableImages.SearchFolderIcon = gkiResources.localizableImages.SearchFolderIcon;
		//#endregion
		
		//#region: ChartModuleHelperResources
		chartModuleHelperResources.localizableImages.SplineImage = gkiResources.localizableImages.SplineImage;
		chartModuleHelperResources.localizableImages.LineImage = gkiResources.localizableImages.LineImage;
		chartModuleHelperResources.localizableImages.BarImage = gkiResources.localizableImages.BarImage;
		chartModuleHelperResources.localizableImages.PieImage = gkiResources.localizableImages.PieImage;
		chartModuleHelperResources.localizableImages.AreasplineImage = gkiResources.localizableImages.AreasplineImage;
		chartModuleHelperResources.localizableImages.FunnelImage = gkiResources.localizableImages.FunnelImage;
		chartModuleHelperResources.localizableImages.ColumnImage = gkiResources.localizableImages.ColumnImage;
		chartModuleHelperResources.localizableImages.ScatterImage = gkiResources.localizableImages.ScatterImage;
		//#endregion
		
		//#region: QuickFilterViewV2Resources
		quickFilterViewV2Resources.localizableImages.GeneralFolderImage = gkiResources.localizableImages.GeneralFolderImage;
		quickFilterViewV2Resources.localizableImages.SearchFolderImage = gkiResources.localizableImages.SearchFolderIcon;
		//#endregion

		//#region: DcmSchemaDesignerResources
		dcmSchemaDesignerResources.localizableImages.DcmSchemaPropertiesPageIcon = gkiResources.localizableImages.DcmSchemaPropertiesPageIcon;
		dcmSchemaDesignerResources.localizableImages.DcmHelpIcon = gkiResources.localizableImages.DcmHelpIcon;
		dcmSchemaDesignerResources.localizableImages.DcmFeedIcon = gkiResources.localizableImages.DcmFeedIcon;
		//#endregion
	});