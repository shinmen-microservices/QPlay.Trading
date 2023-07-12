using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPlay.Common.Repositories.Interfaces;
using QPlay.Trading.Service.Models.Dtos;
using QPlay.Trading.Service.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QPlay.Trading.Service.Controllers;

[ApiController]
[Route("store")]
[Authorize]
public class StoreController : ControllerBase
{
    private readonly IRepository<CatalogItem> catalogRepository;
    private readonly IRepository<InventoryItem> inventoryRepository;
    private readonly IRepository<ApplicationUser> usersRepository;

    public StoreController(
        IRepository<CatalogItem> catalogRepository,
        IRepository<ApplicationUser> usersRepository,
        IRepository<InventoryItem> inventoryRepository
    )
    {
        this.catalogRepository = catalogRepository;
        this.usersRepository = usersRepository;
        this.inventoryRepository = inventoryRepository;
    }

    [HttpGet]
    public async Task<ActionResult<StoreDto>> GetAsync()
    {
        string userId = User.FindFirstValue("sub");

        IReadOnlyCollection<CatalogItem> catalogItems = await catalogRepository.GetAllAsync();
        IReadOnlyCollection<InventoryItem> inventoryItems = await inventoryRepository.GetAllAsync(
            item => item.UserId == Guid.Parse(userId)
        );
        ApplicationUser user = await usersRepository.GetAsync(Guid.Parse(userId));
        
        StoreDto storeDto = GetStoreDto(catalogItems, inventoryItems, user);

        return Ok(storeDto);
    }

    private StoreDto GetStoreDto(
        IReadOnlyCollection<CatalogItem> catalogItems,
        IReadOnlyCollection<InventoryItem> inventoryItems,
        ApplicationUser user
    )
    {
        return new(
            catalogItems.Select(
                catalogItem =>
                    new StoreItemDto(
                        catalogItem.Id,
                        catalogItem.Name,
                        catalogItem.Description,
                        catalogItem.Price,
                        inventoryItems
                            .FirstOrDefault(
                                inventoryItem => inventoryItem.CatalogItemId == catalogItem.Id
                            )
                            ?.Quantity ?? 0
                    )
            ),
            user?.Gil ?? 0
        );
    }
}