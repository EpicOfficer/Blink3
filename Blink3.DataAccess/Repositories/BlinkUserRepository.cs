using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IBlinkUserRepository" />
public class BlinkUserRepository(BlinkDbContext dbContext)
    : GenericRepository<BlinkUser>(dbContext), IBlinkUserRepository;