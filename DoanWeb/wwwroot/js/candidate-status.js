// wwwroot/js/candidate-status.js

function handleStatusChange(select, appId) {
    const form = document.getElementById("form-" + appId);
    const reasonInput = document.getElementById("reasonInput-" + appId);

    if (select.value === "Từ chối") {
        Swal.fire({
            title: 'Chọn lý do từ chối',
            input: 'select',
            inputOptions: {
                'Thiếu kinh nghiệm': 'Thiếu kinh nghiệm',
                'Không phù hợp vị trí': 'Không phù hợp vị trí',
                'Kỹ năng chưa đáp ứng': 'Kỹ năng chưa đáp ứng',
                'Ứng viên không liên hệ': 'Ứng viên không liên hệ'
            },
            inputPlaceholder: '-- Chọn lý do --',
            showCancelButton: true,
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Hủy',
            inputValidator: (value) => {
                if (!value) {
                    return 'Bạn phải chọn lý do!';
                }
            }
        }).then((result) => {
            if (result.isConfirmed) {
                reasonInput.value = result.value;
                form.submit();
            } else {
                // nếu hủy thì reset lại về "Chờ duyệt"
                select.value = "Chờ duyệt";
            }
        });
    } else {
        form.submit();
    }
}
