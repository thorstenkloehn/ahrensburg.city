function onEachFeature(feature, layer) {
    // does this feature have a property named popupContent?
    if (feature.properties && feature.properties.popupContent) {
        layer.bindPopup(feature.properties.popupContent);
    }
}
//<![CDATA[

var map = L.map('map').setView([53.6700755, 10.2071975], 13);

L.tileLayer('https://ahrensburg.city/karte/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="https://ahrensburg.city">ahrensburg.city</a>,<a href="https://thorstenkloehn.github.io/docs/info/Impressum/">Impressum</a>,<a href="https://thorstenkloehn.github.io/docs/info/Datenschutzerkl%C3%A4rung/">Datenschutzerklärung</a> , </a><a href="https://thorstenkloehn.github.io/docs/">Dokument</a>,&copy;<a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
}).addTo(map);

let laden = new XMLHttpRequest;
laden.open('GET', './geoserver/Geoserver/ows?service=WFS&version=1.0.0&request=GetFeature&typeName=Geoserver%3Aahrensburg&maxFeatures=50&outputFormat=application%2Fjson', true);
laden.onload = function () {
    if (laden.status == 200) {

        L.geoJSON(JSON.parse(laden.responseText), {
            onEachFeature: onEachFeature
        }).addTo(map);

    }
}

laden.send();

var gpx = './gpx/Auewanderweg/auewanderweg_gpx_20220519_191032.gpx'; // URL to your GPX file or the GPX itself

new L.GPX(gpx, {
    async: true,
    marker_options: {
        startIconUrl: 'static/Leaflet/gpx/pin-icon-start.png',
        endIconUrl: 'static/Leaflet/gpx/pin-icon-end.png',
        shadowUrl: 'static/Leaflet/gpx/pin-shadow.png',
        wptIconUrls: 'static/Leaflet/gpx/pin-icon-wpt.png'
    }
}).on('loaded', function(e) {
    map.fitBounds(e.target.getBounds());
    var ergenis = e.target;
    var info = "Auewanderweg"
    ergenis.bindPopup(info);

}).addTo(map);