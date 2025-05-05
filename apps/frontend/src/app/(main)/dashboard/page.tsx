"use client";

import { useEffect, useState } from "react";
import { useNextAuth } from "@/hooks/use-next-auth";
import { LogOut } from "lucide-react";
import { Button } from "@/components/ui/button";
import api from "@/lib/axios";
export default function Dashboard() {
    const { isAuthenticated, isLoading, login, logout } = useNextAuth();
    const [apiMessage, setApiMessage] = useState("");
    const [error, setError] = useState("");

    useEffect(() => {
        if (!isLoading && isAuthenticated) {
            api
                .post("/workflows/2a3e98bd-4ac3-4e43-b127-6cc21a74152c/run", {})
                .then((response) => {
                    setApiMessage(JSON.stringify(response.data));
                })
                .catch((err) => {
                    console.error("API Error:", err);
                    setError("Failed to validate workflow data");
                });
        }
    }, [isLoading, isAuthenticated]);

    const handleLogout = () => {
        logout();
    };

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
        <div className="space-y-6">
            <div className="flex flex-row gap-4">
                <h1 className="text-2xl font-bold">Dashboard</h1>
                <Button variant="outline" onClick={handleLogout}>
                    <LogOut />
                    Logout
                </Button>
            </div>

            <div className="bg-card p-6 rounded-lg shadow">
                <h2 className="text-xl font-bold mb-4">API Response</h2>
                {error ? (
                    <p className="text-red-500">{error}</p>
                ) : apiMessage ? (
                    <p className="break-words font-mono text-sm">
                        {apiMessage}
                    </p>
                ) : (
                    <p>Loading API data...</p>
                )}
            </div>
        </div>
    );
}
