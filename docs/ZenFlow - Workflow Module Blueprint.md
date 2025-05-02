# ZenFlow Implementation Plan: Web Automation Focus

## 1. Domain Model for Web Automation Workflows

### 1.1 Core Entities

- **Workflow**
  - Properties: Id, Name, Description, Status, CreatedAt/By, ModifiedAt/By
  - Status options: Draft, Active, Archived

- **WorkflowNode**
  - Properties: Id, WorkflowId, Type, Position (x, y), Data (JSON), Label
  - Types: WebTrigger, ScrollAction, ClickAction, InputAction, WaitAction, ConditionNode, etc.

- **WorkflowEdge**
  - Properties: Id, WorkflowId, SourceNodeId, TargetNodeId, Label, EdgeType
  - EdgeType options: Success, Failure, Condition

- **WorkflowRun**
  - Properties: Id, WorkflowId, Status, StartedAt, CompletedAt, InitiatedBy
  - Status options: Running, Completed, Failed, Cancelled

- **NodeExecution**
  - Properties: Id, WorkflowRunId, NodeId, Status, StartedAt, CompletedAt, Output (JSON), Error (JSON)
  - Status options: Pending, Running, Completed, Failed, Skipped

## 2. Feature Requirements & Endpoints

### 2.1 Workflow Management

**Features:**
- Create, edit, and delete workflows
- List and search workflows
- Clone existing workflows
- Import/export workflows as JSON

**Endpoints:**
```
GET    /api/v1/workflows              - Get all workflows
GET    /api/v1/workflows/{id}         - Get workflow by ID
POST   /api/v1/workflows              - Create new workflow
PUT    /api/v1/workflows/{id}         - Update workflow
DELETE /api/v1/workflows/{id}         - Delete workflow (soft delete)
POST   /api/v1/workflows/{id}/clone   - Clone a workflow
GET    /api/v1/workflows/export/{id}  - Export workflow as JSON
POST   /api/v1/workflows/import       - Import workflow from JSON
```

### 2.2 Node & Edge Management

**Features:**
- Add, update, delete nodes in workflows
- Connect nodes with edges
- Configure node properties specific to action type
- Validate workflow structure

**Endpoints:**
```
GET    /api/v1/workflows/{id}/nodes            - Get all nodes in a workflow
GET    /api/v1/workflows/{id}/edges            - Get all edges in a workflow
POST   /api/v1/workflows/{id}/nodes            - Add a node to workflow
PUT    /api/v1/workflows/{id}/nodes/{nodeId}   - Update a node
DELETE /api/v1/workflows/{id}/nodes/{nodeId}   - Delete a node
POST   /api/v1/workflows/{id}/edges            - Add an edge between nodes
PUT    /api/v1/workflows/{id}/edges/{edgeId}   - Update an edge
DELETE /api/v1/workflows/{id}/edges/{edgeId}   - Delete an edge
POST   /api/v1/workflows/{id}/validate         - Validate workflow structure
```

### 2.3 Workflow Execution

**Features:**
- Run workflows manually
- View execution status and history
- Pause/resume workflow execution
- Debug workflow with step-by-step execution

**Endpoints:**
```
POST   /api/v1/workflows/{id}/run      - Run a workflow
GET    /api/v1/runs                    - Get all workflow runs
GET    /api/v1/runs/{runId}            - Get a specific run
GET    /api/v1/runs/{runId}/nodes      - Get node executions for a run
POST   /api/v1/runs/{runId}/cancel     - Cancel a running workflow
POST   /api/v1/workflows/{id}/debug    - Run workflow in debug mode
```

### 2.4 Node Type Registry

**Features:**
- Register built-in node types
- Get available node types and their configurations
- Group node types by category

**Endpoints:**
```
GET    /api/v1/node-types              - Get all available node types
GET    /api/v1/node-types/{type}       - Get details for a specific node type
GET    /api/v1/node-types/categories   - Get node types grouped by category
```

## 3. Web Automation Node Types (MVP)

For the MVP, focus on these basic web automation node types:

### 3.1 Trigger Nodes
- **WebTrigger** - Start workflow execution manually
- **ScheduleTrigger** - Start workflow at scheduled times
- **WebhookTrigger** - Start workflow via HTTP webhook

### 3.2 Browser Actions
- **OpenPageNode** - Open a specific URL
- **ScrollNode** - Scroll up/down a page
- **ClickNode** - Click an element (with selector options)
- **InputNode** - Enter text into a form field
- **WaitNode** - Wait for element or fixed time
- **ScreenshotNode** - Take screenshot of page
- **ExtractDataNode** - Extract data from page (text, attribute)

### 3.3 Control Flow
- **ConditionNode** - Branch workflow based on conditions
- **LoopNode** - Loop through items or repeat actions
- **DelayNode** - Add a timed delay between steps

