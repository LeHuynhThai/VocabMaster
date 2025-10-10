using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Repositories;
// removed interface usage after merge
using VocabMaster.Core.Entities;

namespace Services.Implementation
{
	public class DictionaryLookupService
	{
		private readonly ILogger<DictionaryLookupService> _logger;
		private readonly IVocabularyRepo _vocabularyRepository;
		private readonly IDictionaryDetailsRepo _dictionaryDetailsRepository;
		private const string API_URL = "https://api.dictionaryapi.dev/api/v2/entries/en/";

		public DictionaryLookupService(
			ILogger<DictionaryLookupService> logger,
			IVocabularyRepo vocabularyRepository,
			IDictionaryDetailsRepo dictionaryDetailsRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
			_dictionaryDetailsRepository = dictionaryDetailsRepository ?? throw new ArgumentNullException(nameof(dictionaryDetailsRepository));
		}

		public async Task<DictionaryResponseDto> GetWordDefinition(string word)
		{
			var result = await GetWordDefinitionFromDatabase(word);

			if (result == null)
			{
				_logger.LogInformation("Word '{Word}' not found in database, getting from API", word);

				result = await GetWordDefinitionFromApi(word);

				if (result != null)
				{
					await SaveWordDefinitionToDatabase(result);
				}
			}

			return result;
		}

