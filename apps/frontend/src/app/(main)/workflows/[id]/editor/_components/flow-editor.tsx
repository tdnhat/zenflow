import {
    Background,
    Controls,
    ReactFlow,
    ReactFlowProvider,
    Panel,
    MiniMap,
    BackgroundVariant,
    ConnectionLineType,
    ConnectionMode,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import "./flow-editor.css";
import { useRef, useCallback } from "react";
import { Button } from "@/components/ui/button";
import { Loader2, Save, Trash2 } from "lucide-react";
import { useParams } from "next/navigation";
import { useWorkflowStore } from "@/store/workflow.store";
import { useUpdateWorkflow } from "../../../_hooks/use-workflows";
import { useShallow } from "zustand/react/shallow";

import { nodeTypes } from "./custom-nodes/index";
import { useFlowSelection } from "../_hooks/useFlowSelection";
import { useDeleteHandler } from "../_hooks/useDeleteHandler";
import { useReactFlow } from "@xyflow/react";
import { v4 as uuidv4 } from "uuid";

// Map of node types to their display names
const nodeTypeDisplayNames: Record<string, string> = {
    "ZenFlow.Activities.Http.HttpRequestActivity": "HTTP Request",
    "ZenFlow.Activities.Email.SendEmailActivity": "Send Email",
    "ZenFlow.Activities.Playwright.ExtractTextFromElementActivity":
        "Extract Data",
};

const selector = (state: ReturnType<typeof useWorkflowStore.getState>) => ({
    nodes: state.nodes,
    edges: state.edges,
    onNodesChange: state.onNodesChange,
    onEdgesChange: state.onEdgesChange,
    onConnect: state.onConnect,
    setNodes: state.setNodes,
});

// Main flow component
const Flow = () => {
    const params = useParams<{ id: string }>();
    const workflowId = params.id as string;
    const reactFlowWrapper = useRef<HTMLDivElement>(null);
    const { screenToFlowPosition } = useReactFlow();

    // Use Zustand store with shallow selector - from official pattern
    const { nodes, edges, onNodesChange, onEdgesChange, onConnect, setNodes } =
        useWorkflowStore(useShallow(selector));

    // Use extracted hooks for selection and delete operations
    const { selectedElements, setSelectedElements, hasSelection } =
        useFlowSelection();
    const { deleteSelectedElements } = useDeleteHandler(
        nodes,
        edges,
        useWorkflowStore.getState().setNodes,
        useWorkflowStore.getState().setEdges,
        selectedElements,
        setSelectedElements
    );

    // Find node title from static mapping
    const findNodeTitle = useCallback((nodeType: string) => {
        return nodeTypeDisplayNames[nodeType] || nodeType;
    }, []);

    // Handle drag over for drag and drop
    const onDragOver = useCallback((event: React.DragEvent) => {
        event.preventDefault();
        event.dataTransfer.dropEffect = "move";
    }, []);

    // Handle drop to create new node
    const onDrop = useCallback(
        (event: React.DragEvent) => {
            event.preventDefault();
            const nodeType = event.dataTransfer.getData(
                "application/reactflow"
            );
            if (!nodeType || typeof nodeType !== "string") {
                return;
            }
            const position = screenToFlowPosition({
                x: event.clientX,
                y: event.clientY,
            });
            const nodeTitle = findNodeTitle(nodeType);
            const nodeId = uuidv4();

            // Initialize activity properties based on node type
            let activityProperties = {};

            if (nodeType === "ZenFlow.Activities.Http.HttpRequestActivity") {
                activityProperties = {
                    url: "https://api.example.com",
                    method: "GET",
                };
            } else if (
                nodeType === "ZenFlow.Activities.Email.SendEmailActivity"
            ) {
                activityProperties = {
                    to: "",
                    subject: "",
                    body: "",
                    isHtml: false,
                };
            } else if (
                nodeType ===
                "ZenFlow.Activities.Playwright.ExtractTextFromElementActivity"
            ) {
                activityProperties = {
                    targetUrl: "https://example.com",
                    elementSelector: ".selector",
                };
            }

            const newNode = {
                id: nodeId,
                type: nodeType,
                position,
                data: {
                    label: nodeTitle,
                    activityProperties: activityProperties,
                },
            };
            setNodes([...nodes, newNode]);
        },
        [screenToFlowPosition, nodes, setNodes, findNodeTitle]
    );

    // Use the update workflow hook from use-workflows.ts
    const { updateWorkflowData, isSaving, isLoading } =
        useUpdateWorkflow(workflowId);

    return (
        <div className="react-flow-wrapper">
            <div className="react-flow-wrapper" ref={reactFlowWrapper}>
                <ReactFlow
                    nodes={nodes}
                    edges={edges}
                    onNodesChange={onNodesChange}
                    onEdgesChange={onEdgesChange}
                    onConnect={onConnect}
                    onDragOver={onDragOver}
                    onDrop={onDrop}
                    nodeTypes={nodeTypes}
                    connectionLineType={ConnectionLineType.SmoothStep}
                    connectionMode={ConnectionMode.Loose}
                    defaultEdgeOptions={{
                        type: "smoothstep",
                        style: {
                            strokeWidth: 2,
                        },
                        animated: true,
                    }}
                    fitView
                    deleteKeyCode={null} // Disable built-in delete to use our custom implementation
                >
                    <MiniMap zoomable pannable position="bottom-right" />
                    <Background variant={BackgroundVariant.Dots} />
                    <Controls />
                    <Panel position="top-right">
                        <div className="flex gap-2">
                            <Button
                                variant="outline"
                                size="icon"
                                onClick={deleteSelectedElements}
                                disabled={!hasSelection}
                            >
                                <Trash2 className="h-4 w-4" />
                            </Button>
                            <Button
                                variant="outline"
                                size="icon"
                                onClick={() => updateWorkflowData(nodes, edges)}
                                disabled={isSaving || isLoading}
                            >
                                {isSaving ? (
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                ) : isLoading ? (
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                ) : (
                                    <Save className="h-4 w-4" />
                                )}
                            </Button>
                        </div>
                    </Panel>
                </ReactFlow>
            </div>
        </div>
    );
};

export const FlowEditor = () => {
    return (
        <ReactFlowProvider>
            <div className="react-flow-wrapper">
                <Flow />
            </div>
        </ReactFlowProvider>
    );
};
