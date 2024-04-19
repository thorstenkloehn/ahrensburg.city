import Link from 'next/link'
import "../globals.css";
export default function DocsLayout({ children }) {
  return (

        <div className="flex">
          <div className="w-5/20">
            <ul className="flex flex-col space-y-1">
              <li><Link href="/doc">Server</Link></li>
              <li><Link href="/doc/IDE">IDE</Link></li>
              <li><Link href="/doc/Programmiersprachen">Programmiersprachen</Link></li>
              <li><Link href="/doc/Server_Mieten">Server Mieten</Link></li>
              <li><Link href="/doc/Analysetool">Analysetool</Link></li>
              <li><Link href="/doc/Webentwicklung">Webentwicklung</Link></li>
              <li><Link href="/doc/Django">Django</Link></li>
              <li><Link href="/doc/PHP_kein">PHP Kein</Link></li>
            </ul>
          </div>
          <div className="flex-grow pl-7 w-full">{children}</div>
        </div>
    
  )
}