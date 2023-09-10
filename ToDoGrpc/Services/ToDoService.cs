using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services
{
    public class ToDoService : ToDoIt.ToDoItBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ToDoService> _logger;

        public ToDoService(AppDbContext dbContext, ILogger<ToDoService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));
            }

            var toDoItem = new ToDoItem
            {
                Title = request.Title,
                Description = request.Description
            };

            await _dbContext.ToDoItems.AddAsync(toDoItem);
            await _dbContext.SaveChangesAsync();

            return await Task.FromResult(new CreateToDoResponse
            {
                Id = toDoItem.Id
            }); 
        }

        public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than 0"));
            }

            var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (toDoItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"No task with id {request.Id}"));
            }

            return await Task.FromResult(new ReadToDoResponse()
            {
                Id = toDoItem.Id,
                Title = toDoItem.Title,
                Description = toDoItem.Description,
                ToDoStatus = toDoItem.ToDoStatus
            });
        }

        public override async Task<GetAllToDoResponse> ListToDo(GetAllToDoRequest request, ServerCallContext context)
        {
            var response = new GetAllToDoResponse();
            var toDoItems = await _dbContext.ToDoItems.ToListAsync();

            foreach (var toDoItem in toDoItems)
            {
                response.ToDo.Add(new ReadToDoResponse
                {
                    Id = toDoItem.Id,
                    Title = toDoItem.Title,
                    Description = toDoItem.Description,
                    ToDoStatus = toDoItem.ToDoStatus
                });
            }

            return await Task.FromResult(response);
        }

        public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0 || 
                string.IsNullOrWhiteSpace(request.Title) || 
                string.IsNullOrWhiteSpace(request.Description))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));
            }

            var toDoItem = _dbContext.ToDoItems.FirstOrDefault(x => x.Id == request.Id);

            if (toDoItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"No task with id {request.Id}"));
            }

            toDoItem.Title = request.Title;
            toDoItem.Description = request.Description;
            toDoItem.ToDoStatus = request.ToDoStatus;

            await _dbContext.SaveChangesAsync();

            return await Task.FromResult(new UpdateToDoResponse()
            {
                Id = toDoItem.Id,
            });
        }

        public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than 0"));
            }

            var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (toDoItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"No task with id {request.Id}"));
            }

            _dbContext.Remove(toDoItem);
            await _dbContext.SaveChangesAsync();

            return await Task.FromResult(new DeleteToDoResponse()
            {
                Id = toDoItem.Id
            });
        }
    }
}