### 3.4 Utility Nodes
- **LogNode** - Log information for debugging
- **JsonTransformNode** - Transform data structure
- **VariableSetNode** - Set workflow variables
- **VariableGetNode** - Get workflow variables

## 4. React Flow Integration

### 4.1 Node Components

Create custom React Flow node components for each node type:

```tsx
// Example structure for custom node components
export const BrowserActionNode = memo(({ data, selected }: NodeProps) => {
  // Node implementation with action-specific UI
  return (
    <div className={`node ${selected ? 'selected' : ''} browser-action-node`}>
      <div className="node-header">
        <div className="node-type">{data.type}</div>
        <div className="node-title">{data.label}</div>
      </div>
      <div className="node-content">
        {/* Node-specific content based on type */}
        {data.type === 'click' && (
          <div className="node-field">
            <label>Selector</label>
            <div className="node-value">{data.selector || 'Not set'}</div>
          </div>
        )}
        {/* Other node-specific fields */}
      </div>
      <Handle type="target" position={Position.Top} />
      <Handle type="source" position={Position.Bottom} />
    </div>
  );
});
```

### 4.2 Edge Components

Custom edges to show flow between nodes:

```tsx
export const CustomEdge = ({
  id,
  sourceX,
  sourceY,
  targetX,
  targetY,
  sourcePosition,
  targetPosition,
  data,
}: EdgeProps) => {
  // Path calculation
  const [edgePath] = getBezierPath({
    sourceX, sourceY, sourcePosition,
    targetX, targetY, targetPosition,
  });
  
  return (
    <>
      <path
        id={id}
        className="react-flow__edge-path"
        d={edgePath}
        strokeWidth={2}
        stroke={data?.type === 'success' ? '#22c55e' : '#ef4444'}
      />
      {data?.label && (
        <EdgeLabelRenderer>
          <div
            style={{
              position: 'absolute',
              transform: `translate(-50%, -50%) translate(${(sourceX + targetX) / 2}px, ${(sourceY + targetY) / 2}px)`,
              pointerEvents: 'all',
              background: 'white',
              padding: '2px 4px',
              borderRadius: 4,
              fontSize: 12,
              fontWeight: 500,
            }}
            className="edge-label"
          >
            {data.label}
          </div>
        </EdgeLabelRenderer>
      )}
    </>
  );
};
```

### 4.3 Node Configuration Panel

For editing node properties:

```tsx
export const NodeConfigPanel = ({ node, onChange, onClose }) => {
  // Different forms based on node type
  const renderForm = () => {
    switch (node?.type) {
      case 'click':
        return <ClickNodeForm node={node} onChange={onChange} />;
      case 'input':
        return <InputNodeForm node={node} onChange={onChange} />;
      // Other node type forms
      default:
        return <p>Select a node to configure</p>;
    }
  };

  return (
    <div className="node-config-panel">
      <div className="config-panel-header">
        <h3>{node ? `Configure ${node.data.label}` : 'Node Configuration'}</h3>
        <button onClick={onClose}>Close</button>
      </div>
      <div className="config-panel-content">
        {node ? renderForm() : <p>Select a node to configure</p>}
      </div>
    </div>
  );
};
```

### 4.4 Workflow Editor State Management

Using Zustand for managing the React Flow state:

```tsx
// workflows/store/workflowEditorStore.ts
import { create } from 'zustand';
import { 
  Edge, 
  Node, 
  addEdge, 
  Connection, 
  OnNodesChange, 
  OnEdgesChange, 
  OnConnect, 
  applyNodeChanges, 
  applyEdgeChanges 
} from 'reactflow';

interface WorkflowEditorState {
  workflowId: string | null;
  nodes: Node[];
  edges: Edge[];
  selectedNode: Node | null;
  isDirty: boolean;
  
  // Actions
  setWorkflowId: (id: string) => void;
  onNodesChange: OnNodesChange;
  onEdgesChange: OnEdgesChange;
  onConnect: OnConnect;
  addNode: (node: Node) => void;
  updateNode: (nodeId: string, data: any) => void;
  removeNode: (nodeId: string) => void;
  setSelectedNode: (node: Node | null) => void;
  clearEditor: () => void;
  loadWorkflow: (nodes: Node[], edges: Edge[]) => void;
  setIsDirty: (isDirty: boolean) => void;
}

export const useWorkflowEditorStore = create<WorkflowEditorState>((set, get) => ({
  workflowId: null,
  nodes: [],
  edges: [],
  selectedNode: null,
  isDirty: false,

  setWorkflowId: (id) => set({ workflowId: id }),
  
  onNodesChange: (changes) => {
    set((state) => ({
      nodes: applyNodeChanges(changes, state.nodes),
      isDirty: true
    }));
  },
  
  onEdgesChange: (changes) => {
    set((state) => ({
      edges: applyEdgeChanges(changes, state.edges),
      isDirty: true
    }));
  },
  
  onConnect: (connection) => {
    set((state) => ({
      edges: addEdge({
        ...connection,
        type: 'custom', // Use your custom edge
        animated: true,
        data: { label: 'Success', type: 'success' }
      }, state.edges),
      isDirty: true
    }));
  },
  
  addNode: (node) => {
    set((state) => ({ 
      nodes: [...state.nodes, node],
      isDirty: true
    }));
  },
  
  updateNode: (nodeId, data) => {
    set((state) => ({
      nodes: state.nodes.map((node) => 
        node.id === nodeId ? { ...node, data: { ...node.data, ...data } } : node
      ),
      isDirty: true
    }));
  },
  
  removeNode: (nodeId) => {
    set((state) => ({
      nodes: state.nodes.filter((node) => node.id !== nodeId),
      edges: state.edges.filter(
        (edge) => edge.source !== nodeId && edge.target !== nodeId
      ),
      isDirty: true
    }));
  },
  
  setSelectedNode: (node) => {
    set({ selectedNode: node });
  },
  
  clearEditor: () => {
    set({ 
      nodes: [], 
      edges: [], 
      selectedNode: null,
      isDirty: false
    });
  },
  
  loadWorkflow: (nodes, edges) => {
    set({ 
      nodes, 
      edges,
      selectedNode: null,
      isDirty: false
    });
  },
  
  setIsDirty: (isDirty) => set({ isDirty })
}));
```

