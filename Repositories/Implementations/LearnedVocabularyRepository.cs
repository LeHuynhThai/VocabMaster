using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Entities;
using VocabMaster.Repositories.Interfaces;

namespace VocabMaster.Repositories.Implementations
{
    public class LearnedVocabularyRepository : ILearnedVocabularyRepository
    {
        private readonly AppDbContext _context;

        public LearnedVocabularyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LearnedVocabulary> GetByIdAsync(int id)
        {
            return await _context.LearnedVocabularies
                .Include(lv => lv.User)
                .FirstOrDefaultAsync(lv => lv.Id == id);
        }

        public async Task<List<LearnedVocabulary>> GetByUserIdAsync(string userId)
        {
            return await _context.LearnedVocabularies
                .Where(lv => lv.UserId == userId)
                .OrderByDescending(lv => lv.Id)
                .ToListAsync();
        }

        public async Task<bool> AddAsync(LearnedVocabulary learnedVocabulary)
        {
            try
            {
                await _context.LearnedVocabularies.AddAsync(learnedVocabulary);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var learnedVocabulary = await _context.LearnedVocabularies.FindAsync(id);
                if (learnedVocabulary == null)
                    return false;

                _context.LearnedVocabularies.Remove(learnedVocabulary);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> IsWordLearnedAsync(string userId, string word)
        {
            return await _context.LearnedVocabularies
                .AnyAsync(lv => lv.UserId == userId && lv.Word == word);
        }

        public async Task<List<string>> GetLearnedWordsAsync(string userId)
        {
            return await _context.LearnedVocabularies
                .Where(lv => lv.UserId == userId)
                .Select(lv => lv.Word)
                .ToListAsync();
        }
    }
} 