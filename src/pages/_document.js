import { Html, Head, Main, NextScript } from "next/document";
import Link from "next/link";


export default function Document() {
  return (
    <Html lang="de">
      <Head>
      <title>ahrensburg.city</title>
       </Head>
      <body>
      <div class="container mx-auto">
      <nav>
      <ul class="flex">
  <li class="mr-3">
        <Link href="/">Startseite</Link>
  </li>
  <li class="mr-3">
        <Link href="/shop">Läden</Link>
        </li>
 <li class="mr-3">
        <Link href="/Impressum">Impressum</Link>
        </li>
        <li class="mr-3">
        <Link href="/Datenschutz">Datenschutz</Link>
        </li>


        </ul>

       


        </nav>
        <Main />
        <NextScript />
 

        </div>
      </body>
    </Html>
  );
}
