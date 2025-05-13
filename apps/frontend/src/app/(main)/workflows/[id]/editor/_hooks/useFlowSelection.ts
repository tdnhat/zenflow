import { Edge, Node, useOnSelectionChange } from "@xyflow/react";
import { useState } from "react";

export const useFlowSelection = () => {
    // Track selected elements
    const [selectedElements, setSelectedElements] = useState<{
        nodes: Node[];
        edges: Edge[];
    }>({
        nodes: [],
        edges: [],
    });

    // Node selection change handler
    useOnSelectionChange({
        onChange: ({ nodes, edges }) => {
            setSelectedElements({ nodes, edges });
        },
    });

    return {
        selectedElements,
        setSelectedElements,
        hasSelection: selectedElements.nodes.length > 0 || selectedElements.edges.length > 0
    };
};