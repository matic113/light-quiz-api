namespace light_quiz_api.Models
{
    public class QuestionType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigational properties
        public ICollection<Question> Questions { get; set; }
    }
}
