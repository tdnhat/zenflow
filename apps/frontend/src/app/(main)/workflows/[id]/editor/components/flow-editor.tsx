import {
    Background,
    Controls,
    Edge,
    Node,
    ReactFlow,
    ReactFlowProvider,
    addEdge,
    useEdgesState,
    useNodesState,
    useReactFlow,
    Panel,
    Connection,
    MiniMap,
    useKeyPress,
    useOnSelectionChange,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import "./flow-editor.css";
import { useCallback, useRef, useState, useEffect } from "react";
import { ManualTriggerNode } from "./custom-nodes/manual-trigger-node";
import { NavigateNode } from "./custom-nodes/navigate-node";
import { InputTextNode } from "./custom-nodes/input-text-node";
import { WaitForSelectorNode } from "./custom-nodes/wait-for-selector-node";
import { ScreenshotNode } from "./custom-nodes/screenshot-node";
import { CrawlDataNode } from "./custom-nodes/crawl-data-node";
import { ClickNode } from "./custom-nodes/click-node";
import { Button } from "@/components/ui/button";
import { Trash2 } from "lucide-react";

const initialNodes: Node[] = [];
const initialEdges: Edge[] = [];

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
    const reactFlowWrapper = useRef<HTMLDivElement>(null);
    const [nodes, setNodes, onNodesChange] = useNodesState(initialNodes);
    const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges);
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

    // Handle deleting selected elements
    const deleteSelectedElements = useCallback(() => {
        if (
            selectedElements.nodes.length > 0 ||
            selectedElements.edges.length > 0
        ) {
            setNodes((nds) =>
                nds.filter(
                    (node) =>
                        !selectedElements.nodes.some((n) => n.id === node.id)
                )
            );
            setEdges((eds) =>
                eds.filter(
                    (edge) =>
                        !selectedElements.edges.some((e) => e.id === edge.id)
                )
            );
        }
    }, [selectedElements, setNodes, setEdges]);

    // Handle key press for deleting elements
    useEffect(() => {
        if (deleteKeyPressed) {
            deleteSelectedElements();
        }
    }, [deleteKeyPressed, deleteSelectedElements]);

    // Handle connections between nodes
    const onConnect = useCallback(
        (params: Connection) => setEdges((eds) => addEdge(params, eds)),
        [setEdges]
    );

    // Handle drag over for drag and drop functionality
    const onDragOver = useCallback((event: React.DragEvent) => {
        event.preventDefault();
        event.dataTransfer.dropEffect = "move";
    }, []);

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

            // Create the new node with appropriate data
            const newNode = {
                id: `node_${nodes.length + 1}`,
                type: nodeType,
                position,
                data:
                    nodeType === "custom"
                        ? {
                              label: `Custom Node ${nodes.length + 1}`,
                              description: "Drag me around!",
                          }
                        : {
                              label: `${nodeType} node`,
                          },
            };

            // Add the new node to the graph
            setNodes((nds) => nds.concat(newNode));
        },
        [screenToFlowPosition, nodes.length, setNodes]
    );

    return (
        <div className="flex h-full">
            <div className="flex-1" ref={reactFlowWrapper}>
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
                >
                    <MiniMap
                        nodeColor="#6366f1"
                        nodeStrokeWidth={2}
                        maskColor="rgba(0, 0, 0, 0.1)"
                    />
                    <Background />
                    <Controls orientation="horizontal" position="top-left" />
                    <Panel position="top-right">
                        <div className="flex gap-2">
                            <Button
                                variant="outline"
                                onClick={deleteSelectedElements}
                                disabled={
                                    !selectedElements.nodes.length &&
                                    !selectedElements.edges.length
                                }
                            >
                                <Trash2 className="h-4 w-4" />
                                Delete Selected
                            </Button>
                            <Button onClick={() => console.log(nodes, edges)}>
                                Save Flow
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
