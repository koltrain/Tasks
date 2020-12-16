Terrasoft.require(["GKIServiceResourcesSwapperResources", "CaseSectionActionsDashboardResources"], 
function(gkiResources, caseSectionActionsDashboardResources) {
	//#region: caseSectionActionsDashboardResources
	caseSectionActionsDashboardResources.localizableImages.PortalMessageTabImage = gkiResources.localizableImages.PortalMessageTabImage;
	//#endregion
	console.log("Service modules' resources have been swapped");
}, 
this, 
function() {console.log("Service modules' resources haven't been swapped")});