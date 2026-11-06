document.addEventListener("DOMContentLoaded", function () {
    const checkBoxes = document.querySelectorAll(".chk-box");
    const popupLinks = document.querySelectorAll(".curriculum-popup-link");
    const bootstrapModal = new bootstrap.Modal(document.getElementById("curriculumModal"));
    const modalTitle = document.getElementById("modalQuestionTitle");
    const modalBody = document.getElementById("modalAnswerBody");

    checkBoxes.forEach(chk => {
        chk.addEventListener("change", function () {
            localStorage.setItem(this.dataset.storageKey, this.checked);
            calculateProgress();
        });
    });

    popupLinks.forEach(link => {
        link.addEventListener("click", function (e) {
            e.preventDefault();

            modalTitle.textContent = this.dataset.title;
            modalBody.innerHTML = this.dataset.answer;
            bootstrapModal.show();

            const checkbox = this.closest('.form-check').querySelector('.chk-box');
            if (checkbox && !checkbox.checked) {
                checkbox.checked = true;
                localStorage.setItem(checkbox.dataset.storageKey, "true");
                calculateProgress();
            }
        });
    });

    calculateProgress();
});

function calculateProgress() {
    const checkBoxes = document.querySelectorAll(".chk-box");
    let checkedCount = 0;

    checkBoxes.forEach(chk => {
        const savedValue = localStorage.getItem(chk.dataset.storageKey);
        if (savedValue === "true") {
            chk.checked = true;
            checkedCount++;
        } else {
            chk.checked = false;
        }

        const wrapper = chk.closest('.form-check');
        if (wrapper) {
            if (chk.checked) {
                wrapper.classList.add("text-decoration-line-through", "text-muted");
            } else {
                wrapper.classList.remove("text-decoration-line-through", "text-muted");
            }
        }
    });

    const dayGroups = document.querySelectorAll(".day-accordion-group");
    dayGroups.forEach(group => {
        const groupCheckboxes = group.querySelectorAll(".chk-box");
        if (groupCheckboxes.length === 0) return;

        let checkedGroupCount = 0;
        groupCheckboxes.forEach(cb => {
            if (cb.checked) checkedGroupCount++;
        });

        const dayPercentage = Math.round((checkedGroupCount / groupCheckboxes.length) * 100);
        const miniBar = group.querySelector(".day-mini-progress");
        const progressLabel = group.querySelector(".day-progress-text");

        if (miniBar) miniBar.style.width = dayPercentage + "%";
        if (progressLabel) progressLabel.innerText = dayPercentage + "%";
    });

    if (checkBoxes.length === 0) return;
    const totalModulePercentage = Math.round((checkedCount / checkBoxes.length) * 100);
    const moduleProgressBar = document.getElementById("moduleProgressBar");
    const moduleProgressPercentText = document.getElementById("moduleProgressPercentText");

    if (moduleProgressBar) {
        moduleProgressBar.style.width = totalModulePercentage + "%";
        moduleProgressBar.innerText = totalModulePercentage + "%";
        moduleProgressBar.setAttribute("aria-valuenow", totalModulePercentage);
    }
    if (moduleProgressPercentText) {
        moduleProgressPercentText.textContent = totalModulePercentage + "%";
    }
}

function resetChecklist() {
    if (confirm("Are you sure you want to clear all checked items for this week?")) {
        const checkboxes = document.querySelectorAll(".chk-box");
        checkboxes.forEach(cb => {
            cb.checked = false;
            const storageKey = cb.getAttribute('data-storage-key');
            localStorage.removeItem(storageKey);

            const wrap = cb.closest('.form-check');
            if (wrap) {
                wrap.classList.remove("text-decoration-line-through", "text-muted");
            }
        });
        calculateProgress();
    }
}

window.resetChecklist = resetChecklist;
