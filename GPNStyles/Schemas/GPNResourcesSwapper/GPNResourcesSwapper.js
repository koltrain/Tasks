define("GPNResourcesSwapper", ["GPNResourcesSwapperResources", "ExtendedFilterEditViewV2Resources", 
"ChartModuleHelperResources", "QuickFilterViewV2Resources", "DcmSchemaDesignerResources", "TimezoneGeneratorResources",
"MiniPageResourceUtilitiesResources", "CTIBaseCommunicationViewModelResources", "BaseCommunicationViewModelResources",
"GridSettingsDesignerContainerResources"],
	function(gkiResources, extendedFilterEditViewV2Resources, 
	chartModuleHelperResources, quickFilterViewV2Resources, dcmSchemaDesignerResources, timezoneGeneratorResources,
	miniPageResourceUtilitiesResources, ctiBaseCommunicationViewModelResources, baseCommunicationViewModelResources,
	gridSettingsDesignerContainerResources) {
		
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
		
		//#region: TimezoneGeneratorResources
		timezoneGeneratorResources.localizableImages.TimeZoneImage = gkiResources.localizableImages.TimeZoneImage;
		//#endregion

		//#region: TimezoneGeneratorResources
		miniPageResourceUtilitiesResources.localizableImages.CallButtonImage = gkiResources.localizableImages.CallButtonImage;
		miniPageResourceUtilitiesResources.localizableImages.EmailButtonImage = gkiResources.localizableImages.EmailButtonImage;
		miniPageResourceUtilitiesResources.localizableImages.AddButtonImage = gkiResources.localizableImages.AddButtonImage;
		//#endregion
		
		//#region: ctiBaseCommunicationViewModelResources
		ctiBaseCommunicationViewModelResources.localizableImages.CallIcon = gkiResources.localizableImages.CallIcon;
		//#endregion
	
		//#region: baseCommunicationViewModel
		baseCommunicationViewModelResources.localizableImages.EmailIcon = gkiResources.localizableImages.EmailIcon;
		baseCommunicationViewModelResources.localizableImages.WebIcon = gkiResources.localizableImages.WebIcon;
		//#endregion
		
		//#region GridSettingsDesignerContainer
		gridSettingsDesignerContainerResources.localizableImages.AddIcon = gkiResources.localizableImages.AddIcon;
		//#endregion
	});