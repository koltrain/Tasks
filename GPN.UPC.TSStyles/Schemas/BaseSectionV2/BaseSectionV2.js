define("BaseSectionV2", ["TSHTMLControlGenerator", "TSHTMLControl", "css!TSGPNStyles"], function () {
    return {
        messages: {},
        attributes: {
            "TSHTMLFull": {
                "dataValueType": Terrasoft.DataValueType.TEXT,
                "type": Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                "value": ''
            },
        },
        methods: {
            onGridDataLoaded: function (response) {
                this.callParent(arguments);
                var dataCollection = response.collection || Ext.create("Terrasoft.Collection");
                if (!dataCollection.collection.items.length) {
                    return;
                }
                this.set("TSHTMLFull", "<div class='new-captions'></div>");
                var profile = this.get("Profile"), sortColumn = "", sortDirection = 0, isTotalListed = false;
                var propertyName = this.getDataGridName();
                var gridProfile = (propertyName ? profile[propertyName] : profile) || this.applyDefaultGridProfile();

                if (!Terrasoft.isEmptyObject(gridProfile)) {
                    isTotalListed = gridProfile.type === Terrasoft.GridType.LISTED;
                }

                if (isTotalListed) {
                    var count = 0, captionsHTMl = "";
                    if (profile.DataGrid) {
                        var dataGrid = profile.DataGrid;
                        if (dataGrid && dataGrid.isTiled !== undefined) {
                            var isTiled = dataGrid.isTiled;
                            var gridsColumnsConfig = isTiled ? dataGrid.tiledConfig : dataGrid.listedConfig;
                            if (gridsColumnsConfig) {
                                var columnsData = this.Ext.decode(gridsColumnsConfig);
                                if (columnsData.items) {
                                    columnsData.items.forEach(function (columnData) {
                                        var sortColumnHTML = "";
                                        if (columnData.orderDirection && (columnData.orderDirection !== "")) {
                                            if (columnData.orderDirection === 1) {
                                                sortColumnHTML = "<div class='active-button grid-sort-arrow grid-sort-arrow-down' data-index='" + count + "'></div>";
                                            }
                                            if (columnData.orderDirection === 2) {
                                                sortColumnHTML = "<div class='grid-sort-arrow grid-sort-arrow-up'></div>";
                                            }
                                        }
                                        captionsHTMl = captionsHTMl + "<div class='active-button grid-cols-" +
                                            columnData.position.colSpan + "' data-index='" + count + "' data-item-marker='" +
                                            columnData.caption + "' id='" + columnData.bindTo + "-caption-" + count +
                                            "'><label class='active-button' data-index='" + count + "'>" +
                                            columnData.caption + "</label>" + sortColumnHTML + "</div>";
                                        count++;
                                    });
                                }
                                this.set("TSHTMLFull", "<div class='new-captions'><div class='grid-captions'>" + captionsHTMl + "</div></div>");
                            }
                        }
                    }
                }
            },

            //Контролы
            getMyParam2: function () {
                return this.get("TSHTMLFull");
            },
            myMethod2: function (json) {
                if (json) {
                    this.set("TSHTMLFull", json);
                } else {
                    this.set("TSHTMLFull", "");
                }
            },
            getClick: function (elementId) {
                var button = document.getElementById(elementId);
                if (button.getAttribute("data-index")) {
                    this.sortColumn(button.getAttribute("data-index"));
                }
                return true;
            },

        },
        diff: [
            {
                "operation": "insert",
                "parentName": "GridUtilsContainer",
                "name": "ColumnsBlock",
                "propertyName": "items",
                "values": {
                    "className": "Terrasoft.TSHTMLControl",
                    "layout": {"colSpan": 24, "rowSpan": 1, "column": 0, "row": 7},
                    "generator": "TSHTMLControlGenerator.generateTSHTMLControl",
                    "getMyParam": "getMyParam2",
                    "myMethod": "myMethod2",
                    "getClick": "getClick",
                },
                "index": 8,
            },
        ]
    };
});
