define("LDAPServerSettings", [], function() {
		return {
			methods: {
				/**
				 * ############# ######### ######## ######### LDAP #######.
				 * @protected
				 * @overridden
				 */
				init: function(callback, scope) {
					this.callParent(arguments);
					this.set("IsAllowLicenseDistributionVisible", false);
				},
				diff: [
					{
						"operation": "insert",
						"name": "LDAPAllowLicenseDistribution",
						"parentName": "CommonServerSettings_GridLayout",
						"propertyName": "items",
						"values": {
							"layout": {
								"column":10,
								"row": 2,
								"colSpan": 8
							},
							"bindTo": "LDAPAllowLicenseDistributionDuringLDAPSync",
							"labelConfig": {
								"caption": {
									"bindTo": "Resources.Strings.LDAPAllowLicenseDistribution"
								}
							},
							"visible": {
								"bindTo": "IsAllowLicenseDistributionVisible"
							}
						}
					}
				]
			}
		};
	});
