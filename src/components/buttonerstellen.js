'use client'
import React, { useState } from 'react';
import Link from 'next/link';

const ButtonErstellen = () => {
    const [isOpen, setIsOpen] = useState(false);

    const handleClick = () => {
        setIsOpen(!isOpen);
    };

    return (
        <>
            <button className="md:hidden bg-blue-500 text-white" onClick={handleClick}>Menü</button>
            {isOpen && (
          <div className="absolute bg-white text-black p-4 flex flex-col">
          <Link href="/" className="block">Startseite</Link>
          <Link href="/Ahrensburg" className="block">Über Ahrensburg</Link>
          <Link href="/karte" className="block">Karte</Link>
          <Link href="/shop" className="block">Läden</Link>
          <Link href="/doc" className="block">Dokumentation</Link>
          <Link href="/Impressum" className="block">Impressum</Link>
      </div>
            )}
        </>
    );
};

export default ButtonErstellen;