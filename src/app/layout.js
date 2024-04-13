import "./globals.css";
import Navigation from "../components/Navigation";
export default function RootLayout({ children }) {
  return (
    <html lang="de">
      <body>
      <div className="container mx-auto">
        {/* Layout UI */}
        <Navigation />
        <main>{children}</main>
        </div>
      </body>
    </html>
  )
}
