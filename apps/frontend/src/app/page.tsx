"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ArrowRightIcon } from "lucide-react";
import Image from "next/image";

export default function Home() {
    return (
        <main className="bg-background text-foreground min-h-screen flex flex-col items-center justify-center py-12 sm:py-24 md:py-32 px-4 overflow-hidden">
            <div className="mx-auto flex max-w-[1200px] flex-col gap-12 pt-16 sm:gap-24">
                <div className="flex flex-col items-center gap-6 text-center sm:gap-12">
                    {/* Badge */}
                    <Badge variant="outline" className="animate-appear gap-2">
                        <span className="text-muted-foreground">
                            Workflow management reimagined
                        </span>
                        <Link
                            href="/features"
                            className="flex items-center gap-1"
                        >
                            Learn more
                            <ArrowRightIcon className="h-3 w-3" />
                        </Link>
                    </Badge>

                    {/* Title */}
                    <h1 className="relative z-10 inline-block animate-appear bg-gradient-to-r from-foreground to-muted-foreground bg-clip-text text-4xl font-semibold leading-tight text-transparent drop-shadow-2xl sm:text-6xl sm:leading-tight md:text-7xl md:leading-tight">
                        Welcome to ZenFlow
                    </h1>

                    {/* Description */}
                    <p className="text-md relative z-10 max-w-[550px] animate-appear font-medium text-muted-foreground opacity-0 delay-100 sm:text-xl">
                        Modern workflow management platform designed to
                        streamline your processes and boost productivity
                    </p>

                    {/* Actions */}
                    <div className="relative z-10 flex animate-appear justify-center gap-4 opacity-0 delay-300">
                        <Button size="lg" asChild>
                            <Link
                                href="/dashboard"
                                className="flex items-center gap-2"
                            >
                                Go to Dashboard
                            </Link>
                        </Button>
                        <Button variant="outline" size="lg" asChild>
                            <Link
                                href="/signup"
                                className="flex items-center gap-2"
                            >
                                Sign Up
                            </Link>
                        </Button>
                    </div>

                    {/* Image */}
                    <div className="relative mt-16 w-full max-w-[900px]">
                        <div className="relative z-20 rounded-lg border border-border/40 bg-background/80 shadow-xl overflow-hidden">
                            <Image
                                src="/dashboard-preview.png"
                                alt="ZenFlow Dashboard Preview"
                                width={1200}
                                height={675}
                                className="w-full h-auto"
                                priority
                                // Fallback if image is not available
                                onError={(e) => {
                                    const target = e.target as HTMLImageElement;
                                    target.src =
                                        "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='1200' height='675' viewBox='0 0 1200 675'%3E%3Crect width='1200' height='675' fill='%23f0f0f0'/%3E%3Ctext x='600' y='337.5' font-family='Arial' font-size='32' text-anchor='middle' fill='%23888888'%3EZenFlow Dashboard Preview%3C/text%3E%3C/svg%3E";
                                }}
                            />
                        </div>

                        {/* Glow effect */}
                        <div className="absolute -z-10 left-1/2 top-1/2 h-[400px] w-[80%] -translate-x-1/2 -translate-y-1/2 rounded-full bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-primary/20 via-primary/5 to-background blur-2xl"></div>
                    </div>
                </div>
            </div>

            {/* Features section preview */}
            <div className="w-full max-w-[1200px] mt-16 sm:mt-24 grid grid-cols-1 md:grid-cols-3 gap-6 px-4">
                {[
                    {
                        title: "Streamlined Workflows",
                        description:
                            "Automate repetitive tasks and create efficient processes",
                    },
                    {
                        title: "Team Collaboration",
                        description:
                            "Work together seamlessly with real-time updates and sharing",
                    },
                    {
                        title: "Insightful Analytics",
                        description:
                            "Make data-driven decisions with comprehensive reporting",
                    },
                ].map((feature, index) => (
                    <div
                        key={index}
                        className="p-6 rounded-lg border border-border bg-card text-card-foreground shadow-sm transition-all hover:shadow-md"
                    >
                        <h3 className="text-xl font-semibold mb-2">
                            {feature.title}
                        </h3>
                        <p className="text-muted-foreground">
                            {feature.description}
                        </p>
                    </div>
                ))}
            </div>
        </main>
    );
}
