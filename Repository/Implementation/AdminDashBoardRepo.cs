using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Implementation
{
    public class AdminDashBoardRepo : IAdminDashBoardRepo
    {
        private readonly AppDbContext _context;

        public AdminDashBoardRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Vocabulary> AddVocabulary(Vocabulary vocabulary)
        {
            _context.Vocabularies.Add(vocabulary);
            await _context.SaveChangesAsync();
            return vocabulary;
        }

        public async Task<bool> DeleteVocabulary(int vocabularyId)
        {
            var vocabulary = await _context.Vocabularies.FindAsync(vocabularyId);
            if (vocabulary == null)
            {
                return false;
            }

            _context.Vocabularies.Remove(vocabulary);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
