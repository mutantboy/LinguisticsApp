using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinguisticsApp.Application.Common.Commands.LanguageContactEventCommands;
using LinguisticsApp.Application.Common.DTOs.LanguageContactEventDTOs;
using LinguisticsApp.Application.Common.Interfaces;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.Application.Common.Models;
using LinguisticsApp.Application.Common.Queries;

namespace LinguisticsApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LanguageContactEventsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public LanguageContactEventsController(IMediator mediator, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PaginatedList<LanguageContactEventDto>>> GetAll(
            [FromQuery] GetFilteredLanguageContactEventsQuery query)
        {
            query ??= new GetFilteredLanguageContactEventsQuery();

            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            return Ok(await _mediator.Send(query));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LanguageContactEventDto>> GetById(Guid id)
        {
            var contactEvent = await _unitOfWork.LanguageContactEvents.GetByIdAsync(new LanguageContactEventId(id));

            return contactEvent switch
            {
                null => NotFound(),
                _ => Ok(_mapper.Map<LanguageContactEventDto>(contactEvent))
            };
        }

        [HttpPost]
        [Authorize(Policy = "RequireResearcherRole")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LanguageContactEventDto>> Create(CreateLanguageContactEventCommand command)
        {
            var id = await _mediator.Send(command);

            var contactEvent = await _unitOfWork.LanguageContactEvents.GetByIdAsync(new LanguageContactEventId(id));
            return CreatedAtAction(nameof(GetById), new { id = id }, _mapper.Map<LanguageContactEventDto>(contactEvent));
        }

        [HttpPost("{id}/loanwords")]
        [Authorize(Policy = "RequireResearcherRole")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddLoanword(Guid id, AddLoanwordCommand command)
        {
            /// Set the contact event ID in the command
            command.ContactEventId = id;

            await _mediator.Send(command);

            return NoContent();
        }

        [HttpPatch("{id}")]
        [Authorize(Policy = "RequireResearcherRole")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, UpdateLanguageContactEventCommand command)
        {
            command.Id = id;

            await _mediator.Send(command);

            return NoContent();
        }
    }
}