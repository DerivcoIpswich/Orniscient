(function (orniscientutils, $, undefined) {
	orniscientutils.stringArrToSelectOptions = function(arr) {
		return arr.map(function (obj) {
			return {
				value: obj,
				label: obj
			}
		});
	}

}(window.orniscientutils = window.orniscientutils || {}, jQuery));