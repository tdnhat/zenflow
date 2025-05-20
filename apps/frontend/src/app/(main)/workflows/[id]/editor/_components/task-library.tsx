"use client";

import TaskNode from "./task-node";
// import { useNodeTypes } from "../../../_hooks/use-workflows";

interface TaskLibraryProps {
    isOpen: boolean;
}

// Static node types to replace the dynamic data from useNodeTypes
const staticNodeTypes = [
    {
        name: "Triggers",
        tasks: [
            {
                title: "Manual Trigger",
                description: "Starts a workflow manually",
                type: "manual-trigger"
            }
        ]
    },
    {
        name: "API",
        tasks: [
            {
                title: "HTTP Request",
                description: "Make HTTP requests to external APIs",
                type: "http-request"
            }
        ]
    },
    {
        name: "Data",
        tasks: [
            {
                title: "Extract Data",
                description: "Extract text from web pages using CSS selectors",
                type: "extract-data"
            }
        ]
    },
    {
        name: "Communication",
        tasks: [
            {
                title: "Send Email",
                description: "Send an email to one or more recipients",
                type: "send-email"
            }
        ]
    },
    {
        name: "Browser Automation",
        tasks: [
            {
                title: "Navigate",
                description: "Navigate to a URL",
                type: "navigate"
            },
            {
                title: "Click",
                description: "Click on an element",
                type: "click"
            },
            {
                title: "Input Text",
                description: "Type text into an input field",
                type: "input-text"
            },
            {
                title: "Wait for Selector",
                description: "Wait for an element to appear on the page",
                type: "wait-for-selector"
            },
            {
                title: "Screenshot",
                description: "Take a screenshot of the page",
                type: "screenshot"
            },
            {
                title: "Crawl Data",
                description: "Extract structured data from a website",
                type: "crawl-data"
            }
        ]
    }
];

export default function TaskLibrary({ isOpen }: TaskLibraryProps) {
    // const { data: nodeTypes, isLoading, error } = useNodeTypes();
    if (!isOpen) return null;
    
    // No need for loading/error states since we're using static data
    const nodeTypes = staticNodeTypes;

    return (
        <div className="flex flex-col h-full">
            <div className="h-16 p-4 border-b flex justify-between items-center">
                <h3 className="font-medium">Task Library</h3>
            </div>

            <div className="p-4 overflow-y-auto flex-1 pb-16">
                <div className="space-y-6">
                    {nodeTypes.map((category) => (
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
