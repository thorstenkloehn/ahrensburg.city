import React from "react";
import Link from "next/link";

export default function Navigation() {
    return (
        <div className="navbar navbar-expand-lg navbar-light bg-light">
            <Link href="/" className="navbar-brand">Startseite</Link>
            <div className="collapse navbar-collapse">
                <ul className="navbar-nav mr-auto">
                    <li className="nav-item">
                        <Link href="/ahrensburgkarte" className="nav-link">Karte</Link>
                    </li>
                    <li className="nav-item">
                        <Link href="/shop" className="nav-link">Läden</Link>
                    </li>
                    <li className="nav-item">
                        <Link href="/Impressum" className="nav-link">Impressum</Link>
                    </li>
                    <li className="nav-item">
                        <Link href="/Datenschutz" className="nav-link">Datenschutz</Link>
                    </li>
                    <li className="nav-item">
                        <a href="https://thorstenkloehn.github.io/ahrensburg.city/" className="nav-link" target="_blank" rel="noopener noreferrer">Dokument</a>
                    </li>
                </ul>
            </div>
        </div>
    );
}