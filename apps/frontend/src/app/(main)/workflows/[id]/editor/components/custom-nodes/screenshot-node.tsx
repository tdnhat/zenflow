import { memo, useState, useEffect } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";
import { useWorkflowStore } from "@/store/workflow.store";

type NodeData = {
    label?: string;
    nodeKind?: string;
    nodeType?: string;
    configJson?: string;
    filename?: string;
    fullPage?: boolean;
};

export const ScreenshotNode = memo(({ id, data }: NodeProps) => {
    const updateNodeData = useWorkflowStore(state => state.updateNodeData);
    const nodeData = data as NodeData;
    
    // Parse existing configJson if available
    const existingConfig = nodeData.configJson ? JSON.parse(nodeData.configJson) : {};
    
    // Initialize with default values, prioritizing parsed values from configJson
    const [label, setLabel] = useState(nodeData.label || "Take Screenshot");
    const [filename, setFilename] = useState(existingConfig.filename || nodeData.filename || "screenshot");
    const [fullPage, setFullPage] = useState(
        existingConfig.fullPage !== undefined ? existingConfig.fullPage :
        nodeData.fullPage !== undefined ? nodeData.fullPage : false
    );

    // Initialize with proper node type and kind if not already set
    useEffect(() => {
        if (!nodeData.nodeType || !nodeData.nodeKind) {
            updateNodeData(id, {
                nodeType: "ScreenshotActivity",
                nodeKind: "BROWSER_AUTOMATION",
                label: label
            });
        }
    }, [id, nodeData.nodeType, nodeData.nodeKind, label, updateNodeData]);

    // Sync state changes back to the store
    useEffect(() => {
        // Only store the configuration in configJson, not other node properties
        const newConfigJson = JSON.stringify({
            filename,
            fullPage
        });
        
        updateNodeData(id, {
            label,
            nodeType: "ScreenshotActivity",
            nodeKind: "BROWSER_AUTOMATION",
            configJson: newConfigJson,
            // Remove properties from direct node data to avoid duplication
            filename: undefined,
            fullPage: undefined
        });
    }, [id, label, filename, fullPage, updateNodeData]);

    return (
        <>
            <Handle
                type="target"
                position={Position.Top}
                id="input"
            />
            <div className="p-4 rounded-md border-2 border-indigo-500 bg-white dark:bg-background shadow-md min-w-[250px]">
                <div className="flex flex-col gap-2">
                    <div className="font-medium text-sm text-indigo-500">
                        📷 {label}
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
                    
                    <div className="mt-2">
                        <label htmlFor="filename" className="block text-sm font-medium mb-1">
                            Filename:
                        </label>
                        <input
                            id="filename"
                            name="filename"
                            type="text"
                            value={filename}
                            onChange={(e) => setFilename(e.target.value)}
                            placeholder="screenshot"
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="Filename input"
                        />
                    </div>
                    
                    <div className="mt-2 flex items-center">
                        <input
                            id="fullPage"
                            name="fullPage"
                            type="checkbox"
                            checked={fullPage}
                            onChange={(e) => setFullPage(e.target.checked)}
                            className="nodrag h-4 w-4 border-gray-300 rounded text-indigo-500 focus:ring-indigo-500"
                            aria-label="Full page checkbox"
                        />
                        <label htmlFor="fullPage" className="ml-2 block text-sm">
                            Capture full page
                        </label>
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

ScreenshotNode.displayName = "ScreenshotNode";