document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".remove-favorite-btn").forEach(btn => {
        btn.addEventListener("click", function () {
            var jobId = btn.getAttribute("data-jobid");

            $.post("/Candidate/ToggleSaveJob", { jobId: jobId }, function (response) {
                if (response.success && response.action === "removed") {
                    var jobCard = document.getElementById("job-" + jobId);
                    if (jobCard) {
                        jobCard.remove();
                    }

                    // Nếu không còn job nào thì hiển thị thông báo trong wrapper
                    if (document.querySelectorAll(".job-card-wrapper").length === 0) {
                        document.getElementById("saved-jobs-wrapper").innerHTML = `
                            <div class="alert alert-info">
                                Bạn chưa lưu công việc nào. Hãy quay lại
                                <a href="/Job/Index">tìm việc</a> nhé!
                            </div>`;
                    }
                } else {
                    alert("⚠️ " + response.message);
                }
            }).fail(function (xhr, status, error) {
                console.error("Lỗi khi gọi ToggleSaveJob:", status, error);
                alert("Có lỗi xảy ra, vui lòng thử lại sau!");
            });
        });
    });
});
