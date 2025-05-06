import { memo } from "react";
import { Handle, Position, NodeProps } from "@xyflow/react";

type NodeData = {
    label: string;
    description?: string;
    triggerName?: string;
};

export const ManualTriggerNode = memo(({ data }: NodeProps) => {
    const nodeData = data as NodeData;

    return (
        <>
            <div className="p-4 rounded-md border-2 border-indigo-500 bg-white dark:bg-background shadow-md min-w-[200px]">
                <div className="flex flex-col gap-2">
                    <div className="font-medium text-sm text-indigo-500">
                        Manual Trigger
                    </div>
                    {nodeData.description && (
                        <p className="text-sm text-muted-foreground">
                            {nodeData.description}
                        </p>
                    )}
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

ManualTriggerNode.displayName = "ManualTriggerNode";
