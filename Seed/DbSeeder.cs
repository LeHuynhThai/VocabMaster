using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using VocabMaster.Entities;
using VocabMaster.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    private sealed class VocabularyMap : ClassMap<Vocabulary>
    {
        public VocabularyMap()
        {
            Map(m => m.Word).Name("Word");
        }
    }

    private sealed class CsvVocabularyRecord
    {
        public string Word { get; set; }
    }

    private static string CleanWord(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // Loại bỏ khoảng trắng và chuyển về chữ thường
        var word = input.Replace(" ", "").Trim().ToLower();
        
        // Chuyển chữ cái đầu thành hoa
        if (word.Length > 0)
        {
            word = char.ToUpper(word[0]) + word.Substring(1);
        }

        return word;
    }

    public static async Task SeedFromCsv(AppDbContext context, ILogger logger = null)
    {
        try
        {
            // Xóa tất cả dữ liệu cũ
            logger?.LogInformation("Clearing existing vocabulary data...");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Vocabularies");
            logger?.LogInformation("Existing vocabulary data cleared.");
        }
        catch (Exception ex)
        {
            logger?.LogError($"Error clearing existing data: {ex.Message}");
            return;
        }

        var fileNames = new[] {
            "oxford_first_1000_words.csv",
            "oxford_second_1000_words.csv",
            "oxford_third_1000_words.csv"
        };

        int totalAdded = 0;
        int totalSkipped = 0;
        var processedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var fileName in fileNames)
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Seed", fileName);
                if (!File.Exists(path))
                {
                    logger?.LogWarning($"File not found: {path}");
                    continue;
                }

                // Đọc tất cả các dòng từ file
                var lines = await File.ReadAllLinesAsync(path);
                var words = new List<string>();

                // Bỏ qua dòng header và xử lý từng dòng
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var cleanWord = CleanWord(line);
                        if (!string.IsNullOrEmpty(cleanWord))
                        {
                            words.Add(cleanWord);
                        }
                    }
                }

                logger?.LogInformation($"Found {words.Count} words in {fileName}");

                // Xử lý từng từ
                foreach (var word in words)
                {
                    if (processedWords.Contains(word))
                    {
                        totalSkipped++;
                        continue;
                    }

                    processedWords.Add(word);

                    try
                    {
                        context.Vocabularies.Add(new Vocabulary { Word = word });
                        totalAdded++;

                        // Save changes every 100 words
                        if (totalAdded % 100 == 0)
                        {
                            await context.SaveChangesAsync();
                            logger?.LogInformation($"Added {totalAdded} words so far...");
                        }
                    }
                    catch (DbUpdateException ex)
                    {
                        logger?.LogWarning($"Could not add word '{word}': {ex.InnerException?.Message ?? ex.Message}");
                        totalSkipped++;
                    }
                }

                // Save any remaining words
                await context.SaveChangesAsync();
                logger?.LogInformation($"Finished processing {fileName}. Added: {totalAdded}, Skipped: {totalSkipped}");
            }
            catch (Exception ex)
            {
                logger?.LogError($"Error processing {fileName}: {ex.Message}");
            }
        }

        logger?.LogInformation($"Seeding completed. Total added: {totalAdded}, Total skipped: {totalSkipped}");
    }
}
