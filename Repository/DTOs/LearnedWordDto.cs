using System.Text.Json.Serialization;

namespace Repository.DTOs
{
    public class LearnedWordDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("word")]
        public string Word { get; set; }
        
        [JsonPropertyName("learnedAt")]
        public DateTime LearnedAt { get; set; }
    }
    
    public class PaginatedResponseDto<T>
    {
        [JsonPropertyName("items")]
        public List<T> Items { get; set; }
        
        [JsonPropertyName("pageInfo")]
        public PageInfoDto PageInfo { get; set; }
    }
    
    public class PageInfoDto
    {
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }
        
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
        
        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }
        
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
    }
}