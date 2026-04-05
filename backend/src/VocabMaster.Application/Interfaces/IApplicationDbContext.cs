using Microsoft.EntityFrameworkCore;
using VocabMaster.Domain.Entities;

namespace VocabMaster.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<LearnedWord> LearnedVocabularies { get; }

    DbSet<Vocabulary> Vocabularies { get; }

    DbSet<QuizQuestion> QuizQuestions { get; }

    DbSet<CompletedQuiz> CompletedQuizzes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}