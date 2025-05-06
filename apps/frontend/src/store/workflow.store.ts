import { create } from "zustand";
import { Node, Edge } from "@xyflow/react";

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
    nodes: Node[];
    edges: Edge[];
    setNodes: (nodes: Node[]) => void;
    setEdges: (edges: Edge[]) => void;
    updateNodeData: (nodeId: string, data: Record<string, any>) => void;
    isSaving: boolean;
    setSaving: (value: boolean) => void;
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
    updateNodeData: (nodeId: string, data: Record<string, any>) => 
        set((state) => ({
            nodes: state.nodes.map((node) => 
                node.id === nodeId 
                    ? { ...node, data: { ...node.data, ...data } } 
                    : node
            )
        })),
    isSaving: false,
    setSaving: (value: boolean) => set({ isSaving: value }),
}));
