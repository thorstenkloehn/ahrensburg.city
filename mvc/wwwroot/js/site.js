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

    // Wiki-Map Initialization
    if (typeof L !== "undefined") {
        const mapElements = document.querySelectorAll(".wiki-map");
        console.log("Found " + mapElements.length + " map elements.");

        mapElements.forEach(function (el, index) {
            try {
                const lat = parseFloat(el.getAttribute("data-lat") || "53.6761");
                const lon = parseFloat(el.getAttribute("data-lon") || "10.2736");
                const zoom = parseInt(el.getAttribute("data-zoom") || "13");
                const markerText = el.getAttribute("data-marker");

                console.log("Initializing map " + index + " at " + lat + ", " + lon);

                const mapId = "map-" + index;
                el.id = mapId;

                const map = L.map(mapId).setView([lat, lon], zoom);

                // Lokaler Tileserver (Ahrensburg Spezial)
                L.tileLayer('https://ahrensburg.city/hot/{z}/{x}/{y}.png', {
                    maxZoom: 19,
                    attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                }).addTo(map);

                if (markerText && markerText.trim() !== "") {
                    L.marker([lat, lon]).addTo(map).bindPopup(markerText);
                } else if (markerText !== null) {
                    L.marker([lat, lon]).addTo(map);
                }

                // Fix für graue Flächen (Resize-Event triggern)
                setTimeout(() => { map.invalidateSize(); }, 200);

            } catch (e) {
                console.error("Error initializing map:", e);
            }
        });
    } else {
        console.error("Leaflet (L) is not defined! Check if leaflet.js is loaded.");
    }
    });
