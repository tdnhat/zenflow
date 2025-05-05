import { Workflow, WorkflowExecution } from '@/models/workflow.model';
import apiClient from './client';

// Base workflow API path
const API_PATH = '/api/workflows';

/**
 * Service for workflow-related API operations
 */
export const WorkflowService = {
  /**
   * Get all workflows
   */
  getAll: async (): Promise<Workflow[]> => {
    const response = await apiClient.get(API_PATH);
    return response.data;
  },

  /**
   * Get a specific workflow by ID
   */
  getById: async (id: string): Promise<Workflow> => {
    const response = await apiClient.get(`${API_PATH}/${id}`);
    return response.data;
  },

  /**
   * Create a new workflow
   */
  create: async (workflow: Partial<Workflow>): Promise<Workflow> => {
    const response = await apiClient.post(API_PATH, workflow);
    return response.data;
  },

  /**
   * Update an existing workflow
   */
  update: async (id: string, workflow: Partial<Workflow>): Promise<Workflow> => {
    const response = await apiClient.put(`${API_PATH}/${id}`, workflow);
    return response.data;
  },

  /**
   * Delete a workflow
   */
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`${API_PATH}/${id}`);
  },

  /**
   * Run a workflow
   */
  run: async (id: string, input?: Record<string, unknown>): Promise<WorkflowExecution> => {
    const response = await apiClient.post(`${API_PATH}/${id}/run`, { input });
    return response.data;
  },

  /**
   * Cancel a workflow execution
   */
  cancel: async (id: string, executionId?: string): Promise<WorkflowExecution> => {
    const response = await apiClient.post(`${API_PATH}/${id}/cancel`, { executionId });
    return response.data;
  },

  /**
   * Get workflow executions for a specific workflow
   */
  getExecutions: async (id: string): Promise<WorkflowExecution[]> => {
    const response = await apiClient.get(`${API_PATH}/${id}/executions`);
    return response.data;
  }
}; 