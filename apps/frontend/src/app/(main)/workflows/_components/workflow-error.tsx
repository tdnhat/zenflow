"use client";

import { useState } from "react";
import { AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardFooter,
    CardHeader,
} from "@/components/ui/card";
import {
    AlertCircle,
    AlertTriangle,
    ArrowRight,
    RefreshCcw,
} from "lucide-react";
import { cn } from "@/lib/utils";

export interface WorkflowErrorProps {
    message?: string;
    errorCode?: string;
    errorType?: "critical" | "warning" | "info";
    retry?: () => void;
    suggestions?: string[];
}

export default function WorkflowError({
    message = "Failed to load workflows",
    errorCode,
    errorType = "critical",
    retry,
    suggestions = [],
}: WorkflowErrorProps) {
    const [isRetrying, setIsRetrying] = useState(false);

    const handleRetry = async () => {
        if (!retry) return;

        setIsRetrying(true);
        try {
            await retry();
        } finally {
            setIsRetrying(false);
        }
    };

    const getIcon = () => {
        switch (errorType) {
            case "critical":
                return <AlertCircle className="h-5 w-5" />;
            case "warning":
                return <AlertTriangle className="h-5 w-5" />;
            default:
                return <AlertCircle className="h-5 w-5" />;
        }
    };

    const getTitle = () => {
        switch (errorType) {
            case "critical":
                return "Error";
            case "warning":
                return "Warning";
            case "info":
                return "Information";
            default:
                return "Error";
        }
    };

    return (
        <div className="flex justify-center w-full mt-16">
            <Card
                className={cn(
                    "overflow-hidden border-l-4 p-0",
                    errorType === "critical"
                        ? "border-l-destructive"
                        : errorType === "warning"
                        ? "border-l-orange-500"
                        : "border-l-blue-500"
                )}
            >
                <CardHeader
                    className={cn(
                        "flex flex-row items-center gap-2 py-3",
                        errorType === "critical"
                            ? "bg-destructive/10"
                            : errorType === "warning"
                            ? "bg-orange-500/10"
                            : "bg-blue-500/10"
                    )}
                >
                    <div
                        className={cn(
                            "rounded-full p-1",
                            errorType === "critical"
                                ? "text-destructive"
                                : errorType === "warning"
                                ? "text-orange-500"
                                : "text-blue-500"
                        )}
                    >
                        {getIcon()}
                    </div>
                    <AlertTitle className="text-base font-medium">
                        {getTitle()}
                    </AlertTitle>
                    {errorCode && (
                        <code className="ml-auto rounded bg-muted px-2 py-1 text-xs">
                            Error: {errorCode}
                        </code>
                    )}
                </CardHeader>
                <CardContent className="pt-4">
                    <AlertDescription className="text-sm">
                        <p className="mb-2">{message}</p>
                        {suggestions.length > 0 && (
                            <ul className="mt-2 space-y-1 text-sm text-muted-foreground">
                                {suggestions.map((suggestion, index) => (
                                    <li
                                        key={index}
                                        className="flex items-center gap-1"
                                    >
                                        <ArrowRight className="h-3 w-3" />
                                        <span>{suggestion}</span>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </AlertDescription>
                </CardContent>
                {retry && (
                    <CardFooter className="flex justify-end gap-2 border-t bg-muted/20 py-2">
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={handleRetry}
                            disabled={isRetrying}
                            className="gap-1"
                        >
                            <RefreshCcw
                                className={cn(
                                    "h-3.5 w-3.5",
                                    isRetrying && "animate-spin"
                                )}
                            />
                            {isRetrying ? "Retrying..." : "Retry"}
                        </Button>
                        {errorType === "critical" && (
                            <Button
                                variant="default"
                                size="sm"
                                onClick={() => window.location.reload()}
                            >
                                Reload Page
                            </Button>
                        )}
                    </CardFooter>
                )}
            </Card>
        </div>
    );
}
