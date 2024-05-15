const withMDX = require('@next/mdx')()
 
/** @type {import('next').NextConfig} */
const nextConfig = {
  // Configure `pageExtensions` to include MDX files
 pageExtensions: ['js', 'jsx', 'mdx', 'ts', 'tsx'],
 
//  basePath: '/ahrensburg.city',
 // assetPrefix: '/ahrensburg.city/',

 output: 'export',
 
  // Optional: Change links `/me` -> `/me/` and emit `/me.html` -> `/me/index.html`
   trailingSlash: true,
 
  // Optional: Prevent automatic `/me` -> `/me/`, instead preserve `href`
   skipTrailingSlashRedirect: true,
 
  // Optional: Change the output directory `out` -> `dist`
 distDir: 'out',
 
}
 

 
module.exports = withMDX(nextConfig)