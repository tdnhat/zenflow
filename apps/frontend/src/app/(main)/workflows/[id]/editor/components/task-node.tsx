"use client";

import { DragEvent } from "react";

interface TaskNodeProps {
    title: string;
    description: string;
    type: string;
}

export default function TaskNode({ title, description, type }: TaskNodeProps) {
    const onDragStart = (event: DragEvent<HTMLDivElement>) => {
        // Set the node type as drag data
        event.dataTransfer.setData("application/reactflow", type);
        event.dataTransfer.effectAllowed = "move";
    };

    return (
        <div
            className="border border-border rounded-md p-3 cursor-grab bg-background hover:border-primary/50 transition-colors"
            onDragStart={onDragStart}
            draggable
        >
            <h4 className="font-medium">{title}</h4>
            <p className="text-xs text-muted-foreground mt-1">{description}</p>
        </div>
    );
}
