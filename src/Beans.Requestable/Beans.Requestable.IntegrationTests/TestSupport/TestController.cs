using Microsoft.AspNetCore.Mvc;

namespace Beans.Requestable.IntegrationTests.TestSupport;

public sealed record LogModel(string Message);

public sealed record ScopeResult(Guid Controller, Guid Handler);

[ApiController]
[Route("test")]
public sealed class TestController(IMediator mediator, RequestScope scope) : ControllerBase
{
    [HttpGet("greet/{name}")]
    public async Task<IActionResult> Greet(string name, CancellationToken cancellationToken)
        => Ok(await mediator.SendAsync(new GreetRequest(name), cancellationToken));

    [HttpPost("log")]
    public async Task<IActionResult> Log(LogModel model, CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new LogRequest(model.Message), cancellationToken);
        return NoContent();
    }

    [HttpGet("scope")]
    public async Task<IActionResult> Scope(CancellationToken cancellationToken)
        => Ok(new ScopeResult(scope.Id, await mediator.SendAsync(new WhoAmIRequest(), cancellationToken)));

    [HttpGet("fail")]
    public async Task<IActionResult> Fail(CancellationToken cancellationToken)
        => Ok(await mediator.SendAsync(new FailRequest(), cancellationToken));

    [HttpGet("unhandled")]
    public async Task<IActionResult> Unhandled(CancellationToken cancellationToken)
        => Ok(await mediator.SendAsync(new UnhandledRequest(), cancellationToken));
}