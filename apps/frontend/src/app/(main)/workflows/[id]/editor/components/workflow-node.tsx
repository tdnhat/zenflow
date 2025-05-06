"use client";

import { Handle, NodeProps, Position } from "@xyflow/react";

// Component props will be of type NodeProps with our custom data
export default function WorkflowNode({ data, isConnectable }: NodeProps) {
    // Access the node data safely
    const label = (data?.label as string) || "Node";
    const description = data?.description as string;

    return (
        <div className="px-4 py-2 shadow-md rounded-lg bg-card border border-border min-w-[150px]">
            {/* Input handle */}
            <Handle
                type="target"
                position={Position.Top}
                isConnectable={isConnectable}
            />

            <div className="flex flex-col">
                <div className="flex justify-between items-center">
                    {/* Node title */}
                    <div className="font-bold">{label}</div>
                </div>

                {/* Node description if available */}
                {description && (
                    <div className="text-xs text-muted-foreground mt-1">
                        {description}
                    </div>
                )}
            </div>

            {/* Output handle */}
            <Handle
                type="source"
                position={Position.Bottom}
                isConnectable={isConnectable}
            />
        </div>
    );
}
