using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace todo_serveless_csharp.Functions.Entities
{
    class TodoEntity : TableEntity
    {
        public DateTime CreateTime { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}
