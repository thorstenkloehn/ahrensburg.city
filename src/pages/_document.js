import { Html, Head, Main, NextScript } from "next/document";
import Link from "next/link";

export default function Document() {
  return (
    <Html lang="de">
      <Head />
      <body>
      <div class="container mx-auto">
      <nav>
      <ul class="flex">
  <li class="mr-3">
        <Link href="/">Startseite</Link>
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
        <NextScript /><ul class="flex">
  <li class="mr-6">
    <a class="text-blue-500 hover:text-blue-800" href="#">Active</a>
  </li>
  <li class="mr-6">
    <a class="text-blue-500 hover:text-blue-800" href="#">Link</a>
  </li>
  <li class="mr-6">
    <a class="text-blue-500 hover:text-blue-800" href="#">Link</a>
  </li>
  <li class="mr-6">
    <a class="text-gray-400 cursor-not-allowed" href="#">Disabled</a>
  </li>
</ul>
        </div>
      </body>
    </Html>
  );
}
