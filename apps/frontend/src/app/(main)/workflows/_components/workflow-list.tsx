import { WorkflowCard } from "./workflow-card";
import { WorkflowDto } from "@/types/workflow.type";
interface WorkflowListProps {
    workflows: WorkflowDto[];
}

export const WorkflowList: React.FC<WorkflowListProps> = ({ workflows }) => {
    return (
        <div className="space-y-4">
            {workflows.map((workflow) => (
                <WorkflowCard key={workflow.id} workflow={workflow} />
            ))}
        </div>
    );
};
