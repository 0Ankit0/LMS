// Chart.js configuration for reports dashboard
window.drawLineChart = (canvasId, data) => {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    // Destroy existing chart if it exists
    if (window.charts && window.charts[canvasId]) {
        window.charts[canvasId].destroy();
    }

    if (!window.charts) {
        window.charts = {};
    }

    const chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(d => d.label),
            datasets: [
                {
                    label: 'New Enrollments',
                    data: data.map(d => d.newEnrollments),
                    borderColor: '#007bff',
                    backgroundColor: 'rgba(0, 123, 255, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#007bff',
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2,
                    pointRadius: 6
                },
                {
                    label: 'Completed',
                    data: data.map(d => d.completedEnrollments),
                    borderColor: '#28a745',
                    backgroundColor: 'rgba(40, 167, 69, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#28a745',
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2,
                    pointRadius: 6
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    labels: {
                        usePointStyle: true,
                        padding: 20,
                        font: {
                            size: 12,
                            weight: '500'
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    borderColor: '#007bff',
                    borderWidth: 1,
                    cornerRadius: 8,
                    displayColors: true,
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        font: {
                            size: 11
                        },
                        color: '#6c757d'
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)',
                        drawBorder: false
                    },
                    ticks: {
                        font: {
                            size: 11
                        },
                        color: '#6c757d'
                    }
                }
            },
            interaction: {
                mode: 'index',
                intersect: false
            },
            elements: {
                point: {
                    hoverRadius: 8
                }
            }
        }
    });

    window.charts[canvasId] = chart;
};

window.drawDoughnutChart = (canvasId, data) => {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    // Destroy existing chart if it exists
    if (window.charts && window.charts[canvasId]) {
        window.charts[canvasId].destroy();
    }

    if (!window.charts) {
        window.charts = {};
    }

    const chart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Excellent (80%+)', 'Good (60-79%)', 'Average (40-59%)', 'Poor (<40%)'],
            datasets: [{
                data: [data.excellent, data.good, data.average, data.poor],
                backgroundColor: [
                    '#28a745',
                    '#17a2b8',
                    '#ffc107',
                    '#dc3545'
                ],
                borderColor: [
                    '#ffffff',
                    '#ffffff',
                    '#ffffff',
                    '#ffffff'
                ],
                borderWidth: 3,
                hoverOffset: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        padding: 15,
                        font: {
                            size: 10,
                            weight: '500'
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    borderColor: '#007bff',
                    borderWidth: 1,
                    cornerRadius: 8,
                    displayColors: true,
                    callbacks: {
                        label: function (context) {
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((context.parsed / total) * 100).toFixed(1);
                            return `${context.label}: ${context.parsed} courses (${percentage}%)`;
                        }
                    }
                }
            },
            cutout: '60%',
            elements: {
                arc: {
                    borderRadius: 5
                }
            }
        }
    });

    window.charts[canvasId] = chart;
};

window.drawEnrollmentChart = (canvasId, data) => {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    // Destroy existing chart if it exists
    if (window.charts && window.charts[canvasId]) {
        window.charts[canvasId].destroy();
    }

    if (!window.charts) {
        window.charts = {};
    }

    const chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.map(d => d.label),
            datasets: [
                {
                    label: 'New Enrollments',
                    data: data.map(d => d.newEnrollments),
                    backgroundColor: 'rgba(0, 123, 255, 0.8)',
                    borderColor: '#007bff',
                    borderWidth: 2,
                    borderRadius: 8,
                    borderSkipped: false
                },
                {
                    label: 'Completed',
                    data: data.map(d => d.completedEnrollments),
                    backgroundColor: 'rgba(40, 167, 69, 0.8)',
                    borderColor: '#28a745',
                    borderWidth: 2,
                    borderRadius: 8,
                    borderSkipped: false
                },
                {
                    label: 'Dropped',
                    data: data.map(d => d.droppedEnrollments),
                    backgroundColor: 'rgba(220, 53, 69, 0.8)',
                    borderColor: '#dc3545',
                    borderWidth: 2,
                    borderRadius: 8,
                    borderSkipped: false
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    labels: {
                        usePointStyle: true,
                        padding: 20,
                        font: {
                            size: 12,
                            weight: '500'
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    borderColor: '#007bff',
                    borderWidth: 1,
                    cornerRadius: 8,
                    displayColors: true
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        font: {
                            size: 11
                        },
                        color: '#6c757d'
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)',
                        drawBorder: false
                    },
                    ticks: {
                        font: {
                            size: 11
                        },
                        color: '#6c757d'
                    }
                }
            }
        }
    });

    window.charts[canvasId] = chart;
};

// Initialize charts when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Chart.js default configuration
    Chart.defaults.font.family = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";
    Chart.defaults.color = '#495057';
    Chart.defaults.scale.grid.color = 'rgba(0, 0, 0, 0.1)';
});

// Cleanup function for when page is unloaded
window.addEventListener('beforeunload', function () {
    if (window.charts) {
        Object.values(window.charts).forEach(chart => {
            if (chart && typeof chart.destroy === 'function') {
                chart.destroy();
            }
        });
    }
});
