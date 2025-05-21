import { ReactFlowProvider } from "@xyflow/react";
import React from "react";

interface FlowEditorProviderProps {
    children: React.ReactNode;
}

export const FlowEditorProviderComponent = ({ children }: FlowEditorProviderProps) => {
    return <ReactFlowProvider>{children}</ReactFlowProvider>;
}; 