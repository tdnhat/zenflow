import { memo, useState } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";

type NodeData = {
    label: string;
    description?: string;
    filename?: string;
    fullPage?: boolean;
};

export const ScreenshotNode = memo(({ data }: NodeProps) => {
    const nodeData = data as NodeData;
    const [filename, setFilename] = useState(nodeData.filename || "screenshot");
    const [fullPage, setFullPage] = useState(nodeData.fullPage !== undefined ? nodeData.fullPage : false);

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
                        📷 Take Screenshot
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
                className="w-3 h-3 bg-indigo-500 border-2 border-white dark:border-gray-800"
            />
        </>
    );
});

ScreenshotNode.displayName = "ScreenshotNode"; 