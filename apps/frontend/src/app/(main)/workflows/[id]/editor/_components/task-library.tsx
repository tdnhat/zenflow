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
                type: "ZenFlow.Triggers.ManualTrigger",
            },
            {
                title: "Webhook Trigger",
                description: "Starts a workflow via an HTTP call",
                type: "ZenFlow.Triggers.WebhookTrigger",
            },
            {
                title: "Scheduled Trigger",
                description: "Starts a workflow at a set time/interval",
                type: "ZenFlow.Triggers.ScheduledTrigger",
            },
        ],
    },
    {
        name: "API",
        tasks: [
            {
                title: "HTTP Request",
                description: "Make HTTP requests to external APIs",
                type: "ZenFlow.Activities.Http.HttpRequestActivity",
            },
        ],
    },
    {
        name: "Data Operations",
        tasks: [
            {
                title: "Extract Data",
                description: "Extract text from web pages using CSS selectors",
                type: "ZenFlow.Activities.Playwright.ExtractTextFromElementActivity",
            },
            {
                title: "Set Variable",
                description: "Store a value in a variable for later use",
                type: "ZenFlow.Activities.Variables.SetVariableActivity",
            },
            {
                title: "Transform Data",
                description:
                    "Modify data structures or values (e.g., JSON, text)",
                type: "ZenFlow.Activities.Data.TransformDataActivity",
            },
        ],
    },
    {
        name: "Control Flow",
        tasks: [
            {
                title: "Conditional (If/Else)",
                description: "Execute different branches based on a condition",
                type: "ZenFlow.Activities.ControlFlow.ConditionalActivity",
            },
            {
                title: "Delay",
                description: "Pause workflow execution for a specified time",
                type: "ZenFlow.Activities.ControlFlow.DelayActivity",
            },
            {
                title: "Loop (For Each)",
                description: "Iterate over a list of items",
                type: "ZenFlow.Activities.ControlFlow.LoopActivity",
            },
            // Future consideration:
            // {
            //     title: "Error Handler",
            //     description: "Catch and manage errors in the workflow",
            //     type: "error-handler"
            // }
        ],
    },
    {
        name: "Communication",
        tasks: [
            {
                title: "Send Email",
                description: "Send an email to one or more recipients",
                type: "ZenFlow.Activities.Email.SendEmailActivity",
            },
        ],
    },
    {
        name: "Browser Interaction", // Renamed for clarity
        tasks: [
            {
                title: "Navigate",
                description: "Navigate to a URL",
                type: "ZenFlow.Activities.Playwright.NavigateActivity",
            },
            {
                title: "Click Element", // Be specific
                description: "Click on an element on the page",
                type: "ZenFlow.Activities.Playwright.ClickElementActivity", // Renamed type for clarity
            },
            {
                title: "Input Text",
                description: "Type text into an input field",
                type: "ZenFlow.Activities.Playwright.InputTextActivity",
            },
            {
                title: "Wait for Selector",
                description: "Wait for an element to appear on the page",
                type: "ZenFlow.Activities.Playwright.WaitForSelectorActivity",
            },
            {
                title: "Screenshot",
                description: "Take a screenshot of the page",
                type: "ZenFlow.Activities.Playwright.ScreenshotActivity",
            },
        ],
    },
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
