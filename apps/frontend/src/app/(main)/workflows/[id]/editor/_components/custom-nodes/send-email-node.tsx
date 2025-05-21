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
import { Switch } from "@/components/ui/switch";
import { MailIcon, SendIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import { useWorkflowStore } from "@/store/workflow.store";

// Define the node data interface
type SendEmailNodeData = {
    label: string;
    nodeType?: string;
    activityProperties: {
        to: string;
        subject: string;
        body: string;
        isHtml: boolean;
    };
};

export default function SendEmailNode({ id, data, selected }: NodeProps) {
    const emailData = data as unknown as SendEmailNodeData;
    const { setNodes } = useReactFlow();
    const setNodeInputActive = useWorkflowStore((state) => state.setNodeInputActive);

    // Ensure activityProperties is initialized for the current data
    const currentActivityProperties = emailData.activityProperties || { to: "", subject: "", body: "", isHtml: false };

    const handleToChange = useCallback(
        (evt: React.ChangeEvent<HTMLInputElement>) => {
            const newValue = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as unknown as SendEmailNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...(nodeData.activityProperties || { to: "", subject: "", body: "", isHtml: false }),
                                    to: newValue,
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

    const handleSubjectChange = useCallback(
        (evt: React.ChangeEvent<HTMLInputElement>) => {
            const newValue = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as unknown as SendEmailNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...(nodeData.activityProperties || { to: "", subject: "", body: "", isHtml: false }),
                                    subject: newValue,
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

    const handleBodyChange = useCallback(
        (evt: React.ChangeEvent<HTMLTextAreaElement>) => {
            const newValue = evt.target.value;
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as unknown as SendEmailNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...(nodeData.activityProperties || { to: "", subject: "", body: "", isHtml: false }),
                                    body: newValue,
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

    const handleIsHtmlChange = useCallback(
        (checked: boolean) => {
            setNodes((nds) =>
                nds.map((node) => {
                    if (node.id === id) {
                        const nodeData = node.data as unknown as SendEmailNodeData;
                        return {
                            ...node,
                            data: {
                                ...nodeData,
                                activityProperties: {
                                    ...(nodeData.activityProperties || { to: "", subject: "", body: "", isHtml: false }),
                                    isHtml: checked,
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

    // Enhanced node selection styles
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
                <CardHeader className="p-3 pb-2 bg-gradient-to-r from-purple-600 to-violet-600 dark:from-purple-800 dark:to-violet-900 text-white">
                    <CardTitle className="text-md flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <MailIcon size={18} className="text-white/90" />
                            <span className="font-medium">
                                {emailData.label || "Send Email"}
                            </span>
                        </div>
                        <Badge
                            className={cn(
                                "text-[10px] text-white border-none",
                                currentActivityProperties.isHtml
                                    ? "bg-violet-500 dark:bg-violet-600"
                                    : "bg-purple-500 dark:bg-purple-600"
                            )}
                        >
                            {currentActivityProperties.isHtml
                                ? "HTML"
                                : "Text"}
                        </Badge>
                    </CardTitle>
                </CardHeader>

                <CardContent className="p-4 space-y-4">
                    <div className="space-y-2">
                        <Label
                            htmlFor={`to-${id}`}
                            className="text-xs font-medium flex items-center gap-1.5"
                        >
                            <span className="w-2 h-2 rounded-full bg-purple-500 dark:bg-purple-400"></span>
                            To
                        </Label>
                        <Input
                            id={`to-${id}`}
                            value={currentActivityProperties?.to || ""}
                            onChange={handleToChange}
                            onFocus={() => setNodeInputActive(true)}
                            onBlur={() => setNodeInputActive(false)}
                            className="h-9 text-sm pl-3 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-purple-500 dark:focus-visible:ring-purple-400"
                            placeholder="recipient@example.com"
                        />
                    </div>

                    <div className="space-y-2">
                        <Label
                            htmlFor={`subject-${id}`}
                            className="text-xs font-medium flex items-center gap-1.5"
                        >
                            <span className="w-2 h-2 rounded-full bg-violet-500 dark:bg-violet-400"></span>
                            Subject
                        </Label>
                        <Input
                            id={`subject-${id}`}
                            value={currentActivityProperties?.subject || ""}
                            onChange={handleSubjectChange}
                            onFocus={() => setNodeInputActive(true)}
                            onBlur={() => setNodeInputActive(false)}
                            className="h-9 text-sm pl-3 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-violet-500 dark:focus-visible:ring-violet-400"
                            placeholder="Email subject"
                        />
                    </div>

                    <div className="space-y-2">
                        <Label
                            htmlFor={`body-${id}`}
                            className="text-xs font-medium flex items-center gap-1.5"
                        >
                            <span className="w-2 h-2 rounded-full bg-purple-500 dark:bg-purple-400"></span>
                            Body
                        </Label>
                        <Textarea
                            id={`body-${id}`}
                            value={currentActivityProperties?.body || ""}
                            onChange={handleBodyChange}
                            onFocus={() => setNodeInputActive(true)}
                            onBlur={() => setNodeInputActive(false)}
                            className="min-h-[80px] text-sm pl-3 bg-background border-input/50 focus-visible:ring-1 focus-visible:ring-purple-500 dark:focus-visible:ring-purple-400"
                            placeholder="Email content"
                        />
                    </div>

                    <div className="flex items-center justify-between bg-muted/50 dark:bg-muted/20 p-2.5 rounded-lg border border-border/50">
                        <Label
                            htmlFor={`html-${id}`}
                            className="text-xs font-medium"
                        >
                            Use HTML Format
                        </Label>
                        <Switch
                            id={`html-${id}`}
                            checked={currentActivityProperties?.isHtml || false}
                            onCheckedChange={handleIsHtmlChange}
                            onFocus={() => setNodeInputActive(true)}
                            onBlur={() => setNodeInputActive(false)}
                            className="data-[state=checked]:bg-violet-500 dark:data-[state=checked]:bg-violet-600"
                        />
                    </div>
                </CardContent>

                <CardFooter className="p-2 pt-0 text-xs text-muted-foreground bg-muted/20 dark:bg-muted/10 flex justify-between items-center">
                    <span className="text-[10px] uppercase tracking-wide font-medium text-muted-foreground/70">
                        Email Communication
                    </span>
                    <div className="flex items-center gap-1">
                        <SendIcon
                            size={10}
                            className="text-purple-500 dark:text-purple-400"
                        />
                        <span className="text-[10px] bg-purple-500/10 dark:bg-purple-400/20 text-purple-700 dark:text-purple-400 px-1.5 py-0.5 rounded-full font-medium">
                            Messaging
                        </span>
                    </div>
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
                    transform: "translateY(-50%) translateX(-5px)",
                }}
            />

            {/* Output handle with improved styling */}
            <Handle
                type="source"
                position={Position.Right}
                id="out"
                className="w-3 h-3 -mr-0.5 rounded-full bg-violet-500 dark:bg-violet-400 border-2 border-background dark:border-card"
                style={{
                    top: "50%",
                    transform: "translateY(-50%) translateX(5px)",
                }}
            />
        </div>
    );
}
