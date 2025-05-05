using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.DDD.ValueObjects;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Auth;

namespace Modules.Workflow.Features.Workflows.CreateWorkflow
{
    public class CreateWorkflowHandler : IRequestHandler<CreateWorkflowCommand, WorkflowDto>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CreateWorkflowHandler> _logger;
        public CreateWorkflowHandler(
            IWorkflowRepository workflowRepository,
            ICurrentUserService currentUser,
            ILogger<CreateWorkflowHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _currentUser = currentUser;
            _logger = logger;
        }
        public async Task<WorkflowDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
        {
            var workflow = DDD.Entities.Workflow.Create(
                request.Name,
                request.Description
            );

            await _workflowRepository.AddAsync(workflow, cancellationToken);
            await _workflowRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Workflow {Id} created by user {UserId}", workflow.Id, _currentUser.UserId);

            return new WorkflowDto(workflow.Id, workflow.Name, workflow.Description, workflow.Status.ToStringValue());
        }
    }
}
