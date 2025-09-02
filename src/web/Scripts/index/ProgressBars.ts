function createFileProgressBar(filename: string): { container: HTMLElement, bar: HTMLElement } {
    // Outer container
    const wrapper = document.createElement("div");
    wrapper.className = "mb-3 file-upload-progress";

    // Label
    const label = document.createElement("label");
    label.className = "form-label";
    label.textContent = filename;
    wrapper.appendChild(label);

    // Progress container
    const progress = document.createElement("div");
    progress.className = "progress";
    progress.style.height = "20px";

    // Progress bar
    const bar = document.createElement("div");
    bar.className = "progress-bar progress-bar-striped progress-bar-animated bg-success";
    bar.setAttribute("role", "progressbar");
    bar.setAttribute("aria-valuenow", "0");
    bar.setAttribute("aria-valuemin", "0");
    bar.setAttribute("aria-valuemax", "100");
    bar.dataset["file"] = filename;
    bar.style.width = "0%";
    bar.textContent = "0%";

    // Assemble
    progress.appendChild(bar);
    wrapper.appendChild(progress);

    return { container: wrapper, bar };
}
function updateProgressBar(bar: HTMLElement, percent: number) {
    percent = Math.max(0, Math.min(100, percent)); // clamp
    bar.style.width = `${percent}%`;
    bar.setAttribute("aria-valuenow", percent.toString());
    bar.textContent = `${percent}%`;
}
export { updateProgressBar, createFileProgressBar };
