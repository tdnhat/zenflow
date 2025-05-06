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
    const updateNodeData = useWorkflowStore(state => state.updateNodeData);
    const nodeData = data as NodeData;
    
    // Parse existing configJson if available
    const existingConfig = nodeData.configJson ? JSON.parse(nodeData.configJson) : {};
    
    // Initialize with default values
    const [label, setLabel] = useState(nodeData.label || "Manual Trigger");

    // Initialize with proper node type and kind if not already set
    useEffect(() => {
        if (!nodeData.nodeType || !nodeData.nodeKind) {
            updateNodeData(id, {
                nodeType: "ManualTriggerActivity",
                nodeKind: "TRIGGER",
                label: label
            });
        }
    }, [id, nodeData.nodeType, nodeData.nodeKind, label, updateNodeData]);

    // Sync state changes back to the store
    useEffect(() => {
        // Only store the configuration in configJson, not other node properties
        const newConfigJson = JSON.stringify({
            // Manual trigger doesn't need any special configuration
        });
        
        updateNodeData(id, {
            label,
            nodeType: "ManualTriggerActivity",
            nodeKind: "TRIGGER",
            configJson: newConfigJson
        });
    }, [id, label, updateNodeData]);

    return (
        <>
            <div className="p-4 rounded-md border-2 border-indigo-500 bg-white dark:bg-background shadow-md min-w-[200px]">
                <div className="flex flex-col gap-2">
                    <div className="font-medium text-sm text-indigo-500">
                        ▶️ {label}
                    </div>
                    
                    <div className="mt-2">
                        <label htmlFor="node-label" className="block text-sm font-medium mb-1">
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
            <Handle
                type="source"
                position={Position.Bottom}
                id="output"
            />
        </>
    );
});

ManualTriggerNode.displayName = "ManualTriggerNode";
