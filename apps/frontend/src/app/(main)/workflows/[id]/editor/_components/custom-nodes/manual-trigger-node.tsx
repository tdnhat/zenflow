import { memo, useEffect, useState } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";
import { useWorkflowStore, nodeTypeTransformers } from "@/store/workflow.store";

type ManualTriggerNodeData = {
    label?: string;
    nodeKind?: string;
    nodeType?: string;
    configJson?: string;
};

export const ManualTriggerNode = memo(({ id, data }: NodeProps) => {
    const updateNodeData = useWorkflowStore((state) => state.updateNodeData);
    const nodeData = data as ManualTriggerNodeData;

    // Initialize with default values
    const [label, setLabel] = useState(nodeData.label || "Manual Trigger");

    // Initialize with proper node type and kind if not already set
    useEffect(() => {
        if (!nodeData.nodeType || !nodeData.nodeKind) {
            updateNodeData(id, {
                // Use the backend node type
                nodeType: nodeTypeTransformers.toBackend("manual-trigger"),
                nodeKind: "TRIGGER",
                label: label,
            });
        }
    }, [id, nodeData.nodeType, nodeData.nodeKind, label, updateNodeData]);

    // Handler for label change
    const handleLabelChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const newLabel = e.target.value;
        setLabel(newLabel);
    };

    // Sync state changes back to the store
    useEffect(() => {
        updateNodeData(id, {
            label,
            // Keep the frontend node type in the type property
            nodeType: nodeTypeTransformers.toBackend("manual-trigger"),
            nodeKind: "TRIGGER",
        });
    }, [id, label, updateNodeData]);

    return (
        <>
            <div className="p-4 rounded-md border-2 border-primary/50 bg-background shadow-md min-w-[200px]">
                <div className="flex flex-col gap-2">
                    <div className="font-medium text-sm text-primary">
                        {label}
                    </div>

                    <div className="mt-2">
                        <label
                            htmlFor={`node-${id}-label`}
                            className="block text-sm font-medium mb-1"
                        >
                            Label:
                        </label>
                        <input
                            id={`node-${id}-label`}
                            name={`node-${id}-label`}
                            type="text"
                            value={label}
                            onChange={handleLabelChange}
                            placeholder="Node Label"
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="Node label input"
                        />
                    </div>
                </div>
            </div>
            <Handle type="source" position={Position.Right} id="output" />
        </>
    );
});

ManualTriggerNode.displayName = "ManualTriggerNode";
