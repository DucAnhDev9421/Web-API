using System;

namespace webApi.Model
{
    public class LessonProgress
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int LessonId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 