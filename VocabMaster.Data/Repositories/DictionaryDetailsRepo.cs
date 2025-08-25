using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Entities;
using VocabMaster.Core.Interfaces.Repositories;

namespace VocabMaster.Data.Repositories
{
    // Repository thao tác với dữ liệu chi tiết từ điển (Vocabulary)
    public class DictionaryDetailsRepo : IDictionaryDetailsRepo
    {
        private readonly AppDbContext _context; // DbContext truy cập database
        private readonly ILogger<DictionaryDetailsRepo> _logger; // Ghi log cho repository

        // Hàm khởi tạo repository, inject context và logger
        public DictionaryDetailsRepo(AppDbContext context, ILogger<DictionaryDetailsRepo> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Lấy thông tin chi tiết từ điển (Vocabulary) theo từ
        public async Task<Vocabulary> GetByWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return null;
            }

            try
            {
                _logger.LogInformation("Getting vocabulary details for word: {Word}", word);
                return await _context.Vocabularies
                    .FirstOrDefaultAsync(v => v.Word.ToLower() == word.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vocabulary details for word: {Word}", word);
                return null;
            }
        }

        // Thêm mới hoặc cập nhật thông tin từ vựng (nếu đã tồn tại thì cập nhật)
        public async Task<Vocabulary> AddOrUpdate(Vocabulary details)
        {
            if (details == null)
            {
                _logger.LogWarning("Vocabulary details parameter is null");
                return null;
            }

            try
            {
                _logger.LogInformation("Adding or updating vocabulary details for word: {Word}", details.Word);

                // Kiểm tra từ đã tồn tại chưa
                var existingDetails = await _context.Vocabularies
                    .FirstOrDefaultAsync(v => v.Word.ToLower() == details.Word.ToLower());

                if (existingDetails != null)
                {
                    _logger.LogInformation("Updating existing vocabulary details for word: {Word}", details.Word);

                    // Cập nhật thông tin entry đã có
                    existingDetails.PhoneticsJson = details.PhoneticsJson;
                    existingDetails.MeaningsJson = details.MeaningsJson;
                    existingDetails.UpdatedAt = DateTime.UtcNow;

                    _context.Vocabularies.Update(existingDetails);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully updated vocabulary details for word: {Word}", details.Word);
                    return existingDetails;
                }
                else
                {
                    _logger.LogInformation("Adding new vocabulary details for word: {Word}", details.Word);

                    // Thêm mới entry
                    await _context.Vocabularies.AddAsync(details);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully added vocabulary details for word: {Word}", details.Word);
                    return details;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or updating vocabulary details for word: {Word}", details.Word);
                return null;
            }
        }

        // Lấy toàn bộ danh sách từ vựng trong hệ thống
        public async Task<List<Vocabulary>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all vocabulary details");
                return await _context.Vocabularies.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vocabulary details");
                return new List<Vocabulary>();
            }
        }

        // Kiểm tra từ vựng đã tồn tại trong hệ thống chưa
        public async Task<bool> Exists(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _logger.LogWarning("Word parameter is null or empty");
                return false;
            }

            try
            {
                _logger.LogInformation("Checking if vocabulary exists for word: {Word}", word);
                return await _context.Vocabularies
                    .AnyAsync(v => v.Word.ToLower() == word.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if vocabulary exists for word: {Word}", word);
                return false;
            }
        }
    }
}