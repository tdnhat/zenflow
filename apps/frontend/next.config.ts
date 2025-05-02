import type { NextConfig } from "next";
import path from "path";

const nextConfig: NextConfig = {
    webpack: (config) => {
        config.resolve.alias = {
            ...config.resolve.alias,
            "@shared": path.resolve(__dirname, "../../libs/shared/src")
        };
        return config;
    },
    turbopack: {
        resolveAlias: {
            "@shared": path.resolve(__dirname, "../../libs/shared/src")
        }
    },
    experimental: {
        externalDir: true  // Allow importing from outside the app directory
    }
};

export default nextConfig;
