import { memo, useState, useEffect } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";
import { useWorkflowStore } from "@/store/workflow.store";

type NodeData = {
    label?: string;
    nodeKind?: string;
    nodeType?: string;
    configJson?: string;
    url?: string;
    timeout?: number;
};

export const NavigateNode = memo(({ id, data }: NodeProps) => {
    const updateNodeData = useWorkflowStore(state => state.updateNodeData);
    const nodeData = data as NodeData;
    
    // Parse existing configJson if available
    const existingConfig = nodeData.configJson ? JSON.parse(nodeData.configJson) : {};
    
    // Initialize with default values, prioritizing parsed values from configJson
    const [label, setLabel] = useState(nodeData.label || "Navigate");
    const [url, setUrl] = useState(existingConfig.url || nodeData.url || "https://example.com");
    const [timeout, setTimeout] = useState(existingConfig.timeout || nodeData.timeout || 30000);
    
    // Always use 'networkidle' for waitUntil
    const waitUntil = "networkidle";

    // Initialize with proper node type and kind if not already set
    useEffect(() => {
        if (!nodeData.nodeType || !nodeData.nodeKind) {
            updateNodeData(id, {
                nodeType: "NavigateActivity",
                nodeKind: "BROWSER_AUTOMATION",
                label: label
            });
        }
    }, [id, nodeData.nodeType, nodeData.nodeKind, label, updateNodeData]);

    // Sync state changes back to the store
    useEffect(() => {
        // Only store the configuration in configJson, not other node properties
        const newConfigJson = JSON.stringify({
            url,
            timeout,
            waitUntil
        });
        
        updateNodeData(id, {
            label,
            nodeType: "NavigateActivity",
            nodeKind: "BROWSER_AUTOMATION",
            configJson: newConfigJson,
            // Remove url and timeout from direct node data to avoid duplication
            url: undefined,
            timeout: undefined
        });
    }, [id, label, url, timeout, updateNodeData]);

    return (
        <>
            <Handle
                type="target"
                position={Position.Left}
                id="input"
            />
            <div className="p-4 rounded-md border-2 border-primary/50 bg-background shadow-md min-w-[250px]">
                <div className="flex flex-col gap-2">
                    <div className="font-medium text-sm text-primary">
                        {label}
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
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="Node label input"
                        />
                    </div>
                    
                    <div className="mt-2">
                        <label htmlFor="url" className="block text-sm font-medium mb-1">
                            URL:
                        </label>
                        <input
                            id="url"
                            name="url"
                            type="text"
                            value={url}
                            onChange={(e) => setUrl(e.target.value)}
                            placeholder="https://example.com"
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="URL input"
                        />
                    </div>
                    
                    <div className="mt-2">
                        <label htmlFor="timeout" className="block text-sm font-medium mb-1">
                            Timeout (ms):
                        </label>
                        <input
                            id="timeout"
                            name="timeout"
                            type="number"
                            value={timeout}
                            onChange={(e) => setTimeout(Number(e.target.value))}
                            placeholder="30000"
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="Timeout input"
                        />
                    </div>
                </div>
            </div>
            <Handle
                type="source"
                position={Position.Right}
                id="output"
            />
        </>
    );
});

NavigateNode.displayName = "NavigateNode";