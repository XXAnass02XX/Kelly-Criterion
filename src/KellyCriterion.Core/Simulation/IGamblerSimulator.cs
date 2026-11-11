using KellyCriterion.Core.Domain;

namespace KellyCriterion.Core.Simulation;

public interface IGamblerSimulator
{
    SimulationResult Simulate(SimulationParameters parameters, IProgress<int>? progress = null, CancellationToken cancellationToken = default);
}
