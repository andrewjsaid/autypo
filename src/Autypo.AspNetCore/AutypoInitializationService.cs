using Microsoft.Extensions.Hosting;

namespace Autypo.AspNetCore;

/// <summary>
/// A hosted service responsible for initializing Autypo eagerly and in the background at app startup.
/// </summary>
/// <remarks>
/// You typically do not register this manually—it's added by default when calling <c>AddAutypoSearch</c> or <c>AddAutypoComplete</c>.
/// </remarks>
internal sealed class AutypoInitializationService(IEnumerable<AutypoFactory> factories) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        foreach (var factory in factories)
        {
            await factory.InitializeIfEager(cancellationToken);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var factory in factories)
        {
            await factory.InitializeIfBackground(stoppingToken);
        }
    }
}
