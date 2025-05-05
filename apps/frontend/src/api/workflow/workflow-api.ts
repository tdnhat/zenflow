import { WorkflowFormValues } from "@/app/(main)/workflows/_schemas/workflow.schemas";
import api from "@/lib/axios";
import { WorkflowDto, WorkflowDetailDto } from "@/types/workflow.type";

// Base endpoint for workflows
const WORKFLOWS_ENDPOINT = "/workflows";

// Workflow CRUD operations
export const fetchWorkflows = async (): Promise<WorkflowDto[]> => {
    const response = await api.get(WORKFLOWS_ENDPOINT);
    return response.data;
};

export const fetchWorkflowById = async (
    id: string
): Promise<WorkflowDetailDto> => {
    const response = await api.get(`${WORKFLOWS_ENDPOINT}/${id}`);
    return response.data;
};

export const createWorkflow = async (
    data: WorkflowFormValues
): Promise<WorkflowDto> => {
    const response = await api.post(WORKFLOWS_ENDPOINT, data);
    return response.data;
};
