window.chartInterop = {
    renderSectorChart: function (canvasId, data) {
        const ctx = document.getElementById(canvasId).getContext('2d');
        
        // Prepare labels and values
        const labels = data.map(item => 'Setor ' + item.sector);
        const values = data.map(item => item.count);
        
        // Modern gradient
        const gradient = ctx.createLinearGradient(0, 0, 0, 400);
        gradient.addColorStop(0, 'rgba(52, 211, 153, 0.8)'); // Emerald 400
        gradient.addColorStop(1, 'rgba(5, 150, 105, 0.2)');  // Emerald 600
        
        if (window.mySectorChart) {
            window.mySectorChart.destroy();
        }
        
        window.mySectorChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Propriedades por Setor',
                    data: values,
                    backgroundColor: gradient,
                    borderColor: 'rgba(52, 211, 153, 1)',
                    borderWidth: 2,
                    borderRadius: 8,
                    borderSkipped: false,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: 'rgba(17, 24, 39, 0.9)',
                        titleFont: { size: 14, weight: 'bold' },
                        bodyFont: { size: 13 },
                        padding: 12,
                        cornerRadius: 8,
                        displayColors: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(255, 255, 255, 0.1)',
                            drawBorder: false
                        },
                        ticks: {
                            color: 'rgba(255, 255, 255, 0.7)',
                            font: { size: 12 },
                            stepSize: 1
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: 'rgba(255, 255, 255, 0.7)',
                            font: { size: 12 }
                        }
                    }
                },
                animation: {
                    duration: 2000,
                    easing: 'easeOutQuart'
                }
            }
        });
    }
};
