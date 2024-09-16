using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
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

        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
        {
            var auctions = await _context.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Make)
                .ToListAsync();
            return _mapper.Map<List<AuctionDto>>(auctions);
        }
         
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuction(Guid id)
        {
            var auction = await _context.Auctions 
                .Include(x => x.Item) 
                .FirstOrDefaultAsync(x => x.Id == id);
            if(auction == null){
                return NotFound();
            }
            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            // toDO: add current user
            auction.Seller = "test";
            _context.Auctions.Add(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if(!result) return BadRequest("Could not save new auction");
            return CreatedAtAction(nameof(GetAuction), new {auction.Id}, _mapper.Map<AuctionDto>(auction));
        }

        
        [HttpPut("{id}")]
        public async Task<ActionResult<AuctionDto>> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
        {
             var auction = await _context.Auctions 
                .Include(x => x.Item) 
                .FirstOrDefaultAsync(x => x.Id == id);
            if(auction == null){
                return NotFound();
            }
            // toDO: check seller == username
            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
            auction.Item.Year = auctionDto.Year;
            auction.Item.Mileage = auctionDto.Mileage;
             
            //_context.Auctions.Update(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if(!result) return BadRequest("Could not update auction");
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<AuctionDto>> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if(auction == null) return NotFound();
            //todo: check seller == username

            _context.Auctions.Remove(auction);
              var result = await _context.SaveChangesAsync() > 0;
            if(!result) return BadRequest("Could not delete auction");
            return Ok();
        }
    }

}
