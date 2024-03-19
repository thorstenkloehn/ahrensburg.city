/** @type {import('next').NextConfig} */
import withMDX from '@next/mdx'

const nextConfig = {
 // reactStrictMode: true,
 pageExtensions: ['js', 'jsx', 'mdx', 'ts', 'tsx'],
};

export default withMDX()(nextConfig)