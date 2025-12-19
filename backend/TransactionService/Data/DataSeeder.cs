using TransactionService.Models;

namespace TransactionService.Data;

public static class DataSeeder
{
    private static readonly string[] MerchantNames = 
    {
        "Woolworths", "Pick n Pay", "Checkers", "Shoprite", "Dis-Chem",
        "Takealot", "Mr Price", "Clicks", "Makro", "Game",
        "Engen", "Shell", "BP", "Sasol", "Total",
        "Netflix", "Spotify", "Amazon Prime", "Showmax", "Apple",
        "Uber", "Bolt", "Mr Delivery", "Uber Eats", "KFC",
        "McDonald's", "Nando's", "Steers", "Ocean Basket", "Spur",
        "Vodacom", "MTN", "Cell C", "Telkom", "Rain",
        "Discovery", "Old Mutual", "Sanlam", "Momentum", "Liberty"
    };

    private static readonly string[] Categories = 
    {
        "Groceries", "Fuel", "Entertainment", "Dining", "Shopping",
        "Utilities", "Transport", "Healthcare", "Insurance", "Telecommunications"
    };

    private static readonly Dictionary<string, string> MerchantToCategory = new()
    {
        { "Woolworths", "Groceries" }, { "Pick n Pay", "Groceries" }, { "Checkers", "Groceries" },
        { "Shoprite", "Groceries" }, { "Dis-Chem", "Healthcare" }, { "Takealot", "Shopping" },
        { "Mr Price", "Shopping" }, { "Clicks", "Healthcare" }, { "Makro", "Groceries" },
        { "Game", "Shopping" }, { "Engen", "Fuel" }, { "Shell", "Fuel" },
        { "BP", "Fuel" }, { "Sasol", "Fuel" }, { "Total", "Fuel" },
        { "Netflix", "Entertainment" }, { "Spotify", "Entertainment" }, { "Amazon Prime", "Entertainment" },
        { "Showmax", "Entertainment" }, { "Apple", "Shopping" }, { "Uber", "Transport" },
        { "Bolt", "Transport" }, { "Mr Delivery", "Dining" }, { "Uber Eats", "Dining" },
        { "KFC", "Dining" }, { "McDonald's", "Dining" }, { "Nando's", "Dining" },
        { "Steers", "Dining" }, { "Ocean Basket", "Dining" }, { "Spur", "Dining" },
        { "Vodacom", "Telecommunications" }, { "MTN", "Telecommunications" }, { "Cell C", "Telecommunications" },
        { "Telkom", "Telecommunications" }, { "Rain", "Telecommunications" },
        { "Discovery", "Insurance" }, { "Old Mutual", "Insurance" }, { "Sanlam", "Insurance" },
        { "Momentum", "Insurance" }, { "Liberty", "Insurance" }
    };

    public static void Seed(TransactionDbContext context)
    {
        if (context.Transactions.Any())
        {
            return; // Already seeded
        }

        var random = new Random(42); // Fixed seed for reproducibility
        var transactions = new List<Transaction>();
        var customerIds = new[] { "CUST001", "CUST002", "CUST003", "CUST004", "CUST005" };
        var cardNumbers = new[] { "4532", "5678", "9012", "3456", "7890" };

        for (int i = 0; i < 100; i++)
        {
            var merchantName = MerchantNames[random.Next(MerchantNames.Length)];
            var category = MerchantToCategory.GetValueOrDefault(merchantName, "Other");
            var customerId = customerIds[random.Next(customerIds.Length)];
            var isCredit = random.NextDouble() > 0.85; // 15% chance of credit
            
            var amount = category switch
            {
                "Groceries" => (decimal)(random.NextDouble() * 2000 + 100),
                "Fuel" => (decimal)(random.NextDouble() * 1500 + 200),
                "Entertainment" => (decimal)(random.NextDouble() * 300 + 50),
                "Dining" => (decimal)(random.NextDouble() * 500 + 80),
                "Shopping" => (decimal)(random.NextDouble() * 5000 + 200),
                "Insurance" => (decimal)(random.NextDouble() * 3000 + 500),
                "Telecommunications" => (decimal)(random.NextDouble() * 500 + 100),
                "Transport" => (decimal)(random.NextDouble() * 300 + 30),
                "Healthcare" => (decimal)(random.NextDouble() * 1000 + 100),
                _ => (decimal)(random.NextDouble() * 1000 + 50)
            };

            transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Description = $"{(isCredit ? "Refund from" : "Payment to")} {merchantName}",
                Amount = Math.Round(amount, 2),
                Currency = "ZAR",
                Category = category,
                MerchantName = merchantName,
                MerchantCategory = category,
                Type = isCredit ? TransactionType.Credit : TransactionType.Debit,
                Status = random.NextDouble() > 0.05 ? TransactionStatus.Completed : TransactionStatus.Pending,
                TransactionDate = DateTime.UtcNow.AddDays(-random.Next(1, 90)),
                CreatedAt = DateTime.UtcNow,
                Reference = $"TXN{random.Next(100000, 999999)}",
                CardLastFourDigits = cardNumbers[random.Next(cardNumbers.Length)],
                IsDisputed = false
            });
        }

        context.Transactions.AddRange(transactions);
        context.SaveChanges();
    }
}
