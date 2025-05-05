"use client";

/**
 * A skeleton loader component for the workflows list
 * Provides a visual placeholder while the actual data is loading
 */
export const WorkflowListSkeleton = () => {
  // Create an array of items to render multiple skeletons
  const skeletons = Array.from({ length: 6 }, (_, i) => i);

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      {skeletons.map((index) => (
        <div 
          key={index}
          className="bg-card border border-border/40 hover:border-primary/20 transition-colors p-6 rounded-lg shadow-sm animate-pulse flex flex-col h-full"
        >
          {/* Title skeleton with status badge */}
          <div className="flex justify-between items-start mb-3">
            <div className="h-6 bg-muted/50 rounded-md w-3/5"></div>
            <div className="h-5 w-16 bg-primary/20 rounded-full"></div>
          </div>
          
          {/* Description skeleton - multiple lines with flex-grow */}
          <div className="space-y-2 mb-4 flex-grow">
            <div className="h-4 bg-muted/50 rounded-md w-full"></div>
            <div className="h-4 bg-muted/50 rounded-md w-4/5"></div>
            <div className="h-4 bg-muted/50 rounded-md w-2/3"></div>
          </div>
          
          {/* Metadata row - timestamp with mt-auto */}
          <div className="mt-auto pt-3 border-t border-border/30">
            <div className="h-3.5 bg-muted/40 rounded-md w-1/3"></div>
          </div>
        </div>
      ))}
    </div>
  );
};

export default WorkflowListSkeleton; 