## 5. Database Schema for Web Automation

```sql
-- Workflow definitions
CREATE TABLE "workflow"."Workflows" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" TEXT,
    "Status" VARCHAR(20) NOT NULL,
    -- Audit fields
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(50) NOT NULL,
    "LastModifiedAt" TIMESTAMP NULL,
    "LastModifiedBy" VARCHAR(50) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedAt" TIMESTAMP NULL,
    "DeletedBy" VARCHAR(50) NULL
);

CREATE TABLE "workflow"."WorkflowNodes" (
    "Id" UUID PRIMARY KEY,
    "WorkflowId" UUID NOT NULL,
    "Type" VARCHAR(50) NOT NULL,
    "Label" VARCHAR(100) NOT NULL,
    "PositionX" FLOAT NOT NULL,
    "PositionY" FLOAT NOT NULL,
    "Data" JSONB NOT NULL,
    -- Audit fields
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(50) NOT NULL,
    "LastModifiedAt" TIMESTAMP NULL,
    "LastModifiedBy" VARCHAR(50) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedAt" TIMESTAMP NULL,
    "DeletedBy" VARCHAR(50) NULL,
    FOREIGN KEY ("WorkflowId") REFERENCES "workflow"."Workflows"("Id")
);

CREATE TABLE "workflow"."WorkflowEdges" (
    "Id" UUID PRIMARY KEY,
    "WorkflowId" UUID NOT NULL,
    "SourceNodeId" UUID NOT NULL,
    "TargetNodeId" UUID NOT NULL,
    "Label" VARCHAR(100) NULL,
    "EdgeType" VARCHAR(20) NOT NULL,
    "Data" JSONB NULL,
    -- Audit fields
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(50) NOT NULL,
    "LastModifiedAt" TIMESTAMP NULL,
    "LastModifiedBy" VARCHAR(50) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedAt" TIMESTAMP NULL,
    "DeletedBy" VARCHAR(50) NULL,
    FOREIGN KEY ("WorkflowId") REFERENCES "workflow"."Workflows"("Id"),
    FOREIGN KEY ("SourceNodeId") REFERENCES "workflow"."WorkflowNodes"("Id"),
    FOREIGN KEY ("TargetNodeId") REFERENCES "workflow"."WorkflowNodes"("Id")
);

CREATE TABLE "workflow"."WorkflowRuns" (
    "Id" UUID PRIMARY KEY,
    "WorkflowId" UUID NOT NULL,
    "Status" VARCHAR(20) NOT NULL,
    "StartedAt" TIMESTAMP NOT NULL,
    "CompletedAt" TIMESTAMP NULL,
    "InitiatedBy" UUID NOT NULL,
    -- Audit fields
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(50) NOT NULL,
    "LastModifiedAt" TIMESTAMP NULL,
    "LastModifiedBy" VARCHAR(50) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedAt" TIMESTAMP NULL,
    "DeletedBy" VARCHAR(50) NULL,
    FOREIGN KEY ("WorkflowId") REFERENCES "workflow"."Workflows"("Id"),
    FOREIGN KEY ("InitiatedBy") REFERENCES "user"."Users"("Id")
);

CREATE TABLE "workflow"."NodeExecutions" (
    "Id" UUID PRIMARY KEY,
    "WorkflowRunId" UUID NOT NULL,
    "NodeId" UUID NOT NULL,
    "Status" VARCHAR(20) NOT NULL,
    "StartedAt" TIMESTAMP NOT NULL,
    "CompletedAt" TIMESTAMP NULL,
    "Output" JSONB NULL,
    "Error" JSONB NULL,
    -- Audit fields
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(50) NOT NULL,
    "LastModifiedAt" TIMESTAMP NULL,
    "LastModifiedBy" VARCHAR(50) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedAt" TIMESTAMP NULL,
    "DeletedBy" VARCHAR(50) NULL,
    FOREIGN KEY ("WorkflowRunId") REFERENCES "workflow"."WorkflowRuns"("Id"),
    FOREIGN KEY ("NodeId") REFERENCES "workflow"."WorkflowNodes"("Id")
);
```

