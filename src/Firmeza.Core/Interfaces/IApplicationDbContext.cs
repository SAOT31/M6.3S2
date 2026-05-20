namespace Firmeza.Core.Interfaces;

// Contrato mínimo de persistencia — Core no toca EF, eso es responsabilidad de Infrastructure
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
