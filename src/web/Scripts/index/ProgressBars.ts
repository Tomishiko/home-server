type ProgressBarElement = {
    Bar: HTMLElement,
    Wrapper: HTMLElement
}
export class ProgressBarCtrl {
    private bars = new Map<string, ProgressBarElement>();

    public createFileProgressBar(filename: string): { container: HTMLElement, bar: HTMLElement } {
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
        this.bars.set(filename, { Bar: bar, Wrapper: wrapper });

        return { container: wrapper, bar };
    }
    public updateProgressBar(fname: string, percent: number) {

        percent = Math.max(0, Math.min(100, percent)); // clamp
        const barElement = this.bars.get(fname);
        if (barElement) {

            barElement.Bar.style.width = `${percent}%`;
            barElement.Bar.setAttribute("aria-valuenow", percent.toString());
            barElement.Bar.textContent = `${percent}%`;
        }
    }
    public hideUploadProgressBars(...args: string[]) {

        if (args.length == 0) {
            this.bars.forEach(x => x.Wrapper.remove());
            this.bars.clear();
        }
        else {
            for (const payloadFileName of args) {
                const bar = this.bars.get(payloadFileName);
                if (bar) {
                    bar.Wrapper.remove();
                    this.bars.delete(payloadFileName);
                }
            }
        }

    }
}
