import { create } from "zustand";

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
}));
