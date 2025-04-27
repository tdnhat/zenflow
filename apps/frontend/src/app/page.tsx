import Link from "next/link";

export default function Home() {
    return (
        <main className="flex min-h-screen flex-col items-center justify-center p-24">
            <h1 className="text-4xl font-bold mb-8">Welcome to ZenFlow</h1>
            <p className="mb-8 text-xl">Modern workflow management platform</p>
            <Link
                href="/dashboard"
                className="px-6 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
            >
                Go to Dashboard
            </Link>
        </main>
    );
}
