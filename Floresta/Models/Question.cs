﻿using System.ComponentModel.DataAnnotations;

namespace Floresta.Models
{
    public class Question
    {
        public int Id { get; set; }
        [Required]
        public string QuestionText { get; set; }
        public bool IsAnswered { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int QuestionTopicId { get; set; }
        public QuestionTopic QuestionTopic { get; set; }

    }
}
