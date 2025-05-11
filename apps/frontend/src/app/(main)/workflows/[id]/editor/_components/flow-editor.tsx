import {
    Background,
    Controls,
    ReactFlow,
    ReactFlowProvider,
    Panel,
    MiniMap,
    BackgroundVariant,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import "./flow-editor.css";
import { useRef } from "react";
import { Button } from "@/components/ui/button";
import { Save, Trash2 } from "lucide-react";
import { useParams } from "next/navigation";
import { useWorkflowStore } from "@/store/workflow.store";
import { useSaveWorkflow } from "../../../_hooks/use-workflows";

// Import custom nodes - fix the path if needed
import { nodeTypes } from "./custom-nodes/index";
import { useFlowSelection } from "../_hooks/useFlowSelection";
import { useFlowNodeOperations } from "../_hooks/useFlowNodeOperations";
import { useFlowEdgeOperations } from "../_hooks/useFlowEdgeOperations";
import { useDeleteHandler } from "../_hooks/useDeleteHandler";

// Main flow component
const Flow = () => {
    const params = useParams<{ id: string }>();
    const workflowId = params.id as string;
    const reactFlowWrapper = useRef<HTMLDivElement>(null);

    // Use Zustand store for workflow state
    const { nodes, edges, setNodes, setEdges } = useWorkflowStore();

    // Use extracted hooks for different aspects of flow functionality
    const { selectedElements, setSelectedElements, hasSelection } =
        useFlowSelection();
    const { onNodesChange, onDrop, onDragOver } = useFlowNodeOperations(
        nodes,
        setNodes
    );
    const { onEdgesChange, onConnect } = useFlowEdgeOperations(edges, setEdges);
    const { deleteSelectedElements } = useDeleteHandler(
        nodes,
        edges,
        setNodes,
        setEdges,
        selectedElements,
        setSelectedElements
    );

    // Use the new save workflow hook from use-workflows.ts
    const { saveWorkflowData, isSaving } = useSaveWorkflow(workflowId);

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
                    fitView
                    deleteKeyCode={null} // Disable built-in delete to use our custom implementation
                >
                    <MiniMap
                        zoomable
                        pannable
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
            <div className="react-flow-wrapper">
                <Flow />
            </div>
        </ReactFlowProvider>
    );
};
