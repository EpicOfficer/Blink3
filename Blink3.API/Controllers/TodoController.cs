using Blink3.Common.Extensions;
using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Interfaces;
using Blink3.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [SwaggerResponse(401, "Unauthorized")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [SwaggerTag("All CRUD operations for todo items")]
    public class TodoController(IUserTodoRepository todoRepository) : ControllerBase
    {
        private ulong UserId => User.GetUserId();
        
        // Get all todos
        [HttpGet]
        [SwaggerOperation(
            Summary = "Returns all todo items",
            Description = "Returns a list of all of the current user's todo items",
            OperationId = "Todo.GetAll",
            Tags = new[] { "Todo" }
        )]
        public async Task<ActionResult<IEnumerable<UserTodo>>> GetAllTodos()
        {
            IReadOnlyCollection<UserTodo> todos = await todoRepository.GetByUserIdAsync(UserId);
            return Ok(todos);
        }

        // Get specific userTodo
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Returns a specific todo item",
            Description = "Returns a todo item by Id",
            OperationId = "Todo.GetTodo",
            Tags = new[] { "Todo" }
        )]
        [SwaggerResponse(200, "Success", typeof(UserTodo))]
        [SwaggerResponse(404, "Todo not found")]
        public async Task<ActionResult<UserTodo>> GetTodo(int id)
        {
            UserTodo? todo = await todoRepository.GetByIdAsync(id);
            if (todo is null) return NotFound();
            if (todo.UserId != UserId) return Unauthorized();
        
            return Ok(todo);
        }

        // Create a new userTodo
        [HttpPost]
        [SwaggerOperation(
            Summary = "Creates a new todo item",
            Description = "Creates a new todo item for the current user",
            OperationId = "Todo.Create",
            Tags = new[] { "Todo" }
        )]
        [SwaggerResponse(201, "Created", typeof(UserTodo))]
        public async Task<ActionResult<UserTodo>> CreateTodo([FromBody] UserTodoDto userTodoDto)
        {
            if (userTodoDto.UserId != UserId) return Unauthorized();
        
            UserTodo createdTodo = await todoRepository.AddAsync(userTodoDto.ToEntity());
        
            return CreatedAtAction(nameof(GetTodo), new { id = createdTodo.Id }, createdTodo);
        }

        // Update existing userTodo
        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Updates a specific todo item",
            Description = "Updates the content of a specific todo item",
            OperationId = "Todo.Update",
            Tags = new[] { "Todo" }
        )]
        [SwaggerResponse(204, "No content")]
        public async Task<ActionResult> UpdateTodo(int id, [FromBody] UserTodoDto todoDto)
        {
            UserTodo? todo = await todoRepository.GetByIdAsync(id);
            if (todo is null) return NotFound();
            if (todo.UserId != UserId) return Unauthorized();

            await todoRepository.AddAsync(todoDto.ToEntity(id));
        
            return NoContent();
        }

        // Delete a specific userTodo
        [HttpDelete("{id:int}")]
        [SwaggerOperation(
            Summary = "Deletes a specific todo item",
            Description = "Deletes a single todo item based on the Id",
            OperationId = "Todo.Delete",
            Tags = new[] { "Todo" }
        )]
        [SwaggerResponse(204, "No content")]
        [SwaggerResponse(404, "Todo not found")]
        public async Task<ActionResult> DeleteTodo(int id)
        {
            UserTodo? todo = await todoRepository.GetByIdAsync(id);
            if (todo is null) return NotFound();
            if (todo.UserId != UserId) return Unauthorized();
        
            await todoRepository.DeleteAsync(todo);
        
            return NoContent();
        }
    }
}