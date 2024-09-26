using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(
            AuctionDbContext context,
            IMapper mapper,
            IPublishEndpoint publishEndpoint
        )
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(x =>
                    x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0
                );
            }
            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuction(Guid id)
        {
            var auction = await _context
                .Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null)
            {
                return NotFound();
            }
            return _mapper.Map<AuctionDto>(auction);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            // toDO: add current user
            auction.Seller = User.Identity.Name;
            _context.Auctions.Add(auction);

            //publish to queue
            var _auction = _mapper.Map<AuctionDto>(auction);
            var item = _mapper.Map<AuctionCreated>(_auction);
            await _publishEndpoint.Publish(item);

            var result = await _context.SaveChangesAsync() > 0;
            if (!result)
                return BadRequest("Could not save new auction");
            return CreatedAtAction(nameof(GetAuction), new { auction.Id }, _auction);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<AuctionDto>> UpdateAuction(
            Guid id,
            UpdateAuctionDto auctionDto
        )
        {
            var auction = await _context
                .Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null)
            {
                return NotFound();
            }
            // toDO: check seller == username
            if (auction.Seller != User.Identity.Name)
            {
                return Forbid();
            }

            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
            auction.Item.Year = auctionDto.Year;
            auction.Item.Mileage = auctionDto.Mileage;

            var item = _mapper.Map<AuctionUpdated>(auction);
            await _publishEndpoint.Publish(item);

            //_context.Auctions.Update(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if (!result)
                return BadRequest("Could not update auction");
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<AuctionDto>> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null)
                return NotFound();

            //todo: check seller == username
            if (auction.Seller != User.Identity.Name)
            {
                return Forbid();
            }

            await _publishEndpoint.Publish(new AuctionDeleted { Id = auction.Id.ToString() });

            _context.Auctions.Remove(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if (!result)
                return BadRequest("Could not delete auction");
            return Ok();
        }
    }
}
