# Core Workflow JSON Structure Definition

This document outlines the proposed JSON structure for the core workflow definition. This structure serves as the top-level container for all workflow elements, including nodes, edges, and global configurations.

## Requirements Addressed:

*   **Serialization/Deserialization**: The structure uses common JSON types (strings, numbers, objects, arrays) that are easily handled by standard serializers/deserializers in both JavaScript (frontend) and C# (backend).
*   **Modularity and DDD**: The separation of `nodes`, `edges`, and `globalVariables` promotes a modular design. The overall workflow acts as an aggregate root in DDD terms.
*   **Scalability**: The structure is designed to be extensible, allowing for future additions to `metadata` or `executionSettings` without breaking existing implementations.

## Core Workflow Schema:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "WorkflowDefinition",
  "description": "Defines the structure for a complete workflow, including its constituent nodes, edges, and metadata.",
  "type": "object",
  "properties": {
    "workflowId": {
      "type": "string",
      "format": "uuid",
      "description": "A unique identifier for the workflow (e.g., a UUID)."
    },
    "name": {
      "type": "string",
      "description": "A user-friendly name for the workflow."
    },
    "version": {
      "type": ["integer", "string"],
      "description": "Version of the workflow definition (e.g., 1, or \"1.0.0\")."
    },
    "description": {
      "type": "string",
      "description": "An optional detailed description of the workflow's purpose."
    },
    "nodes": {
      "type": "array",
      "items": {
        "$ref": "#/definitions/Node"
      },
      "description": "An array of node objects representing the tasks or activities within the workflow. The Node schema will be defined separately."
    },
    "edges": {
      "type": "array",
      "items": {
        "$ref": "#/definitions/Edge"
      },
      "description": "An array of edge objects representing the connections and transitions between nodes. The Edge schema will be defined separately."
    },
    "globalVariables": {
      "type": "object",
      "additionalProperties": true,
      "description": "Key-value pairs for global workflow variables, accessible by all nodes. Values can be of any JSON-compatible type."
    },
    "metadata": {
      "type": "object",
      "properties": {
        "createdAt": {
          "type": "string",
          "format": "date-time",
          "description": "Timestamp of when the workflow was created (ISO 8601 format)."
        },
        "updatedAt": {
          "type": "string",
          "format": "date-time",
          "description": "Timestamp of when the workflow was last updated (ISO 8601 format)."
        },
        "createdBy": {
          "type": "string",
          "description": "Identifier of the user or system that created the workflow (optional)."
        },
        "tags": {
          "type": "array",
          "items": {
            "type": "string"
          },
          "description": "Optional tags for categorizing or searching workflows."
        }
      },
      "required": ["createdAt", "updatedAt"]
    },
    "executionSettings": {
      "type": "object",
      "properties": {
        "defaultTimeoutSeconds": {
          "type": "integer",
          "minimum": 0,
          "description": "Default timeout in seconds for individual tasks if not overridden at the node level (optional)."
        },
        "defaultRetryPolicy": {
          "type": "object",
          "properties": {
            "maxRetries": {
              "type": "integer",
              "minimum": 0
            },
            "retryDelaySeconds": {
              "type": "integer",
              "minimum": 0
            }
          },
          "description": "Default retry policy for failed tasks if not overridden (optional)."
        }
      },
      "description": "Global settings that can influence the execution behavior of the workflow and its nodes."
    }
  },
  "required": [
    "workflowId",
    "name",
    "version",
    "nodes",
    "edges",
    "metadata"
  ],
  "definitions": {
    "Node": {
      "type": "object",
      "description": "Placeholder for Node definition. This will be detailed in the next step."
    },
    "Edge": {
      "type": "object",
      "description": "Placeholder for Edge definition. This will be detailed in a subsequent step."
    }
  }
}
```

This core structure provides a comprehensive yet flexible foundation. The `nodes` and `edges` properties will be populated with arrays of objects whose schemas will be defined in the subsequent steps of this design process. The `globalVariables` allows for workflow-wide configuration, and `executionSettings` can define default behaviors for tasks.

