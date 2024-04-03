import "./globals.css";
import Link from "next/link";
import Navigation from "../components/Navigation";

export default function RootLayout({ children }) {
  return (
    <div className="container mx-auto">
      <Navigation />
        {children}
      </div>
  
  );
}
