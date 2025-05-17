using Microsoft.Playwright;

namespace Modules.Workflow.Domain.Interfaces.Core
{
    public interface IPlaywrightFactory
    {
         Task<IPlaywright> CreateAsync();
    }
}