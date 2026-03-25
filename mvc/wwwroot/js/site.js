// Cookie Banner Logic
document.addEventListener("DOMContentLoaded", function () {
    const banner = document.getElementById("cookieBanner");
    const acceptBtn = document.getElementById("acceptCookies");
    const declineBtn = document.getElementById("declineCookies");

    // Prüfen, ob bereits eine Entscheidung getroffen wurde
    if (!localStorage.getItem("cookieConsent")) {
        banner.style.display = "block";
    }

    acceptBtn.addEventListener("click", function () {
        localStorage.setItem("cookieConsent", "accepted");
        banner.style.display = "none";
    });

    declineBtn.addEventListener("click", function () {
        localStorage.setItem("cookieConsent", "declined");
        banner.style.display = "none";
    });
});