## 6. Implementation Phases for Web Automation Focus

### Phase 1: Core Infrastructure & UI Workflow Editor (2 weeks)
- Set up project structure (monorepo with frontend/backend)
- Implement authentication with Keycloak
- Create basic workflow CRUD operations
- Implement React Flow integration with custom nodes
- Build workflow editor UI with node palette

### Phase 2: Web Automation Node Types (2 weeks)
- Implement browser action nodes (click, scroll, input)
- Create node configuration panels
- Implement workflow validation
- Add workflow export/import functionality

### Phase 3: Workflow Execution Engine (2 weeks)
- Build a headless browser automation engine (Puppeteer/Playwright)
- Implement workflow execution service
- Create execution logs and history
- Add basic error handling and retry logic

### Phase 4: Advanced Features & Polish (2 weeks)
- Add conditional flows and branching
- Implement variables and data extraction
- Add debugging tools and step execution
- Polish UI and improve user experience

### Phase 5: Future Expansion Prep (1 week)
- Document architecture for API integration nodes
- Create scaffolding for external API connectors
- Implement simple webhook functionality
- Prepare for social media and other API integrations

## 7. Frontend Components for React Flow

### 7.1 Main Workflow Editor Component

```tsx
// pages/workflows/[id]/edit.tsx
import { useCallback, useEffect, useState, useRef } from 'react';
import ReactFlow, {
  Background,
  Controls,
  MiniMap,
  Panel,
  ReactFlowProvider,
  useReactFlow,
} from 'reactflow';
import 'reactflow/dist/style.css';

import { useWorkflowEditorStore } from '@/store/workflowEditorStore';
import { useWorkflowApi } from '@/hooks/useWorkflowApi';
import NodeToolbox from '@/components/workflow/NodeToolbox';
import NodeConfigPanel from '@/components/workflow/NodeConfigPanel';
import { CustomNode, CustomEdge } from '@/components/workflow/CustomFlowElements';

// Register custom node types
const nodeTypes = {
  openPage: CustomNode,
  click: CustomNode,
  input: CustomNode,
  scroll: CustomNode,
  wait: CustomNode,
  condition: CustomNode,
  // Add more node types as needed
};

// Register custom edge types
const edgeTypes = {
  custom: CustomEdge,
};

export default function WorkflowEditor() {
  const { workflowId, nodes, edges, selectedNode, isDirty,
          onNodesChange, onEdgesChange, onConnect, setSelectedNode,
          loadWorkflow, setIsDirty } = useWorkflowEditorStore();
  
  const { getWorkflow, saveWorkflow } = useWorkflowApi();
  const reactFlowWrapper = useRef(null);
  const { project } = useReactFlow();

  // Load workflow data when component mounts
  useEffect(() => {
    const fetchWorkflow = async () => {
      if (workflowId) {
        try {
          const workflow = await getWorkflow(workflowId);
          if (workflow) {
            loadWorkflow(workflow.nodes, workflow.edges);
          }
        } catch (error) {
          console.error('Error loading workflow:', error);
        }
      }
    };
    
    fetchWorkflow();
  }, [workflowId, getWorkflow, loadWorkflow]);

  // Handle node selection
  const onNodeClick = useCallback((event, node) => {
    setSelectedNode(node);
  }, [setSelectedNode]);

  // Handle saving the workflow
  const handleSave = useCallback(async () => {
    if (!workflowId) return;
    
    try {
      await saveWorkflow(workflowId, {
        nodes,
        edges
      });
      setIsDirty(false);
    } catch (error) {
      console.error('Error saving workflow:', error);
    }
  }, [workflowId, nodes, edges, saveWorkflow, setIsDirty]);

  // Handle node dropping from toolbox
  const onDragOver = useCallback((event) => {
    event.preventDefault();
    event.dataTransfer.dropEffect = 'move';
  }, []);

  const onDrop = useCallback(
    (event) => {
      event.preventDefault();

      const reactFlowBounds = reactFlowWrapper.current.getBoundingClientRect();
      const nodeType = event.dataTransfer.getData('application/reactflow/type');
      const nodeLabel = event.dataTransfer.getData('application/reactflow/label');
      
      // Check if the dropped element is valid
      if (!nodeType || typeof nodeType !== 'string') {
        return;
      }

      const position = project({
        x: event.clientX - reactFlowBounds.left,
        y: event.clientY - reactFlowBounds.top,
      });

      const newNode = {
        id: `node_${Date.now()}`,
        type: nodeType,
        position,
        data: { 
          label: nodeLabel || `${nodeType} node`,
          type: nodeType 
        },
      };

      useWorkflowEditorStore.getState().addNode(newNode);
    },
    [project]
  );

  // Update node configuration
  const handleNodeConfigChange = useCallback((nodeId, updatedData) => {
    useWorkflowEditorStore.getState().updateNode(nodeId, updatedData);
  }, []);

  return (
    <div className="workflow-editor">
      <div className="editor-header">
        <h1>Edit Workflow</h1>
        <div className="actions">
          <button 
            className={`save-button ${isDirty ? 'dirty' : ''}`}
            onClick={handleSave}
            disabled={!isDirty}
          >
            Save Workflow
          </button>
          <button className="run-button">Run Workflow</button>
        </div>
      </div>
      
      <div className="editor-layout">
        <NodeToolbox />
        
        <div className="flow-container" ref={reactFlowWrapper}>
          <ReactFlowProvider>
            <ReactFlow
              nodes={nodes}
              edges={edges}
              onNodesChange={onNodesChange}
              onEdgesChange={onEdgesChange}
              onConnect={onConnect}
              onNodeClick={onNodeClick}
              nodeTypes={nodeTypes}
              edgeTypes={edgeTypes}
              onDrop={onDrop}
              onDragOver={onDragOver}
              fitView
              attributionPosition="bottom-right"
            >
              <Controls />
              <MiniMap />
              <Background variant="dots" gap={12} size={1} />
              
              <Panel position="top-right">
                <div className="editor-stats">
                  <div>Nodes: {nodes.length}</div>
                  <div>Connections: {edges.length}</div>
                </div>
              </Panel>
            </ReactFlow>
          </ReactFlowProvider>
        </div>
        
        {selectedNode && (
          <NodeConfigPanel 
            node={selectedNode} 
            onChange={handleNodeConfigChange}
            onClose={() => setSelectedNode(null)}  
          />
        )}
      </div>
    </div>
  );
}
```

