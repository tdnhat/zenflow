import {
    Background,
    Controls,
    Edge,
    Node,
    ReactFlow,
    ReactFlowProvider,
    addEdge,
    applyNodeChanges,
    applyEdgeChanges,
    useReactFlow,
    Panel,
    Connection,
    MiniMap,
    useKeyPress,
    useOnSelectionChange,
    BackgroundVariant,
    NodeChange,
    EdgeChange,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import "./flow-editor.css";
import { useCallback, useRef, useEffect, useState } from "react";
import { ManualTriggerNode } from "./custom-nodes/manual-trigger-node";
import { NavigateNode } from "./custom-nodes/navigate-node";
import { InputTextNode } from "./custom-nodes/input-text-node";
import { WaitForSelectorNode } from "./custom-nodes/wait-for-selector-node";
import { ScreenshotNode } from "./custom-nodes/screenshot-node";
import { CrawlDataNode } from "./custom-nodes/crawl-data-node";
import { ClickNode } from "./custom-nodes/click-node";
import { Button } from "@/components/ui/button";
import { Save, Trash2 } from "lucide-react";
import { saveWorkflow } from "@/api/workflow/workflow-api";
import { useParams } from "next/navigation";
import { useWorkflowStore } from "@/store/workflow.store";
import toast from "react-hot-toast";
import { useNodeTypes } from "../../../_hooks/use-workflows";
import { v4 as uuidv4 } from 'uuid';

// Define node types outside the component to avoid recreation on each render
const nodeTypes = {
    "manual-trigger": ManualTriggerNode,
    navigate: NavigateNode,
    click: ClickNode,
    "input-text": InputTextNode,
    "wait-for-selector": WaitForSelectorNode,
    screenshot: ScreenshotNode,
    "crawl-data": CrawlDataNode,
};

const Flow = () => {
    const params = useParams<{ id: string }>();
    const workflowId = params.id;

    // Use Zustand store for workflow state
    const { 
        nodes, 
        edges, 
        setNodes, 
        setEdges, 
        isSaving, 
        setSaving
    } = useWorkflowStore();

    // Fetch node types to use their titles
    const { data: nodeTypesData } = useNodeTypes();

    const reactFlowWrapper = useRef<HTMLDivElement>(null);
    const { screenToFlowPosition } = useReactFlow();

    // Track selected elements
    const [selectedElements, setSelectedElements] = useState<{
        nodes: Node[];
        edges: Edge[];
    }>({
        nodes: [],
        edges: [],
    });

    // Listen for deletion key presses (Delete and Backspace)
    const deleteKeyPressed = useKeyPress(["Delete", "Backspace"]);

    // Node selection change handler
    useOnSelectionChange({
        onChange: ({ nodes, edges }) => {
            setSelectedElements({ nodes, edges });
        },
    });

    // Handle node changes (position, selection, etc.)
    const onNodesChange = useCallback((changes: NodeChange[]) => {
        setNodes(applyNodeChanges(changes, nodes));
    }, [setNodes, nodes]);

    // Handle edge changes
    const onEdgesChange = useCallback((changes: EdgeChange[]) => {
        setEdges(applyEdgeChanges(changes, edges));
    }, [setEdges, edges]);

    // Handle deleting selected elements
    const deleteSelectedElements = useCallback(() => {
        if (
            selectedElements.nodes.length > 0 ||
            selectedElements.edges.length > 0
        ) {
            // Filter out the selected nodes
            setNodes(
                nodes.filter(
                    (node) =>
                        !selectedElements.nodes.some((n) => n.id === node.id)
                )
            );
            
            // Filter out the selected edges
            setEdges(
                edges.filter(
                    (edge) =>
                        !selectedElements.edges.some((e) => e.id === edge.id)
                )
            );
            
            // Clear selection after deletion to prevent infinite loop
            setSelectedElements({ nodes: [], edges: [] });
        }
    }, [selectedElements, setNodes, setEdges, nodes, edges]);

    // Handle key press for deleting elements
    useEffect(() => {
        let deleteTimeoutId: NodeJS.Timeout | null = null;
        
        if (deleteKeyPressed) {
            // Use a setTimeout to prevent potential rapid re-renders
            deleteTimeoutId = setTimeout(() => {
                deleteSelectedElements();
            }, 0);
        }
        
        // Cleanup timeout on unmount or when dependencies change
        return () => {
            if (deleteTimeoutId) {
                clearTimeout(deleteTimeoutId);
            }
        };
    }, [deleteKeyPressed, deleteSelectedElements]);

    // Handle connections between nodes
    const onConnect = useCallback(
        (params: Connection) => {
            // Generate a new UUID for the edge
            const edgeId = uuidv4();
            const newEdge: Edge = {
                id: edgeId,
                source: params.source,
                target: params.target,
                sourceHandle: params.sourceHandle || null,
                targetHandle: params.targetHandle || null,
                type: 'default',
            };
            setEdges(addEdge(newEdge, edges));
        },
        [setEdges, edges]
    );

    // Handle drag over for drag and drop functionality
    const onDragOver = useCallback((event: React.DragEvent) => {
        event.preventDefault();
        event.dataTransfer.dropEffect = "move";
    }, []);

    // Find node title from nodeTypesData based on node type
    const findNodeTitle = useCallback((nodeType: string) => {
        if (!nodeTypesData) return nodeType;
        
        // Flatten all tasks from all categories and find the matching one
        const allTasks = nodeTypesData.flatMap(category => category.tasks);
        const task = allTasks.find(task => task.type === nodeType);
        return task?.title || nodeType;
    }, [nodeTypesData]);

    // Create a new node when dropping onto the canvas
    const onDrop = useCallback(
        (event: React.DragEvent) => {
            event.preventDefault();

            // Get the node type from the dragged element
            const nodeType = event.dataTransfer.getData(
                "application/reactflow"
            );

            // Check if we have a valid node type
            if (!nodeType || typeof nodeType !== "string") {
                return;
            }

            // Get the position where the node was dropped
            const position = screenToFlowPosition({
                x: event.clientX,
                y: event.clientY,
            });

            // Find the title for this node type
            const nodeTitle = findNodeTitle(nodeType);

            // Generate a new UUID for the node
            const nodeId = uuidv4();

            // Create the new node with appropriate data
            const newNode: Node = {
                id: nodeId,
                type: nodeType,
                position,
                data: nodeType === "custom"
                    ? {
                        label: `Custom Node ${nodes.length + 1}`,
                        description: "Drag me around!",
                        nodeKind: "ACTION",
                        nodeType: nodeType,
                    }
                    : {
                        label: nodeTitle,
                        nodeKind: "ACTION", // Default node kind
                        nodeType: nodeType,
                    },
            };

            // Add the new node to the graph
            setNodes([...nodes, newNode]);
        },
        [screenToFlowPosition, nodes, setNodes, findNodeTitle]
    );

    // Handle saving the workflow
    const handleSaveWorkflow = async () => {
        if (!workflowId) {
            toast.error("Workflow ID is missing.");
            return;
        }

        try {
            setSaving(true);
            await saveWorkflow(workflowId, nodes, edges);
            
            toast.success("Workflow saved successfully!");
        } catch (error) {
            console.error("Error saving workflow:", error);
            toast.error("Failed to save workflow. Please try again.");
        } finally {
            setSaving(false);
        }
    };

    return (
        <div className="w-full h-full">
            <div className="w-full h-full" ref={reactFlowWrapper}>
                <ReactFlow
                    nodes={nodes}
                    edges={edges}
                    onNodesChange={onNodesChange}
                    onEdgesChange={onEdgesChange}
                    onConnect={onConnect}
                    onDragOver={onDragOver}
                    onDrop={onDrop}
                    nodeTypes={nodeTypes}
                    fitView
                    deleteKeyCode={null} // Disable built-in delete to use our custom implementation
                    className="w-full h-full"
                >
                    <MiniMap 
                        zoomable 
                        pannable 
                        nodeColor={(node) => {
                            return node.selected ? 'var(--primary)' : 'var(--primary-foreground)';
                        }}
                        maskColor="var(--muted-foreground)"
                        position="bottom-right"
                    />
                    <Background variant={BackgroundVariant.Dots} />
                    <Controls />
                    <Panel position="top-right">
                        <div className="flex gap-2">
                            <Button
                                variant="outline"
                                size="icon"
                                onClick={deleteSelectedElements}
                                disabled={
                                    !selectedElements.nodes.length &&
                                    !selectedElements.edges.length
                                }
                            >
                                <Trash2 className="h-4 w-4" />
                            </Button>
                            <Button
                                variant="outline"
                                size="icon"
                                onClick={handleSaveWorkflow}
                                disabled={isSaving}
                            >
                                <Save className="h-4 w-4" />
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
            <Flow />
        </ReactFlowProvider>
    );
};
