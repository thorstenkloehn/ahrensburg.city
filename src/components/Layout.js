import Link from 'next/link'
import React from 'react'

export default function Layout({ children }) {
  return (
    <div>
      <br></br>
      <Link href="/Installieren">Entwicklungumgebung</Link> - 
      <Link href="/Installieren/Server">Server</Link> - 
      <Link href="/Installieren/Server_Mieten">Server Mieten</Link> - 
      <Link href="/Installieren/Hosting">Hosting</Link> -
      <br></br>
      <br></br>
      {children}
    </div>
  )
}