### 7.2 Node Toolbox Component

```tsx
// components/workflow/NodeToolbox.tsx
import { useCallback, useState } from 'react';

// Node type definitions with icons and categories
const NODE_TYPES = [
  {
    category: 'Browser Actions',
    nodes: [
      { type: 'openPage', label: 'Open Page', icon: '🌐' },
      { type: 'click', label: 'Click Element', icon: '👆' },
      { type: 'input', label: 'Input Text', icon: '✏️' },
      { type: 'scroll', label: 'Scroll Page', icon: '📜' },
      { type: 'wait', label: 'Wait', icon: '⏱️' },
      { type: 'screenshot', label: 'Take Screenshot', icon: '📸' },
      { type: 'extractData', label: 'Extract Data', icon: '🔍' },
    ]
  },
  {
    category: 'Control Flow',
    nodes: [
      { type: 'condition', label: 'Condition', icon: '🔀' },
      { type: 'loop', label: 'Loop', icon: '🔄' },
      { type: 'delay', label: 'Delay', icon: '⏲️' },
    ]
  },
  {
    category: 'Utility',
    nodes: [
      { type: 'log', label: 'Log', icon: '📝' },
      { type: 'variable', label: 'Set Variable', icon: '💾' },
    ]
  }
];

export default function NodeToolbox() {
  const [searchTerm, setSearchTerm] = useState('');
  const [expandedCategories, setExpandedCategories] = useState<string[]>(NODE_TYPES.map(c => c.category));
  
  // Filter nodes based on search term
  const filteredNodeTypes = NODE_TYPES.map(category => ({
    ...category,
    nodes: category.nodes.filter(node => 
      node.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
      node.type.toLowerCase().includes(searchTerm.toLowerCase())
    )
  })).filter(category => category.nodes.length > 0);
  
  // Toggle category expansion
  const toggleCategory = useCallback((category: string) => {
    setExpandedCategories(prev => 
      prev.includes(category)
        ? prev.filter(c => c !== category)
        : [...prev, category]
    );
  }, []);
  
  // Handle drag start
  const onDragStart = useCallback((event, nodeType, nodeLabel) => {
    event.dataTransfer.setData('application/reactflow/type', nodeType);
    event.dataTransfer.setData('application/reactflow/label', nodeLabel);
    event.dataTransfer.effectAllowed = 'move';
  }, []);
  
  return (
    <div className="node-toolbox">
      <div className="toolbox-header">
        <h3>Node Types</h3>
        <div className="search-container">
          <input
            type="text"
            placeholder="Search nodes..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />
        </div>
      </div>
      
      <div className="node-categories">
        {filteredNodeTypes.map((category) => (
          <div key={category.category} className="node-category">
            <div 
              className="category-header" 
              onClick={() => toggleCategory(category.category)}
            >
              <span>{category.category}</span>
              <span className="expand-icon">
                {expandedCategories.includes(category.category) ? '▼' : '►'}
              </span>
            </div>
            
            {expandedCategories.includes(category.category) && (
              <div className="category-nodes">
                {category.nodes.map((node) => (
                  <div
                    key={node.type}
                    className="dnd-node"
                    onDragStart={(event) => onDragStart(event, node.type, node.label)}
                    draggable
                  >
                    <span className="node-icon">{node.icon}</span>
                    <span className="node-label">{node.label}</span>
                  </div>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
```

