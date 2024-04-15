import React from 'react';
import Head from 'next/head';

/**
 * Represents the MapPage component.
 * @class
 * @extends React.Component
 */
export default class MapPage extends React.Component {
  /**
   * Lifecycle method called after the component has been rendered to the DOM.
   * Initializes the map and sets up the tile layer.
   */
  componentDidMount() {
    const L = require('leaflet');
    require('leaflet-hash');

    var map = L.map('map').setView([53.6703, -349.7544], 13);

    L.tileLayer('https://karte.ahrensburg.city/hot/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 21,
      maxNativeZoom: 20
    }).addTo(map);

    var hash = new L.Hash(map);
  }

  /**
   * Renders the MapPage component.
   * @returns {JSX.Element} The rendered MapPage component.
   */
  render() {
    return (
      <div>
        <Head>
          <title>Karte</title>
          <link rel="stylesheet" href="https://unpkg.com/leaflet@1.3/dist/leaflet.css" />
        </Head>
        <div id="map" style={{ height: '100vh' }}></div>
      </div>
    );
  }
}