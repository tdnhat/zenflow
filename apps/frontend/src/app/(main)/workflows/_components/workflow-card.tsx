import { WorkflowDto } from "@/types/workflow.type";

interface WorkflowCardProps {
    workflow: WorkflowDto;
}

export const WorkflowCard: React.FC<WorkflowCardProps> = ({ workflow }) => {
    return (
        <div className="bg-card p-6 rounded-lg shadow border border-border hover:border-primary/30 transition-colors">
            <h2 className="text-lg font-semibold mb-2 truncate">
                {workflow.name}
            </h2>
            <p className="text-muted-foreground text-sm line-clamp-2 mb-4">
                {workflow.description}
            </p>
        </div>
    );
};
