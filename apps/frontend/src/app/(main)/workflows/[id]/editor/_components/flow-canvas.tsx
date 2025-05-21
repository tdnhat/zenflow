import {
    ReactFlow,
    MiniMap,
    Controls,
    Background,
    BackgroundVariant,
    ConnectionLineType,
    ConnectionMode,
    Node,
    Edge,
    OnNodesChange,
    OnEdgesChange,
    OnConnect,
    NodeProps,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css"; // Import React Flow base styles
import "./flow-editor.css"; // Import custom flow editor styles
import { NodeData } from "@/store/workflow.store";

// Define nodeTypes prop structure more explicitly if possible, or use a generic Record
// For now, using Record<string, React.ComponentType<any>> as a general type
interface FlowCanvasProps {
    nodes: Node<NodeData>[];
    edges: Edge[];
    onNodesChange: OnNodesChange;
    onEdgesChange: OnEdgesChange;
    onConnect: OnConnect;
    onDragOver: (event: React.DragEvent) => void;
    onDrop: (event: React.DragEvent) => void;
    nodeTypes: Record<string, React.ComponentType<NodeProps>>; // Use NodeProps
    panOnDrag: boolean;
    zoomOnScroll: boolean;
    nodesDraggable: boolean;
    reactFlowWrapperRef?: React.RefObject<HTMLDivElement | null>; // Allow null for initial ref value
}

export const FlowCanvas = ({
    nodes,
    edges,
    onNodesChange,
    onEdgesChange,
    onConnect,
    onDragOver,
    onDrop,
    nodeTypes,
    panOnDrag,
    zoomOnScroll,
    nodesDraggable,
    reactFlowWrapperRef,
}: FlowCanvasProps) => {
    return (
        <div className="react-flow-wrapper" ref={reactFlowWrapperRef}>
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
                    style: { strokeWidth: 2 },
                    animated: true,
                }}
                fitView
                deleteKeyCode={null} // Custom delete handled by useDeleteHandler
                panOnDrag={panOnDrag}
                zoomOnScroll={zoomOnScroll}
                nodesDraggable={nodesDraggable}
            >
                <MiniMap zoomable pannable position="bottom-right" />
                <Background variant={BackgroundVariant.Dots} />
                <Controls />
                {/* Panel will be a sibling rendered by the orchestrator */}
            </ReactFlow>
        </div>
    );
}; 