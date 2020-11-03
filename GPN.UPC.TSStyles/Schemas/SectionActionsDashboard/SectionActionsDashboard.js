define("SectionActionsDashboard", ["SectionActionsDashboardResources","CallMessagePublisherModule"],
	function(resources) {
		return {
			attributes: {},
			messages: {},
			methods: {
				getExtendedConfig: function(){
					var config = this.callParent(arguments);
					delete config.CallMessageTab;
					delete config.EmailMessageTab;
					delete config.SocialMessageTab;
					delete config.TaskMessageTab;
					return config;
				}
			},
			diff: /**SCHEMA_DIFF*/[]/**SCHEMA_DIFF*/
		};
	}
);
