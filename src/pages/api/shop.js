import { Pool } from 'pg';

export default async function handler(_, res) {
    try {
        const pool = new Pool({
            user: process.env.DB_USER,
            host: process.env.DB_HOST,
            database: process.env.DB_NAME,
            password: process.env.DB_PASSWORD,
            port: 5432,
        });

        const client = await pool.connect();
        const result = await client.query("SELECT ST_AsGeoJSON(way) AS geojson , * FROM planet_osm_point WHERE Shop IS NOT NULL AND tags -> 'addr:city' = 'Ahrensburg'");

        const features = result.rows.map(row => {
            return {
                type: 'Feature',
                geometry: JSON.parse(row.geojson),
                properties: row,
            };
        });

        res.status(200).json({ type: 'FeatureCollection', features: features });
        client.release();
    } catch (error) {
        console.error('Error executing query', error);
        res.status(500).json({ error: 'Datenbank Fehler' });
    }
}
