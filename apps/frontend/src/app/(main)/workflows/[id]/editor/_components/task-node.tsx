"use client";

import { DragEvent } from "react";
import { WorkflowNodeDto } from "@/types/workflow.type";

// Define the props type for TaskNode, using name for title and activityType for type
type TaskNodeProps = Pick<WorkflowNodeDto, "name" | "activityType"> & {
    description: string;
};

export default function TaskNode({
    name: title,
    description,
    activityType: type,
}: TaskNodeProps) {
    const onDragStart = (event: DragEvent<HTMLDivElement>) => {
        // Set the node type as drag data
        event.dataTransfer.setData("application/reactflow", type);
        event.dataTransfer.effectAllowed = "move";
    };

    return (
        <div
            className="bg-card border border-border shadow rounded-md p-3 cursor-grab hover:border-primary/50 transition-colors"
            onDragStart={onDragStart}
            draggable
        >
            <h4 className="font-medium">{title}</h4>
            <p className="text-xs text-muted-foreground mt-1">{description}</p>
        </div>
    );
}
