import { useCallback, useEffect } from "react";
import { Edge, Node, useKeyPress } from "@xyflow/react";

export const useDeleteHandler = (
    nodes: Node[],
    edges: Edge[],
    setNodes: (nodes: Node[]) => void,
    setEdges: (edges: Edge[]) => void,
    selectedElements: { nodes: Node[]; edges: Edge[] },
    setSelectedElements: (elements: { nodes: Node[]; edges: Edge[] }) => void
) => {
    // Listen for deletion key presses (Delete and Backspace)
    const deleteKeyPressed = useKeyPress(["Delete", "Backspace"]);

    // Handle deleting selected elements
    const deleteSelectedElements = useCallback(() => {
        if (
            selectedElements.nodes.length > 0 ||
            selectedElements.edges.length > 0
        ) {
            // Get IDs of selected nodes for efficient lookup
            const selectedNodeIds = new Set(selectedElements.nodes.map(n => n.id));

            // Filter out the selected nodes
            const remainingNodes = nodes.filter(
                (node) => !selectedNodeIds.has(node.id)
            );
            setNodes(remainingNodes);

            // Get IDs of remaining nodes for edge filtering
            const remainingNodeIds = new Set(remainingNodes.map(n => n.id));

            // Filter out selected edges AND edges connected to deleted nodes
            const remainingEdges = edges.filter(edge => {
                // Check if the edge itself is selected
                const isSelectedEdge = selectedElements.edges.some(e => e.id === edge.id);
                if (isSelectedEdge) {
                    return false; // Remove selected edge
                }
                // Check if the edge is connected to a deleted node
                const sourceExists = remainingNodeIds.has(edge.source);
                const targetExists = remainingNodeIds.has(edge.target);
                return sourceExists && targetExists; // Keep edge only if both source and target still exist
            });
            setEdges(remainingEdges);

            // Clear selection after deletion
            setSelectedElements({ nodes: [], edges: [] });
        }
    }, [
        selectedElements,
        setNodes,
        setEdges,
        nodes,
        edges,
        setSelectedElements,
    ]);

    // Handle key press for deleting elements
    useEffect(() => {
        let deleteTimeoutId: NodeJS.Timeout | null = null;

        if (deleteKeyPressed) {
            // Use a setTimeout to prevent potential rapid re-renders
            deleteTimeoutId = setTimeout(() => {
                deleteSelectedElements();
            }, 0);
        }

        // Cleanup timeout on unmount or when dependencies change
        return () => {
            if (deleteTimeoutId) {
                clearTimeout(deleteTimeoutId);
            }
        };
    }, [deleteKeyPressed, deleteSelectedElements]);

    return { deleteSelectedElements };
};
