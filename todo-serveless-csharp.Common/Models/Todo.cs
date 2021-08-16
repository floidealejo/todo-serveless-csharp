using System;

namespace todo_serveless_csharp.Common.Models
{
    public class Todo
    {
        public DateTime CreateTime { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}
