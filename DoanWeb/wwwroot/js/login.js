document.addEventListener("DOMContentLoaded", function () {
    const iconHidden = '/image/icon2.png'; // ẩn
    const iconVisible = '/image/icon1.png'; // hiện

    document.querySelectorAll('.togglePassword').forEach(function (toggle) {
        toggle.addEventListener('click', function () {
            const targetId = this.dataset.target;
            const input = document.getElementById(targetId);

            if (!input) return;

            const type = input.getAttribute('type') === 'password' ? 'text' : 'password';
            input.setAttribute('type', type);

            // đổi icon tương ứng
            this.src = type === 'password' ? iconHidden : iconVisible;
        });
    });
});