## 8. Backend Integration for Web Automation

### 8.1 NodeType Registration System

```csharp
// Workflow Module - NodeType Registration
public class NodeTypeRegistry
{
    private readonly Dictionary<string, NodeTypeDefinition> _nodeTypes = new();
    
    // Register built-in node types
    public NodeTypeRegistry()
    {
        // Browser actions
        RegisterNodeType(new OpenPageNodeType());
        RegisterNodeType(new ClickNodeType());
        RegisterNodeType(new InputNodeType());
        RegisterNodeType(new ScrollNodeType());
        RegisterNodeType(new WaitNodeType());
        RegisterNodeType(new ScreenshotNodeType());
        RegisterNodeType(new ExtractDataNodeType());
        
        // Control flow
        RegisterNodeType(new ConditionNodeType());
        RegisterNodeType(new LoopNodeType());
        RegisterNodeType(new DelayNodeType());
        
        // Utility
        RegisterNodeType(new LogNodeType());
        RegisterNodeType(new VariableNodeType());
    }
    
    public void RegisterNodeType(NodeTypeDefinition nodeType)
    {
        _nodeTypes[nodeType.Type] = nodeType;
    }
    
    public NodeTypeDefinition GetNodeType(string type)
    {
        if (_nodeTypes.TryGetValue(type, out var nodeType))
        {
            return nodeType;
        }
        
        throw new ArgumentException($"Node type '{type}' is not registered");
    }
    
    public IEnumerable<NodeTypeDefinition> GetNodeTypes()
    {
        return _nodeTypes.Values;
    }
    
    public IEnumerable<NodeTypeDefinition> GetNodeTypesByCategory(string category)
    {
        return _nodeTypes.Values.Where(n => n.Category == category);
    }
    
    public IEnumerable<string> GetCategories()
    {
        return _nodeTypes.Values.Select(n => n.Category).Distinct();
    }
}

// Base class for node type definitions
public abstract class NodeTypeDefinition
{
    public abstract string Type { get; }
    public abstract string Label { get; }
    public abstract string Category { get; }
    public abstract string Icon { get; }
    public abstract List<NodeProperty> Properties { get; }
    
    // Define the execution behavior
    public abstract Task<NodeExecutionResult> ExecuteAsync(
        NodeExecutionContext context,
        IDictionary<string, object> nodeData);
}

public class NodeProperty
{
    public string Name { get; set; }
    public string Label { get; set; }
    public string Type { get; set; } // text, number, boolean, select
    public object DefaultValue { get; set; }
    public bool Required { get; set; }
    public List<OptionValue> Options { get; set; } = new List<OptionValue>();
}

public class OptionValue
{
    public string Label { get; set; }
    public string Value { get; set; }
}
```

### 8.2 Web Automation Execution Engine

