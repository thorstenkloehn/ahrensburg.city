import React from 'react'; // Add the missing import statement for React
import { Pool } from 'pg';

function HomePage({ data }) {

    return (
        <div>
            <h1>Läden in Ahrensburg</h1>
            <br></br>
            <ul>
                {data.map(item => (
                    <li key={item.osm_id}>
                        {item.name} - {item.addr_street} - {item.addr_city}
                    </li>
                ))}
            </ul>
        </div>
    );
}

export async function getServerSideProps() {
    const pool = new Pool({
        user: process.env.DB_USER,
        host: process.env.DB_HOST,
        database: "thorsten",
        password: process.env.DB_PASSWORD,
        port: 5432,
    });

    const res = await pool.query(`
    SELECT name, osm_id, way, tags, shop, tags -> 'addr:housenumber' AS housenumber,
    tags -> 'addr:city' AS addr_city,
    tags -> 'addr:street' AS addr_street
FROM (
    SELECT name, osm_id, way, tags, shop
    FROM planet_osm_polygon
    UNION ALL
    SELECT name, osm_id, way, tags, shop
    FROM planet_osm_point
    UNION ALL
    SELECT name, osm_id, way, tags, shop
    FROM planet_osm_line
) AS combined_table
WHERE Shop IS NOT NULL AND tags -> 'addr:city' = 'Ahrensburg';
`);
    const data = res.rows;

    return {
        props: { data },
    }
}

export default HomePage;