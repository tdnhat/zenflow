"use client";

import axios from "axios";
import { getSession } from "next-auth/react";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5050/api";

const apiClient = axios.create({
    baseURL: API_URL,
    headers: {
        "Content-Type": "application/json",
    },
});

// Add auth token to requests
apiClient.interceptors.request.use(async (config) => {
    // Server-side rendering check
    if (typeof window === "undefined") {
        return config;
    }

    // Get session with token
    const session = await getSession();

    if (session?.accessToken) {
        config.headers.Authorization = `Bearer ${session.accessToken}`;
    }

    return config;
});

export default apiClient;
