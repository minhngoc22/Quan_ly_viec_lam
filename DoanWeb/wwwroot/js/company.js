

        function previewLogo(event) {
                                    const file = event.target.files[0];
        const preview = document.getElementById('logoPreview');

        if (file) {
            preview.src = URL.createObjectURL(file);
        preview.classList.remove("d-none");
                                    } else {
            preview.src = "#";
        preview.classList.add("d-none");
                                    }
                                }
