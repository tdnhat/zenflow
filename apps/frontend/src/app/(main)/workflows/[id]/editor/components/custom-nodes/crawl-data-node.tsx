import { memo, useEffect, useState } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";
import { useWorkflowStore } from "@/store/workflow.store";

type NodeData = {
    label?: string;
    nodeKind?: string;
    nodeType?: string;
    configJson?: string;
    selector?: string;
    timeout?: number;
    extractAll?: boolean;
    propertyToExtract?: string;
    outputVariableName?: string;
    waitForNetworkIdle?: boolean;
    waitForDomContentLoaded?: boolean;
};

export const CrawlDataNode = memo(({ id, data }: NodeProps) => {
    const updateNodeData = useWorkflowStore((state) => state.updateNodeData);
    const nodeData = data as NodeData;
    
    // Parse existing configJson if available
    const existingConfig = nodeData.configJson ? JSON.parse(nodeData.configJson) : {};
    
    // Initialize with default values, prioritizing parsed values from configJson
    const [label, setLabel] = useState(nodeData.label || "Crawl Data");
    const [selector, setSelector] = useState(existingConfig.selector || nodeData.selector || "");
    const [timeout, setTimeout] = useState(existingConfig.timeout || nodeData.timeout || 30000);
    const [extractAll, setExtractAll] = useState(
        existingConfig.extractAll !== undefined ? existingConfig.extractAll :
        nodeData.extractAll !== undefined ? nodeData.extractAll : true
    );
    const [propertyToExtract, setPropertyToExtract] = useState(
        existingConfig.propertyToExtract || nodeData.propertyToExtract || "innerText"
    );
    const [outputVariableName, setOutputVariableName] = useState(
        existingConfig.outputVariableName || nodeData.outputVariableName || "extractedData"
    );
    const [waitForNetworkIdle, setWaitForNetworkIdle] = useState(
        existingConfig.waitForNetworkIdle !== undefined ? existingConfig.waitForNetworkIdle :
        nodeData.waitForNetworkIdle !== undefined ? nodeData.waitForNetworkIdle : true
    );
    const [waitForDomContentLoaded, setWaitForDomContentLoaded] = useState(
        existingConfig.waitForDomContentLoaded !== undefined ? existingConfig.waitForDomContentLoaded :
        nodeData.waitForDomContentLoaded !== undefined ? nodeData.waitForDomContentLoaded : true
    );

    // Initialize with proper node type and kind if not already set
    useEffect(() => {
        if (!nodeData.nodeType || !nodeData.nodeKind) {
            updateNodeData(id, {
                nodeType: "CrawlDataActivity",
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
            timeout,
            extractAll,
            propertyToExtract,
            outputVariableName,
            waitForNetworkIdle,
            waitForDomContentLoaded,
        });
        
        updateNodeData(id, {
            label,
            nodeType: "CrawlDataActivity",
            nodeKind: "BROWSER_AUTOMATION",
            configJson: newConfigJson
        });
    }, [
        id,
        label,
        selector,
        timeout,
        extractAll,
        propertyToExtract,
        outputVariableName,
        waitForNetworkIdle,
        waitForDomContentLoaded,
        updateNodeData,
    ]);

    return (
        <>
            <Handle type="target" position={Position.Top} id="input" />
            <div className="p-4 rounded-md border-2 border-indigo-500 bg-white dark:bg-background shadow-md min-w-[300px]">
                <div className="flex flex-col gap-2">
                    <div className="font-medium text-sm text-indigo-500">
                        🕸️ {label}
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

                    <div className="mt-2">
                        <label
                            htmlFor="selector"
                            className="block text-sm font-medium mb-1"
                        >
                            Root Selector:
                        </label>
                        <input
                            id="selector"
                            name="selector"
                            type="text"
                            value={selector}
                            onChange={(e) => setSelector(e.target.value)}
                            placeholder=".products li, table tr, .items"
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="Root selector input"
                        />
                    </div>
                    <div className="mt-2">
                        <label
                            htmlFor="timeout"
                            className="block text-sm font-medium mb-1"
                        >
                            Timeout (ms):
                        </label>
                        <input
                            id="timeout"
                            name="timeout"
                            type="number"
                            value={timeout}
                            onChange={(e) => setTimeout(Number(e.target.value))}
                            placeholder="30000"
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="Timeout input"
                        />
                    </div>
                    <div className="mt-2">
                        <label
                            htmlFor="propertyToExtract"
                            className="block text-sm font-medium mb-1"
                        >
                            Property to Extract:
                        </label>
                        <input
                            id="propertyToExtract"
                            name="propertyToExtract"
                            type="text"
                            value={propertyToExtract}
                            onChange={(e) =>
                                setPropertyToExtract(e.target.value)
                            }
                            placeholder="innerText"
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="Property to extract input"
                        />
                    </div>
                    <div className="mt-2">
                        <label
                            htmlFor="outputVariableName"
                            className="block text-sm font-medium mb-1"
                        >
                            Output Variable Name:
                        </label>
                        <input
                            id="outputVariableName"
                            name="outputVariableName"
                            type="text"
                            value={outputVariableName}
                            onChange={(e) =>
                                setOutputVariableName(e.target.value)
                            }
                            placeholder="extractedData"
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="Output variable name input"
                        />
                    </div>
                    <div className="mt-2 flex items-center">
                        <input
                            id="extractAll"
                            name="extractAll"
                            type="checkbox"
                            checked={extractAll}
                            onChange={(e) => setExtractAll(e.target.checked)}
                            className="nodrag h-4 w-4 border-gray-300 rounded text-indigo-500 focus:ring-indigo-500"
                            aria-label="Extract all checkbox"
                        />
                        <label
                            htmlFor="extractAll"
                            className="ml-2 block text-sm"
                        >
                            Extract all matches
                        </label>
                    </div>
                    <div className="mt-2 flex items-center">
                        <input
                            id="waitForNetworkIdle"
                            name="waitForNetworkIdle"
                            type="checkbox"
                            checked={waitForNetworkIdle}
                            onChange={(e) =>
                                setWaitForNetworkIdle(e.target.checked)
                            }
                            className="nodrag h-4 w-4 border-gray-300 rounded text-indigo-500 focus:ring-indigo-500"
                            aria-label="Wait for network idle checkbox"
                        />
                        <label
                            htmlFor="waitForNetworkIdle"
                            className="ml-2 block text-sm"
                        >
                            Wait for network idle
                        </label>
                    </div>
                    <div className="mt-2 flex items-center">
                        <input
                            id="waitForDomContentLoaded"
                            name="waitForDomContentLoaded"
                            type="checkbox"
                            checked={waitForDomContentLoaded}
                            onChange={(e) =>
                                setWaitForDomContentLoaded(e.target.checked)
                            }
                            className="nodrag h-4 w-4 border-gray-300 rounded text-indigo-500 focus:ring-indigo-500"
                            aria-label="Wait for DOMContentLoaded checkbox"
                        />
                        <label
                            htmlFor="waitForDomContentLoaded"
                            className="ml-2 block text-sm"
                        >
                            Wait for DOMContentLoaded
                        </label>
                    </div>
                </div>
            </div>
            <Handle type="source" position={Position.Bottom} id="output" />
        </>
    );
});

CrawlDataNode.displayName = "CrawlDataNode";
