// wwwroot/js/job-search.js
document.addEventListener("DOMContentLoaded", function () {

    // Auto submit khi chọn tỉnh
    const provinceSelect = document.getElementById('provinceSelect');
    if (provinceSelect) {
        provinceSelect.addEventListener('change', function () {
            document.getElementById('jobSearchForm').submit();
        });
    }

    // Hover effect company-card
    document.querySelectorAll('.company-card').forEach(card => {
        card.addEventListener('mouseenter', () => {
            card.style.transition = "all 0.3s ease";
            card.style.backgroundColor = "#f0f0f0";
            card.style.boxShadow = "0 8px 20px rgba(0,0,0,0.12)";
            card.style.border = "1px solid #dee2e6";
        });
        card.addEventListener('mouseleave', () => {
            card.style.backgroundColor = "white";
            card.style.boxShadow = "0 2px 6px rgba(0,0,0,0.08)";
            card.style.border = "1px solid transparent";
        });
    });

    // Hover effect stat-card
    document.querySelectorAll('.stat-card').forEach(card => {
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

    // =======================
    // 🔍 Debounce tìm kiếm
    // =======================
    let searchTimeout;
    const searchInput = document.getElementById('keywordInput');
    const form = document.getElementById('jobSearchForm');

    if (searchInput && form) {
        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimeout);

            const keyword = this.value.trim();
            // ⏱️ Chờ 600ms sau khi dừng gõ mới submit
            searchTimeout = setTimeout(() => {
                form.submit();
            }, 600);
        });

        // Cho phép nhấn Enter để tìm ngay
        searchInput.addEventListener('keyup', function (e) {
            if (e.key === 'Enter') {
                form.submit();
            }
        });
    }
});
