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
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import { useCallback, useRef } from "react";
import { ManualTriggerNode } from "./manual-trigger-node";

const initialNodes: Node[] = [];
const initialEdges: Edge[] = [];

// Node type definitions with our custom node
const nodeTypes = {
    manualTrigger: ManualTriggerNode,
};

const Flow = () => {
    const reactFlowWrapper = useRef<HTMLDivElement>(null);
    const [nodes, setNodes, onNodesChange] = useNodesState(initialNodes);
    const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges);
    const { screenToFlowPosition } = useReactFlow();

    // Handle connections between nodes
    const onConnect = useCallback(
        (params: Connection) => setEdges((eds) => addEdge(params, eds)),
        [setEdges]
    );

    // Handle drag over for drag and drop functionality
    const onDragOver = useCallback((event: React.DragEvent) => {
        event.preventDefault();
        event.dataTransfer.dropEffect = 'move';
    }, []);

    // Create a new node when dropping onto the canvas
    const onDrop = useCallback(
        (event: React.DragEvent) => {
            event.preventDefault();

            // Get the node type from the dragged element
            const nodeType = event.dataTransfer.getData('application/reactflow');
            
            // Check if we have a valid node type
            if (!nodeType || typeof nodeType !== 'string') {
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
                data: nodeType === 'custom' 
                    ? { 
                        label: `Custom Node ${nodes.length + 1}`, 
                        description: 'Drag me around!'
                    } 
                    : { 
                        label: `${nodeType} node` 
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
                >
                    <Background />
                    <Controls />
                    <Panel position="top-right">
                        <button 
                            className="bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded"
                            onClick={() => console.log(nodes, edges)}
                        >
                            Save Flow
                        </button>
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
