import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || "https://localhost:5001",
  },
  output: "standalone",
  reactStrictMode: true,
  images: {
    domains: ["localhost"],
  },
  reactCompiler: true,
};

export default nextConfig;
