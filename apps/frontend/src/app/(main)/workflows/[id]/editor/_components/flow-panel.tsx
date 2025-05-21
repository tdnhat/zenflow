import { Panel } from "@xyflow/react";
import { Button } from "@/components/ui/button";
import { Loader2, Save, Trash2 } from "lucide-react";
import { Node, Edge } from "@xyflow/react"; // Import Node and Edge types

interface FlowPanelProps {
    deleteSelectedElements: () => void;
    hasSelection: boolean;
    updateWorkflowData: (nodes: Node[], edges: Edge[]) => void; // Use imported Node and Edge types
    isSaving: boolean;
    isLoading: boolean;
    currentNodes: Node[]; // Use imported Node type
    currentEdges: Edge[]; // Use imported Edge type
}

export const FlowPanel = ({
    deleteSelectedElements,
    hasSelection,
    updateWorkflowData,
    isSaving,
    isLoading,
    currentNodes,
    currentEdges,
}: FlowPanelProps) => {
    return (
        <Panel position="top-right">
            <div className="flex gap-2">
                <Button
                    variant="outline"
                    size="icon"
                    onClick={deleteSelectedElements}
                    disabled={!hasSelection}
                    aria-label="Delete selected elements"
                >
                    <Trash2 className="h-4 w-4" />
                </Button>
                <Button
                    variant="outline"
                    size="icon"
                    onClick={() => updateWorkflowData(currentNodes, currentEdges)}
                    disabled={isSaving || isLoading}
                    aria-label="Save workflow"
                >
                    {isSaving || isLoading ? (
                        <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                        <Save className="h-4 w-4" />
                    )}
                </Button>
            </div>
        </Panel>
    );
}; 