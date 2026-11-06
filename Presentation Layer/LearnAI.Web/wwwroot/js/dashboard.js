document.addEventListener("DOMContentLoaded", function () {
    const trueWeekItemTotals = window.trueWeekItemTotals || {};
    const storageKeys = Object.keys(localStorage);
    const weekCheckedCounts = { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0, 6: 0, 7: 0, 8: 0 };
    let totalSumOfPercentages = 0;

    storageKeys.forEach(key => {
        const match = key.match(/^W([1-8])-/);
        if (match) {
            const weekNum = parseInt(match[1]);
            if (localStorage.getItem(key) === "true") {
                weekCheckedCounts[weekNum]++;
            }
        }
    });

    for (let w = 1; w <= 8; w++) {
        const totalItemsInWeek = trueWeekItemTotals[w] || 0;
        const checkedItemsInWeek = weekCheckedCounts[w];
        const textElement = document.getElementById(`progress-text-W${w}`);
        const doneBadge = document.getElementById(`badge-status-W${w}`);
        const tileBar = document.getElementById(`tileProgressBar-W${w}`);

        let weekPercentage = 0;
        if (totalItemsInWeek > 0) {
            weekPercentage = Math.round((checkedItemsInWeek / totalItemsInWeek) * 100);
        }

        totalSumOfPercentages += weekPercentage;

        if (textElement) {
            textElement.innerText = `${weekPercentage}% completed`;
        }

        if (tileBar) {
            tileBar.style.width = weekPercentage + "%";
            tileBar.setAttribute("aria-valuenow", weekPercentage);
        }

        if (weekPercentage === 100 && doneBadge) {
            doneBadge.classList.remove("d-none");
        }
    }

    const overallCoursePercentage = Math.round(totalSumOfPercentages / 8);
    const overallBar = document.getElementById("totalCourseProgressBar");

    if (overallBar) {
        overallBar.style.width = overallCoursePercentage + "%";
        overallBar.innerText = overallCoursePercentage + "%";
        overallBar.setAttribute("aria-valuenow", overallCoursePercentage);
    }
});
