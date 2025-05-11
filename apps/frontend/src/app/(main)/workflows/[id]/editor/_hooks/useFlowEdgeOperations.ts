import { applyEdgeChanges, Connection, Edge, EdgeChange } from "@xyflow/react";

import { addEdge } from "@xyflow/react";
import { useCallback } from "react";
import { v4 as uuidv4 } from "uuid";

export const useFlowEdgeOperations = (
    edges: Edge[],
    setEdges: (edges: Edge[]) => void
) => {
    // Handle edge changes
    const onEdgesChange = useCallback(
        (changes: EdgeChange[]) => {
            setEdges(applyEdgeChanges(changes, edges));
        },
        [setEdges, edges]
    );

    // Handle connections between nodes
    const onConnect = useCallback(
        (params: Connection) => {
            // Generate a new UUID for the edge
            const edgeId = uuidv4();
            const newEdge: Edge = {
                id: edgeId,
                source: params.source,
                target: params.target,
                sourceHandle: params.sourceHandle || null,
                targetHandle: params.targetHandle || null,
                type: "default",
            };
            setEdges(addEdge(newEdge, edges));
        },
        [setEdges, edges]
    );

    return {
        onEdgesChange,
        onConnect,
    };
};
