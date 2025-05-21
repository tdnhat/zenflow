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
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Globe, ArrowUpRight } from "lucide-react";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import { useWorkflowStore } from "@/store/workflow.store";

interface HttpRequestNodeData {
    label: string;
    nodeType?: string;
    activityProperties: {
        url: string;
        method: string;
        headers?: Record<string, string>;
        body?: string;
    };
}

export default function HttpRequestNode({ id, data, selected }: NodeProps) {
    const httpData = data as unknown as HttpRequestNodeData;
    const { setNodes } = useReactFlow();
    const setNodeInputActive = useWorkflowStore((state) => state.setNodeInputActive);

    // Ensure activityProperties is initialized for the current data
    const currentActivityProperties = httpData.activityProperties || { url: "", method: "GET" };

    const handleUrlChange = useCallback(
        (evt: React.ChangeEvent<HTMLInputElement>) => {
            const newUrl = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as unknown as HttpRequestNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...(nodeData.activityProperties || { url: "", method: "GET" }),
                                    url: newUrl,
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

    const handleMethodChange = useCallback(
        (value: string) => {
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as unknown as HttpRequestNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...(nodeData.activityProperties || { url: "", method: "GET" }),
                                    method: value,
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

    const getMethodColor = (method: string) => {
        const colors = {
            GET: "bg-emerald-500 dark:bg-emerald-600",
            POST: "bg-blue-500 dark:bg-blue-600",
            PUT: "bg-amber-500 dark:bg-amber-600",
            DELETE: "bg-rose-500 dark:bg-rose-600",
            PATCH: "bg-purple-500 dark:bg-purple-600",
        };
        return colors[method as keyof typeof colors] || "bg-gray-500";
    };

    // Enhanced node selection styles
    const nodeClasses = selected
        ? "ring-2 ring-primary border-primary shadow-lg"
        : "border-border shadow-sm hover:shadow-md";

    const methodColor = getMethodColor(currentActivityProperties.method);

    return (
        <div className="w-[280px] transition-all duration-200">
            <Card
                className={cn(
                    nodeClasses,
                    "border p-0 rounded-xl overflow-hidden transition-all duration-300 bg-card/80 backdrop-blur-sm"
                )}
            >
                <CardHeader className="p-3 pb-2 bg-gradient-to-r from-emerald-600 to-cyan-600 dark:from-emerald-800 dark:to-cyan-900 text-white">
                    <CardTitle className="text-md flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <Globe size={18} className="text-white/90" />
                            <span className="font-medium">
                                {httpData.label || "HTTP Request"}
                            </span>
                        </div>
                        <Badge
                            className={cn(
                                "text-[10px] text-white border-none",
                                methodColor
                            )}
                        >
                            {currentActivityProperties.method}
                        </Badge>
                    </CardTitle>
                </CardHeader>

                <CardContent className="p-4 space-y-4">
                    <div className="space-y-2">
                        <Label
                            htmlFor={`method-${id}`}
                            className="text-xs font-medium flex items-center gap-1.5"
                        >
                            <span className="w-2 h-2 rounded-full bg-cyan-500 dark:bg-cyan-400"></span>
                            Method
                        </Label>
                        <Select
                            value={currentActivityProperties.method}
                            onValueChange={handleMethodChange}
                        >
                            <SelectTrigger
                                onFocus={() => setNodeInputActive(true)}
                                onBlur={() => setNodeInputActive(false)}
                                id={`method-${id}`}
                                className="h-9 text-sm bg-background border-input/50 focus:ring-1 focus:ring-cyan-500 dark:focus:ring-cyan-400"
                            >
                                <SelectValue placeholder="Select Method" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem
                                    value="GET"
                                    className="text-emerald-600 dark:text-emerald-400 font-medium"
                                >
                                    GET
                                </SelectItem>
                                <SelectItem
                                    value="POST"
                                    className="text-blue-600 dark:text-blue-400 font-medium"
                                >
                                    POST
                                </SelectItem>
                                <SelectItem
                                    value="PUT"
                                    className="text-amber-600 dark:text-amber-400 font-medium"
                                >
                                    PUT
                                </SelectItem>
                                <SelectItem
                                    value="DELETE"
                                    className="text-rose-600 dark:text-rose-400 font-medium"
                                >
                                    DELETE
                                </SelectItem>
                                <SelectItem
                                    value="PATCH"
                                    className="text-purple-600 dark:text-purple-400 font-medium"
                                >
                                    PATCH
                                </SelectItem>
                            </SelectContent>
                        </Select>
                    </div>

                    <div className="space-y-2">
                        <Label
                            htmlFor={`url-${id}`}
                            className="text-xs font-medium flex items-center gap-1.5"
                        >
                            <span className="w-2 h-2 rounded-full bg-emerald-500 dark:bg-emerald-400"></span>
                            URL
                        </Label>
                        <div className="relative">
                            <Input
                                id={`url-${id}`}
                                value={currentActivityProperties?.url || ""}
                                onChange={handleUrlChange}
                                onFocus={() => setNodeInputActive(true)}
                                onBlur={() => setNodeInputActive(false)}
                                className="h-9 text-sm pl-3 pr-8 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-cyan-500 dark:focus-visible:ring-cyan-400"
                                placeholder="https://api.example.com/endpoint"
                            />
                            <ArrowUpRight className="absolute right-2.5 top-2.5 h-4 w-4 text-muted-foreground/70" />
                        </div>
                    </div>

                    <div className="bg-muted/50 dark:bg-muted/20 p-2.5 rounded-lg border border-border/50">
                        <div className="flex items-center gap-2">
                            <Badge
                                variant="outline"
                                className={cn(
                                    "text-[10px] border-emerald-200 dark:border-emerald-800 bg-emerald-100/50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400"
                                )}
                            >
                                API
                            </Badge>
                            <p className="text-xs text-muted-foreground flex-1">
                                Makes {currentActivityProperties.method}{" "}
                                request to the specified endpoint
                            </p>
                        </div>
                    </div>
                </CardContent>

                <CardFooter className="p-2 pt-0 text-xs text-muted-foreground bg-muted/20 dark:bg-muted/10 flex justify-between items-center">
                    <span className="text-[10px] uppercase tracking-wide font-medium text-muted-foreground/70">
                        HTTP Request
                    </span>
                    <div className="flex items-center gap-1">
                        <div className="size-2 rounded-full bg-emerald-500 dark:bg-emerald-400 animate-pulse"></div>
                        <span className="text-[10px]">Ready</span>
                    </div>
                </CardFooter>
            </Card>

            {/* Input handle with improved styling */}
            <Handle
                type="target"
                position={Position.Left}
                id="in"
                className="w-3 h-3 -ml-0.5 rounded-full bg-cyan-500 dark:bg-cyan-400 border-2 border-background dark:border-card"
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
                className="w-3 h-3 -mr-0.5 rounded-full bg-emerald-500 dark:bg-emerald-400 border-2 border-background dark:border-card"
                style={{
                    top: "50%",
                    transform: "translateY(-50%) translateX(5px)",
                }}
            />
        </div>
    );
}
