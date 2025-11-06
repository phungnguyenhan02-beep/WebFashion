
document.addEventListener("DOMContentLoaded", function () {
    const ctx = document.getElementById('earningsChart').getContext('2d');
    const earningsChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],  // Mảng tháng sẽ được cập nhật từ API
            datasets: [{
                label: 'Thu Nhập Năm',
                data: [],  // Dữ liệu thu nhập sẽ được cập nhật từ API
                backgroundColor: 'rgba(78, 115, 223, 0.05)',
                borderColor: 'rgba(78, 115, 223, 1)',
                pointBackgroundColor: 'rgba(78, 115, 223, 1)',
                pointBorderColor: 'rgba(78, 115, 223, 1)',
                pointHoverBackgroundColor: 'rgba(78, 115, 223, 1)',
                pointHoverBorderColor: 'rgba(78, 115, 223, 1)',
            }]
        },
        options: {
            maintainAspectRatio: false,
            layout: {
                padding: {
                    left: 10,
                    right: 25,
                    top: 25,
                    bottom: 0
                }
            },
            scales: {
                xAxes: [{
                    time: {
                        unit: 'month'
                    },
                    gridLines: {
                        display: false,
                        drawBorder: false
                    },
                    ticks: {
                        maxTicksLimit: 12
                    }
                }],
                yAxes: [{
                    ticks: {
                        maxTicksLimit: 5,
                        padding: 10,
                        callback: function (value) {
                            return '$' + value.toLocaleString();
                        }
                    },
                    gridLines: {
                        color: 'rgb(234, 236, 244)',
                        zeroLineColor: 'rgb(234, 236, 244)',
                        drawBorder: false,
                        borderDash: [2],
                        zeroLineBorderDash: [2]
                    }
                }]
            },
            legend: {
                display: false
            },
            tooltips: {
                backgroundColor: 'rgb(255,255,255)',
                bodyFontColor: '#858796',
                titleMarginBottom: 10,
                titleFontColor: '#6e707e',
                titleFontSize: 14,
                borderColor: '#dddfeb',
                borderWidth: 1,
                xPadding: 15,
                yPadding: 15,
                displayColors: false,
                intersect: false,
                mode: 'index',
                caretPadding: 10,
                callbacks: {
                    label: function (tooltipItem, chart) {
                        return '$' + tooltipItem.yLabel.toLocaleString();
                    }
                }
            }
        }
    });

    // Fetch data from API
    fetch('/Admin/GetEarningsByMonth')  // Thay đổi URL API của bạn tại đây
        .then(response => response.json())
        .then(data => {
            // Cập nhật vào biểu đồ
            earningsChart.data.labels = data.months;  // Dữ liệu tháng
            earningsChart.data.datasets[0].data = data.earnings;  // Dữ liệu thu nhập
            earningsChart.update();  // Cập nhật biểu đồ
        })
        .catch(error => console.error('Error fetching data:', error));
});
