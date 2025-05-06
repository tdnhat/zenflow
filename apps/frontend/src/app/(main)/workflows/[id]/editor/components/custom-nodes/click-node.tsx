import { memo, useState } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";

type NodeData = {
    label: string;
    description?: string;
    selector?: string;
    timeout?: number;
};

export const ClickNode = memo(({ data }: NodeProps) => {
    const nodeData = data as NodeData;
    const [selector, setSelector] = useState(nodeData.selector || "");
    const [timeout, setTimeout] = useState(nodeData.timeout || 5000);

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
                        🖱️ Click Element
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
                            placeholder=".button, #id, [attribute]"
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800"
                            aria-label="CSS Selector input"
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
                            placeholder="5000"
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

ClickNode.displayName = "ClickNode"; 