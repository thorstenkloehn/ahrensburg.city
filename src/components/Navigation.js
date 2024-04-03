import Link from 'next/link';
export default function Navigation() {
    return (
       <nav className="flex">
            <ul className="list-none flex flex-row">
            <li>
                <Link href="/" className="text-blue-500">
                Startseite
                </Link>
            </li>
            <li>
                <Link href="/karte" className="text-blue-500 p-4">
                Karte
                </Link>
            </li>
            <li>
                <Link href="/shop" className="text-blue-500 p-4">
                Läden
                </Link>
            </li>
            <li>
                <Link href="/doc" className="text-blue-500 p-4">
                Dokumentation
                </Link>
            </li>


            <li>
                <Link href="/Impressum" className="text-blue-500 p-4">
                Impressum
                </Link>
            </li>

            <li>
                <Link href="/Datenschutz" className="text-blue-500 p-4">
                Datenschutz
                </Link>
            </li>
            
            </ul>
        </nav>

        );
}


