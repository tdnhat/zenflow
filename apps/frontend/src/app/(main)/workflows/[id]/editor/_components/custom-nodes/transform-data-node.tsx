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
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ShuffleIcon, CodeIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { useWorkflowStore } from "@/store/workflow.store";

// Define the node data interface
type TransformDataNodeData = {
    label: string;
    nodeType?: string;
    activityProperties: {
        transformationType: string;
        inputProperty: string;
        outputProperty: string;
        transformationExpression?: string;
        jsonPath?: string;
        regexPattern?: string;
        replaceValue?: string;
    };
};

export default function TransformDataNode({ id, data, selected }: NodeProps) {
    const transformData = data as unknown as TransformDataNodeData;
    const { setNodes } = useReactFlow();
    const setNodeInputActive = useWorkflowStore((state) => state.setNodeInputActive);

    // Ensure activityProperties is initialized for the current data
    const currentActivityProperties = transformData.activityProperties || {
        transformationType: "json",
        inputProperty: "",
        outputProperty: "",
        transformationExpression: "",
        jsonPath: "",
        regexPattern: "",
        replaceValue: ""
    };

    const handleTransformationTypeChange = useCallback(
        (value: string) => {
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as TransformDataNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...currentActivityProperties,
                                    transformationType: value,
                                },
                            },
                        };
                    }
                    return node;
                })
            );
        },
        [id, setNodes, currentActivityProperties]
    );

    const handleInputPropertyChange = useCallback(
        (evt: React.ChangeEvent<HTMLInputElement>) => {
            const newValue = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as TransformDataNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...currentActivityProperties,
                                    inputProperty: newValue,
                                },
                            },
                        };
                    }
                    return node;
                })
            );
        },
        [id, setNodes, currentActivityProperties]
    );

    const handleOutputPropertyChange = useCallback(
        (evt: React.ChangeEvent<HTMLInputElement>) => {
            const newValue = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as TransformDataNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...currentActivityProperties,
                                    outputProperty: newValue,
                                },
                            },
                        };
                    }
                    return node;
                })
            );
        },
        [id, setNodes, currentActivityProperties]
    );

    const handleTransformationExpressionChange = useCallback(
        (evt: React.ChangeEvent<HTMLTextAreaElement>) => {
            const newValue = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as TransformDataNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...currentActivityProperties,
                                    transformationExpression: newValue,
                                },
                            },
                        };
                    }
                    return node;
                })
            );
        },
        [id, setNodes, currentActivityProperties]
    );

    const handleJsonPathChange = useCallback(
        (evt: React.ChangeEvent<HTMLInputElement>) => {
            const newValue = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as TransformDataNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...currentActivityProperties,
                                    jsonPath: newValue,
                                },
                            },
                        };
                    }
                    return node;
                })
            );
        },
        [id, setNodes, currentActivityProperties]
    );

    const handleRegexPatternChange = useCallback(
        (evt: React.ChangeEvent<HTMLInputElement>) => {
            const newValue = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as TransformDataNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...currentActivityProperties,
                                    regexPattern: newValue,
                                },
                            },
                        };
                    }
                    return node;
                })
            );
        },
        [id, setNodes, currentActivityProperties]
    );

    const handleReplaceValueChange = useCallback(
        (evt: React.ChangeEvent<HTMLInputElement>) => {
            const newValue = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as TransformDataNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...currentActivityProperties,
                                    replaceValue: newValue,
                                },
                            },
                        };
                    }
                    return node;
                })
            );
        },
        [id, setNodes, currentActivityProperties]
    );

    // Use different selection styles for better visual feedback
    const nodeClasses = selected
        ? "ring-2 ring-primary border-primary shadow-lg"
        : "border-border shadow-sm hover:shadow-md";

    return (
        <div className="w-[320px] transition-all duration-200">
            <Card
                className={cn(
                    nodeClasses,
                    "border p-0 rounded-xl overflow-hidden transition-all duration-300 bg-card/80 backdrop-blur-sm"
                )}
            >
                <CardHeader className="p-3 pb-2 bg-gradient-to-r from-purple-600 to-pink-600 dark:from-purple-800 dark:to-pink-900 text-white">
                    <CardTitle className="text-md flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <ShuffleIcon size={18} className="text-white/90" />
                            <span className="font-medium">
                                {transformData.label || "Transform Data"}
                            </span>
                        </div>
                        <div className="bg-white/20 hover:bg-white/30 transition-colors rounded-full p-1">
                            <CodeIcon
                                size={14}
                                className="text-white/90"
                            />
                        </div>
                    </CardTitle>
                </CardHeader>
                <CardContent className="p-4 space-y-4">
                    <div className="space-y-2">
                        <Label
                            htmlFor={`transformation-type-${id}`}
                            className="text-xs font-medium flex items-center gap-1.5"
                        >
                            <span className="w-2 h-2 rounded-full bg-purple-500 dark:bg-purple-400"></span>
                            Transformation Type
                        </Label>
                        <Select
                            value={currentActivityProperties.transformationType}
                            onValueChange={handleTransformationTypeChange}
                        >
                            <SelectTrigger className="h-9 text-sm bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-purple-500 dark:focus-visible:ring-purple-400">
                                <SelectValue placeholder="Select transformation type" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="json">JSON Path Extraction</SelectItem>
                                <SelectItem value="regex">Regex Replace</SelectItem>
                                <SelectItem value="text">Text Transformation</SelectItem>
                                <SelectItem value="format">Format String</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>

                    <div className="space-y-2">
                        <Label
                            htmlFor={`input-property-${id}`}
                            className="text-xs font-medium flex items-center gap-1.5"
                        >
                            <span className="w-2 h-2 rounded-full bg-blue-500 dark:bg-blue-400"></span>
                            Input Property
                        </Label>
                        <Input
                            id={`input-property-${id}`}
                            value={currentActivityProperties.inputProperty || ""}
                            onChange={handleInputPropertyChange}
                            onFocus={() => setNodeInputActive(true)}
                            onBlur={() => setNodeInputActive(false)}
                            className="h-9 text-sm pl-3 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-purple-500 dark:focus-visible:ring-purple-400 transition-all"
                            placeholder="sourceData"
                        />
                    </div>

                    <div className="space-y-2">
                        <Label
                            htmlFor={`output-property-${id}`}
                            className="text-xs font-medium flex items-center gap-1.5"
                        >
                            <span className="w-2 h-2 rounded-full bg-pink-500 dark:bg-pink-400"></span>
                            Output Property
                        </Label>
                        <Input
                            id={`output-property-${id}`}
                            value={currentActivityProperties.outputProperty || ""}
                            onChange={handleOutputPropertyChange}
                            onFocus={() => setNodeInputActive(true)}
                            onBlur={() => setNodeInputActive(false)}
                            className="h-9 text-sm pl-3 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-purple-500 dark:focus-visible:ring-purple-400"
                            placeholder="transformedData"
                        />
                    </div>

                    {/* Conditional fields based on transformation type */}
                    {currentActivityProperties.transformationType === "json" && (
                        <div className="space-y-2">
                            <Label
                                htmlFor={`json-path-${id}`}
                                className="text-xs font-medium flex items-center gap-1.5"
                            >
                                <span className="w-2 h-2 rounded-full bg-green-500 dark:bg-green-400"></span>
                                JSON Path
                            </Label>
                            <Input
                                id={`json-path-${id}`}
                                value={currentActivityProperties.jsonPath || ""}
                                onChange={handleJsonPathChange}
                                onFocus={() => setNodeInputActive(true)}
                                onBlur={() => setNodeInputActive(false)}
                                className="h-9 text-sm pl-3 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-purple-500 dark:focus-visible:ring-purple-400"
                                placeholder="$.data.results[*].name"
                            />
                        </div>
                    )}

                    {currentActivityProperties.transformationType === "regex" && (
                        <>
                            <div className="space-y-2">
                                <Label
                                    htmlFor={`regex-pattern-${id}`}
                                    className="text-xs font-medium flex items-center gap-1.5"
                                >
                                    <span className="w-2 h-2 rounded-full bg-orange-500 dark:bg-orange-400"></span>
                                    Regex Pattern
                                </Label>
                                <Input
                                    id={`regex-pattern-${id}`}
                                    value={currentActivityProperties.regexPattern || ""}
                                    onChange={handleRegexPatternChange}
                                    onFocus={() => setNodeInputActive(true)}
                                    onBlur={() => setNodeInputActive(false)}
                                    className="h-9 text-sm pl-3 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-purple-500 dark:focus-visible:ring-purple-400"
                                    placeholder="\d+"
                                />
                            </div>
                            <div className="space-y-2">
                                <Label
                                    htmlFor={`replace-value-${id}`}
                                    className="text-xs font-medium flex items-center gap-1.5"
                                >
                                    <span className="w-2 h-2 rounded-full bg-red-500 dark:bg-red-400"></span>
                                    Replace Value
                                </Label>
                                <Input
                                    id={`replace-value-${id}`}
                                    value={currentActivityProperties.replaceValue || ""}
                                    onChange={handleReplaceValueChange}
                                    onFocus={() => setNodeInputActive(true)}
                                    onBlur={() => setNodeInputActive(false)}
                                    className="h-9 text-sm pl-3 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-purple-500 dark:focus-visible:ring-purple-400"
                                    placeholder="replacement text"
                                />
                            </div>
                        </>
                    )}

                    {(currentActivityProperties.transformationType === "text" || 
                      currentActivityProperties.transformationType === "format") && (
                        <div className="space-y-2">
                            <Label
                                htmlFor={`transformation-expression-${id}`}
                                className="text-xs font-medium flex items-center gap-1.5"
                            >
                                <span className="w-2 h-2 rounded-full bg-yellow-500 dark:bg-yellow-400"></span>
                                Transformation Expression
                            </Label>
                            <Textarea
                                id={`transformation-expression-${id}`}
                                value={currentActivityProperties.transformationExpression || ""}
                                onChange={handleTransformationExpressionChange}
                                onFocus={() => setNodeInputActive(true)}
                                onBlur={() => setNodeInputActive(false)}
                                className="text-sm bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-purple-500 dark:focus-visible:ring-purple-400 min-h-[60px]"
                                placeholder={currentActivityProperties.transformationType === "format" 
                                    ? "Hello {name}, your order #{orderId} is ready!"
                                    : "input.toUpperCase().trim()"}
                                rows={3}
                            />
                        </div>
                    )}

                    <div className="bg-muted/50 dark:bg-muted/20 p-2.5 rounded-lg border border-border/50">
                        <p className="text-xs text-muted-foreground">
                            {currentActivityProperties.transformationType === "json" && "Extract specific values from JSON data using JSONPath expressions"}
                            {currentActivityProperties.transformationType === "regex" && "Find and replace text patterns using regular expressions"}
                            {currentActivityProperties.transformationType === "text" && "Transform text using JavaScript expressions"}
                            {currentActivityProperties.transformationType === "format" && "Format text using template placeholders"}
                            {!currentActivityProperties.transformationType && "Transform and manipulate data structures"}
                        </p>
                    </div>
                </CardContent>
                <CardFooter className="p-2 pt-0 text-xs text-muted-foreground bg-muted/20 dark:bg-muted/10 flex justify-between items-center">
                    <span className="text-[10px] uppercase tracking-wide font-medium text-muted-foreground/70">
                        Data Processing
                    </span>
                    <span className="text-[10px] bg-purple-500/10 dark:bg-purple-400/20 text-purple-700 dark:text-purple-400 px-1.5 py-0.5 rounded-full font-medium">
                        Transform
                    </span>
                </CardFooter>
            </Card>

            {/* Input handle with improved styling */}
            <Handle
                type="target"
                position={Position.Left}
                id="in"
                className="w-3 h-3 -ml-0.5 rounded-full bg-purple-500 dark:bg-purple-400 border-2 border-background dark:border-card"
                style={{
                    top: "50%",
                    transform: "translateY(-50%)",
                }}
            />

            {/* Output handle with improved styling */}
            <Handle
                type="source"
                position={Position.Right}
                id="out"
                className="w-3 h-3 -mr-0.5 rounded-full bg-pink-500 dark:bg-pink-400 border-2 border-background dark:border-card"
                style={{
                    top: "50%",
                    transform: "translateY(-50%)",
                }}
            />
        </div>
    );
} 