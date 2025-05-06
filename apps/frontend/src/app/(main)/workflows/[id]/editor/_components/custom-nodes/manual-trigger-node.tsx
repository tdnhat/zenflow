import { memo, useEffect, useState } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";
import { useWorkflowStore } from "@/store/workflow.store";

type NodeData = {
    label?: string;
    nodeKind?: string;
    nodeType?: string;
    configJson?: string;
};

export const ManualTriggerNode = memo(({ id, data }: NodeProps) => {
    const updateNodeData = useWorkflowStore((state) => state.updateNodeData);
    const nodeData = data as NodeData;

    // Initialize with default values
    const [label, setLabel] = useState(nodeData.label || "Manual Trigger");

    // Initialize with proper node type and kind if not already set
    useEffect(() => {
        if (!nodeData.nodeType || !nodeData.nodeKind) {
            updateNodeData(id, {
                nodeType: "ManualTriggerActivity",
                nodeKind: "TRIGGER",
                label: label,
            });
        }
    }, [id, nodeData.nodeType, nodeData.nodeKind, label, updateNodeData]);

    // Sync state changes back to the store
    useEffect(() => {
        updateNodeData(id, {
            label,
            nodeType: "manual-trigger",
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
                            htmlFor="node-label"
                            className="block text-sm font-medium mb-1"
                        >
                            Label:
                        </label>
                        <input
                            id="node-label"
                            name="node-label"
                            type="text"
                            value={label}
                            onChange={(e) => setLabel(e.target.value)}
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
