window.locationPicker = {
    map: null,
    marker: null,

    initMap: function (elementId, lat, lng) {
        // If map already exists, remove it
        if (this.map) {
            this.map.remove();
            this.map = null;
        }

        // Default to Andradas, MG if no coordinates provided
        var initialLat = lat || -22.067;
        var initialLng = lng || -46.570;
        var zoomLevel = lat ? 15 : 12;

        this.map = L.map(elementId).setView([initialLat, initialLng], zoomLevel);

        L.tileLayer('https://mt1.google.com/vt/lyrs=y&x={x}&y={y}&z={z}', {
            maxZoom: 20,
            attribution: '© Google Maps'
        }).addTo(this.map);

        this.marker = L.marker([initialLat, initialLng], { draggable: true }).addTo(this.map);

        var self = this;

        // Update marker on click
        this.map.on('click', function (e) {
            self.marker.setLatLng(e.latlng);
        });

        // Ensure map renders correctly when modal opens
        setTimeout(function () {
            self.map.invalidateSize();
        }, 200);
    },

    getCoordinates: function () {
        if (!this.marker) return null;
        var latLng = this.marker.getLatLng();
        return {
            lat: latLng.lat,
            lng: latLng.lng
        };
    }
};
