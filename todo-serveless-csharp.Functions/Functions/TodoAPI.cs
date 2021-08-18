using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using todo_serveless_csharp.Common.Models;
using todo_serveless_csharp.Common.Responses;
using todo_serveless_csharp.Functions.Entities;

namespace todo_serveless_csharp.Functions.Functions
{
    public static class TodoAPI
    {
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Recivied a new TODO request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);
            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Message = "The request must have a Task description."

                });
            }
            TodoEntity todoEntity = new TodoEntity
            {
                CreateTime = DateTime.UtcNow,
                ETag = "*",
                IsCompleted = false,
                PartitionKey = "TODO",
                RowKey = Guid.NewGuid().ToString(),
                TaskDescription = todo.TaskDescription
            };

            TableOperation addOperation = TableOperation.Insert(todoEntity);
            await todoTable.ExecuteAsync(addOperation);
            string message = "New TODO stored in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }

        [FunctionName(nameof(UpdateTodo))]
        public static async Task<IActionResult> UpdateTodo(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
           [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
           string id,
           ILogger log)
        {
            log.LogInformation($"Update for todo: {id}, recivied.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            // Validate todo ID 

            TableOperation tableFindById = TableOperation.Retrieve<TodoEntity>("TODO", id);
            TableResult findResult = await todoTable.ExecuteAsync(tableFindById);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Message = "Todo not found."
                });
            }

            // Update Todo
            TodoEntity todoEntity = (TodoEntity)findResult.Result;
            todoEntity.IsCompleted = todo.IsCompleted;
            if (!string.IsNullOrEmpty(todo.TaskDescription))
            {
                todoEntity.TaskDescription = todo.TaskDescription;
            }


            TableOperation replaceOperation = TableOperation.Replace(todoEntity);
            await todoTable.ExecuteAsync(replaceOperation);
            string message = $"TODO  {id} was updated in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }


        [FunctionName(nameof(GetAllTodos))]
        public static async Task<IActionResult> GetAllTodos(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
          [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
          ILogger log)
        {
            log.LogInformation("Get all todos received.");

            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>();
            TableQuerySegment<TodoEntity> todos = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            string message = "Retrieved all todos.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                Message = message,
                Result = todos
            });
        }

        [FunctionName(nameof(GetTodoById))]
        public static IActionResult GetTodoById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
        [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
        string id,
        ILogger log)
        {
            log.LogInformation($"Get todo by id : {id}, received.");
            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Message = "Todo not found."
                });
            }

            string message = $"Todo by id : {id}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }
    }
}
