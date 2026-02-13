window.masks = {
    applyPhoneMask: function (elementId) {
        var input = document.getElementById(elementId);
        if (!input) return;

        function maskPhone(value) {
            value = value.replace(/\D/g, ""); // Remove non-digits

            if (value.length > 11) {
                value = value.substring(0, 11);
            }

            if (value.length > 10) {
                // (NN) NNNNN-NNNN
                return value.replace(/^(\d{2})(\d{5})(\d{4}).*/, "($1) $2-$3");
            } else if (value.length > 5) {
                // (NN) NNNN-NNNN (partial)
                return value.replace(/^(\d{2})(\d{4})(\d{0,4}).*/, "($1) $2-$3");
            } else if (value.length > 2) {
                // (NN) NNN...
                return value.replace(/^(\d{2})(\d{0,5}).*/, "($1) $2");
            } else {
                return value.replace(/^(\d*)/, "($1");
            }
        }

        // Remove existing listener if any (by replacing the element or using a flag)
        // Simplest way for Blazor interop usually: just set oninput property directly
        input.oninput = function (e) {
            var start = input.selectionStart;
            var oldVal = input.value;
            var newVal = maskPhone(oldVal);

            if (oldVal !== newVal) {
                input.value = newVal;
                // Dispatch input event for Blazor bi-binding
                input.dispatchEvent(new Event('change'));
            }
        };

        // Initial application just in case value is already there
        if (input.value) {
            input.value = maskPhone(input.value);
        }
    }
};
