var startMenuActive = false;
var startMenu = document.getElementById("sidebar");
var startMenuButton = document.getElementById("start-menu-button");

startMenuButton.addEventListener("click", startButtonClicked);

function startButtonClicked() {
    if (startMenuActive) {
        closeStartMenu();
    } else {
        startMenu.style.display = "block";
        startMenuActive = true;
    }
}

function closeStartMenu() {
    if (window.getComputedStyle(document.body).display == "grid") {
        return;
    }
    startMenu.style.display = "none";
    startMenuActive = false;
}

document.addEventListener("click", function (event) {
    if (
        !(startMenu.contains(event.target) || startMenuButton.contains(event.target))
        && startMenuActive
    ) {
        closeStartMenu();
    }
});

window.addEventListener("resize", function (event) {
    if (window.innerWidth > 700) {
        startMenu.style.display = "flex";
        startMenuActive = true; s
    }
});
