import { useCallback } from 'react';
import { XYPosition, Node } from '@xyflow/react';
import { v4 as uuidv4 } from 'uuid';
import { NodeData, useWorkflowStore } from '@/store/workflow.store';
import { findNodeTitle } from '../_utils/workflow-node-utils';

interface UseFlowDropHandlerProps {
    screenToFlowPosition: (position: { x: number; y: number }) => XYPosition;
}

export const useFlowDropHandler = ({ screenToFlowPosition }: UseFlowDropHandlerProps) => {
    const nodes = useWorkflowStore((state) => state.nodes);
    const setNodes = useWorkflowStore((state) => state.setNodes);

    const onDragOver = useCallback((event: React.DragEvent) => {
        event.preventDefault();
        event.dataTransfer.dropEffect = 'move';
    }, []);

    const onDrop = useCallback(
        (event: React.DragEvent) => {
            event.preventDefault();
            const nodeType = event.dataTransfer.getData('application/reactflow');
            if (!nodeType || typeof nodeType !== 'string') {
                return;
            }

            const position = screenToFlowPosition({
                x: event.clientX,
                y: event.clientY,
            });

            const nodeTitle = findNodeTitle(nodeType);
            const nodeId = uuidv4();

            // Initialize activity properties based on node type
            let activityProperties = {};
            if (nodeType === 'ZenFlow.Activities.Http.HttpRequestActivity') {
                activityProperties = { url: 'https://api.example.com', method: 'GET' };
            } else if (nodeType === 'ZenFlow.Activities.Email.SendEmailActivity') {
                activityProperties = { to: '', subject: '', body: '', isHtml: false };
            } else if (nodeType === 'ZenFlow.Activities.Playwright.ExtractTextFromElementActivity') {
                activityProperties = { targetUrl: 'https://example.com', elementSelector: '.selector' };
            }

            const newNode: Node<NodeData> = {
                id: nodeId,
                type: nodeType,
                position,
                data: {
                    label: nodeTitle,
                    activityType: nodeType,
                    activityProperties: activityProperties,
                },
            };
            setNodes([...nodes, newNode]);
        },
        [screenToFlowPosition, nodes, setNodes]
    );

    return { onDragOver, onDrop };
}; 