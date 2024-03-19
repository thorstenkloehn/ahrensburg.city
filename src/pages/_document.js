import { Html, Head, Main, NextScript } from "next/document";
import { Container } from "react-bootstrap";
import Navigation  from "../../components/Navigation";

export default function Document() {
  return (
    <Html lang="de">
      <Head />
      <body>
        <Container>
          <Navigation />
          <Main />
          <NextScript />
        </Container>
      </body>
    </Html>
  );
}
