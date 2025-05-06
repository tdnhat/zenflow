"use client";

import { ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import TaskNode from "./task-node";
import { useNodeTypes } from "../../../_hooks/use-workflows";

interface TaskLibraryProps {
    onClose: () => void;
    isOpen: boolean;
}

export default function TaskLibrary({ onClose, isOpen }: TaskLibraryProps) {
    const { data: nodeTypes, isLoading, error } = useNodeTypes();
    if (!isOpen) return null;

    if (isLoading) return <div>Loading...</div>;
    if (error) return <div>Error: {error.message}</div>;

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
                    {nodeTypes?.map((category) => (
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
