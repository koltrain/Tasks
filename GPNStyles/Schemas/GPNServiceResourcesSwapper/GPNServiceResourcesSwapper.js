Terrasoft.require(["GPNServiceResourcesSwapperResources", "CaseSectionActionsDashboardResources", 
	"SupervisorSingleWindowSectionV2Resources", "ServiceEnterpriseIntroResources"], 
function(gkiResources, caseSectionActionsDashboardResources, supervisorSingleWindowSectionV2Resources,
	serviceEnterpriseIntroResources) {
	//#region: caseSectionActionsDashboardResources
	caseSectionActionsDashboardResources.localizableImages.PortalMessageTabImage = gkiResources.localizableImages.PortalMessageTabImage;
	//#endregion
	
	//#region: supervisorSingleWindowSectionV2Resources
	supervisorSingleWindowSectionV2Resources.localizableImages.QueuesSettingsDataViewIcon = gkiResources.localizableImages.QueuesSettingsDataViewIcon;
	//#endregion
	
	//#region: serviceEnterpriseIntroResources
	serviceEnterpriseIntroResources.localizableImages.ServiceIcon = gkiResources.localizableImages.ServiceIcon;
	//#endregion
	console.log("Service modules' resources have been swapped");
}, 
this, 
function() {console.log("Service modules' resources haven't been swapped")});