using Blink3.Core.Caching;
using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IBlinkGuildRepository" />
public class BlinkGuildRepository(BlinkDbContext dbContext, ICachingService cache)
    : GenericRepositoryWithCaching<BlinkGuild>(dbContext, cache), IBlinkGuildRepository;