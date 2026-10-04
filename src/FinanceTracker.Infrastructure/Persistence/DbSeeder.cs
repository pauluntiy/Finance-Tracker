using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Infrastructure.Persistence;

public class DbSeeder
{
    private static readonly (string Name, string ColorHex, CategoryType Type)[] DefaultCategories =
    [
        // Расходы
        ("Продукты", "#EF4444", CategoryType.Expense),
        ("Транспорт и авто", "#F59E0B", CategoryType.Expense),
        ("Жилье и ЖКХ", "#3B82F6", CategoryType.Expense),
        ("Развлечения и досуг", "#8B5CF6", CategoryType.Expense),
        ("Здоровье и аптеки", "#10B981", CategoryType.Expense),
        ("Рестораны и кафе", "#EC4899", CategoryType.Expense),
        ("Покупки и одежда", "#6366F1", CategoryType.Expense),
        ("Подписки и сервисы", "#06B6D4", CategoryType.Expense),

        // Доходы
        ("Зарплата", "#22C55E", CategoryType.Income),
        ("Фриланс и подработка", "#10B981", CategoryType.Income),
        ("Инвестиции", "#3B82F6", CategoryType.Income),
        ("Подарки", "#A855F7", CategoryType.Income)
    ];
    
    public static async Task SeedAsync(ApplicationDbContext dbContext, ILogger logger)
    {
        // Проверяем, есть ли уже системные категории (где UserId == null)
        bool hasSystemCategories = await dbContext.Categories
            .AnyAsync(c => c.UserId == null);

        if (hasSystemCategories)
        {
            logger.LogInformation("System categories already seeded. Skipping.");
            return;
        }

        logger.LogInformation("Seeding default system categories...");

        var categoriesToInsert = new List<Category>();

        foreach (var (name, colorHex, type) in DefaultCategories)
        {
            var categoryResult = Category.Create(
                Guid.NewGuid(),
                name,
                colorHex,
                type,
                null);

            if (categoryResult.IsSuccess)
            {
                categoriesToInsert.Add(categoryResult.Value);
            }
            else
            {
                logger.LogWarning("Failed to create seed category '{Name}': {Error}", name, categoryResult.Error);
            }
        }

        if (categoriesToInsert.Count > 0)
        {
            await dbContext.Categories.AddRangeAsync(categoriesToInsert);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Successfully seeded {Count} system categories.", categoriesToInsert.Count);
        }
    }
}