"use client";

import { useEffect, useState } from "react";
import { useNextAuth } from "@/lib/auth/useNextAuth";
import apiClient from "@/lib/api/apiClient";

export default function Dashboard() {
    const { isAuthenticated, isLoading, user, login, logout } = useNextAuth();
    const [apiMessage, setApiMessage] = useState("");
    const [error, setError] = useState("");

    useEffect(() => {
        if (!isLoading && isAuthenticated) {
            // Test PUT /api/v1/users/{id} with only username field
            apiClient
                .delete("/users/03446ac5-3c17-4ebd-b2c2-fc5fcca0cec4", {
                    // username: "testUser",
                    // roles: ["user"],
                    // isActive: true,
                })
                .then((response) => {
                    setApiMessage(JSON.stringify(response.data));
                })
                .catch((err) => {
                    console.error("API Error:", err);
                    setError("Failed to delete user");
                });
        }
    }, [isLoading, isAuthenticated]);

    if (isLoading) {
        return <div>Loading...</div>;
    }

    if (!isAuthenticated) {
        return (
            <div className="flex min-h-screen flex-col items-center justify-center p-24">
                <h1 className="text-2xl font-bold mb-8">
                    Please login to access the dashboard
                </h1>
                <button
                    onClick={() => login()}
                    className="px-6 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
                >
                    Login
                </button>
            </div>
        );
    }

    return (
        <div className="flex min-h-screen flex-col p-8">
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-2xl font-bold">Dashboard</h1>
                <div className="flex items-center gap-4">
                    <span>Welcome, {user?.name || "User"}</span>
                    <button
                        onClick={() => logout()}
                        className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition"
                    >
                        Logout
                    </button>
                </div>
            </div>

            <div className="bg-white p-6 rounded-lg shadow-md">
                <h2 className="text-xl font-bold mb-4 text-black">API Response</h2>
                {error ? (
                    <p className="text-red-500">{error}</p>
                ) : apiMessage ? (
                    <p className="text-black">{apiMessage}</p>
                ) : (
                    <p>Loading API data...</p>
                )}
            </div>
        </div>
    );
}
