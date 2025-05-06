import { memo, useState } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";

type NodeData = {
    label: string;
    description?: string;
    selector?: string;
    text?: string;
    clear?: boolean;
};

export const InputTextNode = memo(({ data }: NodeProps) => {
    const nodeData = data as NodeData;
    const [selector, setSelector] = useState(nodeData.selector || "");
    const [text, setText] = useState(nodeData.text || "");
    const [clear, setClear] = useState(nodeData.clear !== undefined ? nodeData.clear : true);

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
                        ⌨️ Input Text
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
                className="w-3 h-3 bg-indigo-500 border-2 border-white dark:border-gray-800"
            />
        </>
    );
});

InputTextNode.displayName = "InputTextNode"; 