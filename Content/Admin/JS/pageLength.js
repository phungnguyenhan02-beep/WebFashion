
$(document).ready(function () {
    $('#dataTable').DataTable({
        "pageLength": 10,
        "lengthMenu": [5, 10, 20, 50],
        "language": {
            "lengthMenu": "Hiển thị _MENU_ dòng mỗi trang",
            "zeroRecords": "Không tìm thấy dữ liệu phù hợp",
            "info": "Hiển thị _START_ đến _END_ của _TOTAL_ dòng",
            "infoEmpty": "Không có dữ liệu",
            "infoFiltered": "(lọc từ _MAX_ tổng số dòng)",
            "paginate": {
                "first": "Đầu",
                "last": "Cuối",
                "next": "Sau",
                "previous": "Trước"
            }
        }
    });
});
