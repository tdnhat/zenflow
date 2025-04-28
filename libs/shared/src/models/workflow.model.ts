import { WorkflowStatus } from '../enums/workflow-status.enum';

/**
 * Represents a workflow in the ZenFlow system
 */
export interface Workflow {
  id: string;
  name: string;
  description?: string;
  status: WorkflowStatus;
  version: number;
  createdAt: Date;
  updatedAt: Date;
  createdBy: string;
  updatedBy?: string;
  nodes: WorkflowNode[];
  edges: WorkflowEdge[];
  tags?: string[];
}

/**
 * Represents a node in a workflow
 */
export interface WorkflowNode {
  id: string;
  type: string;
  label: string;
  position: {
    x: number;
    y: number;
  };
  data: Record<string, any>;
  style?: Record<string, any>;
}

/**
 * Represents a connection between two nodes in a workflow
 */
export interface WorkflowEdge {
  id: string;
  source: string;
  target: string;
  sourceHandle?: string;
  targetHandle?: string;
  label?: string;
  type?: string;
  data?: Record<string, any>;
  style?: Record<string, any>;
}