import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useWorkflowStore } from "@/store/workflow.store";
import {
    workflowFormSchema,
    WorkflowFormValues,
} from "../_schemas/workflow.schemas";
import { useCreateWorkflow } from "../_hooks/use-workflows";
import { useEffect } from "react";

// Default values for the form
const defaultValues: Partial<WorkflowFormValues> = {
    name: "",
    description: "",
};

interface WorkflowCreateFormProps {
    onSubmitForm?: () => void;
}

export const WorkflowCreateForm = ({ onSubmitForm }: WorkflowCreateFormProps) => {
    const { closeModal, setSubmitting, setError } = useWorkflowStore();
    const { mutate: createWorkflow, isPending } = useCreateWorkflow();
    
    // Update store submission state when hook state changes - using useEffect to avoid state updates during render
    useEffect(() => {
        setSubmitting(isPending);
    }, [isPending, setSubmitting]);
    
    // Initialize form with react-hook-form and zod validation
    const form = useForm<WorkflowFormValues>({
        resolver: zodResolver(workflowFormSchema),
        defaultValues,
    });

    // Form submission handler
    function onSubmit(data: WorkflowFormValues) {
        createWorkflow(data, {
            onSuccess: () => {
                closeModal();
                onSubmitForm?.();
            },
            onError: (error) => {
                setError(error.message || 'Failed to create workflow');
            }
        });
    }

    return (
        <Form {...form}>
            <form
                onSubmit={form.handleSubmit(onSubmit)}
                className="space-y-4 py-4"
                id="workflow-create-form"
            >
                <FormField
                    control={form.control}
                    name="name"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Name</FormLabel>
                            <FormControl>
                                <Input
                                    placeholder="My Awesome Workflow"
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="description"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Description</FormLabel>
                            <FormControl>
                                <Textarea
                                    placeholder="Describe what this workflow does..."
                                    className="resize-none"
                                    {...field}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
            </form>
        </Form>
    );
};

// Export the useCreateWorkflow hook for other components
export { useCreateWorkflow };