		public async Task<DictionaryResponseDto> GetWordDefinitionFromDatabase(string word)
		{
			if (string.IsNullOrWhiteSpace(word))
			{
				_logger.LogWarning("Word to lookup is empty");
				return null;
			}

			try
			{
				_logger.LogInformation("Getting data from database for word: {Word}", word);
				var vocabulary = await _dictionaryDetailsRepository.GetByWord(word);

				string vietnameseTranslation = vocabulary?.Vietnamese;

				_logger.LogInformation("Vietnamese translation for word {Word}: {Translation}",
					word, vietnameseTranslation ?? "no translation");

				if (vocabulary == null)
				{
					_logger.LogInformation("Word {Word} not found in database", word);
					return null;
				}

				_logger.LogInformation("Found word: {Word} in database", word);

				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};

				var phonetics = string.IsNullOrEmpty(vocabulary.PhoneticsJson)
					? new List<Phonetic>()
					: JsonSerializer.Deserialize<List<Phonetic>>(vocabulary.PhoneticsJson, options);

				var meanings = string.IsNullOrEmpty(vocabulary.MeaningsJson)
					? new List<Meaning>()
					: JsonSerializer.Deserialize<List<Meaning>>(vocabulary.MeaningsJson, options);

				var response = new DictionaryResponseDto
				{
					Word = vocabulary.Word,
					Phonetic = phonetics.FirstOrDefault()?.Text ?? "",
					Phonetics = phonetics,
					Meanings = meanings,
					Vietnamese = vietnameseTranslation
				};

				response.PhoneticsJson = vocabulary.PhoneticsJson;
				response.MeaningsJson = vocabulary.MeaningsJson;

				_logger.LogInformation("Successfully retrieved data for word: {Word}", word);
				return response;
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, "JSON parsing error for word: {Word}", word);
				return null;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting data for word: {Word}", word);
				return null;
			}
		}

		private async Task<DictionaryResponseDto> GetWordDefinitionFromApi(string word)
		{
			if (string.IsNullOrWhiteSpace(word))
			{
				_logger.LogWarning("Word to lookup is empty");
				return null;
			}

			try
			{
				_logger.LogInformation("Calling dictionary API for word: {Word}", word);

				using (var httpClient = new HttpClient())
				{
					string apiUrl = $"{API_URL}{Uri.EscapeDataString(word)}";
					var response = await httpClient.GetAsync(apiUrl);

					if (!response.IsSuccessStatusCode)
					{
						_logger.LogWarning("API returned status code {StatusCode} for word {Word}",
							response.StatusCode, word);
						return null;
					}

					var content = await response.Content.ReadAsStringAsync();
					var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
					var dictionaryResponse = JsonSerializer.Deserialize<List<DictionaryResponseDto>>(content, options);

					var result = dictionaryResponse?.FirstOrDefault();

					if (result == null)
					{
						_logger.LogWarning("No definition found from API for word: {Word}", word);
						return null;
					}

					_logger.LogInformation("Successfully retrieved definition from API for word: {Word}", word);
					return result;
				}
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "HTTP request error when calling API for word: {Word}", word);
				return null;
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, "JSON parsing error from API response for word: {Word}", word);
				return null;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error calling API for word: {Word}", word);
				return null;
			}
		}

		private async Task<bool> SaveWordDefinitionToDatabase(DictionaryResponseDto definition)
		{
			if (definition == null)
			{
				_logger.LogWarning("Definition is null, cannot save to database");
				return false;
			}

			try
			{
				_logger.LogInformation("Saving definition for word: {Word} to database", definition.Word);

				var existingVocabulary = await _dictionaryDetailsRepository.GetByWord(definition.Word);

				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
				};

				var phoneticsJson = definition.Phonetics != null && definition.Phonetics.Any()
					? JsonSerializer.Serialize(definition.Phonetics, options)
					: "[]";

				var meaningsJson = definition.Meanings != null
					? JsonSerializer.Serialize(definition.Meanings, options)
					: "[]";

				if (existingVocabulary != null)
				{
					existingVocabulary.PhoneticsJson = phoneticsJson;
					existingVocabulary.MeaningsJson = meaningsJson;
					existingVocabulary.UpdatedAt = DateTime.UtcNow;

					if (!string.IsNullOrEmpty(definition.Vietnamese))
					{
						existingVocabulary.Vietnamese = definition.Vietnamese;
					}

					await _dictionaryDetailsRepository.AddOrUpdate(existingVocabulary);

					_logger.LogInformation("Updated existing word: {Word} in database", definition.Word);
				}
				else
				{
					var newVocabulary = new Vocabulary
					{
						Word = definition.Word,
						PhoneticsJson = phoneticsJson,
						MeaningsJson = meaningsJson,
						Vietnamese = definition.Vietnamese,
						CreatedAt = DateTime.UtcNow
					};

					await _dictionaryDetailsRepository.AddOrUpdate(newVocabulary);

					_logger.LogInformation("Added new word: {Word} to database", definition.Word);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error saving definition for word: {Word} to database", definition.Word);
				return false;
			}
		}
	}

	public class DictionaryCacheService
	{
		private readonly ILogger<DictionaryCacheService> _logger;
		private readonly IVocabularyRepo _vocabularyRepository;
		private const string API_URL = "https://api.dictionaryapi.dev/api/v2/entries/en/";

		public DictionaryCacheService(
			ILogger<DictionaryCacheService> logger,
			IVocabularyRepo vocabularyRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
		}

		public async Task<int> CacheAllVocabularyDefinitions()
		{
			try
			{
				_logger.LogInformation("Start caching all vocabulary");

				var vocabularies = await _vocabularyRepository.GetAll();
				if (vocabularies == null || !vocabularies.Any())
				{
					_logger.LogWarning("No vocabulary to cache");
					return 0;
				}

				_logger.LogInformation("Found {Count} vocabulary", vocabularies.Count);

				int successCount = 0;
				int failCount = 0;

				using (var httpClient = new HttpClient())
				{
					foreach (var vocab in vocabularies)
					{
						try
						{
							bool needsUpdate = string.IsNullOrEmpty(vocab.MeaningsJson) || vocab.MeaningsJson == "[]";

							if (needsUpdate)
							{
								string apiUrl = $"{API_URL}{Uri.EscapeDataString(vocab.Word)}";
								var response = await httpClient.GetAsync(apiUrl);

								if (response.IsSuccessStatusCode)
								{
									var content = await response.Content.ReadAsStringAsync();
									var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
									var dictionaryResponse = JsonSerializer.Deserialize<List<DictionaryResponseDto>>(content, options);
									var definition = dictionaryResponse?.FirstOrDefault();

									if (definition != null)
									{
										vocab.PhoneticsJson = definition.Phonetics != null && definition.Phonetics.Any()
											? JsonSerializer.Serialize(definition.Phonetics, options)
											: "[]";

										vocab.MeaningsJson = definition.Meanings != null
											? JsonSerializer.Serialize(definition.Meanings, options)
											: "[]";

										_logger.LogInformation("Successfully got definition from API for word: {Word}", vocab.Word);
									}
									else
									{
										_logger.LogWarning("No definition found for word: {Word}", vocab.Word);
									}
								}
								else
								{
									_logger.LogWarning("API returned error {StatusCode} for word {Word}", response.StatusCode, vocab.Word);
								}
							}

							vocab.UpdatedAt = DateTime.UtcNow;

							var success = await _vocabularyRepository.Update(vocab);

							if (success)
							{
								successCount++;
								_logger.LogInformation("Successfully cached word: {Word}", vocab.Word);
							}
							else
							{
								failCount++;
								_logger.LogWarning("Failed to cache word: {Word}", vocab.Word);
							}

							if (needsUpdate)
							{
								await Task.Delay(1000);
							}
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Error processing word: {Word}", vocab.Word);
							failCount++;
						}
					}

					_logger.LogInformation("Finished caching vocabulary. Success: {Success}, Failed: {Fail}",
						successCount, failCount);

					return successCount;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error caching vocabulary");
				return 0;
			}
		}

		public class RandomWordService
		{
			private readonly ILogger<RandomWordService> _logger;
			private readonly IVocabularyRepo _vocabularyRepository;
			private readonly ILearnedWordRepo _learnedWordRepository;
			private readonly DictionaryLookupService _dictionaryLookupService;
			private readonly Random _random = new Random();
			private const int MAX_ATTEMPTS = 5;

			public RandomWordService(
				ILogger<RandomWordService> logger,
				IVocabularyRepo vocabularyRepository,
				ILearnedWordRepo learnedWordRepository,
				DictionaryLookupService dictionaryLookupService)
			{
				_logger = logger ?? throw new ArgumentNullException(nameof(logger));
				_vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
				_learnedWordRepository = learnedWordRepository ?? throw new ArgumentNullException(nameof(learnedWordRepository));
				_dictionaryLookupService = dictionaryLookupService ?? throw new ArgumentNullException(nameof(dictionaryLookupService));
			}

			public async Task<DictionaryResponseDto> GetRandomWord()
			{
				try
				{
					_logger.LogInformation("Getting random word from vocabulary repository");
					var vocabulary = await _vocabularyRepository.GetRandom();

					if (vocabulary == null)
					{
						_logger.LogWarning("No vocabulary found in the repository");
						return null;
					}

					try
					{
						_logger.LogInformation("Getting definition for word: {Word}", vocabulary.Word);
						var response = await _dictionaryLookupService.GetWordDefinitionFromDatabase(vocabulary.Word);

						if (response == null)
						{
							_logger.LogWarning("GetWordDefinitionFromDatabase returned null for word: {Word}, trying from direct API...", vocabulary.Word);
							response = await _dictionaryLookupService.GetWordDefinition(vocabulary.Word);
						}

						if (response == null)
						{
							_logger.LogWarning("Both database and direct API returned null for word: {Word}. Creating basic response...", vocabulary.Word);
							return null;
						}

						return response;
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error getting word definition for {Word}", vocabulary.Word);
						return null;
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error getting random word");
					return null;
				}
			}

			public async Task<DictionaryResponseDto> GetRandomWordExcludeLearned(int userId)
			{
				try
				{
					_logger.LogInformation("Getting random word excluding learned ones for user {UserId}", userId);
					var result = await TryGetRandomWordExcludeLearned(userId);

					if (result == null)
					{
						_logger.LogInformation("Falling back to any random word for user {UserId}", userId);
						return await GetRandomWord();
					}

					return result;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error in GetRandomWordExcludeLearned for user {UserId}, falling back to any random word", userId);
					return await GetRandomWord();
				}
			}

			private async Task<DictionaryResponseDto> TryGetRandomWordExcludeLearned(int userId)
			{
				try
				{
					_logger.LogInformation("Getting learned words for user {UserId}", userId);
					var learnedVocabularies = await _learnedWordRepository.GetByUserId(userId);

					if (learnedVocabularies == null)
					{
						_logger.LogWarning("learnedVocabularies is null for user {UserId}", userId);
						learnedVocabularies = new List<LearnedWord>();
					}

					if (!learnedVocabularies.Any())
					{
						_logger.LogInformation("No learned vocabularies found for user {UserId}, returning any random word", userId);
						return await GetRandomWord();
					}

					var learnedWords = learnedVocabularies.Select(lv => lv.Word.ToLowerInvariant()).ToHashSet();
					_logger.LogInformation("User {UserId} has learned {Count} words", userId, learnedWords.Count);

					var vocabulary = await _vocabularyRepository.GetRandomExcludeLearned(learnedWords.ToList());

					if (vocabulary == null)
					{
						_logger.LogInformation("No unlearned words found for user {UserId} in the first attempt", userId);

						for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
						{
							var randomVocab = await _vocabularyRepository.GetRandom();
							if (randomVocab != null && !learnedWords.Contains(randomVocab.Word.ToLowerInvariant()))
							{
								_logger.LogInformation("Found unlearned random word on attempt {Attempt}: {Word}", attempt + 1, randomVocab.Word);
								try
								{
									_logger.LogInformation("Getting definition for word: {Word}", randomVocab.Word);
									var response = await _dictionaryLookupService.GetWordDefinitionFromDatabase(randomVocab.Word);

									if (response == null)
									{
										_logger.LogWarning("GetWordDefinitionFromDatabase returned null for word: {Word}, trying direct API...", randomVocab.Word);
										response = await _dictionaryLookupService.GetWordDefinition(randomVocab.Word);
									}

									if (response != null)
									{
										return response;
									}
									else
									{
										_logger.LogWarning("Both database and API failed for word: {Word}, creating basic response", randomVocab.Word);
										return new DictionaryResponseDto
										{
											Word = randomVocab.Word,
											Vietnamese = randomVocab.Vietnamese
										};
									}
								}
								catch (Exception ex)
								{
									_logger.LogError(ex, "Error getting definition for word {Word}, creating basic response", randomVocab.Word);
									return new DictionaryResponseDto
									{
										Word = randomVocab.Word,
										Vietnamese = randomVocab.Vietnamese
									};
								}
							}
						}

						_logger.LogInformation("All attempts to find unlearned word failed, returning any random word");
						return await GetRandomWord();
					}

					_logger.LogInformation("Found random unlearned word: {Word}", vocabulary.Word);
					try
					{
						_logger.LogInformation("Getting definition for word: {Word}", vocabulary.Word);
						var response = await _dictionaryLookupService.GetWordDefinitionFromDatabase(vocabulary.Word);

						if (response == null)
						{
							_logger.LogWarning("GetWordDefinitionFromDatabase returned null for word: {Word}, trying direct API...", vocabulary.Word);
							response = await _dictionaryLookupService.GetWordDefinition(vocabulary.Word);
						}

						if (response == null)
						{
							_logger.LogWarning("Both database and API failed for word: {Word}, creating basic response", vocabulary.Word);
							return new DictionaryResponseDto
							{
								Word = vocabulary.Word,
								Vietnamese = vocabulary.Vietnamese
							};
						}
						return response;
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error getting definition for word {Word}, creating basic response", vocabulary.Word);
						return new DictionaryResponseDto
						{
							Word = vocabulary.Word,
							Vietnamese = vocabulary.Vietnamese
						};
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error in TryGetRandomWordExcludeLearned for user {UserId}", userId);
					return null;
				}
			}
		}
	}
}


