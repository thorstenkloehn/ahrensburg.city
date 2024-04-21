import Link from 'next/link'
import "../globals.css";
export default function DocsLayout({ children }) {
  return (

        <div className="flex">
          <div className="w-5/20">
            <ul className="flex flex-col space-y-1">
              <li><Link href="/Ahrensburg">Nachtleben</Link></li>
         
              </ul>
          </div>
          <div className="flex-grow pl-7 w-full">{children}</div>
        </div>
    
  )
}