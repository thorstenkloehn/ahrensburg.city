import Navigation from "../components/Navigation";
import { Html, Head, Main, NextScript } from "next/document";


export default function Document() {
  return (
    <Html lang="en">
      <Head />
      <body>
      <div className="container mx-auto">
      <Navigation />
        <Main />
        <NextScript />
        </div>
      </body>
    </Html>
  );
}
