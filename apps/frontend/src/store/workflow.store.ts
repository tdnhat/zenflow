import { create } from "zustand";
import {
    Node,
    Edge,
    OnNodesChange,
    OnEdgesChange,
    OnConnect,
    applyNodeChanges,
    applyEdgeChanges,
    addEdge,
} from "@xyflow/react";
import { WorkflowDefinitionDto, WorkflowNodeDto, WorkflowEdgeDto } from "@/types/workflow.type";
import { v4 as uuidv4 } from "uuid";

// Define a more explicit NodeData type with required fields
export type NodeData = {
    label: string;
    activityType: string;
    activityProperties: Record<string, unknown>;
    [key: string]: unknown;
};

// Types slice for better organization
type ModalState = {
    isOpen: boolean;
    openModal: () => void;
    closeModal: () => void;
};

type OperationState = {
    isSubmitting: boolean;
    setSubmitting: (value: boolean) => void;
    error: string | null;
    setError: (error: string | null) => void;
    clearError: () => void;
};

type FlowEditorState = {
    nodes: Node<NodeData>[];
    edges: Edge[];
    setNodes: (nodes: Node<NodeData>[]) => void;
    setEdges: (edges: Edge[]) => void;
    updateNodeData: (nodeId: string, data: Record<string, unknown>) => void;
    isSaving: boolean;
    setSaving: (value: boolean) => void;
    onNodesChange: OnNodesChange;
    onEdgesChange: OnEdgesChange;
    onConnect: OnConnect;
    isNodeInputActive: boolean;
    setNodeInputActive: (isActive: boolean) => void;
};

type WorkflowState = {
    isInitialized: boolean;
    initializeWorkflow: (workflow: WorkflowDefinitionDto | null) => void;
    clearWorkflow: () => void;
};

// Combined store type
type WorkflowStore = ModalState &
    OperationState &
    FlowEditorState &
    WorkflowState;

// Node type mapping utilities - moved to separate transformer section
const nodeTypeTransformers = {
    // Map backend node types to frontend node types
    toFrontend: (backendNodeType: string): string => {
        const nodeTypeMap: Record<string, string> = {
            ManualTriggerActivity: "manual-trigger",
            // Add more mappings as needed
            // "BackendType": "frontend-type"
        };
        return nodeTypeMap[backendNodeType] || backendNodeType;
    },

    // Map frontend node types to backend node types (for saving)
    toBackend: (frontendNodeType: string): string => {
        const nodeTypeMap: Record<string, string> = {
            "manual-trigger": "ManualTriggerActivity",
            // Add other mappings as needed
        };
        return nodeTypeMap[frontendNodeType] || frontendNodeType;
    },
};

// Data transformation utilities
const transformers = {
    // Transform backend workflow to frontend nodes/edges
    workflowToFlow: (
        workflow: WorkflowDefinitionDto | null
    ): { nodes: Node<NodeData>[]; edges: Edge[] } => {
        if (!workflow) {
            return { nodes: [], edges: [] };
        }

        // Convert backend node format to React Flow format
        const nodes: Node<NodeData>[] = workflow.nodes.map((node: WorkflowNodeDto) => ({
            id: node.id,
            type: nodeTypeTransformers.toFrontend(node.activityType),
            position: node.position,
            data: {
                label: node.name,
                activityType: node.activityType,
                activityProperties: node.activityProperties || {},
            },
        }));

        // Convert backend edge format to React Flow format
        const edges: Edge[] = workflow.edges.map((edge: WorkflowEdgeDto) => ({
            id: edge.id,
            source: edge.source,
            target: edge.target,
            type: "smoothstep",
            animated: true,
            data: edge.condition ? { condition: edge.condition } : {},
        }));

        return { nodes, edges };
    },
};

// Create store with better separation of concerns
export const useWorkflowStore = create<WorkflowStore>((set) => ({
    // Modal state
    isOpen: false,
    openModal: () => set({ isOpen: true }),
    closeModal: () => set({ isOpen: false }),

    // Operation state
    isSubmitting: false,
    setSubmitting: (value: boolean) => set({ isSubmitting: value }),
    error: null,
    setError: (error: string | null) => set({ error }),
    clearError: () => set({ error: null }),

    // Flow editor state
    nodes: [],
    edges: [],
    setNodes: (nodes: Node<NodeData>[]) => set({ nodes }),
    setEdges: (edges: Edge[]) => set({ edges }),
    updateNodeData: (nodeId: string, data: Record<string, unknown>) =>
        set((state) => ({
            nodes: state.nodes.map((node) =>
                node.id === nodeId
                    ? { ...node, data: { ...node.data, ...data } }
                    : node
            ),
        })),
    onNodesChange: (changes) =>
        set((state) => ({
            nodes: applyNodeChanges(changes, state.nodes) as Node<NodeData>[],
        })),
    onEdgesChange: (changes) =>
        set((state) => ({
            edges: applyEdgeChanges(changes, state.edges),
        })),
    onConnect: (connection) =>
        set((state) => ({
            edges: addEdge(
                { 
                    ...connection, 
                    id: uuidv4() // Ensure we use a valid UUID for new connections
                }, 
                state.edges
            ),
        })),
    isSaving: false,
    setSaving: (value: boolean) => set({ isSaving: value }),
    isNodeInputActive: false,
    setNodeInputActive: (isActive: boolean) => set({ isNodeInputActive: isActive }),

    // Workflow initialization
    isInitialized: false,
    initializeWorkflow: (workflow: WorkflowDefinitionDto | null) => {
        const { nodes, edges } = transformers.workflowToFlow(workflow);
        set({ nodes, edges, isInitialized: true });
    },
    clearWorkflow: () =>
        set({
            nodes: [],
            edges: [],
            isInitialized: false,
        }),
}));

// Export transformers for use in other components
export { nodeTypeTransformers, transformers };
