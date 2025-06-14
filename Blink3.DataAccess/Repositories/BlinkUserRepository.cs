using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IBlinkUserRepository" />
public class BlinkUserRepository(BlinkDbContext dbContext)
    : GenericRepository<BlinkUser>(dbContext), IBlinkUserRepository;