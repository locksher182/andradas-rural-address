window.dynamicMap = {
    map: null,
    markers: [],

    // 14 Distinct colors maximizing contrast between sequential neighbors
    sectorColors: {
        1: "#E02F44",  // Red - Starts bold
        2: "#1E3A8A",  // Dark Blue - High contrast to Red
        3: "#FACC15",  // Bright Yellow - High contrast to Dark Blue
        4: "#581C87",  // Deep Purple - High contrast to Yellow
        5: "#10B981",  // Emerald Green - Fresh, distinct from Purple
        6: "#EC4899",  // Hot Pink - Distinct from Green
        7: "#F97316",  // Orange - Warm, distinct from Pink
        8: "#06B6D4",  // Cyan - Cool, distinct from Orange
        9: "#7F1D1D",  // Maroon/Dark Red - Dark, distinct from Cyan
        10: "#3B82F6", // Sky Blue - Distinct from Maroon
        11: "#D97706", // Amber/Gold - Distinct from Blue
        12: "#4C1D95", // Indigo - Dark, distinct from Amber
        13: "#84CC16", // Lime - Bright, distinct from Indigo
        14: "#64748B"  // Slate Gray - Neutral finish
    },

    getSectorColor: function (sector) {
        return this.sectorColors[sector] || "#000000"; // Black fallback
    },

    createCustomIcon: function (color) {
        const svg = `
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 36" width="30" height="45">
                <path fill="${color}" d="M12 0C5.37 0 0 5.37 0 12c0 9 12 24 12 24s12-15 12-24c0-6.63-5.37-12-12-12z"/>
            </svg>
        `;

        return L.divIcon({
            className: 'custom-pin',
            html: svg,
            iconSize: [30, 45],
            iconAnchor: [15, 45],
            popupAnchor: [0, -45]
        });
    },

    initMap: function (elementId, properties) {
        if (this.map) {
            this.map.remove();
            this.map = null;
        }
        this.markers = [];

        var initialLat = -22.067;
        var initialLng = -46.570;

        this.map = L.map(elementId).setView([initialLat, initialLng], 12);

        L.tileLayer('https://mt1.google.com/vt/lyrs=y&x={x}&y={y}&z={z}', {
            maxZoom: 20,
            attribution: '© Google Maps'
        }).addTo(this.map);

        var bounds = L.latLngBounds();
        var validPoints = 0;

        properties.forEach(p => {
            if (p.latitude && p.longitude) {
                var color = this.getSectorColor(p.setor);
                var icon = this.createCustomIcon(color);

                var link = `/propriedades/editar/${p.id}`;
                var marker = L.marker([p.latitude, p.longitude], { icon: icon })
                    .bindPopup(`
                        <div style="font-family: 'Inter', sans-serif; text-align: center;">
                            <strong style="color: ${color}; font-size: 1.1em;">${p.nome}</strong><br>
                            <span style="font-weight: 600; color: #555;">Setor ${p.setor}</span><br>
                            <span style="color: #777; font-size: 0.9em;">${p.primeiraPessoa || 'Sem proprietário'}</span><br>
                            <a href="${link}" style="display: inline-block; margin-top: 8px; color: #007bff; text-decoration: none; font-weight: bold; font-size: 0.9em; border: 1px solid #007bff; padding: 2px 8px; border-radius: 4px;">Ver Cadastro</a>
                        </div>
                    `)
                    .addTo(this.map);

                this.markers.push(marker);
                bounds.extend([p.latitude, p.longitude]);
                validPoints++;
            }
        });

        if (validPoints > 0) {
            this.map.fitBounds(bounds, { padding: [50, 50] });
        }
    }
};
