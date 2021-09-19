using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Enums;
using Contracts.Models.RequestModels;
using Contracts.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Options;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("todos")]
    public class TodosController : ControllerBase
    {
        private readonly ITodosRepository _todosRepository;
        private readonly IUserRepository _userRepository;
        
        public TodosController(ITodosRepository todosRepository, IUserRepository userRepository)
        {
            _todosRepository = todosRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodosItemResponse>>> GetAll()
        {
            var todos = await _todosRepository.GetAllAsync();

            return new ActionResult<IEnumerable<TodosItemResponse>>(todos.Select(todo => todo.MapToTodoItemResponse()));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<TodosItemResponse>> Get(Guid id)
        {
            var todoItem = await _todosRepository.GetAsync(id);

            if (todoItem is null)
            {
                return NotFound($"Todo item with id: '{id}' does not exist");
            }

            return todoItem.MapToTodoItemResponse();
        }

        [HttpPost]
        public async Task<ActionResult<TodosItemResponse>> Create(CreateTodoItemRequest request)
        {
            var todoItemReadModel = new TodoItemReadModel
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Difficulty = request.Difficulty,
                IsDone = false,
                DateCreated = DateTime.Now
            };

            var rowsAffected = await _todosRepository.SaveOrUpdateAsync(todoItemReadModel);

            if (rowsAffected > 1)
            {
                throw new Exception("Something went wrong");
            }

            return CreatedAtAction(nameof(Get), new { todoItemReadModel.Id }, todoItemReadModel.MapToTodoItemResponse());
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult<TodosItemResponse>> Update(Guid id, UpdateTodoItemRequest request)
        {
            var todoItem = await _todosRepository.GetAsync(id);

            if (todoItem is null)
            {
                return NotFound($"Todo item with id: '{id}' does not exist");
            }
            
            todoItem.Title = request.Title;
            todoItem.Description = request.Description;

            await _todosRepository.SaveOrUpdateAsync(todoItem);

            return todoItem.MapToTodoItemResponse();
        }

        [HttpPatch]
        [Route("{id}/toggleStatus")]
        public async Task<ActionResult<TodosItemResponse>> UpdateStatus(Guid id)
        {
            var todoItem = await _todosRepository.GetAsync(id);

            if (todoItem is null)
            {
                return NotFound($"Todo item with id: '{id}' does not exist");
            }

            todoItem.IsDone = !todoItem.IsDone;
            
            await _todosRepository.SaveOrUpdateAsync(todoItem);

            return todoItem.MapToTodoItemResponse();
        }
        

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var todoItem = await _todosRepository.GetAsync(id);

            if (todoItem is null)
            {
                return NotFound($"Todo item with id: '{id}' does not exist");
            }
            
            await _todosRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}