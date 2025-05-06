import { memo, useState } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";

type NodeData = {
    label: string;
    description?: string;
    url?: string;
    timeout?: number;
};

export const NavigateNode = memo(({ data }: NodeProps) => {
    const nodeData = data as NodeData;
    const [url, setUrl] = useState(nodeData.url || "https://example.com");
    const [timeout, setTimeout] = useState(nodeData.timeout || 30000);

    return (
        <>
            <Handle
                type="target"
                position={Position.Top}
                id="input"
                className="w-3 h-3 bg-gray-400 border-2 border-white dark:border-gray-800"
            />
            <div className="p-4 rounded-md border-2 border-indigo-500 bg-white dark:bg-background shadow-md min-w-[250px]">
                <div className="flex flex-col gap-2">
                    <div className="font-medium text-sm text-indigo-500">
                        🌐 Navigate
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
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800"
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
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="Timeout input"
                        />
                    </div>
                </div>
            </div>
            <Handle
                type="source"
                position={Position.Bottom}
                id="output"
                className="w-3 h-3 bg-indigo-500 border-2 border-white dark:border-gray-800"
            />
        </>
    );
});

NavigateNode.displayName = "NavigateNode"; 