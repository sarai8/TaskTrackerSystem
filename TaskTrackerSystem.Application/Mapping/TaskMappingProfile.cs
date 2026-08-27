using AutoMapper;
using TaskTrackerSystem.Application.DTOs;
using TaskTrackerSystem.Domain.Entities;

namespace TaskTrackerSystem.Application.Mapping;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskItem, TaskDto>();

        CreateMap<CreateTaskDto, TaskItem>();

        CreateMap<UpdateTaskDto, TaskItem>();
    }
}