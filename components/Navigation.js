import React from "react";
import Link from "next/link";

export default function Navigation() {
    return (
        <div>

            <Link href="/">Startseite</Link> - 
            <Link href="/ahrensburgkarte">Karte</Link> -
            <Link href="/shop">Läden</Link> -
            <Link href="/Impressum">Impressum</Link> - 
            <Link href="/Datenschutz">Datenschutz</Link> 
        </div>
    );
}