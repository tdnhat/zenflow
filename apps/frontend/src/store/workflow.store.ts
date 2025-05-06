import { create } from "zustand";
import { Node, Edge } from "@xyflow/react";
import { WorkflowDetailDto } from "@/types/workflow.type";

// Define a more flexible NodeData type that allows for generic nodes
export type NodeData = {
    label: string;
    nodeKind: string;
    nodeType: string;
    configJson?: string | Record<string, unknown>;
    [key: string]: unknown;
};

// Map backend node types to frontend node types
const mapNodeType = (backendNodeType: string): string => {
    const nodeTypeMap: Record<string, string> = {
        "ManualTriggerActivity": "manual-trigger",
        // Add more mappings as needed
        // "BackendType": "frontend-type"
    };

    return nodeTypeMap[backendNodeType] || backendNodeType;
};

type WorkflowStore = {
    // Modal state
    isOpen: boolean;
    openModal: () => void;
    closeModal: () => void;
    
    // Operation state
    isSubmitting: boolean;
    setSubmitting: (value: boolean) => void;
    error: string | null;
    setError: (error: string | null) => void;
    clearError: () => void;

    // Flow editor state
    nodes: Node[]; // Use generic Node type for flexibility
    edges: Edge[];
    setNodes: (nodes: Node[]) => void;
    setEdges: (edges: Edge[]) => void;
    updateNodeData: (nodeId: string, data: Record<string, unknown>) => void;
    isSaving: boolean;
    setSaving: (value: boolean) => void;
    
    // Workflow initialization
    isInitialized: boolean;
    initializeWorkflow: (workflow: WorkflowDetailDto | null) => void;
    clearWorkflow: () => void;
};

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
    setNodes: (nodes: Node[]) => set({ nodes }),
    setEdges: (edges: Edge[]) => set({ edges }),
    updateNodeData: (nodeId: string, data: Record<string, unknown>) => 
        set((state) => ({
            nodes: state.nodes.map((node) => 
                node.id === nodeId 
                    ? { ...node, data: { ...node.data, ...data } } 
                    : node
            )
        })),
    isSaving: false,
    setSaving: (value: boolean) => set({ isSaving: value }),
    
    // Workflow initialization
    isInitialized: false,
    initializeWorkflow: (workflow: WorkflowDetailDto | null) => {
        if (!workflow) {
            set({ nodes: [], edges: [], isInitialized: true });
            return;
        }
        
        // Convert backend node format to React Flow format
        const nodes: Node[] = workflow.nodes.map(node => ({
            id: node.id,
            type: mapNodeType(node.nodeType), // Use the mapping function here
            position: { x: node.x, y: node.y },
            data: {
                label: node.label,
                nodeKind: node.nodeKind,
                nodeType: node.nodeType, // Keep original nodeType in data for saving back to server
                configJson: node.configJson,
                // Parse configJson if it's a string
                ...(typeof node.configJson === 'string' && node.configJson ? 
                    JSON.parse(node.configJson) as Record<string, unknown> : 
                    typeof node.configJson === 'object' ? node.configJson as Record<string, unknown> : {})
            },
        }));
        
        // Convert backend edge format to React Flow format
        const edges: Edge[] = workflow.edges.map(edge => ({
            id: edge.id,
            source: edge.sourceNodeId,
            target: edge.targetNodeId,
            sourceHandle: edge.sourceHandle || null,
            targetHandle: edge.targetHandle || null,
            type: edge.edgeType || 'default',
            data: edge.conditionJson ? 
                (typeof edge.conditionJson === 'string' ? 
                    JSON.parse(edge.conditionJson) as Record<string, unknown> : 
                    edge.conditionJson as Record<string, unknown>) : 
                {},
            label: edge.label || undefined,
        }));
        
        set({ 
            nodes, 
            edges, 
            isInitialized: true 
        });
    },
    clearWorkflow: () => set({ 
        nodes: [], 
        edges: [], 
        isInitialized: false 
    }),
}));
