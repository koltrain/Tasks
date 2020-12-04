Ext.ns("Terrasoft");
/**
 * @class Terrasoft.controls.GKIHTMLControl
 * Класс элемента управления для отображения
 */
Ext.define("Terrasoft.controls.GKIHTMLControl", {
	extend: "Terrasoft.Component",
	alternateClassName: "Terrasoft.GKIHTMLControl",
	mixins: {
	},
	myParam: "",
	myCode: "",
	pageName: "",
	minWidth: 0,
	months: false,
	/**
	 * Этапы идей.
	 * @type {Object[]}
	 */
	WSList: [],
	/**
	 * Элементы.
	 * @type {Terrasoft.Collection}
	 */
	myTerrasoftCollection: null,
	/**
	 * @inheritdoc Terrasoft.Component#tpl
	 * @protected
	 * @overridden
	 */
	tpl: [
		/*jshint white:false */
		"<div id='{id}-usr-my-control' class='{wrapClass}'>",
		//"All collection datas: {myParam}",
		"{%this.prepareCollection(out, values)%}",
		"</div>"
		/*jshint white:true */
	],
	getTplData: function() {
		var tplData = this.callParent(arguments);
		tplData.myParam = this.myParam;
		tplData.prepareCollection = function(out,values){
			var $this = values.self;
			out.push("<div class='html-block'>" + $this.myParam + "</div>");
		};
		this.updateSelectors(tplData);
		
		return tplData;
	},
	/**
	 * @inheritDoc Terrasoft.Component#init
	 * @protected
	 * @overridden
	 */
	init: function() {
		this.callParent(arguments);
		this.addEvents(
			"myMethod",
			"getClick"
		);
	},
	/**
	 * @inheritDoc Terrasoft.Component#constructor
	 * @overridden
	 */
	constructor: function() {
		this.callParent(arguments);
	},
	
	updateSelectors: function(tplData) {
		var id = tplData.id;
		var baseIdSuffix = "-usr-my-control";
		var selectors = this.selectors = {};
		selectors.wrapEl = "#" + id + baseIdSuffix;
		return selectors;
	},
	/**
	 * Подписка на события элементов элемента управления
	 */
	initDomEvents: function() {
		this.callParent(arguments);
		var wrapEl = this.getWrapEl();
		if (wrapEl) {
			wrapEl.on("click", this.onClick, this);
		}
		this.reCalcSizes();
		Ext.EventManager.on(window, "resize", this.reCalcSizes, this);
		var resizeEvent = window.document.createEvent('UIEvents'); 
		resizeEvent.initUIEvent('resize', true, false, window, 0); 
		window.dispatchEvent(resizeEvent);
	},
	
	reCalcSizes: function(){
		return true;
	},
	/**
	 * Обработчик события click на основном элементе
	 * @param event
	 */
	onClick: function(e) {
		var targetEl = Ext.get(e.target);
		var targetArr = targetEl.id;
		e.stopEvent();
		if( (targetEl.dom.className.indexOf('active-button') + 1) || (targetEl.dom.className.indexOf('islink') + 1)  ){
			this.fireEvent("getClick", targetArr);
		} else {
			return true;
		}
	},
	/**
	 * Возвращает конфигурацию привязки к модели. Реализует интерфейс миксина {@link Terrasoft.Bindable}.
	 * @overridden
	 */
	getBindConfig: function() {
		var parentBindConfig = this.callParent(arguments);
		var bindConfig = {
			myParam: {
				changeMethod: "setMyParam"
			},
			myCode: {
				changeMethod: "setMyCode"
			},
			pageName: {
				changeMethod: "setPageName"
			},
		};
		Ext.apply(bindConfig, parentBindConfig);
		return bindConfig;
	},
	
	setMyParam: function(val) {
		this.myParam = val || this.myParam;
		if (this.allowRerender()) {
			this.reRender();
		}
	},
	setMyCode: function(val) {
		this.myCode = val || this.myCode;
		if (this.allowRerender()) {
			this.reRender();
		}
	},
	setPageName: function(val) {
		this.pageName = val || this.pageName;
		if (this.allowRerender()) {
			this.reRender();
		}
	},
	setVisible: function(parameter){
		this.visible = parameter;
	},
	/**
	 * @overridden
	 * @inheritdoc Terrasoft.Component#destroy
	 */
	onDestroy: function() {
		var wrapEl = this.getWrapEl();
		if (wrapEl) {
			wrapEl.un("getClick", this.onClick, this);
		}
		Ext.EventManager.un(window, "resize", this.reCalcSizes, this);
		this.callParent(arguments);
	},
	/**
	 * Устанавливает значение флага только для чтения
	 * @param {Boolean} readonly Значение флага только для чтения. Если true - элемент управления устанавливается в
	 * режим только для чтения, false - рабочий режим элемента управления
	 */
	setReadonly: function(readonly) {
		if (this.readonly === readonly) {
			return;
		}
		this.readonly = readonly;
		if (this.allowRerender()) {
			this.reRender();
		}
	},
	
	reRender: function(index, container) {
		this.startRender(arguments);
	},
	
	//Рендерим
	startRender: function(index, container){
		
    	if (!Ext.isEmpty(index) || !Ext.isEmpty(container)) {
			this.deprecatedReRender(index, container);
			return;
		}
		this.rendering = true;
		this.fireEvent("beforeRerender", this);
		var wrapEl = this.getWrapEl();
		if (Ext.isEmpty(wrapEl)) {
			if (this.ownerCt.allowRerender()) {
				this.ownerCt.reRender();
			}
			return;
		}
		this.clearDomListeners();
		this.removeElementsBySelectors(true);
		var html = this.generateHtml();
		var parent = wrapEl.parent();
		var domNode = Ext.DomHelper.append(parent, html);
		wrapEl.dom.parentNode.replaceChild(domNode, wrapEl.dom);
		var el = Ext.get(domNode);
		this.fireEvent("afterrerender", this, el);
	},
});
