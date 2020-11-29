define("TSLeftPanelTopMenuModule", ["TSLeftPanelTopMenuModuleResources", "LookupUtilities",
        "ConfigurationConstants", "LeftPanelUtilitiesV2", "HoverMenuButton",
        "CheckModuleDestroyMixin", "ProcessEntryPointUtilities", "MaskHelper", 
        "ProcessModuleUtilities", "ServiceHelper",
        "MainMenuUtilities", "css!TSLeftPanelTopMenuModule"],
    function (resources, LookupUtilities, ConfigurationConstants, LeftPanelUtilities) {

        Ext.define("Terrasoft.configuration.TSLeftPanelTopMenuModuleViewModel", {
            alternateClassName: "Terrasoft.TSLeftPanelTopMenuModuleViewModel",
            override: "Terrasoft.LeftPanelTopMenuModuleViewModel",

            getTopMenuConfig: function() {
                return [
                    {
                        id: "collapse-button",
                        tag: "CollapseMenu",
                        className: "Terrasoft.HoverMenuButton",
                        style: Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        classes: {
                            imageClass: ["button-image-size"],
                            wrapperClass: ["collapse-button-wrapperEl"]
                        },
                        imageConfig: resources.localizableImages.collapseIconSvg2,
                        click: {
                            bindTo: "collapseSideBar"
                        },
                        hint: this.getCollapseSideBarMenuItemCaptionConfig(),
                        markerValue: this.getCollapseSideBarMenuItemCaptionConfig()
                    },
                    {
                        id: "menu-button",
                        tag: "MainMenu",
                        className: "Terrasoft.HoverMenuButton",
                        style: Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
                        hint: {bindTo: "getMenuButtonHint"},
                        markerValue: resources.localizableStrings.MenuButtonHint,
                        classes: {
                            imageClass: ["button-image-size"],
                            wrapperClass: ["menu-button-wrapperEl"]
                        },
                        imageConfig: resources.localizableImages.menuIconSvg2,
                        menu: {
                            items: {bindTo: "MainMenuItems"},
                            "alignType": "tr?",
                            "ulClass": "position-fixed"
                        },
                        delayedShowEnabled: {
                            bindTo: "Collapsed"
                        },
                        showDelay: this.get("ShowDelay"),
                        hideDelay: this.get("HideDelay")
					},
					{
						id: "menu-startprocess-button",
						tag: "StartProcessMenu",
						className: "Terrasoft.HoverMenuButton",
						style: Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
						hint: {bindTo: "getStartProcessMenuButtonHint"},
						markerValue: resources.localizableStrings.StartProcessButtonHint,
						classes: {
							imageClass: ["button-image-size"],
							wrapperClass: ["menu-startprocess-button-wrapperEl"]
						},
						imageConfig: resources.localizableImages.processIconSvg2,
						menu: {
							items: {bindTo: "startProcessMenu"},
							"alignType": "tr?",
							"ulClass": "position-fixed"
						},
						click: {
							bindTo: "startProcessMenuButtonClick"
						},
						visible: {
							bindTo: "IsSSP",
							bindConfig: {
								converter: function(value) {
									return !value;
								}
							}
						},
						delayedShowEnabled: {
							bindTo: "Collapsed"
						},
						showDelay: this.get("ShowDelay"),
						hideDelay: this.get("HideDelay")
					},
					{
						id: "menu-quickadd-button",
						tag: "quickAddMenu",
						className: "Terrasoft.HoverMenuButton",
						style: Terrasoft.controls.ButtonEnums.style.TRANSPARENT,
						classes: {
							imageClass: ["button-image-size"],
							wrapperClass: ["menu-quickadd-button-wrapperEl"]
						},
						hint: {
							bindTo: "getQuickAddHint"
						},
						markerValue: resources.localizableStrings.AddButtonHint,
						imageConfig: resources.localizableImages.quickaddIconSvg2,
						menu: {
							items: {bindTo: "quickAddMenu"},
							"alignType": "tr?",
							"ulClass": "position-fixed"
						},
						visible: {
							bindTo: "IsSSP",
							bindConfig: {
								converter: function(value) {
									return !value;
								}
							}
						},
						delayedShowEnabled: {
							bindTo: "Collapsed"
						},
						showDelay: this.get("ShowDelay"),
						hideDelay: this.get("HideDelay")
					}
                ];
            },
            loadItemsMainMenu: function () {
                var cacheKey = "ApplicationMainMenu_TopPanel_Items";
                var esq = this.Ext.create("Terrasoft.EntitySchemaQuery", {
                    rootSchemaName: "ApplicationMainMenu",
                    isDistinct: true,
                    clientESQCacheParameters: {
                        cacheItemName: cacheKey
                    },
                    serverESQCacheParameters: {
                        cacheLevel: Terrasoft.ESQServerCacheLevels.SESSION,
                        cacheGroup: "ApplicationMainMenu",
                        cacheItemName: cacheKey
                    }
                });
                esq.addColumn("Id");
                esq.addColumn("IntroPageUId");
                esq.addColumn("Name");
                esq.addColumn("[SysSchema:UId:IntroPageUId].Name", "Tag");
                esq.getEntityCollection(function (result) {
                    if (!result.success) {
                        return;
                    }
                    var menuCollection = this.Ext.create("Terrasoft.BaseViewModelCollection");
                    var entities = result.collection;
                    var mainMenuConfig = {
                        Id: "menu-menu-item",
                        Tag: "MainMenu",
                        Caption: resources.localizableStrings.mainManuMenuItemCaption,
                        Visible: {
                            bindTo: "IsSSP",
                            bindConfig: {
                                converter: function (value) {
                                    return !value;
                                }
                            }
                        }
                    };
                    var entitiesCount = entities.getCount();
                    if (entitiesCount === 0) {
                        mainMenuConfig.Class = "menu-item";
                        mainMenuConfig.Click = {bindTo: "goToFromMenu"};
                        menuCollection.add(mainMenuConfig.Id, this.Ext.create("Terrasoft.BaseViewModel", {
                            values: mainMenuConfig
                        }));
                    } else if (entitiesCount === 1) {
                        entities.each(function (entity) {
                            var menuItem = this.getConfigMenuItem(entity);
                            menuItem.Caption = mainMenuConfig.Caption;
                            menuCollection.add(menuItem.Id, this.Ext.create("Terrasoft.BaseViewModel", {
                                values: menuItem
                            }));
                        }, this);
                    } else {
                        mainMenuConfig.Type = "Terrasoft.MenuSeparator";
                        menuCollection.add(mainMenuConfig.Id, this.Ext.create("Terrasoft.BaseViewModel", {
                            values: mainMenuConfig
                        }));
                        entities.each(function (entity) {
                            var menuItem = this.getConfigMenuItem(entity);
                            menuCollection.add(menuItem.Id, this.Ext.create("Terrasoft.BaseViewModel", {
                                values: menuItem
                            }));
                        }, this);
                        var id = "end-menu-menu-item";
                        menuCollection.add(id, this.Ext.create("Terrasoft.BaseViewModel", {
                            values: {
                                Id: id,
                                Type: "Terrasoft.MenuSeparator"
                            }
                        }));
                    }
                    var mainMenuItems = this.get("MainMenuItems");
                    mainMenuItems.collection.remove(mainMenuItems.collection.getByKey("process-menu-item"));
                    menuCollection.loadAll(mainMenuItems);
                    mainMenuItems.clear();
                    menuCollection.collection.remove(
                    	menuCollection.collection.getByKey(result.collection.collection.first().get("IntroPageUId"))
                	);
                    mainMenuItems.loadAll(menuCollection);
                }, this);
            },
        });
    });