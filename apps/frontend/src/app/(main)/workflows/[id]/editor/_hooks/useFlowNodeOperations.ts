import { Node, NodeChange } from "@xyflow/react";
import { v4 as uuidv4 } from "uuid";
import { applyNodeChanges } from "@xyflow/react";
import { useReactFlow } from "@xyflow/react";
import { useCallback } from "react";
import { useNodeTypes } from "../../../_hooks/use-workflows";

export const useFlowNodeOperations = (
    nodes: Node[],
    setNodes: (nodes: Node[]) => void
) => {
    const { screenToFlowPosition } = useReactFlow();
    const { data: nodeTypesData } = useNodeTypes();

    // Find node title from nodeTypesData based on node type
    const findNodeTitle = useCallback(
        (nodeType: string) => {
            if (!nodeTypesData) return nodeType;

            // Flatten all tasks from all categories and find the matching one
            const allTasks = nodeTypesData.flatMap(
                (category) => category.tasks
            );
            const task = allTasks.find((task) => task.type === nodeType);
            return task?.title || nodeType;
        },
        [nodeTypesData]
    );

    // Handle node changes (position, selection, etc.)
    const onNodesChange = useCallback(
        (changes: NodeChange[]) => {
            setNodes(applyNodeChanges(changes, nodes));
        },
        [setNodes, nodes]
    );

    // Create a new node when dropping onto the canvas
    const onDrop = useCallback(
        (event: React.DragEvent) => {
            event.preventDefault();

            // Get the node type from the dragged element
            const nodeType = event.dataTransfer.getData(
                "application/reactflow"
            );

            // Check if we have a valid node type
            if (!nodeType || typeof nodeType !== "string") {
                return;
            }

            // Get the position where the node was dropped
            const position = screenToFlowPosition({
                x: event.clientX,
                y: event.clientY,
            });

            // Find the title for this node type
            const nodeTitle = findNodeTitle(nodeType);

            // Generate a new UUID for the node
            const nodeId = uuidv4();

            // Create the new node with appropriate data
            const newNode: Node = {
                id: nodeId,
                type: nodeType,
                position,
                data:
                    nodeType === "custom"
                        ? {
                            label: `Custom Node ${nodes.length + 1}`,
                            description: "Drag me around!",
                            nodeKind: "ACTION",
                            nodeType: nodeType,
                        }
                    : {
                            label: nodeTitle,
                            nodeKind: "ACTION", // Default node kind
                            nodeType: nodeType,
                        },
            };

            // Add the new node to the graph
            setNodes([...nodes, newNode]);
        },
        [screenToFlowPosition, nodes, setNodes, findNodeTitle]
    );

    // Handle drag over for drag and drop functionality
    const onDragOver = useCallback((event: React.DragEvent) => {
        event.preventDefault();
        event.dataTransfer.dropEffect = "move";
    }, []);

    return {
        onNodesChange,
        onDrop,
        onDragOver,
    };
};
