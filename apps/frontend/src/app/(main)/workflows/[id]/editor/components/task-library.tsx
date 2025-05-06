"use client";

import { ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import TaskNode from "./task-node";

// Task definitions - these could come from an API
const TASK_CATEGORIES = [
    {
        name: "Triggers",
        tasks: [
            {
                title: "Webhook",
                description: "Trigger on HTTP request",
                type: "webhook-trigger",
            },
            {
                title: "Schedule",
                description: "Run at scheduled times",
                type: "schedule-trigger",
            },
        ],
    },
    {
        name: "Actions",
        tasks: [
            {
                title: "HTTP Request",
                description: "Make HTTP requests",
                type: "http-action",
            },
            {
                title: "Database Query",
                description: "Execute database operations",
                type: "database-action",
            },
            {
                title: "Email",
                description: "Send email notifications",
                type: "email-action",
            },
        ],
    },
    {
        name: "Conditions",
        tasks: [
            {
                title: "If/Else",
                description: "Branch based on conditions",
                type: "if-condition",
            },
            {
                title: "Switch",
                description: "Multiple branching",
                type: "switch-condition",
            },
        ],
    },
];

interface TaskLibraryProps {
    onClose: () => void;
    isOpen: boolean;
}

export default function TaskLibrary({ onClose, isOpen }: TaskLibraryProps) {
    if (!isOpen) return null;

    return (
        <div className="flex flex-col h-full">
            <div className="p-4 border-b border-border flex justify-between items-center">
                <h3 className="font-medium">Task Library</h3>
                <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8"
                    onClick={onClose}
                >
                    <ChevronRight className="h-4 w-4" />
                </Button>
            </div>

            <div className="p-4 overflow-y-auto flex-1">
                <div className="space-y-6">
                    {TASK_CATEGORIES.map((category) => (
                        <div key={category.name} className="space-y-3">
                            <div className="text-sm font-medium text-muted-foreground">
                                {category.name}
                            </div>

                            <div className="space-y-2">
                                {category.tasks.map((task) => (
                                    <TaskNode
                                        key={task.type}
                                        title={task.title}
                                        description={task.description}
                                        type={task.type}
                                    />
                                ))}
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}
