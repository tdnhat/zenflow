import { useCallback } from "react";
import { Handle, Position, NodeProps, useReactFlow } from "@xyflow/react";
import {
    Card,
    CardContent,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Code2Icon, ExternalLinkIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { useWorkflowStore } from "@/store/workflow.store";

// Define the node data interface
type ExtractDataNodeData = {
    label: string;
    nodeType?: string;
    activityProperties: {
        targetUrl: string;
        elementSelector: string;
    };
};

export default function ExtractDataNode({ id, data, selected }: NodeProps) {
    const extractData = data as unknown as ExtractDataNodeData;
    const { setNodes } = useReactFlow();
    const setNodeInputActive = useWorkflowStore((state) => state.setNodeInputActive);

    // Ensure activityProperties is initialized for the current data
    const currentActivityProperties = extractData.activityProperties || { targetUrl: "", elementSelector: "" };

    const handleUrlChange = useCallback(
        (evt: React.ChangeEvent<HTMLInputElement>) => {
            const newUrl = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as ExtractDataNodeData; // Type assertion
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...(nodeData.activityProperties || { targetUrl: "", elementSelector: "" }),
                                    targetUrl: newUrl,
                                },
                            },
                        };
                    }
                    return node;
                })
            );
        },
        [id, setNodes]
    );

    const handleSelectorChange = useCallback(
        (evt: React.ChangeEvent<HTMLInputElement>) => {
            const newSelector = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as ExtractDataNodeData; // Type assertion
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...(nodeData.activityProperties || { targetUrl: "", elementSelector: "" }),
                                    elementSelector: newSelector,
                                },
                            },
                        };
                    }
                    return node;
                })
            );
        },
        [id, setNodes]
    );

    // Use different selection styles for better visual feedback
    const nodeClasses = selected
        ? "ring-2 ring-primary border-primary shadow-lg"
        : "border-border shadow-sm hover:shadow-md";

    return (
        <div className="w-[280px] transition-all duration-200">
            <Card
                className={cn(
                    nodeClasses,
                    "border p-0 rounded-xl overflow-hidden transition-all duration-300 bg-card/80 backdrop-blur-sm"
                )}
            >
                <CardHeader className="p-3 pb-2 bg-gradient-to-r from-blue-600 to-indigo-600 dark:from-blue-800 dark:to-indigo-900 text-white">
                    <CardTitle className="text-md flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <Code2Icon size={18} className="text-white/90" />
                            <span className="font-medium">
                                {extractData.label || "Extract Data"}
                            </span>
                        </div>
                        <div className="bg-white/20 hover:bg-white/30 transition-colors rounded-full p-1">
                            <ExternalLinkIcon
                                size={14}
                                className="text-white/90"
                            />
                        </div>
                    </CardTitle>
                </CardHeader>
                <CardContent className="p-4 space-y-4">
                    <div className="space-y-2">
                        <Label
                            htmlFor={`url-${id}`}
                            className="text-xs font-medium flex items-center gap-1.5"
                        >
                            <span className="w-2 h-2 rounded-full bg-blue-500 dark:bg-blue-400"></span>
                            Target URL
                        </Label>
                        <div className="relative">
                            <Input
                                id={`url-${id}`}
                                value={
                                    currentActivityProperties.targetUrl ||
                                    ""
                                }
                                onChange={handleUrlChange}
                                onFocus={() => setNodeInputActive(true)}
                                onBlur={() => setNodeInputActive(false)}
                                className="h-9 text-sm pl-3 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-blue-500 dark:focus-visible:ring-blue-400 transition-all"
                                placeholder="https://example.com"
                            />
                        </div>
                    </div>

                    <div className="space-y-2">
                        <Label
                            htmlFor={`selector-${id}`}
                            className="text-xs font-medium flex items-center gap-1.5"
                        >
                            <span className="w-2 h-2 rounded-full bg-indigo-500 dark:bg-indigo-400"></span>
                            CSS Selector
                        </Label>
                        <Input
                            id={`selector-${id}`}
                            value={
                                currentActivityProperties
                                    .elementSelector || ""
                            }
                            onChange={handleSelectorChange}
                            onFocus={() => setNodeInputActive(true)}
                            onBlur={() => setNodeInputActive(false)}
                            className="h-9 text-sm pl-3 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-blue-500 dark:focus-visible:ring-blue-400"
                            placeholder=".quote .text"
                        />
                    </div>

                    <div className="bg-muted/50 dark:bg-muted/20 p-2.5 rounded-lg border border-border/50">
                        <p className="text-xs text-muted-foreground">
                            Extracts content from elements matching the CSS
                            selector
                        </p>
                    </div>
                </CardContent>
                <CardFooter className="p-2 pt-0 text-xs text-muted-foreground bg-muted/20 dark:bg-muted/10 flex justify-between items-center">
                    <span className="text-[10px] uppercase tracking-wide font-medium text-muted-foreground/70">
                        Data Extraction
                    </span>
                    <span className="text-[10px] bg-blue-500/10 dark:bg-blue-400/20 text-blue-700 dark:text-blue-400 px-1.5 py-0.5 rounded-full font-medium">
                        Scraping
                    </span>
                </CardFooter>
            </Card>

            {/* Input handle with improved styling */}
            <Handle
                type="target"
                position={Position.Left}
                id="in"
                className="w-3 h-3 -ml-0.5 rounded-full bg-blue-500 dark:bg-blue-400 border-2 border-background dark:border-card"
                style={{
                    top: "50%",
                    transform: "translateY(-50%) translateX(-5px)",
                }}
            />

            {/* Output handle with improved styling */}
            <Handle
                type="source"
                position={Position.Right}
                id="out"
                className="w-3 h-3 -mr-0.5 rounded-full bg-indigo-500 dark:bg-indigo-400 border-2 border-background dark:border-card"
                style={{
                    top: "50%",
                    transform: "translateY(-50%) translateX(5px)",
                }}
            />
        </div>
    );
}
