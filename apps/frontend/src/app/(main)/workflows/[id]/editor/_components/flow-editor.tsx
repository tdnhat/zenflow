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
import { Button } from "@/components/ui/button";
import { Save, Trash2 } from "lucide-react";
import { useParams } from "next/navigation";
import { useWorkflowStore } from "@/store/workflow.store";
import { v4 as uuidv4 } from 'uuid';
import { useSaveWorkflow, useNodeTypes } from "../../../_hooks/use-workflows";

// Import custom nodes - fix the path if needed
import { nodeTypes } from "./custom-nodes/index";

// Custom hooks for flow editor functionality
const useFlowSelection = () => {
    // Track selected elements
    const [selectedElements, setSelectedElements] = useState<{
        nodes: Node[];
        edges: Edge[];
    }>({
        nodes: [],
        edges: [],
    });

    // Node selection change handler
    useOnSelectionChange({
        onChange: ({ nodes, edges }) => {
            setSelectedElements({ nodes, edges });
        },
    });

    return {
        selectedElements,
        setSelectedElements,
        hasSelection: selectedElements.nodes.length > 0 || selectedElements.edges.length > 0
    };
};

const useFlowNodeOperations = (nodes: Node[], setNodes: (nodes: Node[]) => void) => {
    const { screenToFlowPosition } = useReactFlow();
    const { data: nodeTypesData } = useNodeTypes();

    // Find node title from nodeTypesData based on node type
    const findNodeTitle = useCallback((nodeType: string) => {
        if (!nodeTypesData) return nodeType;
        
        // Flatten all tasks from all categories and find the matching one
        const allTasks = nodeTypesData.flatMap(category => category.tasks);
        const task = allTasks.find(task => task.type === nodeType);
        return task?.title || nodeType;
    }, [nodeTypesData]);

    // Handle node changes (position, selection, etc.)
    const onNodesChange = useCallback((changes: NodeChange[]) => {
        setNodes(applyNodeChanges(changes, nodes));
    }, [setNodes, nodes]);

    // Create a new node when dropping onto the canvas
    const onDrop = useCallback(
        (event: React.DragEvent) => {
            event.preventDefault();

            // Get the node type from the dragged element
            const nodeType = event.dataTransfer.getData("application/reactflow");

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

    // Handle drag over for drag and drop functionality
    const onDragOver = useCallback((event: React.DragEvent) => {
        event.preventDefault();
        event.dataTransfer.dropEffect = "move";
    }, []);

    return {
        onNodesChange,
        onDrop,
        onDragOver
    };
};

const useFlowEdgeOperations = (edges: Edge[], setEdges: (edges: Edge[]) => void) => {
    // Handle edge changes
    const onEdgesChange = useCallback((changes: EdgeChange[]) => {
        setEdges(applyEdgeChanges(changes, edges));
    }, [setEdges, edges]);

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

    return {
        onEdgesChange,
        onConnect
    };
};

// Custom hook to handle deletions
const useDeleteHandler = (
    nodes: Node[], 
    edges: Edge[], 
    setNodes: (nodes: Node[]) => void, 
    setEdges: (edges: Edge[]) => void,
    selectedElements: { nodes: Node[], edges: Edge[] },
    setSelectedElements: (elements: { nodes: Node[], edges: Edge[] }) => void
) => {
    // Listen for deletion key presses (Delete and Backspace)
    const deleteKeyPressed = useKeyPress(["Delete", "Backspace"]);

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
    }, [selectedElements, setNodes, setEdges, nodes, edges, setSelectedElements]);

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

    return { deleteSelectedElements };
};

// Main flow component
const Flow = () => {
    const params = useParams<{ id: string }>();
    const workflowId = params.id as string;
    const reactFlowWrapper = useRef<HTMLDivElement>(null);

    // Use Zustand store for workflow state
    const { 
        nodes, 
        edges, 
        setNodes, 
        setEdges,
    } = useWorkflowStore();

    // Use extracted hooks for different aspects of flow functionality
    const { selectedElements, setSelectedElements, hasSelection } = useFlowSelection();
    const { onNodesChange, onDrop, onDragOver } = useFlowNodeOperations(nodes, setNodes);
    const { onEdgesChange, onConnect } = useFlowEdgeOperations(edges, setEdges);
    const { deleteSelectedElements } = useDeleteHandler(nodes, edges, setNodes, setEdges, selectedElements, setSelectedElements);
    
    // Use the new save workflow hook from use-workflows.ts
    const { saveWorkflowData, isSaving } = useSaveWorkflow(workflowId);

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
                                disabled={!hasSelection}
                            >
                                <Trash2 className="h-4 w-4" />
                            </Button>
                            <Button
                                variant="outline"
                                size="icon"
                                onClick={() => saveWorkflowData(nodes, edges)}
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
