
        document.addEventListener("DOMContentLoaded", function () {
            document.querySelectorAll(".favorite-btn").forEach(btn => {
                btn.addEventListener("click", function () {
                    var jobId = btn.getAttribute("data-job-id"); // 👈 lấy đúng attribute
                    console.log("Clicked favorite for JobId:", jobId);

                    $.post("/Candidate/ToggleSaveJob", { jobId: jobId }, function (response) {
                        console.log("Server response:", response);

                        if (response.success) {
                            if (response.action === "added") {
                                alert("✅ Đã lưu công việc!");
                                btn.querySelector("i").classList.remove("far");
                                btn.querySelector("i").classList.add("fas"); // solid heart
                            } else if (response.action === "removed") {
                                alert("❌ Đã bỏ lưu công việc!");
                                btn.querySelector("i").classList.remove("fas");
                                btn.querySelector("i").classList.add("far"); // outline heart
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
 
