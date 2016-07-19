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

    orniscientutils.methodsToSelectOptions = function(arr) {
        return arr.map(function (obj) {
            return {
                value: obj.MethodHashCode,
                label: obj.Name,
                parameters: obj.Parameters
            }
        });
    }

}(window.orniscientutils = window.orniscientutils || {}, jQuery));