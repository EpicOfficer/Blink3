using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

public class BlinkMixRepository(BlinkDbContext dbContext) :
    BaseGameRepository<BlinkMix>(dbContext), IBlinkMixRepository;