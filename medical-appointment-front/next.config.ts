import type { NextConfig } from "next";

// The browser calls /api/* on this same origin and Next.js proxies it to the API
// gateway. Same origin means the httpOnly auth cookie works with SameSite=Strict
// and no CORS is needed. API_GATEWAY_URL is server-only (not exposed to the browser).
const apiGatewayUrl = process.env.API_GATEWAY_URL ?? "http://localhost:5216";

const nextConfig: NextConfig = {
  experimental: {
    agentFeedback: true,
  },
  turbopack: {
    rules: {
      "*.css": {
        loaders: ["@tailwindcss/turbopack"],
        as: "*.css",
      },
    },
  },
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${apiGatewayUrl}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
