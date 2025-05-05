import axios from "axios";
import { getSession } from "next-auth/react";
import { toast } from "react-hot-toast";
const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5050/api",
    headers: {
        "Content-Type": "application/json",
    },
});

// Request interceptor for adding auth token
api.interceptors.request.use(
    async (config) => {
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
    },
    (error) => Promise.reject(error)
);

// Response interceptor for handling errors globally
api.interceptors.response.use(
    (response) => response,
    (error) => {
        // Handle specific error codes
        if (error.response) {
            const { status } = error.response;

            if (status === 401) {
                window.location.href = "/signin";
            }

            if (status === 403) {
                toast.error("You are not authorized to access this resource.");
            }

            if (status === 500) {
                toast.error(
                    "An unexpected error occurred. Please try again later."
                );
            }
        }

        return Promise.reject(error);
    }
);

export default api;
