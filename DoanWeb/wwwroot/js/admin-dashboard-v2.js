document.addEventListener("DOMContentLoaded", function () {
    var ctx = document.getElementById('adminChart');
    if (!ctx) return; // Nếu không có canvas thì dừng

    var adminChart = new Chart(ctx.getContext('2d'), {
        type: 'bar',
        data: {
            labels: ["Người dùng", "Nhà tuyển dụng", "Việc làm", "Báo cáo", "Cẩm nang"],
            datasets: [{
                label: 'Số lượng',
                data: JSON.parse(ctx.dataset.values), // <-- phải đọc 5 số
                backgroundColor: ['#0d6efd', '#ffc107', '#198754', '#dc3545', '#6f42c1'], // <-- đúng 5 màu
                barThickness: 40,
                maxBarThickness: 50
            }]
        },
        options: {
            maintainAspectRatio: false,
            responsive: true,
            plugins: {
                legend: { display: false },
                tooltip: {
                    enabled: true,
                    callbacks: {
                        label: function (context) {
                            return context.raw + ' mục';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { stepSize: 1 }
                }
            }
        }
    });
});
