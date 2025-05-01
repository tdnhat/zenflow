import { NodeKind } from '../enums/node-kind.enum';

/**
 * Represents a property definition for a node
 */
export interface NodePropertyDefinition {
  name: string;
  label: string;
  type: 'text' | 'number' | 'boolean' | 'select' | 'code' | 'json';
  defaultValue: any;
  required: boolean;
  options?: Array<{ label: string; value: string }>;
}

/**
 * Defines a node type that can be used in workflows
 */
export interface NodeTypeDefinition {
  type: string;
  label: string;
  category: string;
  kind: NodeKind;
  description?: string;
  icon: string;
  properties: NodePropertyDefinition[];
}

/**
 * Request to get node types by category
 */
export interface GetNodeTypesByCategoryRequest {
  category?: string;
}

/**
 * Response containing node types
 */
export interface GetNodeTypesResponse {
  nodeTypes: NodeTypeDefinition[];
  categories: string[];
}