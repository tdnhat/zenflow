import { getTimeAgo } from "@/lib/utils";
import { WorkflowDefinitionDto } from "@/types/workflow.type";
import Link from "next/link";

interface WorkflowCardProps {
    workflow: WorkflowDefinitionDto;
}

export const WorkflowCard: React.FC<WorkflowCardProps> = ({ workflow }) => {
    const timeAgo = getTimeAgo(workflow.updatedAt);

    return (
        <Link
            href={`/workflows/${workflow.id}/editor`}
            className="block h-full"
        >
            <div className="bg-card border border-border/40 hover:border-primary/30 transition-colors p-6 rounded-lg shadow-sm group flex flex-col h-full cursor-pointer">
                <h2 className="text-lg font-semibold truncate group-hover:text-primary transition-colors">
                    {workflow.name}
                </h2>

                {/* Description with multi-line clamp - flex-grow to push footer down */}
                <p className="text-muted-foreground text-sm line-clamp-3 mb-4 flex-grow">
                    {workflow.description || "No description provided"}
                </p>

                {/* Footer: Last modified info - mt-auto ensures it sticks to bottom */}
                <div className="mt-auto pt-3 border-t border-border/30">
                    <span className="text-xs text-muted-foreground">
                        {workflow.updatedAt
                            ? `Updated ${timeAgo}`
                            : `Created ${timeAgo}`}
                    </span>
                </div>
            </div>
        </Link>
    );
};