```csharp
// Web Automation Executor Service
public class WebAutomationExecutor : IWorkflowExecutor
{
    private readonly ILogger<WebAutomationExecutor> _logger;
    private readonly NodeTypeRegistry _nodeTypeRegistry;
    private readonly WorkflowRepository _workflowRepository;
    private readonly WorkflowRunRepository _runRepository;
    private readonly NodeExecutionRepository _nodeExecutionRepository;
    
    public WebAutomationExecutor(
        ILogger<WebAutomationExecutor> logger,
        NodeTypeRegistry nodeTypeRegistry,
        WorkflowRepository workflowRepository,
        WorkflowRunRepository runRepository,
        NodeExecutionRepository nodeExecutionRepository)
    {
        _logger = logger;
        _nodeTypeRegistry = nodeTypeRegistry;
        _workflowRepository = workflowRepository;
        _runRepository = runRepository;
        _nodeExecutionRepository = nodeExecutionRepository;
    }
    
    public async Task<WorkflowRun> ExecuteWorkflowAsync(Guid workflowId, Guid userId)
    {
        // Create a new run record
        var run = new WorkflowRun
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            Status = "Running",
            StartedAt = DateTime.UtcNow,
            InitiatedBy = userId
        };
        
        await _runRepository.AddAsync(run);
        
        try
        {
            // Get workflow definition
            var workflow = await _workflowRepository.GetByIdWithNodesAndEdgesAsync(workflowId);
            if (workflow == null)
            {
                throw new ArgumentException($"Workflow with ID {workflowId} not found");
            }
            
            // Find start node (trigger)
            var startNode = workflow.Nodes.FirstOrDefault(n => 
                n.Type.EndsWith("Trigger") || 
                workflow.Edges.All(e => e.TargetNodeId != n.Id));
            
            if (startNode == null)
            {
                throw new InvalidOperationException("No start node found in workflow");
            }
            
            // Initialize browser automation
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = true
            });
            
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            
            // Initialize execution context
            var executionContext = new NodeExecutionContext
            {
                WorkflowRun = run,
                Page = page,
                Variables = new Dictionary<string, object>(),
                CancellationToken = CancellationToken.None
            };
            
            // Execute the workflow starting from the start node
            await ExecuteNodeAsync(startNode, workflow, executionContext);
            
            // Update run status
            run.Status = "Completed";
            run.CompletedAt = DateTime.UtcNow;
            await _runRepository.UpdateAsync(run);
            
            return run;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow {WorkflowId}", workflowId);
            
            // Update run with error status
            run.Status = "Failed";
            run.CompletedAt = DateTime.UtcNow;
            run.Error = ex.Message;
            await _runRepository.UpdateAsync(run);
            
            return run;
        }
    }
    
    private async Task<NodeExecutionResult> ExecuteNodeAsync(
        WorkflowNode node, 
        Workflow workflow, 
        NodeExecutionContext context)
    {
        // Create node execution record
        var nodeExecution = new NodeExecution
        {
            Id = Guid.NewGuid(),
            WorkflowRunId = context.WorkflowRun.Id,
            NodeId = node.Id,
            Status = "Running",
            StartedAt = DateTime.UtcNow
        };
        
        await _nodeExecutionRepository.AddAsync(nodeExecution);
        
        try
        {
            // Get node type definition
            var nodeType = _nodeTypeRegistry.GetNodeType(node.Type);
            
            // Execute the node
            var result = await nodeType.ExecuteAsync(context, node.Data);
            
            // Update node execution
            nodeExecution.Status = result.Success ? "Completed" : "Failed";
            nodeExecution.CompletedAt = DateTime.UtcNow;
            nodeExecution.Output = result.Output;
            nodeExecution.Error = result.Error;
            await _nodeExecutionRepository.UpdateAsync(nodeExecution);
            
            // If successful, find and execute next nodes
            if (result.Success)
            {
                var outgoingEdges = workflow.Edges
                    .Where(e => e.SourceNodeId == node.Id)
                    .ToList();
                
                foreach (var edge in outgoingEdges)
                {
                    // For conditional edges, check condition
                    if (edge.EdgeType == "condition" && edge.Data.TryGetValue("condition", out var condition))
                    {
                        // Evaluate condition based on result
                        if (!EvaluateCondition(condition.ToString(), result.Output))
                        {
                            continue;
                        }
                    }
                    
                    var nextNode = workflow.Nodes.FirstOrDefault(n => n.Id == edge.TargetNodeId);
                    if (nextNode != null)
                    {
                        await ExecuteNodeAsync(nextNode, workflow, executionContext);
                    }
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing node {NodeId} of type {NodeType}", node.Id, node.Type);
            
            // Update node execution with error
            nodeExecution.Status = "Failed";
            nodeExecution.CompletedAt = DateTime.UtcNow;
            nodeExecution.Error = ex.Message;
            await _nodeExecutionRepository.UpdateAsync(nodeExecution);
            
            return new NodeExecutionResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
    
    private bool EvaluateCondition(string condition, IDictionary<string, object> data)
    {
        // Simple condition evaluation for demo
        // In production, use a proper expression evaluator
        if (string.IsNullOrEmpty(condition))
        {
            return true;
        }
        
        // Implement condition evaluation logic
        // This is a simplified example
        return true;
    }
}

public class NodeExecutionContext
{
    public WorkflowRun WorkflowRun { get; set; }
    public IPage Page { get; set; }
    public IDictionary<string, object> Variables { get; set; }
    public CancellationToken CancellationToken { get; set; }
}

public class NodeExecutionResult
{
    public bool Success { get; set; }
    public IDictionary<string, object> Output { get; set; } = new Dictionary<string, object>();
    public string Error { get; set; }
}
```

## 9. Sample Node Type Implementations

### 9.1 Click Node Implementation

