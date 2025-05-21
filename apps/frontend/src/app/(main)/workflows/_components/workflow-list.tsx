import { WorkflowCard } from "./workflow-card";
import { WorkflowDefinitionDto } from "@/types/workflow.type";

interface WorkflowListProps {
    workflows: WorkflowDefinitionDto[];
}

export const WorkflowList: React.FC<WorkflowListProps> = ({ workflows }) => {
    return (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {workflows.map((workflow) => (
                <WorkflowCard key={workflow.id} workflow={workflow} />
            ))}
        </div>
    );
};
