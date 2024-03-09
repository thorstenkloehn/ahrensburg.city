import { Pool } from 'pg';
import { Pool } from 'pg';
const pool = new Pool({
    user: 'your_username',
    host: 'your_host',
    database: 'your_database',
    password: 'your_password',
    port: 5432, // or your custom port
});

export default async function handler(req, res) {
    try {
        const pool = new Pool({
            user: 'your_username',
            host: dbHost,
            database: 'your_database',
            password: 'your_password',
            port: 5432, // or your custom port
        });

        const client = await pool.connect();
        const result = await client.query("SELECT * FROM planet_osm_point WHERE Shop IS NOT NULL AND tags -> 'addr:city' = 'Ahrensburg'");
        res.status(200).json(result.rows);
        client.release();
    } catch (error) {
        console.error('Error executing query', error);
        res.status(500).json({ error: 'Datnbank Fehler' });
    }
}
