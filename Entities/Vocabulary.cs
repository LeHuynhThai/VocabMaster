using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Entities
{
    public class Vocabulary
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Word { get; set; }
    }
}
