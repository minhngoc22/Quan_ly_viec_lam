// Load danh sách thông báo
function loadNotifications() {
    fetch('/Notification/GetNotifications')
        .then(res => res.json())
        .then(data => {
            const list = document.getElementById('notificationDropdownMenu');
            const badge = document.getElementById('notificationBadge');

            // reset danh sách
            list.innerHTML = '';

            if (!data || data.length === 0) {
                list.innerHTML = '<li><span class="dropdown-item text-muted">Không có thông báo</span></li>';
                badge.style.display = "none";
                return;
            }

            // đếm số chưa đọc
            const unreadCount = data.filter(n => !n.isRead).length;

            if (unreadCount > 0) {
                badge.textContent = unreadCount;
                badge.style.display = "inline-block";
            } else {
                badge.style.display = "none";
            }

            // render danh sách (ngắn gọn)
            data.forEach(n => {
                const li = document.createElement('li');
                li.classList.add('dropdown-item', 'notification-item');

                // Nếu chưa đọc thì fill màu
                if (!n.isRead) {
                    li.classList.add('unread-notification');
                }

                // rút gọn message cho gọn dropdown
                let shortMsg = n.message.length > 40 ? n.message.substring(0, 40) + "..." : n.message;

                li.innerHTML = `
                    <span style="cursor:pointer;">
                        ${shortMsg}
                        <br><small class="text-muted">${n.createdAt}</small>
                    </span>
                `;

                // Khi click thì mở chi tiết
                li.addEventListener('click', () => showNotification(n));
                list.appendChild(li);
            });
        })
        .catch(err => {
            console.error("Lỗi load thông báo:", err);
        });
}

// Hiển thị chi tiết bằng SweetAlert
function showNotification(notification) {
    Swal.fire({
        title: 'Thông báo',
        html: `
            <p>${notification.message}</p>
          ${notification.reason ? `<p><b>Lý do:</b> ${notification.reason}</p>` : ""}
            <small class="text-muted">${notification.createdAt}</small>
        `,
        icon: 'info',
        confirmButtonText: 'Đóng'
    });

    // Đánh dấu đã đọc
    fetch('/Notification/MarkAsRead', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Id: notification.id })
    }).then(() => loadNotifications());
}

// load khi mở trang
document.addEventListener('DOMContentLoaded', loadNotifications);
