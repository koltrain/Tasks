define("GPNDashboardWidgetCustomization", ["GPNDashboardWidgetCustomizationResources", 
"css!GPNDashboardWidgetCustomization"],
function(resources) {

	function addWidgetStyles() {

		Terrasoft.DashboardEnums.WidgetColorSet = [
			"#d60100", // - Красный
			"#ffb100", // - Желтый
			"#46b145", // - Зеленый
			"#793dc0", // - Фиолетовый
			"#03a5a5", // - Бирюзовый
			"#5d607b", // - Антрацитовый
			"#9fa7b5", // - Нейтральный-серый
			"#a8abb9", // - Бело-алюминиевый
			"#c4c6d3", // - Кадетский синий Крайола
			"#d4dce5", // - Светлый серо-лазурный
			"#dde0e6", // - Светло-серый
			"#ebebeb", // - Дымчато-белый
			"#f3f4f8", // - Гейнсборо
			"#003d76", // - Транспортно-синий
			"#00569b", // - Насыщенный синий
			"#006fba", // - Ярко-синий
			"#2378d8", // - Темно-синий Крайола
			"#0097d8", // - Аква
			"#2cb4e9", // - Цвет Твиттера
			"#71c5f0", // - Светло-голубой
			"#a1daf8", // - Бирюзово-голубой
			"#bce4fa", // - Бледно-синий
		];
		
		Terrasoft.DashboardEnums.StyleColors = {
			"widget-green": Terrasoft.DashboardEnums.WidgetColorSet[0], // Красный
			"widget-mustard": Terrasoft.DashboardEnums.WidgetColorSet[1], // Желтый
			"widget-orange": Terrasoft.DashboardEnums.WidgetColorSet[2], // Зеленый
			"widget-coral": Terrasoft.DashboardEnums.WidgetColorSet[3], // Фиолетовый
			"widget-violet": Terrasoft.DashboardEnums.WidgetColorSet[4], // Бирюзовый
			"widget-navy": Terrasoft.DashboardEnums.WidgetColorSet[5], // Антрацитовый
			"widget-blue": Terrasoft.DashboardEnums.WidgetColorSet[6], // Нейтральный-серый
			"widget-turquoise": Terrasoft.DashboardEnums.WidgetColorSet[7], // Бело-алюминиевый
			"widget-dark-turquoise": Terrasoft.DashboardEnums.WidgetColorSet[8], // Кадетский синий Крайола
			"widget-d4dce5": Terrasoft.DashboardEnums.WidgetColorSet[9], // Светлый серо-лазурный
			"widget-dde0e6": Terrasoft.DashboardEnums.WidgetColorSet[10], // - Светло-серый
			"widget-ebebeb": Terrasoft.DashboardEnums.WidgetColorSet[11], // - Дымчато-белый
			"widget-f3f4f8": Terrasoft.DashboardEnums.WidgetColorSet[12], // - Гейнсборо
			"widget-003d76": Terrasoft.DashboardEnums.WidgetColorSet[13], // - Транспортно-синий
			"widget-00569b": Terrasoft.DashboardEnums.WidgetColorSet[14], // - Насыщенный синий
			"widget-006fba": Terrasoft.DashboardEnums.WidgetColorSet[15], // - Ярко-синий
			"widget-2378d8": Terrasoft.DashboardEnums.WidgetColorSet[16], // - Темно-синий Крайола
			"widget-0097d8": Terrasoft.DashboardEnums.WidgetColorSet[17], // - Аква
			"widget-2cb4e9": Terrasoft.DashboardEnums.WidgetColorSet[18], // - Цвет Твиттера
			"widget-71c5f0": Terrasoft.DashboardEnums.WidgetColorSet[19], // - Светло-голубой
			"widget-a1daf8": Terrasoft.DashboardEnums.WidgetColorSet[20], // - Бирюзово-голубой
			"widget-bce4fa": Terrasoft.DashboardEnums.WidgetColorSet[21], // - Бледно-синий
		};
		
		//Описания цветов графиков
		Terrasoft.DashboardEnums.WidgetColor = {
			"widget-green": {
				value: "widget-green",
				displayValue: resources.localizableStrings.Styled60100Caption,
				imageConfig: resources.localizableImages.Styled60100Image
			},
			"widget-mustard": {
				value: "widget-mustard",
				displayValue: resources.localizableStrings.Styleffb100Caption,
				imageConfig: resources.localizableImages.Styleffb100Image
			},
			"widget-orange": {
				value: "widget-orange",
				displayValue: resources.localizableStrings.Style46b145Caption,
				imageConfig: resources.localizableImages.Style46b145Image
			},
			"widget-coral": {
				value: "widget-coral",
				displayValue: resources.localizableStrings.Style793dc0Caption,
				imageConfig: resources.localizableImages.Style793dc0Image
			},
			"widget-violet": {
				value: "widget-violet",
				displayValue: resources.localizableStrings.Style03a5a5Caption,
				imageConfig: resources.localizableImages.Style03a5a5Image
			},
			"widget-navy": {
				value: "widget-navy",
				displayValue: resources.localizableStrings.Style5d607bCaption,
				imageConfig: resources.localizableImages.Style5d607bImage
			},
			"widget-blue": {
				value: "widget-blue",
				displayValue: resources.localizableStrings.Style9fa7b5Caption,
				imageConfig: resources.localizableImages.Style9fa7b5Image
			},
			"widget-dark-turquoise": {
				value: "widget-dark-turquoise",
				displayValue: resources.localizableStrings.Stylea8abb9Caption,
				imageConfig: resources.localizableImages.Stylea8abb9Image
			},
			"widget-turquoise": {
				value: "widget-turquoise",
				displayValue: resources.localizableStrings.Stylec4c6d3Caption,
				imageConfig: resources.localizableImages.Stylec4c6d3Image
			},
			"widget-d4dce5": {
				value: "widget-d4dce5",
				displayValue: resources.localizableStrings.Styled4dce5Caption,
				imageConfig: resources.localizableImages.Styled4dce5Image
			},
			"widget-dde0e6": {
				value: "widget-dde0e6",
				displayValue: resources.localizableStrings.Styledde0e6Caption,
				imageConfig: resources.localizableImages.Styledde0e6Image
			},
			"widget-ebebeb": {
				value: "widget-ebebeb",
				displayValue: resources.localizableStrings.StyleebebebCaption,
				imageConfig: resources.localizableImages.StyleebebebImage
			},
			"widget-f3f4f8": {
				value: "widget-f3f4f8",
				displayValue: resources.localizableStrings.Stylef3f4f8Caption,
				imageConfig: resources.localizableImages.Stylef3f4f8Image
			},
			"widget-003d76": {
				value: "widget-003d76",
				displayValue: resources.localizableStrings.Style003d76Caption,
				imageConfig: resources.localizableImages.Style003d76Image
			},
			"widget-00569b": {
				value: "widget-00569b",
				displayValue: resources.localizableStrings.Style00569bCaption,
				imageConfig: resources.localizableImages.Style00569bImage
			},
			"widget-006fba": {
				value: "widget-006fba",
				displayValue: resources.localizableStrings.Style006fbaCaption,
				imageConfig: resources.localizableImages.Style006fbaImage
			},
			"widget-2378d8": {
				value: "widget-2378d8",
				displayValue: resources.localizableStrings.Style2378d8Caption,
				imageConfig: resources.localizableImages.Style2378d8Image
			},
			"widget-0097d8": {
				value: "widget-0097d8",
				displayValue: resources.localizableStrings.Style0097d8Caption,
				imageConfig: resources.localizableImages.Style0097d8Image
			},
			"widget-2cb4e9": {
				value: "widget-2cb4e9",
				displayValue: resources.localizableStrings.Style2cb4e9Caption,
				imageConfig: resources.localizableImages.Style2cb4e9Image
			},
			"widget-71c5f0": {
				value: "widget-71c5f0",
				displayValue: resources.localizableStrings.Style71c5f0Caption,
				imageConfig: resources.localizableImages.Style71c5f0Image
			},
			"widget-a1daf8": {
				value: "widget-a1daf8",
				displayValue: resources.localizableStrings.Stylea1daf8Caption,
				imageConfig: resources.localizableImages.Stylea1daf8Image
			},
			"widget-bce4fa": {
				value: "widget-bce4fa",
				displayValue: resources.localizableStrings.Stylebce4faCaption,
				imageConfig: resources.localizableImages.Stylebce4faImage
			},
		};
	}
	
	addWidgetStyles();
	
	return {};
});