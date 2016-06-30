(function (orniscientutils, $, undefined) {
	orniscientutils.stringArrToSelectOptions = function(arr) {
		return arr.map(function (obj) {
			return {
				value: obj,
				label: obj
			}
		});
	}

	orniscientutils.stringArrToFilterNames = function (arr) {
	    return arr.map(function (obj) {
	        return {
	            value: obj.FullName,
	            label: obj.ShortName
	        }
	    });
	}

}(window.orniscientutils = window.orniscientutils || {}, jQuery));