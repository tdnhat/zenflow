import { z } from "zod";

// Position schema to use in node schema
const positionSchema = z.object({
    x: z.number(),
    y: z.number(),
});

// Input mapping schema
const inputMappingSchema = z.object({
    sourceNodeId: z.string(),
    sourceProperty: z.string(),
    targetProperty: z.string(),
});

// Output mapping schema
const outputMappingSchema = z.object({
    sourceProperty: z.string(),
    targetProperty: z.string(),
    targetNodeId: z.string().optional(),
});

// Condition schema
const conditionSchema = z.object({
    type: z.string().optional(),
    expression: z.string(),
});

// Schema for workflow nodes
export const workflowNodeSchema = z.object({
    id: z.string(),
    name: z.string(),
    activityType: z.string(),
    activityProperties: z.record(z.unknown()),
    position: positionSchema,
    inputMappings: z.array(inputMappingSchema).optional(),
    outputMappings: z.array(outputMappingSchema).optional(),
});

// Schema for workflow edges
export const workflowEdgeSchema = z.object({
    id: z.string(),
    source: z.string(),
    target: z.string(),
    condition: conditionSchema.optional(),
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
