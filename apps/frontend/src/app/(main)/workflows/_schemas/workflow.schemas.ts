import { z } from "zod";

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