```csharp
public class ClickNodeType : NodeTypeDefinition
{
    public override string Type => "click";
    public override string Label => "Click Element";
    public override string Category => "Browser Actions";
    public override string Icon => "👆";
    
    public override List<NodeProperty> Properties => new()
    {
        new NodeProperty
        {
            Name = "selector",
            Label = "CSS Selector",
            Type = "text",
            DefaultValue = "",
            Required = true
        },
        new NodeProperty
        {
            Name = "clickCount",
            Label = "Click Count",
            Type = "number",
            DefaultValue = 1,
            Required = false
        },
        new NodeProperty
        {
            Name = "delay",
            Label = "Delay After Click (ms)",
            Type = "number",
            DefaultValue = 100,
            Required = false
        }
    };
    
    public override async Task<NodeExecutionResult> ExecuteAsync(
        NodeExecutionContext context,
        IDictionary<string, object> nodeData)
    {
        var result = new NodeExecutionResult { Success = false };
        
        try
        {
            var selector = nodeData.GetValueOrDefault("selector")?.ToString();
            if (string.IsNullOrEmpty(selector))
            {
                result.Error = "Selector is required";
                return result;
            }
            
            int clickCount = 1;
            if (nodeData.TryGetValue("clickCount", out var clickCountObj) && 
                int.TryParse(clickCountObj.ToString(), out var parsedClickCount))
            {
                clickCount = parsedClickCount;
            }
            
            int delay = 100;
            if (nodeData.TryGetValue("delay", out var delayObj) && 
                int.TryParse(delayObj.ToString(), out var parsedDelay))
            {
                delay = parsedDelay;
            }
            
            // Wait for the element to be visible
            await context.Page.WaitForSelectorAsync(selector, new() { State = WaitForSelectorState.Visible });
            
            // Perform the click action
            await context.Page.ClickAsync(selector, new() { ClickCount = clickCount });
            
            // Wait for specified delay
            if (delay > 0)
            {
                await Task.Delay(delay, context.CancellationToken);
            }
            
            result.Success = true;
            result.Output["message"] = $"Clicked element with selector: {selector}";
            return result;
        }
        catch (Exception ex)
        {
            result.Error = $"Click operation failed: {ex.Message}";
            return result;
        }
    }
}
```

### 9.2 Input Node Implementation

```csharp
public class InputNodeType : NodeTypeDefinition
{
    public override string Type => "input";
    public override string Label => "Input Text";
    public override string Category => "Browser Actions";
    public override string Icon => "✏️";
    
    public override List<NodeProperty> Properties => new()
    {
        new NodeProperty
        {
            Name = "selector",
            Label = "CSS Selector",
            Type = "text",
            DefaultValue = "",
            Required = true
        },
        new NodeProperty
        {
            Name = "text",
            Label = "Text to Enter",
            Type = "text",
            DefaultValue = "",
            Required = true
        },
        new NodeProperty
        {
            Name = "clearFirst",
            Label = "Clear Field First",
            Type = "boolean",
            DefaultValue = true,
            Required = false
        },
        new NodeProperty
        {
            Name = "delay",
            Label = "Typing Delay (ms)",
            Type = "number",
            DefaultValue = 50,
            Required = false
        }
    };
    
    public override async Task<NodeExecutionResult> ExecuteAsync(
        NodeExecutionContext context,
        IDictionary<string, object> nodeData)
    {
        var result = new NodeExecutionResult { Success = false };
        
        try
        {
            var selector = nodeData.GetValueOrDefault("selector")?.ToString();
            if (string.IsNullOrEmpty(selector))
            {
                result.Error = "Selector is required";
                return result;
            }
            
            var text = nodeData.GetValueOrDefault("text")?.ToString() ?? "";
            
            bool clearFirst = true;
            if (nodeData.TryGetValue("clearFirst", out var clearFirstObj))
            {
                bool.TryParse(clearFirstObj.ToString(), out clearFirst);
            }
            
            int delay = 50;
            if (nodeData.TryGetValue("delay", out var delayObj) && 
                int.TryParse(delayObj.ToString(), out var parsedDelay))
            {
                delay = parsedDelay;
            }
            
            // Wait for the element to be visible
            await context.Page.WaitForSelectorAsync(selector, new() { State = WaitForSelectorState.Visible });
            
            // Clear the input field if requested
            if (clearFirst)
            {
                await context.Page.EvaluateAsync($"document.querySelector('{selector}').value = ''");
            }
            
            // Type the text
            await context.Page.FillAsync(selector, text, new() { Timeout = 5000 });
            
            result.Success = true;
            result.Output["message"] = $"Entered text into element with selector: {selector}";
            return result;
        }
        catch (Exception ex)
        {
            result.Error = $"Input operation failed: {ex.Message}";
            return result;
        }
    }
}
```