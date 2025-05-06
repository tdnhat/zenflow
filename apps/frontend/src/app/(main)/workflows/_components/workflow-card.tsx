import { getStatusBadgeClass, getTimeAgo } from "@/lib/utils";
import { WorkflowDto } from "@/types/workflow.type";
import Link from "next/link";

interface WorkflowCardProps {
    workflow: WorkflowDto;
}

export const WorkflowCard: React.FC<WorkflowCardProps> = ({ workflow }) => {
    const timeAgo = getTimeAgo(workflow.lastModifiedAt ?? workflow.createdAt);

    return (
        <Link href={`/workflows/${workflow.id}/editor`} className="block h-full">
            <div className="bg-card border border-border/40 hover:border-primary/30 transition-colors p-6 rounded-lg shadow-sm group flex flex-col h-full cursor-pointer">
                {/* Header: Title with status badge */}
                <div className="flex justify-between items-start mb-3">
                    <h2 className="text-lg font-semibold truncate group-hover:text-primary transition-colors">
                        {workflow.name}
                    </h2>
                    <span
                        className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadgeClass(
                            workflow.status
                        )}`}
                    >
                        {workflow.status || "Draft"}
                    </span>
                </div>

                {/* Description with multi-line clamp - flex-grow to push footer down */}
                <p className="text-muted-foreground text-sm line-clamp-3 mb-4 flex-grow">
                    {workflow.description || "No description provided"}
                </p>

                {/* Footer: Last modified info - mt-auto ensures it sticks to bottom */}
                <div className="mt-auto pt-3 border-t border-border/30">
                    <span className="text-xs text-muted-foreground">
                        {workflow.lastModifiedAt
                            ? `Updated ${timeAgo}`
                            : `Created ${timeAgo}`}
                    </span>
                </div>
            </div>
        </Link>
    );
};
