document.addEventListener("DOMContentLoaded", function () {

    // Hover effect cho job-card
    document.querySelectorAll('.job-card').forEach(card => {
        card.addEventListener('mouseenter', () => {
            card.style.transition = "all 0.3s ease";
            card.style.transform = "translateY(-3px)";
            card.style.boxShadow = "0 8px 20px rgba(0,0,0,0.12)";
        });
        card.addEventListener('mouseleave', () => {
            card.style.transform = "translateY(0)";
            card.style.boxShadow = "0 2px 6px rgba(0,0,0,0.08)";
        });
    });

    // Multi-select địa điểm với Select2 + hiển thị tag
    if (typeof $ !== 'undefined' && $.fn.select2) {
        const $provinceSelect = $('#provinceSelect');
        const $tagBox = $('#selectedProvincesBox');
        const $form = $('#filterForm');

        if ($provinceSelect.length && $tagBox.length) {
            $provinceSelect.select2({
                placeholder: "Chọn địa điểm",
                allowClear: true,
                closeOnSelect: false
            });

            function updateTagBox() {
                $tagBox.empty();
                const selected = $provinceSelect.select2('data');
                if (selected.length === 0) {
                    $tagBox.html('<span class="text-muted">Chưa chọn địa điểm nào</span>');
                } else {
                    selected.forEach(item => {
                        const tag = $('<span class="badge bg-primary me-1 mb-1" style="font-size:0.8rem; cursor:pointer;">' + item.text + ' <i class="fas fa-times ms-1"></i></span>');
                        tag.on('click', function () {
                            const val = item.id;
                            const current = $provinceSelect.val() || [];
                            const newVal = current.filter(v => v !== val);
                            $provinceSelect.val(newVal).trigger('change');
                        });
                        $tagBox.append(tag);
                    });
                }
            }

            $provinceSelect.on('change', function () {
                updateTagBox();
                if ($form.length) $form.submit();
            });

            updateTagBox();
        }
    }

    // Submit tự động cho các bộ lọc khác
    const form = document.getElementById('filterForm');
    if (form) {
        const keywordInput = form.querySelector('input[name="Keyword"]');
        if (keywordInput) {
            keywordInput.addEventListener('input', function () {
                form.submit();
            });
        }
    }
});
