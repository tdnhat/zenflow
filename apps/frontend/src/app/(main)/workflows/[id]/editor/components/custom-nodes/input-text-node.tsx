import { memo, useState, useEffect } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";
import { useWorkflowStore } from "@/store/workflow.store";

type NodeData = {
    label?: string;
    nodeKind?: string;
    nodeType?: string;
    configJson?: string;
    selector?: string;
    text?: string;
    clear?: boolean;
};

export const InputTextNode = memo(({ id, data }: NodeProps) => {
    const updateNodeData = useWorkflowStore(state => state.updateNodeData);
    const nodeData = data as NodeData;
    
    // Parse existing configJson if available
    const existingConfig = nodeData.configJson ? JSON.parse(nodeData.configJson) : {};
    
    // Initialize with default values, prioritizing parsed values from configJson
    const [label, setLabel] = useState(nodeData.label || "Input Text");
    const [selector, setSelector] = useState(existingConfig.selector || nodeData.selector || "");
    const [text, setText] = useState(existingConfig.text || nodeData.text || "");
    const [clear, setClear] = useState(
        existingConfig.clear !== undefined ? existingConfig.clear :
        nodeData.clear !== undefined ? nodeData.clear : true
    );

    // Initialize with proper node type and kind if not already set
    useEffect(() => {
        if (!nodeData.nodeType || !nodeData.nodeKind) {
            updateNodeData(id, {
                nodeType: "InputTextActivity",
                nodeKind: "BROWSER_AUTOMATION",
                label: label
            });
        }
    }, [id, nodeData.nodeType, nodeData.nodeKind, label, updateNodeData]);

    // Sync state changes back to the store
    useEffect(() => {
        // Only store the configuration in configJson, not other node properties
        const newConfigJson = JSON.stringify({
            selector,
            text,
            clear
        });
        
        updateNodeData(id, {
            label,
            nodeType: "InputTextActivity",
            nodeKind: "BROWSER_AUTOMATION",
            configJson: newConfigJson,
            // Remove properties from direct node data to avoid duplication
            selector: undefined,
            text: undefined,
            clear: undefined
        });
    }, [id, label, selector, text, clear, updateNodeData]);

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
                        ⌨️ {label}
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
                        <label htmlFor="selector" className="block text-sm font-medium mb-1">
                            CSS Selector:
                        </label>
                        <input
                            id="selector"
                            name="selector"
                            type="text"
                            value={selector}
                            onChange={(e) => setSelector(e.target.value)}
                            placeholder="input, textarea, [contenteditable]"
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="CSS Selector input"
                        />
                    </div>
                    
                    <div className="mt-2">
                        <label htmlFor="text" className="block text-sm font-medium mb-1">
                            Text to Input:
                        </label>
                        <textarea
                            id="text"
                            name="text"
                            value={text}
                            onChange={(e) => setText(e.target.value)}
                            placeholder="Enter text to type"
                            rows={3}
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800 resize-none"
                            aria-label="Text input"
                        />
                    </div>
                    
                    <div className="mt-2 flex items-center">
                        <input
                            id="clear"
                            name="clear"
                            type="checkbox"
                            checked={clear}
                            onChange={(e) => setClear(e.target.checked)}
                            className="nodrag h-4 w-4 border-gray-300 rounded text-indigo-500 focus:ring-indigo-500"
                            aria-label="Clear checkbox"
                        />
                        <label htmlFor="clear" className="ml-2 block text-sm">
                            Clear field before typing
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

InputTextNode.displayName = "InputTextNode";