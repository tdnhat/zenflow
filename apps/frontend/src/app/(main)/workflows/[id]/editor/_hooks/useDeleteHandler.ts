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
            // Filter out the selected nodes
            setNodes(
                nodes.filter(
                    (node) =>
                        !selectedElements.nodes.some((n) => n.id === node.id)
                )
            );

            // Filter out the selected edges
            setEdges(
                edges.filter(
                    (edge) =>
                        !selectedElements.edges.some((e) => e.id === edge.id)
                )
            );

            // Clear selection after deletion to prevent infinite loop
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
