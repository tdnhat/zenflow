import { useRef, useCallback } from "react";
import { useParams } from "next/navigation";
import { useWorkflowStore, NodeData } from "@/store/workflow.store";
import { useUpdateWorkflow } from "../../../_hooks/use-workflow";
import { useShallow } from "zustand/react/shallow";
import { useReactFlow, Node, Edge } from "@xyflow/react";

import { nodeTypes } from "./custom-nodes/index";
import { useFlowSelection } from "../_hooks/useFlowSelection";
import { useDeleteHandler } from "../_hooks/useDeleteHandler";
import { useFlowDropHandler } from "../_hooks/useFlowDropHandler";

import { FlowCanvas } from "./flow-canvas";
import { FlowPanel } from "./flow-panel";
import { FlowEditorProviderComponent } from "./flow-editor-provider";

// Zustand selector
const selector = (state: ReturnType<typeof useWorkflowStore.getState>) => ({
    nodes: state.nodes,
    edges: state.edges,
    onNodesChange: state.onNodesChange,
    onEdgesChange: state.onEdgesChange,
    onConnect: state.onConnect,
    setNodes: state.setNodes,
    setEdges: state.setEdges,
    isNodeInputActive: state.isNodeInputActive,
});

const FlowOrchestrator = () => {
    const params = useParams<{ id: string }>();
    const workflowId = params.id as string;
    const reactFlowWrapper = useRef<HTMLDivElement>(null); // Still needed for FlowCanvas if it uses it
    const { screenToFlowPosition } = useReactFlow();

    const {
        nodes,
        edges,
        onNodesChange,
        onEdgesChange,
        onConnect,
        setNodes,
        setEdges,
        isNodeInputActive,
    } = useWorkflowStore(useShallow(selector));

    // Adapter for setNodes for useDeleteHandler
    const setNodesForDeleteHandler = useCallback((updatedNodes: Node[]) => {
        setNodes(updatedNodes as Node<NodeData>[]);
    }, [setNodes]);

    const { selectedElements, setSelectedElements, hasSelection } =
        useFlowSelection();
    const { deleteSelectedElements } = useDeleteHandler(
        nodes,
        edges,
        setNodesForDeleteHandler,
        setEdges,
        selectedElements,
        setSelectedElements
    );

    const { onDragOver, onDrop } = useFlowDropHandler({ screenToFlowPosition });

    const { updateWorkflowData, isSaving, isLoading } =
        useUpdateWorkflow(workflowId);

    // Wrapper to satisfy FlowPanel's prop type while using the specific hook function.
    // FlowPanel's updateWorkflowData prop expects (nodes: Node[], edges: Edge[]) => void.
    // Our hook's updateWorkflowData is (nodes: Node<NodeData>[], edges: Edge[]) => Promise<void>.
    // Since FlowOrchestrator passes currentNodes (which are Node<NodeData>[]) to FlowPanel,
    // FlowPanel will call this callback with nodes that are actually Node<NodeData>[].
    const handleUpdateWorkflowDataForPanel = useCallback(
        (nodesFromPanel: Node[], edgesFromPanel: Edge[]): void => {
            // We cast nodesFromPanel because we know they originate from Node<NodeData>[]
            // and FlowPanel's prop type is just looser than the actual data flow.
            updateWorkflowData(nodesFromPanel as Node<NodeData>[], edgesFromPanel);
        },
        [updateWorkflowData]
    );

    return (
        <div className="react-flow-wrapper" ref={reactFlowWrapper}> {/* This outer div might be redundant if FlowCanvas handles it */}
            <FlowCanvas
                nodes={nodes}
                edges={edges}
                onNodesChange={onNodesChange}
                onEdgesChange={onEdgesChange}
                onConnect={onConnect}
                onDragOver={onDragOver}
                onDrop={onDrop}
                nodeTypes={nodeTypes}
                panOnDrag={!isNodeInputActive}
                zoomOnScroll={!isNodeInputActive}
                nodesDraggable={!isNodeInputActive}
                reactFlowWrapperRef={reactFlowWrapper} // Pass ref to FlowCanvas
            />
            <FlowPanel
                deleteSelectedElements={deleteSelectedElements}
                hasSelection={hasSelection}
                updateWorkflowData={handleUpdateWorkflowDataForPanel}
                isSaving={isSaving}
                isLoading={isLoading}
                currentNodes={nodes}
                currentEdges={edges}
            />
        </div>
    );
};

// The main export for the editor page
export const FlowEditor = () => {
    return (
        <FlowEditorProviderComponent>
            <FlowOrchestrator />
        </FlowEditorProviderComponent>
    );
};
