import { z } from "zod";

// Schema for workflow nodes
export const workflowNodeSchema = z.object({
    id: z.string(),
    nodeType: z.string(),
    nodeKind: z.string(),
    label: z.string(),
    x: z.number(),
    y: z.number(),
    configJson: z.string(),
});

// Schema for workflow edges
export const workflowEdgeSchema = z.object({
    id: z.string(),
    sourceNodeId: z.string(),
    targetNodeId: z.string(),
    label: z.string().optional().default(""),
    edgeType: z.string().optional().default("default"),
    conditionJson: z.string().optional().default("{}"),
    sourceHandle: z.string().optional().default(""),
    targetHandle: z.string().optional().default(""),
});

// Schema for bulk saving workflow
export const workflowSaveSchema = z.object({
    name: z.string().optional(),
    description: z.string().optional(),
    nodes: z.array(workflowNodeSchema),
    edges: z.array(workflowEdgeSchema),
});

// Define the form schema with Zod
export const workflowFormSchema = z.object({
    name: z
        .string()
        .min(3, { message: "Workflow name must be at least 3 characters" })
        .max(50, {
            message: "Workflow name must be less than 50 characters",
        }),
    description: z
        .string()
        .min(3, { message: "Description must be at least 3 characters" })
        .max(500, {
            message: "Description must be less than 500 characters",
        }),
});

// Type for the form values
export type WorkflowFormValues = z.infer<typeof workflowFormSchema>;
export type WorkflowSaveValues = z.infer<typeof workflowSaveSchema>;
