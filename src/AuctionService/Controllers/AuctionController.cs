using System;
using System.Diagnostics.CodeAnalysis;
using AuctionService.Data;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(x=> x.Item)
            .OrderBy(x=> x.Item.Make)
            .ToListAsync();

        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {

        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x=> x.Id == id);

        if(auction == null) return NotFound();

        return _mapper.Map<AuctionDto>(auction);

    

    }

    [HttpPost]

    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);

        // todo : add current user as seller

        auction.Seller = "test"; // Setting the seller value in Auction entity

        _context.Auctions.Add(auction);

        var result = await _context.SaveChangesAsync() > 0; // it will return boolean if the save is successful or not

        if(!result) return BadRequest("Could not save changes to the DB");

        return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions.Include(x=> x.Item).FirstOrDefaultAsync(x=>x.Id == id);

        if(auction == null) return NotFound();

        // todo : check seller == username

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;

        if(result) return Ok();

        return BadRequest("Problem in saving changes");

    }

    [HttpDelete("{id}")]

    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if(auction == null) return NotFound();

        // todo : check seller == username

        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync() > 0;
        if(result) return Ok();

        return BadRequest("Could not update DB");

    }


}
