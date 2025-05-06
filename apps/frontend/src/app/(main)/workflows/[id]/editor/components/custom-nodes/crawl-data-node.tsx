import { memo, useState } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";

type NodeData = {
    label: string;
    description?: string;
    selector?: string;
    fields?: string;
};

export const CrawlDataNode = memo(({ data }: NodeProps) => {
    const nodeData = data as NodeData;
    const [selector, setSelector] = useState(nodeData.selector || "");
    const defaultFields = `[
  {
    "name": "title",
    "selector": "h1",
    "attribute": "innerText"
  }
]`;
    const [fields, setFields] = useState(nodeData.fields || defaultFields);

    return (
        <>
            <Handle
                type="target"
                position={Position.Top}
                id="input"
                className="w-3 h-3 bg-gray-400 border-2 border-white dark:border-gray-800"
            />
            <div className="p-4 rounded-md border-2 border-indigo-500 bg-white dark:bg-background shadow-md min-w-[300px]">
                <div className="flex flex-col gap-2">
                    <div className="font-medium text-sm text-indigo-500">
                        🕸️ Crawl Data
                    </div>
                    
                    <div className="mt-2">
                        <label htmlFor="selector" className="block text-sm font-medium mb-1">
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
                        <label htmlFor="fields" className="block text-sm font-medium mb-1">
                            Fields to Extract (JSON):
                        </label>
                        <textarea
                            id="fields"
                            name="fields"
                            value={fields}
                            onChange={(e) => setFields(e.target.value)}
                            rows={6}
                            className="nodrag w-full px-3 py-2 border border-gray-300 dark:border-gray-700 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent bg-white dark:bg-gray-800 font-mono text-sm"
                            aria-label="Fields JSON input"
                        />
                    </div>
                    
                    <div className="mt-2 text-xs text-gray-500">
                        <p>Format: Array of objects with name, selector, and attribute properties</p>
                        <p className="mt-1">Example: Extract title from H1 tags</p>
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

CrawlDataNode.displayName = "CrawlDataNode"; 